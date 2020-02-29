using log4net;

namespace DotnetSpider.Monitor
{
    /// <summary>
    /// 需要记录日志的类，应实现此接口。
    /// </summary>
    public interface IRecordable
    {
        /// <summary>
        /// 日志记录器。
        /// 调用时应确保线程安全，简单来说，在任务启动前赋值或直接不赋值。
        /// 应使用Logger?.Info的形式调用，避免调用null。
        /// ISpider的实现需要初始化此属性，并初始化它成员中的其他IRecordable.Logger。
        /// </summary>
        ILog Logger { get; set; }
    }
}