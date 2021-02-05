using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Runner.Helper
{
    public class BaseZeroDisposableHelper : IDisposable
    {
        public Action Leave { get; set; }

        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Leave?.Invoke();
        }
    }
}
