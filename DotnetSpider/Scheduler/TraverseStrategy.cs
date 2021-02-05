namespace DotnetSpider.Scheduler
{
    /// <summary>
    /// 遍历策略
    /// </summary>
    public enum TraverseStrategy
    {
        /// <summary>
        /// 不关心如何遍历，即随机遍历、顺序等均有可能，由Scheduler自行实现。
        /// </summary>
        DoNotCare,
        /// <summary>
        /// 深度优先
        /// </summary>
        DFS,
        /// <summary>
        /// 先进后出
        /// </summary>
        FILO = DFS,
        /// <summary>
        /// 广度优先
        /// </summary>
        BFS,
        /// <summary>
        /// 先进先出
        /// </summary>
        FIFO = BFS,
    }
}
