using DotnetSpider.Runner.Helper;
using System;
using System.Threading;

namespace DotnetSpider.Runner
{
    /// <summary>
    /// 作为基类，可以等待所有任务或操作都完成。
    /// </summary>
    public abstract class BaseZeroDisposable : IDisposable
    {
        private readonly ManualResetEvent _waitZeroHandle = new ManualResetEvent(true);
        private uint _runnerCount = 0;
        private readonly object _runnerCountLocker = new object();

        #region 属性
        private bool _isDisposed = false;
        private readonly object _isDisposedLocker = new object();
        /// <summary>
        /// 是否已处于释放资源、停止的状态。
        /// </summary>
        protected bool IsDisposed
        {
            get 
            {
                lock (_isDisposedLocker)
                {
                    return _isDisposed;
                }
            }
        }

        private TimeSpan _maxDisposeWait = TimeSpan.FromMinutes(1);
        private readonly object _maxDisposeWaitLocker = new object();
        /// <summary>
        /// 等待Dispose完成的最长时间，默认为1分钟。
        /// </summary>
        protected TimeSpan MaxDisposeWait
        {
            get
            {
                lock (_maxDisposeWaitLocker)
                {
                    return _maxDisposeWait;
                }
            }
            set 
            {
                lock (_maxDisposeWaitLocker)
                {
                    _maxDisposeWait = value;
                }
            }
        }
        #endregion

        public void Dispose()
        {
            lock (_isDisposedLocker)
            {
                if (_isDisposed)
                {
                    return;
                }

                _isDisposed = true;
            }

            _waitZeroHandle.WaitOne(MaxDisposeWait);
            DisposeOthers();
        }

        /// <summary>
        /// 释放其他资源，在Dispose后调用。不会重复调用。
        /// </summary>
        protected virtual void DisposeOthers()
        {
            
        }

        #region 派生类执行操作或任务前后需要执行的方法
        /// <summary>
        /// 启动一个任务或执行一项操作。
        /// </summary>
        protected void Enter()
        {
            lock (_runnerCountLocker)
            {
                ++_runnerCount;
                _waitZeroHandle.Reset();
            }
        }

        /// <summary>
        /// 完成一项任务或操作。
        /// </summary>
        protected void Leave()
        {
            lock (_runnerCountLocker)
            {
                if (_runnerCount > 0)
                {
                    --_runnerCount;
                }

                if (_runnerCount == 0)
                {
                    _waitZeroHandle.Set();
                }
            }
        }

        /// <summary>
        /// 等待所有任务完成或归零。
        /// </summary>
        protected void WaitFinished()
        {
            _waitZeroHandle.WaitOne();
        }

        /// <summary>
        /// 获取自动设置Enter/Leave状态的辅助对象。
        /// </summary>
        /// <returns>辅助对象实例</returns>
        protected BaseZeroDisposableHelper GetAutoLeaveHelper()
        {
            Enter();
            return new BaseZeroDisposableHelper
            {
                Leave = Leave
            };
        }
        #endregion
    }
}
