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
    }
}
