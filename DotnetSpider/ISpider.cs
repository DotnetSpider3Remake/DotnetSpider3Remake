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
        /// 是否继续获取<seealso cref="Request"/>。
        /// </summary>
        /// <returns>是否继续获取<seealso cref="Request"/></returns>
        bool ContinuePollRequest();
    }
}
