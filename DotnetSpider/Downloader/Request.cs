﻿using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using Newtonsoft.Json;
using DotnetSpider.Common;
using System.Text;
using System.Net;
using System;
using System.IO;
using System.IO.Compression;
using K4os.Compression.LZ4;

namespace DotnetSpider.Downloader
{
    /// <summary>
    /// 链接请求
    /// </summary>
    public class Request
    {
        #region Headers

        /// <summary>
        /// User-Agent
        /// </summary>
        [JsonIgnore]
        public string UserAgent
        {
            get => Headers.GetValueOrDefault("User-Agent");
            set => Headers["User-Agent"] = value;
        }

        /// <summary>
        /// 请求链接时Referer参数的值
        /// </summary>
        [JsonIgnore]
        public string Referer
        {
            get => Headers.GetValueOrDefault("Referer");
            set => Headers["Referer"] = value;
        }

        /// <summary>
        /// 请求链接时Origin参数的值
        /// </summary>
        [JsonIgnore]
        public string Origin
        {
            get => Headers.GetValueOrDefault("Origin");
            set => Headers["Origin"] = value;
        }

        /// <summary>
        /// Accept
        /// </summary>
        [JsonIgnore]
        public string Accept
        {
            get => Headers.GetValueOrDefault("Accept");
            set => Headers["Accept"] = value;
        }

        /// <summary>
        /// 仅在发送 POST/PUT 请求时需要设置。
        /// </summary>
        [JsonIgnore]
        public string ContentType
        {
            get => Headers.GetValueOrDefault("Content-Type");
            set => Headers["Content-Type"] = value;
        }

        /// <summary>
        /// X-Requested-With
        /// </summary>
        [JsonIgnore]
        public string XRequestedWith
        {
            get => Headers.GetValueOrDefault("X-Requested-With");
            set => Headers["X-Requested-With"] = value;
        }

        /// <summary>
        /// Http请求头。
        /// 不包括<see cref="Cookies"/>内容。
        /// </summary>
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Cookies
        /// </summary>
        public Dictionary<string, string> Cookies { get; } = new Dictionary<string, string>();

        #endregion

        private string _encodingName = null;
        private bool _encodingNameChanged = false;
        /// <summary>
        /// 字符编码，默认为UTF-8。
        /// 只影响<see cref="Content"/>转为<seealso cref="ContentData"/>时的编码格式。
        /// </summary>
        public string EncodingName 
        { 
            get => _encodingName;
            set
            {
                _encodingName = value;
                _encodingNameChanged = true;
            }
        }

        /// <summary>
        /// 请求链接的方法
        /// </summary>
        public HttpMethod Method { get; set; } = HttpMethod.Get;

        /// <summary>
        /// 存储此链接对应的额外数据字典
        /// </summary>
        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        private string _content = null;
        private bool _contentChanged = false;
        /// <summary>
        /// 请求此链接时需要POST/PUT的数据，文本格式。
        /// </summary>
        public string Content 
        { 
            get => _content;
            set
            {
                _content = value;
                _contentChanged = true;
            } 
        }

        private byte[] _contentData = null;
        /// <summary>
        /// 请求此链接时需要POST/PUT的数据，二进制格式。
        /// 当Content不为null时，会使用Content代替。(根据EncodingName转换编码)
        /// <seealso cref="Content"/>
        /// <seealso cref="EncodingName"/>
        /// </summary>
        public byte[] ContentData
        {
            get
            {
                if (_contentChanged || 
                    (_encodingNameChanged && _content != null))
                {
                    var encoding = string.IsNullOrEmpty(EncodingName) ? Encoding.UTF8 : Encoding.GetEncoding(EncodingName);
                    _contentData = (_content == null) ? null : encoding.GetBytes(_content);
                    _contentChanged = false;
                    _encodingNameChanged = false;
                }

                return _contentData;
            }
            set
            {
                _contentData = value;
            }
        }

