using DotnetSpider;
using DotnetSpider.Downloader;
using DotnetSpider.Logger;
using DotnetSpider.Pipeline;
using DotnetSpider.Processor;
using System;
using System.Xml;

namespace GetBaiduLinks
{
    class ResponseProcessor : BasePageProcessor
    {
        protected override ProcessorResult ProcessSync(Response page)
        {
            ProcessorResult result = new ProcessorResult(page.Request);
            try
            {
                var links = page.HtmlNode?.SelectNodes("//a[@href]");
                if (links != null)
                {
                    foreach (var link in links)
                    {
                        result.AddTargetRequest(link.Attributes["href"].Value);
                    }
                }
            }
            catch (Exception e)
            {
                Logger?.Error($"Can not process page :\n{ page }\nExpection:\n{ e }");
                result.SkipTargetRequests = true;
            }

            return result;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Spider spider = new Spider()
            {
                Logger = LoggerCreator.GetLogger("baidu"),
                Name = "baidu",
                Parallels = 8,
                ConditionOfStop = s => ((Spider)s).Finished >= 1000
            };

            spider.PageProcessors.Add(new ResponseProcessor()
            {
                Name = "processor",
                Logger = LoggerCreator.GetLogger("processor")
            });
            spider.Pipelines.Add(new ConsolePipeline());
            spider.Scheduler.TraverseStrategy = DotnetSpider.Scheduler.TraverseStrategy.BFS;
            spider.Scheduler.Push(new Request("https://www.baidu.com/"));
            spider.Run();
        }
    }
}
