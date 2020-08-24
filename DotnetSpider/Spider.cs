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
    public class Spider : BaseZeroDisposable, ISpider
    {
        #region 继承自ISpider的属性
        public IScheduler Scheduler { get; set; } = new DuplicateRemovedScheduler();
        public IDownloader Downloader { get; set; } = new HttpClientDownloader();
        public List<IPipeline> Pipelines { get; } = new List<IPipeline>();
        public List<IResponseProcessor> PageProcessors { get; } = new List<IResponseProcessor>();
        public IHttpProxy HttpProxy { get; set; } = null;
        public ILog Logger { get; set; } = null;
        public string Name { get; set; } = "Spider";
        public int Parallels { get; set; } = 1;
        public TimeSpan? RequestInterval { get; set; } = null;
        public TimeSpan? FixedRequestDuration { get; set; } = null;
        #endregion

        #region 新增属性
        private bool _isRunning = false;
        private bool _hasExit = false;
        private readonly object _stateLocker = new object();
        /// <summary>
        /// 是否正在运行。
        /// </summary>
        public bool IsRunning
        {
            get
            {
                lock (_stateLocker)
                {
                    return !_hasExit && _isRunning;
                }
            }
        }

        /// <summary>
        /// 是否已经退出。
        /// </summary>
        public bool HasExit
        {
            get
            {
                lock (_stateLocker)
                {
                    return _hasExit;
                }
            }
        }

        private bool _hasStarted = false;
        private readonly object _hasStartedLocker = new object();
        /// <summary>
        /// 是否已经启动过。
        /// 即使已经退出，仍然返回true。
        /// </summary>
        public bool HasStarted
        {
            get
            {
                lock (_hasStartedLocker)
                {
                    return _hasStarted;
                }
            }
        }

        private long _countSuccess = 0;
        private long _countFailed = 0;
        private long _countStarted = 0;
        private readonly object _countLocker = new object();
        /// <summary>
        /// 成功的任务数量。
        /// </summary>
        public long Success
        {
            get 
            {
                lock (_countLocker)
                {
                    return _countSuccess;
                }
            }
        }

        /// <summary>
        /// 失败的任务数量。
        /// </summary>
        public long Failed
        {
            get
            {
                lock (_countLocker)
                {
                    return _countFailed;
                }
            }
        }

        /// <summary>
        /// 结束的任务数量。
        /// </summary>
        public long Finished
        {
            get
            {
                lock (_countLocker)
                {
                    return _countFailed + _countSuccess;
                }
            }
        }

        /// <summary>
        /// 已经启动的任务数量。
        /// </summary>
        public long Started
        {
            get
            {
                lock (_countLocker)
                {
                    return _countStarted;
                }
            }
        }

        /// <summary>
        /// 执行中的任务数量。
        /// </summary>
        public long Unfinished
        {
            get
            {
                lock (_countLocker)
                {
                    return _countStarted - _countFailed - _countSuccess;
                }
            }
        }
        #endregion

        #region 私有常量
        static readonly TimeSpan _pollTimeout = TimeSpan.FromMilliseconds(10);
        #endregion

        #region 实现接口
        public async Task<bool> Continue()
        {
            return await Task.Run<bool>(() =>
            {
                lock (_stateLocker)
                {
                    if (_hasExit || _isRunning)
                    {
                        return false;
                    }
                    else
                    {
                        _isRunning = true;
                        return true;
                    }
                }
            });
        }

        public async Task<bool> Exit()
        {
            return await Task.Run<bool>(() =>
            {
                lock (_stateLocker)
                {
                    if (_hasExit)
                    {
                        return false;
                    }
                    else
                    {
                        _hasExit = true;
                    }
                }

                Scheduler.Clear();
                WaitFinished();
                return true;
            });
        }

        public async Task<bool> Pause(Action action = null)
        {
            return await Task.Run<bool>(() =>
            {
                bool success;
                lock (_stateLocker)
                {
                    if (!_hasExit && _isRunning)
                    {
                        _isRunning = false;
                        success = true;
                    }
                    else
                    {
                        success = false;
                    }
                }

                if (success)
                {
                    action?.Invoke();
                }

                return success;
            });
        }

        public void Run()
        {
            lock (_hasStartedLocker)
            {
                if (_hasStarted)
                {
                    return;
                }
                else
                {
                    _hasStarted = true;
                }
            }

            InitSpider();
            if (CheckConfiguration())
            {
                Parallel.For(0, Parallels, RunSpiderThread);
            }

            Exit().Wait();
        }

        public async Task RunAsync()
        {
            await Task.Run(Run);
        }
        #endregion

        #region 可以在派生类中重新实现的接口函数
        public virtual bool ContinuePollRequest()
        {
            return true;
        }
        #endregion

        #region 可以在派生类中重新实现的函数
        protected override void DisposeOthers()
        {
            Exit().Wait();

            Scheduler.Dispose();
            Scheduler = null;
            Downloader.Dispose();
            Downloader = null;
            foreach (var i in Pipelines)
            {
                i.Dispose();
            }

            Pipelines.Clear();

            foreach (var i in PageProcessors)
            {
                i.Dispose();
            }

            PageProcessors.Clear();

            HttpProxy?.Dispose();
            HttpProxy = null;
            Logger = null;
        }

        /// <summary>
        /// 初始化爬虫。
        /// </summary>
        protected virtual void InitSpider()
        {

        }

        /// <summary>
        /// 检测配置是否正确。
        /// </summary>
        /// <returns>配置是否正确</returns>
        protected virtual bool CheckConfiguration()
        {
            bool success = true;

            if (Scheduler == null)
            {
                Logger?.Fatal("Scheduler is null.");
                success = false;
            }

            if (Downloader == null)
            {
                Logger?.Fatal("Downloader is null.");
                success = false;
            }

            if (Pipelines.Count == 0)
            {
                Logger?.Fatal("Pipelines are empty.");
                success = false;
            }

            if (PageProcessors.Count == 0)
            {
                Logger?.Fatal("PageProcessors are empty.");
                success = false;
            }

            if (Parallels < 1)
            {
                Logger?.Fatal("Parallels is less than 1.");
                success = false;
            }

            if (RequestInterval != null && FixedRequestDuration != null)
            {
                Logger?.Fatal("RequestInterval and FixedRequestDuration can not exist both.");
                success = false;
            }

            return success;
        }

        #endregion

        #region 保护方法
        /// <summary>
        /// 增加成功的任务数量。
        /// </summary>
        protected void AddSuccess()
        {
            lock (_countLocker)
            {
                ++_countSuccess;
            }
        }

        /// <summary>
        /// 增加启动的任务数量。
        /// </summary>
        /// <returns>最后增加的任务的索引。</returns>
        protected long AddStarting()
        {
            lock (_countLocker)
            {
                return ++_countStarted;
            }
        }

        /// <summary>
        /// 增加失败的任务数量。
        /// </summary>
        protected void AddFailed()
        {
            lock (_countLocker)
            {
                ++_countFailed;
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 运行爬虫线程
        /// </summary>
        private void RunSpiderThread(int _)
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
                        RunRequest(request).Wait();
                        AddSuccess();
                        Logger?.Info($"Request { index } is exec successful in { (DateTime.Now - begin).TotalSeconds } seconds.");
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
        private async Task RunRequest(Request request)
        {
            IWebProxy proxy = await HttpProxy?.GetProxy(request);
            Response response = Downloader.Download(request, proxy);
            if (proxy != null && HttpProxy != null)
            {
                await HttpProxy.ReturnProxy(proxy, response);
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
        }
        #endregion
    }
}
