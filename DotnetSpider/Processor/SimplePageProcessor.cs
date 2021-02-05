using DotnetSpider.Downloader;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Processor
{
    /// <summary>
    /// 简单页面解析器。
    /// 无需重载，改写<see cref="Processor"/>即可。
    /// </summary>
    public sealed class SimplePageProcessor : BasePageProcessor
    {
        /// <summary>
        /// 用于解析的函数，等价于<see cref="ProcessSync(Response)"/>。
        /// 第一参数<see cref="SimplePageProcessor"/>是调用此函数的实例。
        /// </summary>
        public Func<SimplePageProcessor, Response, ProcessorResult> Processor { get; set; }

        protected override ProcessorResult ProcessSync(Response page)
        {
            return Processor?.Invoke(this, page);
        }
    }
}
