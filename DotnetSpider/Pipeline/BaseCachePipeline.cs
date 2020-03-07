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
    /// 需要实现Name属性和ProcessCache函数。
    /// </summary>
    public abstract class BaseCachePipeline : BaseZeroDisposable, IPipeline
    {
        private List<Tuple<List<IReadOnlyDictionary<string, object>>, dynamic>> _caches = new List<Tuple<List<IReadOnlyDictionary<string, object>>, dynamic>>();
        private readonly object _cachesLocker = new object();
        private bool _hasInitCachePipeline = false;
        private readonly object _hasInitCachePipelineLocker = new object();

        public bool IsCacheSupported => true;
        public bool EnableCache { get; set; } = true;
        public TimeSpan MaxCacheTime { get; set; } = TimeSpan.FromSeconds(1);
        public ILog Logger { get; set; }
        public abstract string Name { get; set; }

        public async Task Process(IReadOnlyDictionary<string, object> resultItems, dynamic sender = null)
        {
            InitCachePipeline();
            await Task.Run(() =>
            {
                lock (_cachesLocker)
                {
                    if (_caches.Count == 0 || _caches.Last().Item2 != sender)
                    {
                        _caches.Add(Tuple.Create(
                            new List<IReadOnlyDictionary<string, object>>() { resultItems }, 
                            sender));
                    }
                    else 
                    {
                        _caches.Last().Item1.Add(resultItems);
                    }
                }
            });
        }

        public async Task Process(IEnumerable<IReadOnlyDictionary<string, object>> resultItems, dynamic sender = null)
        {
            InitCachePipeline();
            await Task.Run(() =>
            {
                lock (_cachesLocker)
                {
                    if (_caches.Count == 0 || _caches.Last().Item2 != sender)
                    {
                        _caches.Add(Tuple.Create(
                            new List<IReadOnlyDictionary<string, object>>(resultItems),
                            sender));
                    }
                    else
                    {
                        _caches.Last().Item1.AddRange(resultItems);
                    }
                }
            });
        }

        /// <summary>
        /// 获取缓存的内容，此操作将清空缓存。
        /// </summary>
        /// <returns>缓存</returns>
        protected List<Tuple<List<IReadOnlyDictionary<string, object>>, dynamic>> TakeCaches()
        {
            lock (_cachesLocker)
            {
                var res = _caches;
                _caches = new List<Tuple<List<IReadOnlyDictionary<string, object>>, dynamic>>();
                return res;
            }
        }

        /// <summary>
        /// 处理缓存。
        /// </summary>
        /// <param name="caches">缓存，用一个二元对象的数组表示。二元对象的第一个成员表示resultItems，第二个成员表示sender。</param>
        /// <returns></returns>
        protected abstract Task ProcessCache(List<Tuple<List<IReadOnlyDictionary<string, object>>, dynamic>> caches);

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
            Enter();
            while (IsDisposed == false)
            {
                Task.WaitAll(ProcessCache(), WaitForNextProcess());
            }

            ProcessCache().Wait();//在执行Dispose时，最后一次调用ProcessCache，清空缓存。
            Leave();
        }

        private async Task ProcessCache()
        {
            List<Tuple<List<IReadOnlyDictionary<string, object>>, dynamic>> caches = null;
            lock (_cachesLocker)
            {
                caches = _caches;
                _caches = new List<Tuple<List<IReadOnlyDictionary<string, object>>, dynamic>>();
            }

            if (caches.Count > 0)
            {
                try
                {
                    await ProcessCache(caches);
                }
                catch (Exception e)
                {
                    Logger?.Error($"{ Name } has an exception when exec ProcessCache.", e);
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
