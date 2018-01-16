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
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;

    static partial class LineReader
    {
        public static IEnumerable<string> SplitIntoLines(this string input) =>
            ReadLines(() => new StringReader(input));

        public static IEnumerable<string> ReadLinesFromFile(string path) =>
            ReadLines(Source.TextFile(path));

        public static IEnumerable<string> ReadLinesFromFile(string path, Encoding encoding) =>
            ReadLines(Source.TextFile(path, encoding));

        public static IEnumerable<string> ReadLinesFromStream(Func<Stream> streamFactory) =>
            ReadLinesFromStream(streamFactory, null);

        public static IEnumerable<string> ReadLinesFromStream(Func<Stream> streamFactory, Encoding encoding)
        {
            if (streamFactory == null) throw new ArgumentNullException(nameof(streamFactory));
            return _(); IEnumerable<string> _()
            {
                using (var line = streamFactory().OpenTextReader(encoding).ReadLines())
                while (line.MoveNext())
                    yield return line.Current;
            }
        }

        static StreamReader OpenTextReader(this Stream stream, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            try
            {
                return encoding == null
                     ? new StreamReader(stream)
                     : new StreamReader(stream, encoding);
            }
            catch
            {
                stream.Close();
                throw;
            }
        }

        public static IEnumerable<string> ReadLines(Func<TextReader> readerFactory)
        {
            if (readerFactory == null) throw new ArgumentNullException(nameof(readerFactory));
            return _(); IEnumerable<string> _()
            {
                using (var line = readerFactory().ReadLines())
                {
                    while (line.MoveNext())
                        yield return line.Current;
                }
            }
        }

        public static IEnumerator<string> ReadLines(this TextReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            string line;
            using (reader)
            while ((line = reader.ReadLine()) != null)
                yield return line;
        }

        public static IEnumerable<string> ReadLines(Func<HttpWebRequest> requestFactory) =>
            ReadLines(requestFactory, null);

        public static IEnumerable<string> ReadLines(Func<HttpWebRequest> requestFactory,
                                                    Encoding overridingEncoding)
        {
            if (requestFactory == null) throw new ArgumentNullException(nameof(requestFactory));
            return ReadLines(() => (HttpWebResponse) requestFactory().GetResponse(), overridingEncoding);
        }

        public static IEnumerable<string> ReadLines(Func<HttpWebResponse> responseFactory) =>
            ReadLines(responseFactory, null);

        public static IEnumerable<string> ReadLines(Func<HttpWebResponse> responseFactory,
                                                    Encoding overridingEncoding)
        {
            if (responseFactory == null) throw new ArgumentNullException(nameof(responseFactory));

            return _(); IEnumerable<string> _()
            {
                using (var response = responseFactory())
                {
                    var encoding =
                        overridingEncoding ??
                        (response.CharacterSet is string charSet
                         && charSet.Length > 0
                         ? Encoding.GetEncoding(charSet)
                         : null);

                    using (var line = response.GetResponseStream()
                                              .OpenTextReader(encoding)
                                              .ReadLines())
                    {
                        while (line.MoveNext())
                            yield return line.Current;
                    }
                }
            }
        }

        public static IEnumerable<string> ReadLines(this HttpClient client, Uri url) =>
            ReadLines(client, () => new HttpRequestMessage(HttpMethod.Get, url));

        public static IEnumerable<string> ReadLines(this HttpClient client,
                                                    Func<HttpRequestMessage> requestFactory) =>
            ReadLines(client, requestFactory, null);

        public static IEnumerable<string> ReadLines(this HttpClient client,
                                                    Func<HttpRequestMessage> requestFactory,
                                                    Encoding overridingEncoding)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (requestFactory == null) throw new ArgumentNullException(nameof(requestFactory));

            return ReadLines(() =>
            {
                using (var request = requestFactory())
                    return client.SendAsync(request).GetAwaiter().GetResult();
            });
        }

        public static IEnumerable<string> ReadLines(Func<HttpResponseMessage> responseFactory) =>
            ReadLines(responseFactory, null);

        public static IEnumerable<string> ReadLines(Func<HttpResponseMessage> responseFactory,
                                                    Encoding overridingEncoding)
        {
            if (responseFactory == null) throw new ArgumentNullException(nameof(responseFactory));

            return _(); IEnumerable<string> _()
            {
                using (var response = responseFactory())
                {
                    response.EnsureSuccessStatusCode();
                    if (response.Content != null)
                    {
                        var encoding =
                            overridingEncoding
                            ?? (response.Content.Headers.ContentType?.CharSet is string charSet
                                && charSet.Length > 0
                                ? Encoding.GetEncoding(charSet)
                                : null);

                        using (var line = response.Content
                                                  .ReadAsStreamAsync()
                                                  .GetAwaiter().GetResult()
                                                  .OpenTextReader(encoding)
                                                  .ReadLines())
                        {
                            while (line.MoveNext())
                                yield return line.Current;
                        }
                    }
                }
            }
        }
    }
}
