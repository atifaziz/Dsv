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

namespace Yax.Tests
{
    using Xunit;

    public sealed class FormatTests
    {
        public sealed class CustomDelimiterFormat
        {
            readonly Format _delimiterInitializedFormat = new Format('\0');

            [Fact]
            public void ReturnsCustomDelimiter() =>
                Assert.Equal('\0', _delimiterInitializedFormat.Delimiter);

            [Fact]
            public void QuoteIsQuote() =>
                Assert.Equal('"', _delimiterInitializedFormat.Quote);

            [Fact]
            public void EscapeIsQuote() =>
                Assert.Equal(_delimiterInitializedFormat.Quote, _delimiterInitializedFormat.Escape);

            [Fact]
            public void NewLineIsLineFeed() =>
                Assert.Equal("\n", _delimiterInitializedFormat.NewLine);
        }

        public sealed class Csv
        {
            [Fact]
            public void ReturnsCustomDelimiter() =>
                Assert.Equal(',', Format.Csv.Delimiter);

            [Fact]
            public void QuoteIsQuote() =>
                Assert.Equal('"', Format.Csv.Quote);

            [Fact]
            public void EscapeIsQuote() =>
                Assert.Equal(Format.Csv.Quote, Format.Csv.Escape);

            [Fact]
            public void NewLineIsLineFeed() =>
                Assert.Equal("\n", Format.Csv.NewLine);
        }

        public sealed class WithDelimiter
        {
            [Fact]
            public void SameDelimiterReturnsSameFormat() =>
                Assert.Same(Format.Csv, Format.Csv.WithDelimiter(','));

            [Fact]
            public void ReturnsNewFormatWithChangedDelimiter()
            {
                const char delimiter = '\t';
                var @base = Format.Csv;
                var format = @base.WithDelimiter(delimiter);
                Assert.NotSame(@base, format);
                Assert.Equal(delimiter, format.Delimiter);
                Assert.Equal(@base.Quote, format.Quote);
                Assert.Equal(@base.Escape, format.Escape);
                Assert.Equal(@base.NewLine, format.NewLine);
            }
        }

        public sealed class WithQuote
        {
            [Fact]
            public void SameDelimiterReturnsSameFormat() =>
                Assert.Same(Format.Csv, Format.Csv.WithQuote('"'));

            [Fact]
            public void ReturnsNewFormatWithChangedDelimiter()
            {
                const char quote = '\'';
                var @base = Format.Csv;
                var format = @base.WithQuote(quote);
                Assert.NotSame(@base, format);
                Assert.Equal(@base.Delimiter, format.Delimiter);
                Assert.Equal(quote, format.Quote);
                Assert.Equal(@base.Escape, format.Escape);
                Assert.Equal(@base.NewLine, format.NewLine);
            }
        }

        public sealed class WithEscape
        {
            [Fact]
            public void SameEscapeReturnsSameFormat() =>
                Assert.Same(Format.Csv, Format.Csv.WithEscape('"'));

            [Fact]
            public void ReturnsNewFormatWithChangedEscape()
            {
                const char escape = '\\';
                var @base = Format.Csv;
                var format = @base.WithEscape(escape);
                Assert.NotSame(@base, format);
                Assert.Equal(@base.Delimiter, format.Delimiter);
                Assert.Equal(@base.Quote, format.Quote);
                Assert.Equal(escape, format.Escape);
                Assert.Equal(@base.NewLine, format.NewLine);
            }
        }

        public sealed class WithNewLine
        {
            [Fact]
            public void SameNewLineReturnsSameFormat() =>
                Assert.Same(Format.Csv, Format.Csv.WithNewLine("\n"));

            [Fact]
            public void ReturnsNewFormatWithChangedNewLine()
            {
                const string crlf = "\r\n";
                var @base = Format.Csv;
                var format = @base.WithNewLine(crlf);
                Assert.NotSame(@base, format);
                Assert.Equal(@base.Delimiter, format.Delimiter);
                Assert.Equal(@base.Quote, format.Quote);
                Assert.Equal(@base.Escape, format.Escape);
                Assert.Equal(crlf, format.NewLine);
            }
        }

        public sealed class Unquoted
        {
            [Fact]
            public void ReturnsSameFormatWhenAlreadyUnquoted()
            {
                var @base = Format.Csv.Unquoted();
                Assert.Same(@base, @base.Unquoted());
            }

            [Fact]
            public void ReturnsNewFormatWhenQuotedBefore()
            {
                var @base = Format.Csv;
                Assert.NotSame(@base, @base.Unquoted());
            }

            [Fact]
            public void SetsEscapeToNullChar()
            {
                var @base = Format.Csv;
                var unquoted = @base.Unquoted();
                Assert.Equal('\0', unquoted.Escape);
                Assert.NotEqual(unquoted.Escape, @base.Escape);
            }
        }
    }
}
