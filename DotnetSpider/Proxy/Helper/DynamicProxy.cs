using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace DotnetSpider.Proxy.Helper
{
    /// <summary>
    /// 动态HTTP代理，一般用于<see cref="HttpClient"/>中动态修改代理名称。
    /// </summary>
    public class DynamicProxy : IWebProxy
    {
        /// <summary>
        /// 内部<see cref="IWebProxy"/>代理实例，为null时表示不使用代理。
        /// </summary>
        public IWebProxy InnerProxy { get; set; }

        private static readonly ICredentials _emptyCredentials = new NetworkCredential();
        public ICredentials Credentials 
        {
            get
            {
                if (InnerProxy != null)
                {
                    return InnerProxy.Credentials;
                }
                else 
                {
                    return _emptyCredentials;
                }
            } 
            set 
            {
                if (InnerProxy != null)
                {
                    InnerProxy.Credentials = value;
                }
            }
        }

        public Uri GetProxy(Uri destination)
        {
            return InnerProxy?.GetProxy(destination) ?? destination;
        }

        public bool IsBypassed(Uri host)
        {
            return InnerProxy?.IsBypassed(host) ?? true;
        }
    }
}
