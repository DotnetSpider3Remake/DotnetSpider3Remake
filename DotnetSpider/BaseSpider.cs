using DotnetSpider.Downloader;
using DotnetSpider.Logger;
using DotnetSpider.Monitor;
using DotnetSpider.Pipeline;
using DotnetSpider.Processor;
using DotnetSpider.Proxy;
using DotnetSpider.Runner;
using DotnetSpider.Scheduler;
using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpider
{
    public abstract class BaseSpider : BaseZeroDisposable, ISpider
    {
        #region 继承自ISpider的属性
        public IScheduler Scheduler { get; set; } = null;
        public IDownloader Downloader { get; set; } = null;
        public List<IPipeline> Pipelines { get; } = new List<IPipeline>();
        public List<IResponseProcessor> PageProcessors { get; } = new List<IResponseProcessor>();
        public IHttpProxy HttpProxy { get; set; } = null;
        public uint MaxRetry { get; set; } = 2;
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(1);
        public ILog Logger { get; set; } = null;
        public string Name { get; set; } = "Spider";
        public int Parallels { get; set; } = 1;
        public TimeSpan? RequestInterval { get; set; } = null;
        public TimeSpan? FixedRequestDuration { get; set; } = null;
        public Func<ISpider, bool> ConditionOfStop { get; set; } = null;

        private bool _isRunning = false;
        private bool _hasExit = false;
        private readonly object _stateLocker = new object();
        /// <summary>
        /// 是否正在运行。
        /// </summary>
        public bool IsRunning
        {
            get
            {
                lock (_stateLocker)
                {
                    return !_hasExit && _isRunning;
                }
            }
        }

        /// <summary>
        /// 是否已经退出。
        /// </summary>
        public bool HasExit
        {
            get
            {
                lock (_stateLocker)
                {
                    return _hasExit;
                }
            }
        }

        private bool _hasStarted = false;
        private readonly object _hasStartedLocker = new object();
        /// <summary>
        /// 是否已经启动过。
        /// 即使已经退出，仍然返回true。
        /// </summary>
        public bool HasStarted
        {
            get
            {
                lock (_hasStartedLocker)
                {
                    return _hasStarted;
                }
            }
        }

        private long _countSuccess = 0;
        private long _countFailed = 0;
        private long _countStarted = 0;
        private readonly object _countLocker = new object();
        /// <summary>
        /// 成功的任务数量。
        /// </summary>
        public long Success
        {
            get
            {
                lock (_countLocker)
                {
                    return _countSuccess;
                }
            }
        }

        /// <summary>
        /// 失败的任务数量。
        /// </summary>
        public long Failed
        {
            get
            {
                lock (_countLocker)
                {
                    return _countFailed;
                }
            }
        }

        /// <summary>
        /// 结束的任务数量。
        /// </summary>
        public long Finished
        {
            get
            {
                lock (_countLocker)
                {
                    return _countFailed + _countSuccess;
                }
            }
        }

        /// <summary>
        /// 已经启动的任务数量。
        /// </summary>
        public long Started
        {
            get
            {
                lock (_countLocker)
                {
                    return _countStarted;
                }
            }
        }

        /// <summary>
        /// 执行中的任务数量。
        /// </summary>
        public long Unfinished
        {
            get
            {
                lock (_countLocker)
                {
                    return _countStarted - _countFailed - _countSuccess;
                }
            }
        }
        #endregion

        #region 实现接口
        public bool Continue()
        {
            lock (_stateLocker)
            {
                if (_hasExit || _isRunning)
                {
                    return false;
                }
                else
                {
                    _isRunning = true;
                    return true;
                }
            }
        }

        public async Task<bool> ContinueAsync()
        {
            return await Task.Run(Continue);
        }

        public bool Exit()
        {
            lock (_stateLocker)
            {
                if (_hasExit)
                {
                    return false;
                }
                else
                {
                    _hasExit = true;
                }
            }

            Scheduler?.Clear();
            WaitFinished();
            return true;
        }

        public async Task<bool> ExitAsync()
        {
            return await Task.Run(Exit);
        }

        public bool Pause()
        {
            bool success;
            lock (_stateLocker)
            {
                if (!_hasExit && _isRunning)
                {
                    _isRunning = false;
                    success = true;
                }
                else
                {
                    success = false;
                }
            }

            return success;
        }

        public async Task<bool> PauseAsync()
        {
            return await Task.Run(Pause);
        }

        public void Run()
        {
            lock (_hasStartedLocker)
            {
                if (_hasStarted)
                {
                    return;
                }
                else
                {
                    _hasStarted = true;
                }
            }

            lock (_stateLocker)
            {
                _isRunning = true;
            }

            InitSpider();
            InitLogger();
            if (CheckConfiguration())
            {
                Logger?.Info("Spider start.");
                if (ConditionOfStop != null)
                {
                    StartThread(JudgeConditionThread);
                }

                Parallel.For(0, Parallels, _ => RunSpiderThread());
            }

            Exit();
            Logger?.Info("Spider exit.");
        }

        public async Task RunAsync()
        {
            await Task.Run(Run);
        }
        #endregion

        #region 可以在派生类中重新实现的函数
        protected override void DisposeOthers()
        {
            Exit();

            Scheduler?.Dispose();
            Scheduler = null;
            Downloader?.Dispose();
            Downloader = null;
            foreach (var i in Pipelines)
            {
                i.Dispose();
            }

            Pipelines.Clear();

            foreach (var i in PageProcessors)
            {
                i.Dispose();
            }

            PageProcessors.Clear();

            HttpProxy?.Dispose();
            HttpProxy = null;
            Logger = null;
        }

        /// <summary>
        /// 初始化爬虫。
        /// </summary>
        protected virtual void InitSpider()
        {

        }

        /// <summary>
        /// 初始化所有部件的日志模块，如果Logger不为null，将跳过初始化。
        /// </summary>
        protected virtual void InitLogger()
        {
            if (Logger != null)
            {
                return;
            }

            SetLogger(this);
            SetLogger(Scheduler);
            SetLogger(Downloader);
            SetLogger(HttpProxy);
            foreach (var i in Pipelines)
            {
                SetLogger(i);
            }

            foreach (var i in PageProcessors)
            {
                SetLogger(i);
            }
        }

        /// <summary>
        /// 设置子部件的日志模块，在<seealso cref="InitLogger"/>中被调用。
        /// </summary>
        /// <param name="rd">子部件，可能为null。</param>
        protected virtual void SetLogger(IRecordable rd)
        {
            if (rd == null)
            {
                return;
            }

            if (rd.Name == null)
            {
                rd.Logger = LoggerCreator.GetLogger(rd.GetType());
            }
            else
            {
                rd.Logger = LoggerCreator.GetLogger(rd.Name);
            }
        }

        /// <summary>
        /// 检测配置是否正确。
        /// </summary>
        /// <returns>配置是否正确</returns>
        protected virtual bool CheckConfiguration()
        {
            bool success = true;

            if (Scheduler == null)
            {
                Logger?.Fatal("Scheduler is null.");
                success = false;
            }

            if (Downloader == null)
            {
                Logger?.Fatal("Downloader is null.");
                success = false;
            }

            if (Pipelines.Count == 0)
            {
                Logger?.Fatal("Pipelines are empty.");
                success = false;
            }

            if (PageProcessors.Count == 0)
            {
                Logger?.Fatal("PageProcessors are empty.");
                success = false;
            }

            if (Parallels < 1)
            {
                Logger?.Fatal("Parallels is less than 1.");
                success = false;
            }

            if (RequestInterval != null && FixedRequestDuration != null)
            {
                Logger?.Fatal("RequestInterval and FixedRequestDuration can not exist both.");
                success = false;
            }

            return success;
        }

        #endregion

        #region 抽象函数
        /// <summary>
        /// 运行爬虫线程，不应该抛出异常。
        /// </summary>
        protected abstract void RunSpiderThread();
        #endregion

        #region 保护方法
        /// <summary>
        /// 增加成功的任务数量。
        /// </summary>
        protected void AddSuccess()
        {
            lock (_countLocker)
            {
                ++_countSuccess;
            }
        }

        /// <summary>
        /// 增加启动的任务数量。
        /// </summary>
        /// <returns>最后增加的任务的索引。</returns>
        protected long AddStarting()
        {
            lock (_countLocker)
            {
                return ++_countStarted;
            }
        }

        /// <summary>
        /// 增加失败的任务数量。
        /// </summary>
        protected void AddFailed()
        {
            lock (_countLocker)
            {
                ++_countFailed;
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 判断是否应当停止运行的线程。
        /// </summary>
        private void JudgeConditionThread()
        {
            while (IsRunning)
            {
                try
                {
                    using (GetAutoLeaveHelper())
                    {
                        if (ConditionOfStop(this))
                        {
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger?.Error($"Exception ocurred when judge stop condition.Exception:\n{ e }");
                }
                finally
                {
                    Thread.Sleep(100);
                }
            }

            if (IsRunning)
            {
                Exit();
            }
        }

        /// <summary>
        /// 启动新线程。
        /// </summary>
        /// <param name="fun">线程函数</param>
        private void StartThread(ThreadStart fun)
        {
            Thread thread = new Thread(fun);
            thread.IsBackground = true;
            thread.Start();
        }

        #endregion
    }
}