        /// <summary>
        /// 如果是 POST/PUT 请求, 可以设置上传数据的压缩模式。
        /// 此设置不会修改请求头。
        /// 默认值为<see cref="CompressMode.None"/>。
        /// 可以调用<seealso cref="GetCompressedContentData"/>获取压缩后的数据。
        /// </summary>
        public CompressMode CompressMode { get; set; } = CompressMode.None;

        /// <summary>
        /// 请求链接, 不使用 Uri 的原因是可能引起多重编码的问题
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        public Request()
        {

        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="url">链接</param>
        public Request(string url)
        {
            Url = url;
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="url">链接</param>
        /// <param name="properties">额外属性</param>
        public Request(string url, Dictionary<string, object> properties)
        {
            Url = url;
            if (properties != null)
            {
                Properties = properties;
            }
        }

        /// <summary>
        /// 返回Uri实例。
        /// </summary>
        /// <returns>Uri实例。</returns>
        public Uri GetUri()
        {
            return new Uri(Url);
        }

        /// <summary>
        /// 获取压缩后的内容数据。
        /// 压缩方式由<see cref="CompressMode"/>指定。
        /// </summary>
        /// <returns>压缩后的内容数据</returns>
        /// <exception cref="NullReferenceException"/>
        public byte[] GetCompressedContentData()
        {
            var data = ContentData;
            if (data == null)
            {
                throw new NullReferenceException("Can not compress null content data!");
            }

            switch (CompressMode)
            {
                case CompressMode.Lz4:
                    byte[] temp = new byte[LZ4Codec.MaximumOutputSize(data.Length)];
                    int encodedLength = LZ4Codec.Encode(
                        data, 0, data.Length,
                        temp, 0, temp.Length);
                    Array.Resize(ref temp, encodedLength);
                    data = temp;
                    break;

                case CompressMode.None:
                    break;

                case CompressMode.Gzip:
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Compress);
                        compressedzipStream.Write(data, 0, data.Length);
                        compressedzipStream.Close();
                        data = ms.ToArray();
                    }

                    break;

                case CompressMode.Deflate:
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using DeflateStream compressedzipStream = new DeflateStream(ms, CompressionMode.Compress);
                        compressedzipStream.Write(data, 0, data.Length);
                        compressedzipStream.Close();
                        data = ms.ToArray();
                    }

                    break;
            }

            return data;
        }

        /// <summary>
        /// 比较obj是否与当前实例相等。
        /// 不比较Properties属性。
        /// </summary>
        /// <param name="obj">需要比较的对象</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            Request a = this;
            Request b = (Request)obj;

            if (Equals(a.Url, b.Url) == false ||
                Equals(a.Method, b.Method) == false ||
                Equals(a.CompressMode, b.CompressMode) == false ||
                Equals(a.Content, b.Content) == false ||
                Equals(a.ContentData, b.ContentData) == false ||
                Equals(a.EncodingName, b.EncodingName) == false ||
                Comparaor.AreEquivalent(a.Headers, b.Headers) == false ||
                Comparaor.AreEquivalent(a.Cookies, b.Cookies) == false ||
                Comparaor.AreEquivalent(a.Properties, b.Properties) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 计算Hash值。
        /// </summary>
        /// <returns>Hash值</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int c = HashCodeExtend.BeginCode;
                c = HashCodeExtend.GetHashCode(c, CompressMode);
                c = HashCodeExtend.GetHashCode(c, Content);
                c = HashCodeExtend.GetHashCode(c, ContentData);
                c = HashCodeExtend.GetHashCode(c, Cookies);
                c = HashCodeExtend.GetHashCode(c, EncodingName);
                c = HashCodeExtend.GetHashCode(c, Headers);
                c = HashCodeExtend.GetHashCode(c, Method);
                c = HashCodeExtend.GetHashCode(c, Properties);
                c = HashCodeExtend.GetHashCode(c, Url);
                return c;
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// 从JSON字符串构建Request。
        /// </summary>
        /// <param name="s">JSON字符串</param>
        /// <returns>Request实例</returns>
        public static Request FromString(string s)
        {
            return JsonConvert.DeserializeObject<Request>(s);
        }
    }
}