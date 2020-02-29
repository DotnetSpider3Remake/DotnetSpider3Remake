using DotnetSpider.Monitor;
using DotnetSpider.Downloader;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetSpider.Scheduler
{
    /// <summary>
    /// URL调度队列, 实现广度优化或深度优化策略, 实现URL去重, 并且队列需要可被监控。
    /// 考虑性能原因, 队列没有和ISpider的解耦, 因此同一个Scheduler不能被不同的Spider的使用。
    /// </summary>
    public interface IScheduler : IDisposable, IRecordable
    {
        /// <summary>
        /// 遍历方式，仅能在任务运行前修改。
        /// </summary>
        TraverseStrategy TraverseStrategy { get; set; }

        /// <summary>
        /// 添加请求对象到队列
        /// </summary>
        /// <param name="request">请求对象</param>
        void Push(Request request);

        /// <summary>
        /// 添加多个请求对象到队列。
        /// </summary>
        /// <param name="requests">请求对象们</param>
        void Push(IEnumerable<Request> requests);

        /// <summary>
        /// 异步添加请求对象到队列
        /// </summary>
        /// <param name="request">请求对象</param>
        Task PushAsync(Request request);

        /// <summary>
        /// 异步添加多个请求对象到队列。
        /// </summary>
        /// <param name="requests">请求对象们</param>
        Task PushAsync(IEnumerable<Request> requests);

        /// <summary>
        /// 取得一个需要处理的请求对象
        /// </summary>
        /// <returns>请求对象</returns>
        Request Poll();

        /// <summary>
        /// 异步取得一个需要处理的请求对象
        /// </summary>
        /// <returns>请求对象</returns>
        Task<Request> PollAsync();

        /// <summary>
        /// 清空队列
        /// </summary>
        void Clear();
    }
}
