using log4net.Config;
using log4net;
using log4net.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using log4net.Core;
using System.Reflection;

namespace DotnetSpider.Logger
{
    /// <summary>
    /// 日志模块生成器，配置文件位于 Logger/log4ds3r.config 。
    /// </summary>
    public static class LoggerCreator
    {
        /// <summary>
        /// 获取日志模块。
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>日志模块实例，获取失败时返回null。</returns>
        public static ILog GetLogger(string name)
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            if (LogManager.GetCurrentLoggers(assembly).Length == 0)
            {
                ILoggerRepository repository = LogManager.GetRepository(assembly);
                FileInfo fi = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "/Logger/log4ds3r.config");
                XmlConfigurator.Configure(repository, fi);
            }

            return LogManager.GetLogger(assembly, name);
        }

        /// <summary>
        /// 获取日志模块。
        /// </summary>
        /// <param name="type">需要使用日志的模块</param>
        /// <returns>日志模块实例，获取失败时返回null。</returns>
        public static ILog GetLogger(Type type)
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            if (LogManager.GetCurrentLoggers(assembly).Length == 0)
            {
                ILoggerRepository repository = LogManager.GetRepository(assembly);
                FileInfo fi = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "/Logger/log4ds3r.config");
                XmlConfigurator.Configure(repository, fi);
            }

            return LogManager.GetLogger(assembly, type.Name);
        }
    }
}
