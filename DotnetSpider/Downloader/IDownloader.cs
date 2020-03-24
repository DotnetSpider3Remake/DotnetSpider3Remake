using DotnetSpider.Monitor;
using DotnetSpider.Proxy;
using System;
using System.Net;
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
        /// <param name="request">链接请求</param>
        /// <param name="proxy">代理 <see cref="IHttpProxy"/></param>
        /// <returns>链接请求结果</returns>
        Task<Response> Download(Request request, WebProxy proxy = null);
    }
}
