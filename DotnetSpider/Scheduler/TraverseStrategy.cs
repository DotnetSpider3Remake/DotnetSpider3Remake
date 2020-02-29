namespace DotnetSpider.Scheduler
{
    /// <summary>
    /// 遍历策略
    /// </summary>
    public enum TraverseStrategy
    {
        /// <summary>
        /// 不关心如何遍历，即随机、深度优先、广度优等均有可能，由Scheduler自行实现。
        /// </summary>
        DoNotCare,
        /// <summary>
        /// 深度优先
        /// </summary>
        Dfs,
        /// <summary>
        /// 广度优先
        /// </summary>
        Bfs,
    }
}
