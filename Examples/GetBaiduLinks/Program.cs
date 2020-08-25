using DotnetSpider;
using DotnetSpider.Downloader;
using DotnetSpider.Logger;
using DotnetSpider.Pipeline;
using DotnetSpider.Processor;
using System;

namespace GetBaiduLinks
{
    class ResponseProcessor : BasePageProcessor
    {
        protected override ProcessorResult ProcessSync(Response page)
        {
            ProcessorResult result = new ProcessorResult(page.Request);
            result.ResultItems["web"] = page.Content;
            return result;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using Spider spider = new Spider()
            {
                Logger = LoggerCreator.GetLogger("baidu"),
                Name = "baidu"
            };

            spider.PageProcessors.Add(new ResponseProcessor());
            spider.Pipelines.Add(new ConsolePipeline());
            spider.Scheduler.Push(new Request("https://www.baidu.com/"));
            spider.Run();
        }
    }
}
