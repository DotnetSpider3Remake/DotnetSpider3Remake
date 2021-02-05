using System.Threading.Tasks;

namespace DotnetSpider.Runner
{
    public interface IRunnable : IControllable
    {
        /// <summary>
        /// 运行程序
        /// </summary>
        void Run();

        /// <summary>
        /// 异步运行程序
        /// </summary>
        /// <returns></returns>
        Task RunAsync();

        /// <summary>
        /// 是否正在运行。
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 是否已经退出。
        /// </summary>
        bool HasExit { get; }

        /// <summary>
        /// 是否已经启动过。
        /// 即使已经退出，仍然返回true。
        /// </summary>
        bool HasStarted { get; }
    }
}
