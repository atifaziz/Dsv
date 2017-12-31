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
            ReadLines(Opener.TextFile(path));

        public static IEnumerable<string> ReadLinesFromFile(string path, Encoding encoding) =>
            ReadLines(Opener.TextFile(path, encoding));

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

        public static IEnumerable<string> ReadLines(Func<HttpWebResponse> httpWebResponseFactory) =>
            ReadLines(httpWebResponseFactory, null);

        public static IEnumerable<string> ReadLines(Func<HttpWebResponse> httpWebResponseFactory,
                                                    Encoding encodingOverride)
        {
            if (httpWebResponseFactory == null) throw new ArgumentNullException(nameof(httpWebResponseFactory));

            return _(); IEnumerable<string> _()
            {
                using (var response = httpWebResponseFactory())
                {
                    var encoding =
                        encodingOverride ??
                        (string.IsNullOrEmpty(response.CharacterSet)
                         ? Encoding.GetEncoding(response.CharacterSet)
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

        public static IEnumerable<string> ReadLines(Func<HttpResponseMessage> httpResponseMessageFactory) =>
            ReadLines(httpResponseMessageFactory, null);

        public static IEnumerable<string> ReadLines(Func<HttpResponseMessage> httpResponseMessageFactory,
                                                    Encoding encodingOverride)
        {
            if (httpResponseMessageFactory == null) throw new ArgumentNullException(nameof(httpResponseMessageFactory));

            return _(); IEnumerable<string> _()
            {
                using (var response = httpResponseMessageFactory())
                {
                    if (response.Content != null)
                    {
                        var encoding =
                            encodingOverride
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
