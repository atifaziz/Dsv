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
    using System.IO;
    using System.Net;
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

        public static Func<HttpWebResponse>
            HttpGet(Uri url,
                    string accept = null,
                    string userAgent = null,
                    bool useDefaultCredentials = false,
                    ICredentials credentials = null,
                    TimeSpan? timeout = null,
                    IWebProxy proxy = null,
                    DecompressionMethods automaticDecompression = DecompressionMethods.None)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            return () =>
            {
                var request = WebRequest.CreateHttp(url);
                request.UseDefaultCredentials = useDefaultCredentials;
                if (timeout != null)
                    request.Timeout = (int) timeout.Value.TotalMilliseconds;
                request.Accept = accept;
                request.UserAgent = userAgent;
                request.Proxy = proxy;
                request.AutomaticDecompression = automaticDecompression;
                request.Credentials = credentials;
                return (HttpWebResponse) request.GetResponse();
            };
        }
    }
}
