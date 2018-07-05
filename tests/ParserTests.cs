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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Mannex.Text.RegularExpressions;
    using MoreLinq;
    using Xunit;

    public class ParserTests
    {
        public class EnumerableSource
        {
            [Fact]
            public void ParseWithNullLinesThrows()
            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    Parser.ParseDsv<object, object>(
                        (IEnumerable<string>) null, Format.Csv,
                        lineFilter  : delegate { throw new NotImplementedException(); },
                        headSelector: delegate { throw new NotImplementedException(); },
                        rowSelector : delegate { throw new NotImplementedException(); }));
                Assert.Equal("lines", e.ParamName);
            }

            [Fact]
            public void ParseWithNullFormatThrows()
            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    new string[0].ParseDsv<object, object>(
                        format: null,
                        lineFilter  : delegate { throw new NotImplementedException(); },
                        headSelector: delegate { throw new NotImplementedException(); },
                        rowSelector : delegate { throw new NotImplementedException(); }));
                Assert.Equal("format", e.ParamName);
            }

            [Fact]
            public void ParseWithNullRowFilterThrows()
            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    new string[0].ParseDsv<object, object>(Format.Csv,
                        lineFilter  : null,
                        headSelector: delegate { throw new NotImplementedException(); },
                        rowSelector : delegate { throw new NotImplementedException(); }));
                Assert.Equal("lineFilter", e.ParamName);
            }

            [Fact]
            public void ParseWithNullHeadSelectorThrows()
            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    new string[0].ParseDsv<object, object>(Format.Csv,
                        lineFilter  : delegate { throw new NotImplementedException(); },
                        headSelector: null,
                        rowSelector : delegate { throw new NotImplementedException(); }));
                Assert.Equal("headSelector", e.ParamName);
            }

            [Fact]
            public void ParseIsLazy()
            {
                IEnumerable<string> Lines()
                {
                    throw new InvalidOperationException();
                    #pragma warning disable 162
                    yield return null;
                    #pragma warning restore 162
                }

                Lines().ParseDsv<object, object>(
                    format: Format.Csv,
                    lineFilter  : delegate { throw new NotImplementedException(); },
                    headSelector: delegate { throw new NotImplementedException(); },
                    rowSelector : delegate { throw new NotImplementedException(); });
            }
        }

        public class ObservableSource
        {
            [Fact]
            public void ParseWithNullLinesThrows()
            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    Parser.ParseDsv<object, object>(
                        (IObservable<string>) null, Format.Csv,
                        lineFilter  : delegate { throw new NotImplementedException(); },
                        headSelector: delegate { throw new NotImplementedException(); },
                        rowSelector : delegate { throw new NotImplementedException(); }));
                Assert.Equal("lines", e.ParamName);
            }

            [Fact]
            public void ParseWithNullFormatThrows()
            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    Observable.Empty<string>().ParseDsv<object, object>(
                        format: null,
                        lineFilter  : delegate { throw new NotImplementedException(); },
                        headSelector: delegate { throw new NotImplementedException(); },
                        rowSelector : delegate { throw new NotImplementedException(); }));
                Assert.Equal("format", e.ParamName);
            }

            [Fact]
            public void ParseWithNullRowFilterThrows()
            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    Observable.Empty<string>().ParseDsv<object, object>(Format.Csv,
                        lineFilter  : null,
                        headSelector: delegate { throw new NotImplementedException(); },
                        rowSelector : delegate { throw new NotImplementedException(); }));
                Assert.Equal("lineFilter", e.ParamName);
            }

            [Fact]
            public void ParseWithNullHeadSelectorThrows()
            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    Observable.Empty<string>().ParseDsv<object, object>(Format.Csv,
                        lineFilter  : delegate { throw new NotImplementedException(); },
                        headSelector: null,
                        rowSelector : delegate { throw new NotImplementedException(); }));
                Assert.Equal("headSelector", e.ParamName);
            }

            [Fact]
            public void ParseIsLazy()
            {
                IObservable<string> Lines() => throw new InvalidOperationException();

                Observable
                    .Defer(Lines)
                    .ParseDsv<object, object>(
                        format: Format.Csv,
                        lineFilter  : delegate { throw new NotImplementedException(); },
                        headSelector: delegate { throw new NotImplementedException(); },
                        rowSelector : delegate { throw new NotImplementedException(); });
            }
        }

        [Fact]
        public void ParseWithNullRowSelectorThrows()
        {
            var e = Assert.Throws<ArgumentNullException>(() =>
                new string[0].ParseDsv<object, object>(Format.Csv,
                    lineFilter  : delegate { throw new NotImplementedException(); },
                    headSelector: delegate { throw new NotImplementedException(); },
                    rowSelector : null));
            Assert.Equal("rowSelector", e.ParamName);
        }

        static void ParseDsv(char delimiter, char? quote, char escape, string newline, bool skipBlanks,
                             IEnumerable<(int Line, string[] Fields)> rows,
                             Type errorType, string errorMessage,
                             Func<Format, Func<string, bool>, IEnumerable<TextRow>> rowParser)
        {
            var format = new Format(delimiter).WithQuote(quote)
                                              .WithEscape(escape)
                                              .WithNewLine(newline);

            var lineFilter = skipBlanks
                           ? string.IsNullOrWhiteSpace
                           : new Func<string, bool>(_ => false);

            if (errorType == null)
            {
                using (var row = rows.GetEnumerator())
                {
                    foreach (var fields in rowParser(format, lineFilter))
                    {
                        Assert.True(row.MoveNext(), "Source has too many rows.");
                        var (ln, fs) = row.Current;
                        Assert.Equal(ln, fields.LineNumber);
                        Assert.Equal(fs, fields.ToArray());
                    }

                    Assert.False(row.MoveNext(), "Source has too few rows.");
                }
            }
            else
            {
                var e = Assert.Throws(errorType, () => rowParser(format, lineFilter).Consume());
                Assert.Equal(errorMessage, e.Message);
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void
            ParseDsvWithEnumerable(
                char delimiter, char? quote, char escape, string newline, bool skipBlanks,
                IEnumerable<string> lines, IEnumerable<(int Line, string[] Fields)> rows,
                Type errorType, string errorMessage) =>
            ParseDsv(delimiter, quote, escape, newline, skipBlanks, rows, errorType, errorMessage,
                     lines.ParseDsv);

        [Theory]
        [MemberData(nameof(GetData))]
        public void
            ParseDsvWithObservable(
                char delimiter, char? quote, char escape, string newline, bool skipBlanks,
                IEnumerable<string> lines, IEnumerable<(int Line, string[] Fields)> rows,
                Type errorType, string errorMessage) =>
            ParseDsv(delimiter, quote, escape, newline, skipBlanks, rows, errorType, errorMessage,
                     (f, rf) => lines.ToObservable()
                                     .ParseDsv(f, rf)
                                     .ToEnumerable());

        public static TheoryData<char, char?, char, string, bool,
                                 IEnumerable<string>,
                                 IEnumerable<(int LineNumber, string[] Fields)>,
                                 Type, string>
            GetData()
        {
            var type = MethodBase.GetCurrentMethod().DeclaringType;

            var config = new[] { "delimiter", "quote", "escape", "newline", "blanks" };
            var nils   = new[] { "null", "nil", "none", "undefined" };
            var proto  = new[] { new { ln = default(int), row = default(string[]) } };

            var data =
                from q in new[]
                {
                    from g in LineReader.ReadLinesFromStream(() => type.GetManifestResourceStream("Tests.md"))
                                        .Scan(new { Code = false, Line = default(string) },
                                              (s, line) => new
                                              {
                                                  Code = line.StartsWith("```", StringComparison.Ordinal) ? !s.Code : s.Code,
                                                  Line = line
                                              })
                                        .Skip(1) // skip seed
                                        .GroupAdjacent(e => e.Code)
                    select
                        from e in g.Skip(1) // skip "```"
                        select e.Line       // and keep just the lines
                }
                from e in q.Batch(4)
                select e.Pad(4) into e
                where e.All(s => s != null)
                select e.Fold((s, inp, _, exp) => new { Suppositions = string.Join(Environment.NewLine, s), Input = inp, Expected = exp }) into e
                let throws = '[' != e.Expected.FirstOrDefault()?.TrimStart()[0]
                select
                    config
                        .Select(p => Regex.Match(e.Suppositions, $@"(?<=\b{Regex.Escape(p)}( +is| *[=:]) *`)[^`]+(?=`)", RegexOptions.ExplicitCapture))
                        .Select(m => m.Success ? m.Value : null)
                        .Fold((d, q, esc, nl, blanks) => new
                        {
                            Delimiter  = d?[0] ?? ',',
                            Quote      = q == null
                                       ? '"'
                                       : nils.Contains(q, StringComparer.OrdinalIgnoreCase)
                                       ? (char?) null
                                       : q[0],
                            Escape     = esc?[0] ?? '"',
                            NewLine    = nl != null
                                       ? nils.Contains(nl, StringComparer.OrdinalIgnoreCase)
                                       ? null
                                       : Regex.Replace(nl, @"\\[rn]", m => m.Value[1] == 'r' ? "\r"
                                                                         : m.Value[1] == 'n' ? "\n"
                                                                         : throw new FormatException())
                                       : "\n",
                            SkipBlanks = "skip".Equals(blanks, StringComparison.OrdinalIgnoreCase),
                            e.Input,
                            Expected   = throws
                                       ? null
                                       : from r in Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(string.Join(Environment.NewLine, e.Expected), proto)
                                         select (r.ln, r.row),
                            Error      = throws
                                       ? Regex.Match(e.Expected.First(), @"^ *([^: ]+) *: *(.+)").BindNum((t, m) => new
                                         {
                                             Type    = t.Success
                                                     ? Type.GetType(t.Value, throwOnError: true)
                                                     : throw new Exception("Test exception type name missing."),
                                             Message = m.Success
                                                     ? m.Value.Trim()
                                                     : throw new Exception("Test exception message missing."),
                                         })
                                       : null,
                        })
                into e
                select (e.Delimiter, e.Quote, e.Escape, e.NewLine, e.SkipBlanks,
                        e.Input, e.Expected,
                        e.Error?.Type, e.Error?.Message);

            return data.ToTheoryData();
        }

        [Fact]
        public void ParseCsvHeaderBindingWithEnumerable()
        {
            const string csv
                = "baz,foo,bar\n"
                + "789,123,456\n";

            var data = csv
                .SplitIntoLines()
                .ParseCsv(
                    row => new
                    {
                        Foo = row.FindFirstIndex(h => h == "foo") ?? -1,
                        Bar = row.FindFirstIndex(h => h == "bar") ?? -1,
                        Baz = row.FindFirstIndex(h => h == "baz") ?? -1,
                    },
                    (i, row) => new[]
                    {
                        row[i.Foo],
                        row[i.Bar],
                        row[i.Baz],
                    })
                .Select(row =>
                    row.Select(s => int.Parse(s, CultureInfo.InvariantCulture))
                       .Fold((foo, bar, baz) => new
                       {
                           Foo = foo,
                           Bar = bar,
                           Baz = baz
                       }));

            var expected = new
            {
                Foo = 123,
                Bar = 456,
                Baz = 789
            };

            Assert.Equal(expected, data.Single());
        }

        [Fact]
        public void ParseCsvWithHeaderBindingWithObservable()
        {
            const string csv
                = "baz,foo,bar\n"
                  + "789,123,456\n";

            var data = csv
                .SplitIntoLines()
                .ToObservable()
                .ParseCsv(
                    row => new
                    {
                        Foo = row.FindFirstIndex(h => h == "foo") ?? -1,
                        Bar = row.FindFirstIndex(h => h == "bar") ?? -1,
                        Baz = row.FindFirstIndex(h => h == "baz") ?? -1,
                    },
                    (h, row) => new[]
                    {
                        row[h.Foo],
                        row[h.Bar],
                        row[h.Baz],
                    })
                .Select(row =>
                    row.Select(s => int.Parse(s, CultureInfo.InvariantCulture))
                       .Fold((foo, bar, baz) => new
                       {
                           Foo = foo,
                           Bar = bar,
                           Baz = baz
                       }));

            var result = data.SingleAsync().GetAwaiter().GetResult();

            var expected = new
            {
                Foo = 123,
                Bar = 456,
                Baz = 789
            };

            Assert.Equal(expected, result);
        }
    }
}
