#region Copyright 2017 Atif Aziz. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

namespace Dsv.Tests
{
    using System;
    using System.IO;
    using System.Text;

    static class Source
    {
        public static Func<FileStream> File(string path) =>
            () => new FileStream(path, FileMode.Open);

        public static Func<StreamReader> TextFile(string path) =>
            () => new StreamReader(path);

        public static Func<StreamReader> TextFile(string path, Encoding encoding) =>
            () => new StreamReader(path, encoding);

        public static Func<MemoryStream> Bytes(byte[] data) =>
            () => new MemoryStream(data, writable: false);

        public static Func<MemoryStream> Bytes(byte[] data, int offset, int length) =>
            () => new MemoryStream(data, offset, length, writable: false);

        public static Func<StringReader> String(string input) =>
            () => new StringReader(input);
    }
}

namespace Dsv.Tests
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Net.Cache;
    using System.Net.Mime;
    using System.Net.Security;
    using System.Text;

    static class Http
    {
        public static Func<HttpWebResponse> Get(Uri url) =>
            Get(url, null);

        public static Func<HttpWebResponse> Get(Uri url, Action<HttpWebRequest> requestModifier)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            return () =>
            {
                var request = CreateHttpRequest(url, requestModifier);
                request.Method = "GET";
                return (HttpWebResponse) request.GetResponse();
            };
        }

        public static Func<HttpWebResponse> Post(Uri url, string mediaType,
            Func<Stream> streamFactory, Action<HttpWebRequest> requestModifier)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (streamFactory == null) throw new ArgumentNullException(nameof(streamFactory));

            return Post(url, requestModifier, new ContentType { MediaType = mediaType }, output =>
            {
                using (var input = streamFactory())
                    input.CopyTo(output);
            });
        }

        public static Func<HttpWebResponse> Post(Uri url, string data, string mediaType, Encoding encoding) =>
            Post(url, data, mediaType, encoding, null);

        public static Func<HttpWebResponse> Post(Uri url, string data, string mediaType, Encoding encoding, Action<HttpWebRequest> requestModifier)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            encoding = encoding ?? Encoding.GetEncoding("ISO-8859-1");

            var contentType = new ContentType
            {
                MediaType = mediaType,
                CharSet   = encoding.WebName
            };

            return Post(url, requestModifier, contentType, sr =>
            {
                var bytes = encoding.GetBytes(data);
                sr.Write(bytes, 0, bytes.Length);
            });
        }

        static Func<HttpWebResponse> Post(Uri url, Action<HttpWebRequest> requestModifier, ContentType contentType, Action<Stream> poster)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (contentType == null) throw new ArgumentNullException(nameof(contentType));
            if (poster == null) throw new ArgumentNullException(nameof(poster));

            return () =>
            {
                var request = CreateHttpRequest(url, requestModifier);
                request.Method = "POST";
                request.ContentType = contentType.ToString();

                using (var stream = request.GetRequestStream())
                    poster(stream);

                return (HttpWebResponse) request.GetResponse();
            };
        }

        public static Func<HttpWebResponse> Post(Uri url, NameValueCollection form) =>
            Post(url, form, null);

        public static Func<HttpWebResponse> Post(Uri url, NameValueCollection form, Action<HttpWebRequest> requestModifier)
        {
            return Post(url, W3FormEncode(form), "application/x-www-form-urlencoded", Encoding.ASCII, requestModifier);

            string W3FormEncode(NameValueCollection collection)
            {
                if (collection == null) throw new ArgumentNullException(nameof(collection));

                if (collection.Count == 0)
                    return string.Empty;

                var sb = new StringBuilder();

                var names = collection.AllKeys;
                for (var i = 0; i < names.Length; i++)
                {
                    var name = names[i];
                    var values = collection.GetValues(i);

                    if (values == null)
                        continue;

                    foreach (var value in values)
                    {
                        if (sb.Length > 0)
                            sb.Append('&');

                        if (!string.IsNullOrEmpty(name))
                            sb.Append(name).Append('=');

                        sb.Append(string.IsNullOrEmpty(value)
                                  ? string.Empty
                                  : Uri.EscapeDataString(value));
                    }
                }

                return sb.ToString();
            }
        }

        static HttpWebRequest CreateHttpRequest(Uri url, Action<HttpWebRequest> requestModifier)
        {
            var request = WebRequest.CreateHttp(url);
            requestModifier?.Invoke(request);
            return request;
        }

        public static Action<HttpWebRequest> WebRequestSetup(
                string accept                  = null,
                string userAgent               = null,
                NameValueCollection headers    = null,
                bool useDefaultCredentials     = false,
                ICredentials credentials       = null,
                bool preAuthenticate           = false,
                DateTime? ifModifiedSince      = null,
                TimeSpan? timeout              = null,
                TimeSpan? readWriteTimeout     = null,
                IWebProxy proxy                = null,
                DecompressionMethods automaticDecompression = DecompressionMethods.None,
                string referer                 = null,
                RemoteCertificateValidationCallback serverCertificateValidationCallback = null,
                bool disallowAutoRedirect      = false,
                RequestCachePolicy cachePolicy = null) =>
            request =>
            {
                if ((request.Credentials = credentials) == null)
                {
                    request.UseDefaultCredentials = useDefaultCredentials;
                }
                else if (useDefaultCredentials)
                {
                    throw new ArgumentException("Credentials supplied when use of default credentials was also requested. " +
                                                "Specify one or the other.",
                                                nameof(credentials));
                }

                request.Accept = accept;
                request.UserAgent = userAgent;
                if ((headers?.Count ?? 0) > 0) request.Headers.Add(headers);
                request.PreAuthenticate = preAuthenticate;
                if (ifModifiedSince != null) request.IfModifiedSince = ifModifiedSince.Value;
                if (timeout != null) request.Timeout = (int) timeout.Value.TotalMilliseconds;
                if (readWriteTimeout != null) request.ReadWriteTimeout = (int) readWriteTimeout.Value.TotalMilliseconds;
                request.Proxy = proxy;
                request.AutomaticDecompression = automaticDecompression;
                request.Referer = referer;
                request.ServerCertificateValidationCallback = serverCertificateValidationCallback;
                request.AllowAutoRedirect = !disallowAutoRedirect;
                if (cachePolicy != null) request.CachePolicy = cachePolicy;
            };
    }
}
