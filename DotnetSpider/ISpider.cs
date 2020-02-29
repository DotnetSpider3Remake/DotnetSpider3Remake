using DotnetSpider.Monitor;
using DotnetSpider.Pipeline;
using DotnetSpider.Processor;
using DotnetSpider.Scheduler;
using DotnetSpider.Downloader;
using System;
using System.Collections.Generic;

namespace DotnetSpider.Core
{
    /// <summary>
    /// 爬虫接口定义。
    /// IDisposable
    /// </summary>
    public interface ISpider : IDisposable, IRunnable, IControllable, IRecordable
    {
        /// <summary>
        /// URL调度队列
        /// </summary>
        IScheduler Scheduler { get; }

        /// <summary>
        /// 下载器
        /// </summary>
        IDownloader Downloader { get;}

        /// <summary>
        /// 数据管道列表
        /// </summary>
        IEnumerable<IPipeline> Pipelines { get; }

        /// <summary>
        /// 页面解析器列表
        /// </summary>
        IEnumerable<IPageProcessor> PageProcessors { get; }
    }
}
