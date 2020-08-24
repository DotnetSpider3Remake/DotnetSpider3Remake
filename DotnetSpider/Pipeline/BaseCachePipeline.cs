using DotnetSpider.Runner;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpider.Pipeline
{
    /// <summary>
    /// 带缓存的数据管道基类。
    /// 需要实现Name属性和Process函数。
    /// </summary>
    public abstract class BaseCachePipeline : BaseZeroDisposable, IPipeline
    {
        private List<Tuple<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>> _caches = new List<Tuple<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>>();
        private readonly object _cachesLocker = new object();
        private bool _hasInitCachePipeline = false;
        private readonly object _hasInitCachePipelineLocker = new object();

        public bool IsCacheSupported => true;
        public bool EnableCache { get; set; } = true;
        public TimeSpan MaxCacheTime { get; set; } = TimeSpan.FromSeconds(1);
        public ILog Logger { get; set; }
        public abstract string Name { get; set; }

        public async Task Process(IReadOnlyDictionary<string, object> resultItems, ISpider sender = null)
        {
            if (EnableCache)
            {
                InitCachePipeline();
                await Task.Run(() =>
                {
                    lock (_cachesLocker)
                    {
                        if (_caches.Count == 0 || _caches.Last().Item2 != sender)
                        {
                            _caches.Add(Tuple.Create<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>(
                                new List<IReadOnlyDictionary<string, object>>() { resultItems },
                                sender));
                        }
                        else
                        {
                            ((List<IReadOnlyDictionary<string, object>>)(_caches.Last().Item1)).Add(resultItems);
                        }
                    }
                });
            }
            else
            {
                await Process(new Tuple<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>[]
                    {
                        Tuple.Create<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>(
                            new IReadOnlyDictionary<string, object>[] { resultItems }, 
                            sender)
                    });
            }
        }

        public async Task Process(IEnumerable<IReadOnlyDictionary<string, object>> resultItems, ISpider sender = null)
        {
            if (EnableCache)
            {
                InitCachePipeline();
                await Task.Run(() =>
                {
                    lock (_cachesLocker)
                    {
                        if (_caches.Count == 0 || _caches.Last().Item2 != sender)
                        {
                            _caches.Add(Tuple.Create<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>(
                                new List<IReadOnlyDictionary<string, object>>(resultItems),
                                sender));
                        }
                        else
                        {
                            ((List<IReadOnlyDictionary<string, object>>)(_caches.Last().Item1)).AddRange(resultItems);
                        }
                    }
                });
            }
            else
            {
                await Process(new Tuple<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>[]
                    {
                        Tuple.Create(resultItems, sender)
                    });
            }
        }

        /// <summary>
        /// 处理缓存。
        /// </summary>
        /// <param name="caches">缓存，用一个二元对象的数组表示。二元对象的第一个成员表示resultItems，第二个成员表示sender。</param>
        /// <returns></returns>
        protected abstract Task Process(IEnumerable<Tuple<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>> caches);

        private void InitCachePipeline()
        {
            if (IsDisposed)
            {
                return;
            }

            lock (_hasInitCachePipelineLocker)
            {
                if (_hasInitCachePipeline)
                {
                    return;
                }

                _hasInitCachePipeline = true;
            }

            var t = new Thread(RunCachePipeline)
            {
                IsBackground = true
            };
            t.Start();
        }

        private void RunCachePipeline()
        {
            using (var helper = GetAutoLeaveHelper())
            {
                while (IsDisposed == false)
                {
                    Task.WaitAll(Process(), WaitForNextProcess());
                }

                Process().Wait();//在执行Dispose时，最后一次调用Process，清空缓存。
            }
        }

        private async Task Process()
        {
            List<Tuple<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>> caches = null;
            lock (_cachesLocker)
            {
                caches = _caches;
                _caches = new List<Tuple<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>>();
            }

            if (caches.Count > 0)
            {
                try
                {
                    await Process(caches);
                }
                catch (Exception e)
                {
                    Logger?.Error($"{ Name } has an exception when exec Process.", e);
                }
            }
        }

        private async Task WaitForNextProcess()
        {
            DateTime begin = DateTime.Now;
            while (DateTime.Now - begin >= MaxCacheTime && IsDisposed == false)
            {
                await Task.Delay(1);//每隔1ms检测一次是否执行过Dispose。
            }
        }
    }
}
