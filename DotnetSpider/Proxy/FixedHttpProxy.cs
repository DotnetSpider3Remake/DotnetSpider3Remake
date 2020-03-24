using DotnetSpider.Downloader;
using DotnetSpider.Runner;
using log4net;
using System.Net;
using System.Threading.Tasks;

namespace DotnetSpider.Proxy
{
    /// <summary>
    /// 固定代理获取器，返回一个固定的代理。
    /// 可以重写GetProxy，实现复杂的身份认证。
    /// 派生类需要注意Enter和Leave的调用
    /// </summary>
    public class FixedHttpProxy : BaseZeroDisposable, IHttpProxy
    {
        protected readonly WebProxy _proxy = null;

        public ILog Logger { get; set; }
        public string Name { get; set; }

        public FixedHttpProxy(WebProxy proxy)
        {
            _proxy = proxy;
        }

        protected FixedHttpProxy()
        {
            
        }

        public virtual Task<WebProxy> GetProxy(Request request)
        {
            return Task.FromResult(_proxy);
        }

        public virtual Task ReturnProxy(WebProxy proxy, Response response)
        {
            return Task.CompletedTask;
        }
    }
}
