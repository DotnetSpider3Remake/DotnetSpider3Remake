using log4net.Config;
using log4net.Core;
using log4net.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DotnetSpider.Logger
{
    /// <summary>
    /// 日志模块生成器，配置文件位于 Logger/log4ds3r.config 。
    /// </summary>
    public static class LoggerCreator
    {
        private static readonly ILoggerRepository _loggerRepository;

        static LoggerCreator()
        {
            try
            {
                FileInfo fi = new FileInfo(Path.GetFileName(AppDomain.CurrentDomain.BaseDirectory + "/Logger/log4ds3r.config"));
                _loggerRepository = LoggerManager.CreateRepository("ds3r");
                XmlConfigurator.Configure(_loggerRepository, fi);
            }
            catch (Exception e)
            {
                _loggerRepository = null;
                Console.WriteLine($"初始化日志模块失败，异常为：{ e.Message }");
            }
        }

        /// <summary>
        /// 获取日志模块。
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>日志模块实例，获取失败时返回null。</returns>
        public static ILogger GetLogger(string name)
        {
            return _loggerRepository?.GetLogger(name);
        }
    }
}
