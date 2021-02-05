using DotnetSpider.Runner;
using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Downloader
{
    /// <summary>
    /// IDownloader的抽象化实现，析构时会等待下载完成。
    /// </summary>
    public abstract class BaseDowloader : BaseZeroDisposable, IDownloader
    {
        public ILog Logger { get; set; }
        public string Name { get; set; }
        public bool AllowAutoRedirect { get; set; } = true;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(8);

        public async Task<Response> DownloadAsync(Request request, IWebProxy proxy = null)
        {
            using (var helper = GetAutoLeaveHelper())
            {
                return await Downloading(request, proxy);
            }
        }

        public Response Download(Request request, IWebProxy proxy = null)
        {
            using (var helper = GetAutoLeaveHelper())
            {
                return Downloading(request, proxy).Result;
            }
        }

        /// <summary>
        /// 异步下载链接内容。
        /// 不需要考虑Enter、Leave问题，已经由调用者处理了。
        /// </summary>
        /// <param name="request">链接请求</param>
        /// <param name="proxy">代理 <see cref="DotnetSpider.Proxy.IHttpProxy"/></param>
        /// <returns>链接请求结果</returns>
        protected abstract Task<Response> Downloading(Request request, IWebProxy proxy = null);
    }
}
