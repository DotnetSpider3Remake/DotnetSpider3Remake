using System;
using System.Threading.Tasks;

namespace DotnetSpider.Runner
{
    /// <summary>
    /// 任务控制接口
    /// 实现任务的暂停、继续、退出
    /// </summary>
    public interface IControllable
    {
        /// <summary>
        /// 暂停任务。
        /// 暂停一个不在运行的任务应该提示警告。
        /// </summary>
        /// <returns>是否暂停了一个正在运行的任务。</returns>
        Task<bool> PauseAsync();

        /// <summary>
        /// 继续任务。
        /// 继续一个不在暂停的任务应该提示警告。
        /// </summary>
        /// <returns>是否继续了一个处于暂停的任务。</returns>
        Task<bool> ContinueAsync();

        /// <summary>
        /// 退出任务。
        /// </summary>
        /// <returns>是否退出一个不处于退出状态的任务。</returns>
        Task<bool> ExitAsync();
        /// <summary>
        /// 暂停任务。
        /// 暂停一个不在运行的任务应该提示警告。
        /// </summary>
        /// <returns>是否暂停了一个正在运行的任务。</returns>
        bool Pause();

        /// <summary>
        /// 继续任务。
        /// 继续一个不在暂停的任务应该提示警告。
        /// </summary>
        /// <returns>是否继续了一个处于暂停的任务。</returns>
        bool Continue();

        /// <summary>
        /// 退出任务。
        /// </summary>
        /// <returns>是否退出一个不处于退出状态的任务。</returns>
        bool Exit();
    }
}
