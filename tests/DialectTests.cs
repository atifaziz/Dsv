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
    using System;
    using Xunit;

    public sealed class DialectTests
    {
        public sealed class CustomDelimiterDialect
        {
            readonly Dialect _delimiterInitializedDialect = new Dialect('\0');

            [Fact]
            public void ReturnsCustomDelimiter() =>
                Assert.Equal('\0', _delimiterInitializedDialect.Delimiter);

            [Fact]
            public void QuoteIsQuote() =>
                Assert.Equal('"', _delimiterInitializedDialect.Quote);

            [Fact]
            public void EscapeIsQuote() =>
                Assert.Equal(_delimiterInitializedDialect.Quote, _delimiterInitializedDialect.Escape);

            [Fact]
            public void NewLineIsLineFeed() =>
                Assert.Equal("\n", _delimiterInitializedDialect.NewLine);
        }

        public sealed class Csv
        {
            [Fact]
            public void ReturnsCustomDelimiter() =>
                Assert.Equal(',', Dialect.Csv.Delimiter);

            [Fact]
            public void QuoteIsQuote() =>
                Assert.Equal('"', Dialect.Csv.Quote);

            [Fact]
            public void EscapeIsQuote() =>
                Assert.Equal(Dialect.Csv.Quote, Dialect.Csv.Escape);

            [Fact]
            public void NewLineIsLineFeed() =>
                Assert.Equal("\n", Dialect.Csv.NewLine);

            [Fact]
            public void RowFilterIsNull() =>
                Assert.Null(Dialect.Csv.RowFilter);
        }

        public sealed class WithDelimiter
        {
            [Fact]
            public void SameDelimiterReturnsSameDialect() =>
                Assert.Same(Dialect.Csv, Dialect.Csv.WithDelimiter(','));

            [Fact]
            public void ReturnsNewDialectWithChangedDelimiter()
            {
                const char delimiter = '\t';
                var @base = Dialect.Csv;
                var dialect = @base.WithDelimiter(delimiter);
                Assert.NotSame(@base, dialect);
                Assert.Equal(delimiter, dialect.Delimiter);
                Assert.Equal(@base.Quote, dialect.Quote);
                Assert.Equal(@base.Escape, dialect.Escape);
                Assert.Equal(@base.NewLine, dialect.NewLine);
                Assert.Equal(@base.RowFilter, dialect.RowFilter);
            }
        }

        public sealed class WithQuote
        {
            [Fact]
            public void SameDelimiterReturnsSameDialect() =>
                Assert.Same(Dialect.Csv, Dialect.Csv.WithQuote('"'));

            [Fact]
            public void ReturnsNewDialectWithChangedDelimiter()
            {
                const char quote = '\'';
                var @base = Dialect.Csv;
                var dialect = @base.WithQuote(quote);
                Assert.NotSame(@base, dialect);
                Assert.Equal(@base.Delimiter, dialect.Delimiter);
                Assert.Equal(quote, dialect.Quote);
                Assert.Equal(@base.Escape, dialect.Escape);
                Assert.Equal(@base.NewLine, dialect.NewLine);
                Assert.Equal(@base.RowFilter, dialect.RowFilter);
            }
        }

        public sealed class WithEscape
        {
            [Fact]
            public void SameEscapeReturnsSameDialect() =>
                Assert.Same(Dialect.Csv, Dialect.Csv.WithEscape('"'));

            [Fact]
            public void ReturnsNewDialectWithChangedEscape()
            {
                const char escape = '\\';
                var @base = Dialect.Csv;
                var dialect = @base.WithEscape(escape);
                Assert.NotSame(@base, dialect);
                Assert.Equal(@base.Delimiter, dialect.Delimiter);
                Assert.Equal(@base.Quote, dialect.Quote);
                Assert.Equal(escape, dialect.Escape);
                Assert.Equal(@base.NewLine, dialect.NewLine);
                Assert.Equal(@base.RowFilter, dialect.RowFilter);
            }
        }

        public sealed class WithNewLine
        {
            [Fact]
            public void SameNewLineReturnsSameDialect() =>
                Assert.Same(Dialect.Csv, Dialect.Csv.WithNewLine("\n"));

            [Fact]
            public void ReturnsNewDialectWithChangedNewLine()
            {
                const string crlf = "\r\n";
                var @base = Dialect.Csv;
                var dialect = @base.WithNewLine(crlf);
                Assert.NotSame(@base, dialect);
                Assert.Equal(@base.Delimiter, dialect.Delimiter);
                Assert.Equal(@base.Quote, dialect.Quote);
                Assert.Equal(@base.Escape, dialect.Escape);
                Assert.Equal(crlf, dialect.NewLine);
                Assert.Equal(@base.RowFilter, dialect.RowFilter);
            }
        }

        public sealed class WithRowFilter
        {
            [Fact]
            public void SameRowFilterSameDialect() =>
                Assert.Same(Dialect.Csv, Dialect.Csv.WithRowFilter(null));

            [Fact]
            public void ReturnsNewDialectWithChangedRowFilter()
            {
                var @base = Dialect.Csv;
                var filter = new Func<string, bool>(_ => true);
                var dialect = @base.WithRowFilter(filter);
                Assert.NotSame(@base, dialect);
                Assert.Equal(@base.Delimiter, dialect.Delimiter);
                Assert.Equal(@base.Quote, dialect.Quote);
                Assert.Equal(@base.Escape, dialect.Escape);
                Assert.Equal(@base.NewLine, dialect.NewLine);
                Assert.Equal(filter, dialect.RowFilter);
            }
        }

        public sealed class OrWithRowFilter
        {
            [Fact]
            public void ReturnsNewDialectWithChangedRowFilter()
            {
                var @base = Dialect.Csv;
                var dialect = @base.WithRowFilter(s => s == "foo")
                                   .OrWithRowFilter(s => s == "baz");
                Assert.NotSame(@base, dialect);
                Assert.False(dialect.RowFilter(null));
                Assert.False(dialect.RowFilter(string.Empty));
                Assert.True(dialect.RowFilter("foo"));
                Assert.False(dialect.RowFilter("bar"));
                Assert.True(dialect.RowFilter("baz"));
            }
        }
    }
}
