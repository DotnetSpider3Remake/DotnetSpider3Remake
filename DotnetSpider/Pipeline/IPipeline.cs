using DotnetSpider.Monitor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetSpider.Pipeline
{
    /// <summary>
    /// 数据管道接口, 通过数据管道把解析的数据存到不同的存储中(文件、数据库）。
    /// Dispose方法被调用后，应阻塞等待Process方法完成。实现时，可以继承BaseZeroDisposable或BaseCachePipeline来实现。
    /// </summary>
    public interface IPipeline : IDisposable, IRecordable
    {
        /// <summary>
        /// 是否支持缓存模式。
        /// </summary>
        bool IsCacheSupported { get; }

        /// <summary>
        /// 缓存模式启用状态。
        /// 在第一次调用Process后不能修改。
        /// IsCacheSupported为true时才生效。
        /// </summary>
        bool EnableCache { get; set; }

        /// <summary>
        /// 最大缓存时间。
        /// 在第一次调用Process后不能修改。
        /// IsCacheSupported为true时才生效。
        /// </summary>
        TimeSpan MaxCacheTime { get; set; }

        /// <summary>
        /// 处理页面解析器解析到的数据结果
        /// </summary>
        /// <param name="resultItems">数据结果</param>
        /// <param name="sender">调用者</param>
        Task Process(IReadOnlyDictionary<string, object> resultItems, ISpider sender = null);

        /// <summary>
        /// 批量处理页面解析器解析到的数据集。
        /// 仅用于任务即将结束时或有缓冲区的任务。
        /// </summary>
        /// <param name="resultItems">数据集</param>
        /// <param name="sender">调用者</param>
        Task Process(IEnumerable<IReadOnlyDictionary<string, object>> resultItems, ISpider sender = null);
    }
}