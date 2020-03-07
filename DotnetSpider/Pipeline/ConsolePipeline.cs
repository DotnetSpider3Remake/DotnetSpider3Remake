using log4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetSpider.Pipeline
{
    public class ConsolePipeline : IPipeline
    {
        public ILog Logger { get; set; }
        public bool IsCacheSupported => false;
        public bool EnableCache { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public TimeSpan MaxCacheTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Dispose()
        {
            
        }

        public Task Process(IReadOnlyDictionary<string, object> resultItems, dynamic _ = null)
        {
            if (resultItems == null)
            {
                return Task.CompletedTask;
            }

            int count = 0;
            foreach (var i in resultItems)
            {
                Console.WriteLine(i.Key + ":\t" + i.Value.ToString());
                ++count;
            }

            Logger?.Info($"Out put to Console : { count } lines.");
            return Task.CompletedTask;
        }

        public Task Process(IEnumerable<IReadOnlyDictionary<string, object>> resultItems, dynamic _ = null)
        {
            if (resultItems == null)
            {
                return Task.CompletedTask;
            }

            int count = 0;
            foreach (var i in resultItems)
            {
                if (i == null)
                {
                    continue;
                }

                foreach (var j in i)
                {
                    Console.WriteLine(j.Key + ":\t" + j.Value.ToString());
                    ++count;
                }
            }

            Logger?.Info($"Out put to Console : { count } lines.");
            return Task.CompletedTask;
        }
    }
}
