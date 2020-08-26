using DotnetSpider.Downloader;
using DotnetSpider.Pipeline;
using DotnetSpider.Processor;
using DotnetSpider.Proxy;
using DotnetSpider.Runner;
using DotnetSpider.Scheduler;
using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpider
{
    public class Spider : BaseSpider
    {
        #region 公共属性
        /// <summary>
        /// 当没有剩余请求时停止运行。
        /// </summary>
        public bool StopWhenNoRequest { get; set; } = false;

        /// <summary>
        /// 没有剩余请求时的最大等待时间。
        /// </summary>
        public TimeSpan MaxNoRequestDuration { get; set; } = TimeSpan.FromSeconds(20);
        #endregion

        #region 私有常量
        private static readonly TimeSpan _pollTimeout = TimeSpan.FromMilliseconds(10);
        #endregion

        #region 私有变量
        /// <summary>
        /// 第一次剩余请求为空的时间。
        /// </summary>
        private DateTime _firstEmptyTime = DateTime.MinValue;
        #endregion

        /// <summary>
        /// 构造爬虫实例。
        /// </summary>
        /// <param name="name">爬虫名称</param>
        /// <param name="scheduler">调度器</param>
        /// <param name="downloader">下载器</param>
        public Spider(string name = null, IScheduler scheduler = null, IDownloader downloader = null)
        {
            Name = name ?? "Spider";
            Scheduler = scheduler ?? new DuplicateRemovedScheduler();
            Downloader = downloader ?? new HttpClientDownloader();
        }

        protected override void InitSpider()
        {
            base.InitSpider();
            if (StopWhenNoRequest)
            {
                if (ConditionOfStop == null)
                {
                    ConditionOfStop = CheckRequestIsEmtpy;
                }
                else 
                {
                    var condition = ConditionOfStop;
                    ConditionOfStop = s => condition(s) || CheckRequestIsEmtpy(s);
                }
            }
        }

        /// <summary>
        /// 运行爬虫线程
        /// </summary>
        protected override void RunSpiderThread()
        {
            while (IsRunning)
            {
                Request request = null;
                while (IsRunning && request == null)
                {
                    request = Scheduler.Poll(_pollTimeout);
                }

                if (request == null)
                {
                    break;
                }

                DateTime begin = DateTime.Now;
                long index = AddStarting();
                Logger?.Info($"Task { index } has been started.\nRequest:{ request }");
                try
                {
                    using (GetAutoLeaveHelper())
                    {
                        uint tries = 0;
                        bool success = false;
                        do
                        {
                            if (tries == 0)
                            {
                                Logger?.Info($"Task { index } has been started.\nRequest:{ request }");
                            }
                            else
                            {
                                Logger?.Warn($"Request { index } was timeout.");
                                Thread.Sleep(RetryInterval);
                                Logger?.Info($"Task { index } has been retried({ tries }).");
                            }
                            success = RunRequest(request).Result;
                            ++tries;
                        } while (success == false && tries <= MaxRetry);

                        if (success)
                        {
                            AddSuccess();
                            Logger?.Info($"Request { index } is exec successful in { (DateTime.Now - begin).TotalSeconds } seconds.");
                        }
                        else
                        {
                            AddFailed();
                            Logger?.Warn($"Request { index } was timeout.");
                        }
                    }
                }
                catch (Exception e)
                {
                    AddFailed();
                    Logger?.Error($"Error occured!\nTask { index }:{ request }\nException:{ e.Message }");
                }
                finally
                {
                    Logger?.Info($"Running:{ Unfinished },Success:{ Success },Failed:{ Failed },Total:{ Started }.");
                    Wait(begin);
                }
            }
        }

        #region 私有方法
        /// <summary>
        /// 在爬虫线程中等待指定时间。
        /// </summary>
        /// <param name="begin">任务开始时间</param>
        private void Wait(DateTime begin)
        {
            TimeSpan wait;
            if (RequestInterval.HasValue)
            {
                wait = RequestInterval.Value;
            }
            else if (FixedRequestDuration.HasValue)
            {
                wait = FixedRequestDuration.Value - (DateTime.Now - begin);
            }
            else
            {
                wait = TimeSpan.Zero;
            }

            if (wait < TimeSpan.Zero)
            {
                wait = TimeSpan.Zero;
            }

            Thread.Sleep(wait);
        }

        /// <summary>
        /// 运行爬虫任务。
        /// </summary>
        /// <param name="request">爬虫任务</param>
        /// <returns>成功获取服务器返回内容时返回true，连接超时返回false。</returns>
        private async Task<bool> RunRequest(Request request)
        {
            IWebProxy proxy = null;
            if (HttpProxy != null)
            {
                proxy = await HttpProxy.GetProxy(request);
            } 

            Response response = Downloader.Download(request, proxy);
            if (proxy != null)
            {
                await HttpProxy.ReturnProxy(proxy, response);
            }

            if (response.IsDownloaderTimeout)
            {
                return false;
            }

            bool retry = false;
            List<Request> targetRequests = new List<Request>();
            List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
            foreach (var i in PageProcessors)
            {
                ProcessorResult result = await i.Process(response);
                retry = retry || result.Retry;
                if (result.SkipTargetRequests == false)
                {
                    targetRequests.AddRange(result.TargetRequests);
                }

                if (result.ResultItems.Count > 0)
                {
                    items.Add(result.ResultItems);
                }
            }

            foreach (var i in Pipelines)
            {
                await i.Process(items, this);
            }

            Scheduler.Push(targetRequests);
            if (retry)
            {
                Scheduler.Push(request);
            }

            return true;
        }

        private bool CheckRequestIsEmtpy(ISpider spider)
        {
            if (spider.Scheduler.Count > 0)
            {
                _firstEmptyTime = DateTime.MinValue;
            }
            else if (MaxNoRequestDuration <= TimeSpan.Zero)
            {
                return true;
            }
            else if (_firstEmptyTime != DateTime.MinValue)
            {
                return DateTime.Now - _firstEmptyTime > MaxNoRequestDuration;
            }

            return false;
        }
        #endregion
    }
}
