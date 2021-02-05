using DotnetSpider.Downloader;
using DotnetSpider.Runner;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpider.Scheduler
{
    /// <summary>
    /// 普通连续调度器。
    /// </summary>
    public class ListScheduler : BaseZeroDisposable, IScheduler
    {
        private readonly LinkedList<Request> _requestList = new LinkedList<Request>();
        private readonly object _requestLocker = new object();

        public TraverseStrategy TraverseStrategy { get; set; } = TraverseStrategy.DoNotCare;
        public ILog Logger { get; set; }
        public string Name { get; set; } = "ListScheduler";

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
            return Poll(Timeout.InfiniteTimeSpan);
        }

        public async Task<Request> PollAsync()
        {
            return await Task.Run(Poll);
        }

        public Request Poll(TimeSpan timeout)
        {
            using (GetAutoLeaveHelper())
            {
                DateTime begin = DateTime.Now;
                while (IsDisposed == false &&
                    (timeout == Timeout.InfiniteTimeSpan || DateTime.Now - begin < timeout))
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

                    Thread.Sleep(1);
                }

                return null;
            }
        }

        public void Push(Request request)
        {
            using (GetAutoLeaveHelper())
            {
                lock (_requestLocker)
                {
                    _requestList.AddLast(request);
                }
            }
        }

        public void Push(IEnumerable<Request> requests)
        {
            using (GetAutoLeaveHelper())
            {
                lock (_requestLocker)
                {
                    foreach (var i in requests)
                    {
                        _requestList.AddLast(i);
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
            }
        }
    }
}
