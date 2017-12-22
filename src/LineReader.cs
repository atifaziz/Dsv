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
#endregion

namespace Yax
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    static partial class LineReader
    {
        public static IEnumerable<string> ReadLinesFromStream(Func<Stream> streamFactory) =>
            ReadLinesFromStream(streamFactory, null);

        public static IEnumerable<string> ReadLinesFromStream(Func<Stream> streamFactory, Encoding encoding)
        {
            if (streamFactory == null) throw new ArgumentNullException(nameof(streamFactory));
            return _(); IEnumerable<string> _()
            {
                using (var stream = streamFactory())
                {
                    foreach (var line in ReadLines(() => stream.OpenTextReader(encoding)))
                        yield return line;
                }
            }
        }

        static StreamReader OpenTextReader(this Stream stream, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            return encoding == null
                 ? new StreamReader(stream)
                 : new StreamReader(stream, encoding);
        }

        public static IEnumerable<string> ReadLines(Func<TextReader> readerFactory)
        {
            if (readerFactory == null) throw new ArgumentNullException(nameof(readerFactory));
            return _(); IEnumerable<string> _()
            {
                using (var reader = readerFactory())
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                        yield return line;
                }
            }
        }
    }
}
