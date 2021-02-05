﻿using DotnetSpider.Downloader;
using DotnetSpider.Runner;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Processor
{
    public abstract class BasePageProcessor : BaseZeroDisposable, IResponseProcessor
    {
        public ILog Logger { get; set; }
        public string Name { get; set; }

        public async Task<ProcessorResult> Process(Response page)
        {
            using (var helper = GetAutoLeaveHelper())
            {
                return await Task.Run(() =>
                {
                    return ProcessSync(page);
                });
            }
        }

        /// <summary>
        /// 同步执行页面解析。
        /// </summary>
        /// <param name="page">页面数据</param>
        /// <returns>解析的结果</returns>
        protected abstract ProcessorResult ProcessSync(Response page);
    }
}