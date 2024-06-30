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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Finq;
using Mannex.Text.RegularExpressions;
using MoreLinq;
using Xunit;

namespace Dsv.Tests;

public class ParserTests
{
    public class EnumerableSource
    {
        [Fact]
        public void ParseWithNullLinesThrows()
        {
            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    Parser.ParseDsv<object, object>(
                        (IEnumerable<string>) null!, Format.Csv,
                        lineFilter  : delegate { throw new NotImplementedException(); },
                        headSelector: delegate { throw new NotImplementedException(); },
                        rowSelector : delegate { throw new NotImplementedException(); }));
                Assert.Equal("lines", e.ParamName);
            }

            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    Parser.ParseDsv((IEnumerable<string>) null!, Format.Csv,
                        delegate { throw new NotImplementedException(); }));
                Assert.Equal("lines", e.ParamName);
            }
        }

        [Fact]
        public void ParseWithNullFormatThrows()
        {
            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    Array.Empty<string>().ParseDsv<object, object>(
                        format: null!,
                        lineFilter  : delegate { throw new NotImplementedException(); },
                        headSelector: delegate { throw new NotImplementedException(); },
                        rowSelector : delegate { throw new NotImplementedException(); }));
                Assert.Equal("format", e.ParamName);
            }

            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    Array.Empty<string>().ParseDsv(null!, delegate { throw new NotImplementedException(); }));
                Assert.Equal("format", e.ParamName);
            }
        }

        [Fact]
        public void ParseWithNullRowFilterThrows()
        {
            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    Array.Empty<string>().ParseDsv<object, object>(Format.Csv,
                        lineFilter  : null!,
                        headSelector: delegate { throw new NotImplementedException(); },
                        rowSelector : delegate { throw new NotImplementedException(); }));
                Assert.Equal("lineFilter", e.ParamName);
            }

            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    Array.Empty<string>().ParseDsv(Format.Csv, null!));
                Assert.Equal("lineFilter", e.ParamName);
            }
        }

        [Fact]
        public void ParseWithNullHeadSelectorThrows()
        {
            var e = Assert.Throws<ArgumentNullException>(() =>
                Array.Empty<string>().ParseDsv<object, object>(Format.Csv,
                    lineFilter  : delegate { throw new NotImplementedException(); },
                    headSelector: null!,
                    rowSelector : delegate { throw new NotImplementedException(); }));
            Assert.Equal("headSelector", e.ParamName);
        }

        [Fact]
        public void ParseWithNullRowSelectorThrows()
        {
            var e = Assert.Throws<ArgumentNullException>(() =>
                Array.Empty<string>().ParseDsv<object, object>(Format.Csv,
                    lineFilter  : delegate { throw new NotImplementedException(); },
                    headSelector: delegate { throw new NotImplementedException(); },
                    rowSelector : null!));
            Assert.Equal("rowSelector", e.ParamName);
        }

        [Fact]
        public void ParseIsLazy()
        {
            static IEnumerable<string> Lines()
            {
                throw new InvalidOperationException();
                #pragma warning disable 162 // Unreachable code detected
                yield return null;
                #pragma warning restore 162
            }

            _ = Lines()
                .ParseDsv<object, object>(
                    format: Format.Csv,
                    lineFilter  : delegate { throw new NotImplementedException(); },
                    headSelector: delegate { throw new NotImplementedException(); },
                    rowSelector : delegate { throw new NotImplementedException(); });
        }

#pragma warning disable DSV001 // Type is for evaluation purposes only and is subject to change or removal in future updates.

        [Fact]
        public void ParseWithReader()
        {
            var reader =
                from hr in TextRow.Reader
                select new
                {
                    Name = hr.GetFirstIndex("Name", StringComparison.OrdinalIgnoreCase),
                    Age  = hr.GetFirstIndex("Age", StringComparison.OrdinalIgnoreCase),
                    City = hr.GetFirstIndex("City", StringComparison.OrdinalIgnoreCase),
                }
                into hr
                select
                    from dr in TextRow.Reader
                    select new
                    {
                        Name = dr[hr.Name],
                        Age  = int.Parse(dr[hr.Age], NumberStyles.None, CultureInfo.InvariantCulture),
                        City = dr[hr.City]
                    };

            const string csv = """
                Name,Age,City
                Alice,25,New York
                Bob,30,Los Angeles
                Charlie,35,Chicago
                """;

            var result = csv.SplitIntoLines()
                            .ParseDsv(Format.Csv, _ => false, reader);

            Assert.Equal([
                             new { Name = "Alice"  , Age = 25, City = "New York"    },
                             new { Name = "Bob"    , Age = 30, City = "Los Angeles" },
                             new { Name = "Charlie", Age = 35, City = "Chicago"     },
                         ],
                         result);
        }

