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

        public async Task<Response> Download(Request request, WebProxy proxy = null)
        {
            Enter();
            var res = await DownloadSync(request, proxy);
            Leave();
            return res;
        }

        /// <summary>
        /// 同步下载链接内容。
        /// </summary>
        /// <param name="request">链接请求</param>
        /// <param name="proxy">代理 <see cref="DotnetSpider.Proxy.IHttpProxy"/></param>
        /// <returns>链接请求结果</returns>
        protected abstract Task<Response> DownloadSync(Request request, WebProxy proxy = null);
    }
}
