using DotnetSpider.Downloader;
using DotnetSpider.Monitor;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Proxy
{
    /// <summary>
    /// 代理获取器接口
    /// </summary>
    public interface IHttpProxy : IDisposable, IRecordable
    {
        /// <summary>
        /// 获取代理
        /// </summary>
        /// <param name="request">使用该代理的请求，可以做一些修改</param>
        /// <returns>代理</returns>
        Task<WebProxy> GetProxy(Request request);

        /// <summary>
        /// 结束代理的使用。
        /// 每一个请求结束后，ISpider会调用这个方法。
        /// </summary>
        /// <param name="proxy">代理</param>
        /// <param name="response">该请求的返回信息，一般需要注意<see cref="Response.StatusCode"/>和<see cref="Response.IsDownloaderTimeout"/></param>
        Task ReturnProxy(WebProxy proxy, Response response);
    }
}
