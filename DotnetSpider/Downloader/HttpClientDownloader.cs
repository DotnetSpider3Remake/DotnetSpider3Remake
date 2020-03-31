using LZ4;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Downloader
{
    public class HttpClientDownloader : BaseDowloader
    {
        /// <summary>
        /// What mediatype should not be treated as file to download.
        /// </summary>
        /// <summary xml:lang="zh-CN">
        /// 定义哪些类型的内容不需要当成文件下载
        /// </summary>
        public readonly List<string> ExcludeMediaTypes = new List<string>
        {
            "",
            "text/html",
            "text/plain",
            "text/richtext",
            "text/xml",
            "text/XML",
            "text/json",
            "text/javascript",
            "application/soap+xml",
            "application/xml",
            "application/json",
            "application/x-javascript",
            "application/javascript",
            "application/x-www-form-urlencoded"
        };

        protected override Task<Response> DownloadSync(Request request, WebProxy proxy = null)
        {
            throw new NotImplementedException();
        }

        private HttpRequestMessage GenerateHttpRequestMessage(Request request)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(request.Method, request.Url);

            // Headers 的优先级低于 Request.UserAgent 这种特定设置, 因此先加载所有 Headers, 再使用 Request.UserAgent 覆盖
            foreach (var header in request.Headers)
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value?.ToString());
            }

            if (!string.IsNullOrWhiteSpace(request.UserAgent))
            {
                var header = "User-Agent";
                httpRequestMessage.Headers.Remove(header);
                httpRequestMessage.Headers.TryAddWithoutValidation(header, request.UserAgent);
            }

            if (!string.IsNullOrWhiteSpace(request.Referer))
            {
                var header = "Referer";
                httpRequestMessage.Headers.Remove(header);
                httpRequestMessage.Headers.TryAddWithoutValidation(header, request.Referer);
            }

            if (!string.IsNullOrWhiteSpace(request.Origin))
            {
                var header = "Origin";
                httpRequestMessage.Headers.Remove(header);
                httpRequestMessage.Headers.TryAddWithoutValidation(header, request.Origin);
            }

            if (!string.IsNullOrWhiteSpace(request.Accept))
            {
                var header = "Accept";
                httpRequestMessage.Headers.Remove(header);
                httpRequestMessage.Headers.TryAddWithoutValidation(header, request.Accept);
            }

            //添加CONTENT，可以为空字符串。
            //理论上所有类型的请求都可以有CONTENT，但大部分人只用在POST/PUT时使用。
            if (request.Content != null)
            {
                var bytes = CompressContent(request);
                httpRequestMessage.Content = new ByteArrayContent(bytes);

                if (!string.IsNullOrWhiteSpace(request.ContentType))
                {
                    var header = "Content-Type";
                    httpRequestMessage.Content.Headers.Remove(header);
                    httpRequestMessage.Content.Headers.TryAddWithoutValidation(header, request.ContentType);
                }

                var xRequestedWithHeader = "X-Requested-With";
                if (request.Headers.ContainsKey(xRequestedWithHeader) &&
                    request.Headers[xRequestedWithHeader].ToString() == "NULL")
                {
                    httpRequestMessage.Content.Headers.Remove(xRequestedWithHeader);
                }
                else
                {
                    if (!httpRequestMessage.Content.Headers.Contains(xRequestedWithHeader) &&
                        !httpRequestMessage.Headers.Contains(xRequestedWithHeader))
                    {
                        httpRequestMessage.Content.Headers.TryAddWithoutValidation(xRequestedWithHeader, "XMLHttpRequest");
                    }
                }
            }

            return httpRequestMessage;
        }

        private byte[] CompressContent(Request request)
        {
            var encoding = string.IsNullOrEmpty(request.EncodingName) ? Encoding.UTF8 : Encoding.GetEncoding(request.EncodingName);
            var bytes = encoding.GetBytes(request.Content);

            switch (request.CompressMode)
            {
                case CompressMode.Lz4:
                    bytes = LZ4Codec.Wrap(bytes);
                    break;

                case CompressMode.None:
                    break;

                case CompressMode.Gzip:
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Compress))
                        {
                            compressedzipStream.Write(bytes, 0, bytes.Length);
                            compressedzipStream.Close();
                            bytes = ms.ToArray();
                        }
                    }

                    break;

                default:
                    throw new NotImplementedException(request.CompressMode.ToString());
            }

            return bytes;
        }
    }
}
