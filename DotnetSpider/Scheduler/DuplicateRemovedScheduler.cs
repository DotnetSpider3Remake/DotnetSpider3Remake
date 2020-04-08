using DotnetSpider.Downloader;
using DotnetSpider.Runner;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Scheduler
{
    /// <summary>
    /// 去重调度器。
    /// 默认遍历方式为DoNotCare。
    /// 空间复杂度O(N)，N为所有不同请求的数量。
    /// </summary>
    public class DuplicateRemovedScheduler : BaseZeroDisposable, IScheduler
    {
        private readonly LinkedList<Request> _requestList = new LinkedList<Request>();
        private readonly HashSet<Request> _requestSet = new HashSet<Request>();
        private readonly object _requestLocker = new object();

        public TraverseStrategy TraverseStrategy { get; set; } = TraverseStrategy.DoNotCare;
        public ILog Logger { get; set; }
        public string Name { get; set; } = "DuplicateRemovedScheduler";

        public long Count
        {
            get
            {
                lock (_requestLocker)
                {
                    return _requestList.Count;
                }
            }
        }

        public void Clear() 
        {
            lock (_requestLocker)
            {
                _requestList.Clear();
            }
        }

        public Request Poll()
        {
            return PollAsync().Result;
        }

        public async Task<Request> PollAsync()
        {
            using (var helper = GetAutoLeaveHelper())
            {
                while (IsDisposed == false)
                {
                    lock (_requestLocker)
                    {
                        if (_requestList.Count > 0)
                        {
                            Request res;
                            if (TraverseStrategy == TraverseStrategy.BFS)
                            {
                                res = _requestList.First.Value;
                                _requestList.RemoveFirst();
                            }
                            else
                            {
                                res = _requestList.Last.Value;
                                _requestList.RemoveLast();
                            }

                            return res;
                        }
                    }

                    await Task.Delay(1);
                }

                return null;
            }
        }

        public void Push(Request request)
        {
            using (var helper = GetAutoLeaveHelper())
            {
                lock (_requestLocker)
                {
                    if (_requestSet.Contains(request) == false)
                    {
                        _requestList.AddLast(request);
                        _requestSet.Add(request);
                    }
                }
            }
        }

        public void Push(IEnumerable<Request> requests)
        {
            using (var helper = GetAutoLeaveHelper())
            {
                lock (_requestLocker)
                {
                    foreach (var i in requests)
                    {
                        if (_requestSet.Contains(i) == false)
                        {
                            _requestList.AddLast(i);
                            _requestSet.Add(i);
                        }
                    }
                }
            }
        }

        public async Task PushAsync(Request request)
        {
            await Task.Run(() => Push(request));
        }

        public async Task PushAsync(IEnumerable<Request> requests)
        {
            await Task.Run(() => Push(requests));
        }

        protected override void DisposeOthers()
        {
            lock (_requestLocker)
            {
                _requestList.Clear();
                _requestSet.Clear();
            }
        }
    }
}
