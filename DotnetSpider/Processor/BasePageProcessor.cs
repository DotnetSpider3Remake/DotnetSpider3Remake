using DotnetSpider.Downloader;
using DotnetSpider.Runner;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Processor
{
    public abstract class BasePageProcessor : BaseZeroDisposable, IResponseProcessor
    {
        public ILog Logger { get; set; }
        public string Name { get; set; }

        public async Task<ProcessorResult> Process(Response page)
        {
            return await Task.Run(() =>
            {
                Enter();
                try
                {
                    return ProcessSync(page);
                }
                finally
                {
                    Leave();
                }
            });
        }

        protected abstract ProcessorResult ProcessSync(Response page);
    }
}
