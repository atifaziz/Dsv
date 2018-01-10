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

namespace Dsv
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Net.Cache;
    using System.Net.Security;
    using System.Text;

    static partial class Opener
    {
        public static Func<FileStream> File(string path) =>
            () => new FileStream(path, FileMode.Open);

        public static Func<StreamReader> TextFile(string path) =>
            () => new StreamReader(path);

        public static Func<StreamReader> TextFile(string path, Encoding encoding) =>
            () => new StreamReader(path, encoding);

        public static Func<StringReader> String(string input) =>
            () => new StringReader(input);

        public static Func<HttpWebRequest> Http(Uri url, Action<HttpWebRequest> modifier)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (modifier == null) throw new ArgumentNullException(nameof(modifier));
            return () =>
            {
                var request = WebRequest.CreateHttp(url);
                modifier(request);
                return request;
            };
        }

        public static Func<HttpWebRequest>
            Http(Uri url,
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
            Http(url, request =>
            {
                request.Accept = accept;
                request.UserAgent = userAgent;
                if ((headers?.Count ?? 0) > 0) request.Headers.Add(headers);
                request.UseDefaultCredentials = useDefaultCredentials;
                request.Credentials = credentials;
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
            });
    }
}
