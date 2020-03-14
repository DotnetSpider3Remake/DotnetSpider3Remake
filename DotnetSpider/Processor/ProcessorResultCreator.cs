using DotnetSpider.Downloader;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Processor
{
    public static class ProcessorResultCreator
    {
        public static ProcessorResult CreateResult(this Response response)
        {
            return new ProcessorResult(response.Request);
        }

        public static ProcessorResult CreateResult(this IResponseProcessor _, Response response)
        {
            return new ProcessorResult(response.Request);
        }
    }
}