#pragma warning restore DSV001
    }

    #if !NO_ASYNC_STREAM

    public class AsyncEnumerableSource
    {
        [Fact]
        public void ParseWithNullLinesThrows()
        {
            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    Parser.ParseDsv<object, object>(
                        (IAsyncEnumerable<string>) null!, Format.Csv,
                        lineFilter  : delegate { throw new NotImplementedException(); },
                        headSelector: delegate { throw new NotImplementedException(); },
                        rowSelector : delegate { throw new NotImplementedException(); }));
                Assert.Equal("lines", e.ParamName);
            }

            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    Parser.ParseDsv(
                        (IAsyncEnumerable<string>) null!, Format.Csv,
                        delegate { throw new NotImplementedException(); }));
                Assert.Equal("lines", e.ParamName);
            }
        }

        [Fact]
        public void ParseWithNullFormatThrows()
        {
            var source = AsyncEnumerable.Empty<string>();

            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    source.ParseDsv<object, object>(
                        format: null!,
                        lineFilter  : delegate { throw new NotImplementedException(); },
                        headSelector: delegate { throw new NotImplementedException(); },
                        rowSelector : delegate { throw new NotImplementedException(); }));
                Assert.Equal("format", e.ParamName);
            }

            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    source.ParseDsv(null!, delegate { throw new NotImplementedException(); }));
                Assert.Equal("format", e.ParamName);
            }
        }

        [Fact]
        public void ParseWithNullRowFilterThrows()
        {
            var source = AsyncEnumerable.Empty<string>();

            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    source.ParseDsv<object, object>(Format.Csv,
                        lineFilter  : null!,
                        headSelector: delegate { throw new NotImplementedException(); },
                        rowSelector : delegate { throw new NotImplementedException(); }));
                Assert.Equal("lineFilter", e.ParamName);
            }

            {
                var e = Assert.Throws<ArgumentNullException>(() =>
                    source.ParseDsv(Format.Csv, null!));
                Assert.Equal("lineFilter", e.ParamName);
            }
        }

        [Fact]
        public void ParseWithNullHeadSelectorThrows()
        {
            var source = AsyncEnumerable.Empty<string>();
            var e = Assert.Throws<ArgumentNullException>(() =>
                source.ParseDsv<object, object>(Format.Csv,
                    lineFilter  : delegate { throw new NotImplementedException(); },
                    headSelector: null!,
                    rowSelector : delegate { throw new NotImplementedException(); }));
            Assert.Equal("headSelector", e.ParamName);
        }

        [Fact]
        public void ParseWithNullRowSelectorThrows()
        {
            var source = AsyncEnumerable.Empty<string>();
            var e = Assert.Throws<ArgumentNullException>(() =>
                source.ParseDsv<object, object>(Format.Csv,
                    lineFilter  : delegate { throw new NotImplementedException(); },
                    headSelector: delegate { throw new NotImplementedException(); },
                    rowSelector : null!));
            Assert.Equal("rowSelector", e.ParamName);
        }

        [Fact]
        public void ParseIsLazy()
        {
            #pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously

            static async IAsyncEnumerable<string> Lines()
            {
                throw new InvalidOperationException();
                #pragma warning disable 162 // Unreachable code detected
                yield return null;
                #pragma warning restore 162
            }

            #pragma warning restore 1998

            _ = Lines()
                .ParseDsv<object, object>(
                    format: Format.Csv,
                    lineFilter  : delegate { throw new NotImplementedException(); },
                    headSelector: delegate { throw new NotImplementedException(); },
                    rowSelector : delegate { throw new NotImplementedException(); });
        }

        [Fact]
        public async Task PassesCancellationTokenToSource()
        {
            const string test = nameof(test);

            using var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            CancellationToken capturedCancellationToken = default;
            await using var e = TestSource(test).ParseCsv().GetAsyncEnumerator(cancellationToken);

            Assert.True(await e.MoveNextAsync());
            Assert.Equal(test, e.Current[0]);
            Assert.Equal(cancellationToken, capturedCancellationToken);

            IAsyncEnumerable<string> TestSource(string test)
            {
                return new DelegatingAsyncEnumerable<string>(_);
                async IAsyncEnumerator<string> _(CancellationToken cancellationToken)
                {
                    capturedCancellationToken = cancellationToken;
                    await Task.Delay(TimeSpan.Zero, cancellationToken).ConfigureAwait(false);
                    yield return test;
                }
            }
        }
    }

    #endif // !NO_ASYNC_STREAM

    public class ObservableSource
    {
        [Fact]
        public void ParseWithNullLinesThrows()
        {
            var e = Assert.Throws<ArgumentNullException>(() =>
                Parser.ParseDsv<object, object>(
                    (IObservable<string>) null!, Format.Csv,
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
                    format: null!,
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
                    lineFilter  : null!,
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
                    headSelector: null!,
                    rowSelector : delegate { throw new NotImplementedException(); }));
            Assert.Equal("headSelector", e.ParamName);
        }

        [Fact]
        public void ParseWithNullRowSelectorThrows()
        {
            var e = Assert.Throws<ArgumentNullException>(() =>
                Observable.Empty<string>().ParseDsv<object, object>(Format.Csv,
                    lineFilter  : delegate { throw new NotImplementedException(); },
                    headSelector: delegate { throw new NotImplementedException(); },
                    rowSelector : null!));
            Assert.Equal("rowSelector", e.ParamName);
        }

        [Fact]
        public void ParseIsLazy()
        {
            static IObservable<string> Lines() => throw new InvalidOperationException();

            _ = Observable
                .Defer(Lines)
                .ParseDsv<object, object>(
                    format: Format.Csv,
                    lineFilter  : delegate { throw new NotImplementedException(); },
                    headSelector: delegate { throw new NotImplementedException(); },
                    rowSelector : delegate { throw new NotImplementedException(); });
        }
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
            using var row = rows.GetEnumerator();
            foreach (var fields in rowParser(format, lineFilter))
            {
                Assert.True(row.MoveNext(), "Source has too many rows.");
                var (ln, fs) = row.Current;
                Assert.Equal(ln, fields.LineNumber);
                Assert.Equal(fs.AsSpan(), [..fields]);
            }

            Assert.False(row.MoveNext(), "Source has too few rows.");
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

    #if !NO_ASYNC_STREAM

    [Theory]
    [MemberData(nameof(GetData))]
    public void
        ParseDsvWithAsyncEnumerable(
            char delimiter, char? quote, char escape, string newline, bool skipBlanks,
            IEnumerable<string> lines, IEnumerable<(int Line, string[] Fields)> rows,
            Type errorType, string errorMessage) =>
        ParseDsv(delimiter, quote, escape, newline, skipBlanks, rows, errorType, errorMessage,
                 (f, rf) => lines.ToAsyncEnumerable()
                                 .ParseDsv(f, rf)
                                 .ToEnumerable());

    #endif // !NO_ASYNC_STREAM

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
        var type = MethodBase.GetCurrentMethod()?.DeclaringType ?? throw new NullReferenceException();

        var config = new[] { "delimiter", "quote", "escape", "newline", "blanks" };
        var nils   = new[] { "null", "nil", "none", "undefined" };
        var proto  = new[] { new { ln = default(int), row = default(string[]) } };

        var data =
            from q in new[]
            {
                from g in LineReader.ReadLinesFromStream(() => type.GetManifestResourceStream("Tests.md"))
                                    .Scan(new { Code = false, Line = string.Empty },
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
                        Delimiter  = d is { } ds ? ds == @"\t" ? '\t' : ds[0] : ',',
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

        var data =
            from e in csv.SplitIntoLines()
                         .ParseCsv(row => new
                         {
                             Foo = row.FindFirstIndex(h => h == "foo") ?? -1,
                             Bar = row.FindFirstIndex(h => h == "bar") ?? -1,
                             Baz = row.FindFirstIndex(h => h == "baz") ?? -1,
                         })

            select new[]
            {
                e.Row[e.Header.Foo],
                e.Row[e.Header.Bar],
                e.Row[e.Header.Baz],
            }
            into e
            select e.Select(s => int.Parse(s, CultureInfo.InvariantCulture))
                    .Fold((foo, bar, baz) => new
                    {
                        Foo = foo,
                        Bar = bar,
                        Baz = baz
                    });

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

        var data =
            from e in csv.SplitIntoLines()
                         .ToObservable()
                         .ParseCsv(row => new
                         {
                             Foo = row.FindFirstIndex(h => h == "foo") ?? -1,
                             Bar = row.FindFirstIndex(h => h == "bar") ?? -1,
                             Baz = row.FindFirstIndex(h => h == "baz") ?? -1,
                         })

            select new[]
            {
                e.Row[e.Header.Foo],
                e.Row[e.Header.Bar],
                e.Row[e.Header.Baz],
            }
            into e
            select e.Select(s => int.Parse(s, CultureInfo.InvariantCulture))
                    .Fold((foo, bar, baz) => new
                    {
                        Foo = foo,
                        Bar = bar,
                        Baz = baz
                    });

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

static class AsyncEnumerable
{
    public static IAsyncEnumerable<T> Empty<T>() => EmptyAsyncEnumerable<T>.Instance;

    sealed class EmptyAsyncEnumerable<T> : IAsyncEnumerable<T>, IAsyncEnumerator<T>
    {
        public static readonly IAsyncEnumerable<T> Instance = new EmptyAsyncEnumerable<T>();

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => this;

        public ValueTask DisposeAsync() => new(Task.CompletedTask);
        public ValueTask<bool> MoveNextAsync() => new(Task.FromResult(false));
        public T Current => throw new InvalidOperationException();
    }
}
