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
        /// 是否自动跳转。
        /// </summary>
        bool AllowAutoRedirect { get; set; }
        /// <summary>
        /// 下载超时时间。
        /// </summary>
        TimeSpan Timeout { get; set; }

        /// <summary>
        /// 下载链接内容，实现应该线程安全、可重入。
        /// （无需关心公共属性的线程安全，那应该由使用者维护）
        /// </summary>
        /// <param name="request">链接请求</param>
        /// <param name="proxy">代理 <see cref="IHttpProxy"/></param>
        /// <returns>链接请求结果</returns>
        Task<Response> DownloadAsync(Request request, IWebProxy proxy = null);

        /// <summary>
        /// 下载链接内容，实现应该线程安全、可重入。
        /// （无需关心公共属性的线程安全，那应该由使用者维护）
        /// </summary>
        /// <param name="request">链接请求</param>
        /// <param name="proxy">代理 <see cref="IHttpProxy"/></param>
        /// <returns>链接请求结果</returns>
        Response Download(Request request, IWebProxy proxy = null);
    }
}
