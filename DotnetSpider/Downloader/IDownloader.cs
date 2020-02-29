using DotnetSpider.Monitor;
using System;
using System.Threading.Tasks;

namespace DotnetSpider.Downloader
{
    /// <summary>
    /// 下载器接口。
    /// Dispose方法被调用后，应阻塞等待Download方法完成。
    /// </summary>
    public interface IDownloader : IDisposable, IRecordable
    {
        /// <summary>
        /// 下载链接内容，实现应该线程安全、可重入。
        /// </summary>
        /// <param name="request">链接请求 <see cref="Request"/></param>
        /// <returns>链接请求结果 <see cref="Response"/></returns>
        Task<Response> Download(Request request);
    }
}
