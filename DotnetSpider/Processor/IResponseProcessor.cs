using DotnetSpider.Downloader;
using DotnetSpider.Monitor;
using System;
using System.Threading.Tasks;

namespace DotnetSpider.Processor
{
    /// <summary>
    /// 页面解析器、抽取器。
    /// Dispose方法被调用后，应阻塞等待Process方法完成。
    /// </summary>
    public interface IResponseProcessor : IDisposable, IRecordable
    {
        /// <summary>
        /// 解析数据结果, 解析目标链接
        /// </summary>
        /// <param name="page">页面数据</param>
        Task Process(Response page);
    }
}
