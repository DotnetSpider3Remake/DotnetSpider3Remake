using DotnetSpider.Monitor;
using DotnetSpider.Pipeline;
using DotnetSpider.Processor;
using DotnetSpider.Scheduler;
using DotnetSpider.Downloader;
using System;
using System.Collections.Generic;
using DotnetSpider.Runner;
using DotnetSpider.Proxy;

namespace DotnetSpider
{
    /// <summary>
    /// 爬虫接口定义。
    /// 所有属性应该在爬虫启动前初始化完毕。
    /// </summary>
    public interface ISpider : IDisposable, IRunnable, IRecordable
    {
        /// <summary>
        /// URL调度队列
        /// </summary>
        IScheduler Scheduler { get; set; }

        /// <summary>
        /// 下载器
        /// </summary>
        IDownloader Downloader { get; set; }

        /// <summary>
        /// 数据管道列表
        /// </summary>
        List<IPipeline> Pipelines { get; }

        /// <summary>
        /// 页面解析器列表
        /// </summary>
        List<IResponseProcessor> PageProcessors { get; }

        /// <summary>
        /// 代理获取器
        /// </summary>
        IHttpProxy HttpProxy { get; set; }

        #region 超时重试属性。不放在Downloader中，是为更换代理做准备。
        /// <summary>
        /// 最大下载超时失败重试次数。
        /// </summary>
        uint MaxRetry { get; set; }

        /// <summary>
        /// 下载超时后，重试下载的等待时间。
        /// </summary>
        TimeSpan RetryInterval { get; set; }
        #endregion

        /// <summary>
        /// 并行运行任务数量
        /// </summary>
        int Parallels { get; set; }

        /// <summary>
        /// 同一个线程内，当前请求结束时间到下一个请求开始时间的间隔。
        /// 不能与<seealso cref="FixedRequestDuration"/>同时设置。
        /// </summary>
        TimeSpan? RequestInterval { get; set; }

        /// <summary>
        /// 同一个线程内，当前请求开始时间到下一个请求开始时间的间隔。
        /// 不能与<seealso cref="RequestInterval"/>同时设置。
        /// </summary>
        TimeSpan? FixedRequestDuration { get; set; }

        /// <summary>
        /// 判断是否应该停止运行爬虫的函数。
        /// </summary>
        Func<ISpider, bool> ConditionOfStop { get; set; }

        /// <summary>
        /// 成功的任务数量。
        /// </summary>
        long Success { get; }

        /// <summary>
        /// 失败的任务数量。
        /// </summary>
        long Failed { get; }

        /// <summary>
        /// 结束的任务数量。
        /// </summary>
        long Finished { get; }

        /// <summary>
        /// 已经启动的任务数量。
        /// </summary>
        long Started { get; }

        /// <summary>
        /// 执行中的任务数量。
        /// </summary>
        long Unfinished { get; }
    }
}
