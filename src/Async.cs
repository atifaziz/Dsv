#region Copyright 2019 Atif Aziz. All rights reserved.
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

#if !NO_ASYNC_STREAM

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

namespace Dsv;

static partial class Parser
{
    public static IAsyncEnumerable<(T Header, TextRow Row)>
        ParseCsv<T>(this IAsyncEnumerable<string> lines, Func<TextRow, T> headSelector) =>
        lines.ParseDsv(Format.Csv, headSelector, ValueTuple.Create);

    public static IAsyncEnumerable<TRow>
        ParseCsv<THead, TRow>(this IAsyncEnumerable<string> lines,
                              Func<TextRow, THead> headSelector,
                              Func<THead, TextRow, TRow> rowSelector) =>
        lines.ParseDsv(Format.Csv, headSelector, rowSelector);

    [Experimental("DSV001")]
    public static IAsyncEnumerable<T>
        ParseCsv<T>(this IAsyncEnumerable<string> lines,
                    Func<TextRow, Func<TextRow, T>> selector) =>
        lines.ParseDsv(Format.Csv, selector);

    public static IAsyncEnumerable<(T Header, TextRow Row)>
        ParseDsv<T>(this IAsyncEnumerable<string> lines, Format format,
                    Func<TextRow, T> headSelector) =>
        lines.ParseDsv(format, _ => false, headSelector);

    public static IAsyncEnumerable<(T Header, TextRow Row)>
        ParseDsv<T>(this IAsyncEnumerable<string> lines, Format format,
                    Func<string, bool> lineFilter,
                    Func<TextRow, T> headSelector) =>
        lines.ParseDsv(format, lineFilter, headSelector, ValueTuple.Create);

    public static IAsyncEnumerable<TRow>
        ParseDsv<THead, TRow>(this IAsyncEnumerable<string> lines, Format format,
                              Func<TextRow, THead> headSelector,
                              Func<THead, TextRow, TRow> rowSelector) =>
        lines.ParseDsv(format, _ => false, headSelector, rowSelector);

    [Experimental("DSV001")]
    public static IAsyncEnumerable<T>
        ParseDsv<T>(this IAsyncEnumerable<string> lines, Format format,
                    Func<TextRow, Func<TextRow, T>> selector) =>
        lines.ParseDsv(format, _ => false, selector);

    [Experimental("DSV001")]
    public static IAsyncEnumerable<T>
        ParseDsv<T>(this IAsyncEnumerable<string> lines, Format format,
                    Func<string, bool> lineFilter,
                    Func<TextRow, Func<TextRow, T>> selector) =>
        lines.ParseDsv(format, lineFilter, selector, (head, row) => head(row));

    public static IAsyncEnumerable<TRow>
        ParseDsv<THead, TRow>(this IAsyncEnumerable<string> lines, Format format,
                              Func<string, bool> lineFilter,
                              Func<TextRow, THead> headSelector,
                              Func<THead, TextRow, TRow> rowSelector)
    {
        if (lines == null) throw new ArgumentNullException(nameof(lines));
        if (format == null) throw new ArgumentNullException(nameof(format));
        if (lineFilter == null) throw new ArgumentNullException(nameof(lineFilter));
        if (headSelector == null) throw new ArgumentNullException(nameof(headSelector));
        if (rowSelector == null) throw new ArgumentNullException(nameof(rowSelector));

        return Iterator(lines, format, lineFilter, headSelector, rowSelector);

        static async IAsyncEnumerable<TRow>
            Iterator(IAsyncEnumerable<string> lines, Format format, Func<string, bool> lineFilter,
                     Func<TextRow, THead> headSelector, Func<THead, TextRow, TRow> rowSelector,
                     [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var row = lines.ParseDsv(format, lineFilter).GetAsyncEnumerator(cancellationToken);
            await using var _ = row.ConfigureAwait(false);

            if (!await row.MoveNextAsync().ConfigureAwait(false))
                yield break;

            var head = headSelector(row.Current);

            while (await row.MoveNextAsync().ConfigureAwait(false))
                yield return rowSelector(head, row.Current);
        }
    }

    public static IAsyncEnumerable<TextRow> ParseCsv(this IAsyncEnumerable<string> lines) =>
        lines.ParseDsv(Format.Csv);

    public static IAsyncEnumerable<TextRow> ParseCsv(this IAsyncEnumerable<string> lines,
                                                     Func<string, bool> lineFilter) =>
        lines.ParseDsv(Format.Csv, lineFilter);

    public static IAsyncEnumerable<TextRow> ParseDsv(this IAsyncEnumerable<string> lines,
                                                     Format format) =>
        lines.ParseDsv(format, (string _) => false);

    public static IAsyncEnumerable<TextRow> ParseDsv(this IAsyncEnumerable<string> lines,
                                                     Format format,
                                                     Func<string, bool> lineFilter)
    {
        if (lines == null) throw new ArgumentNullException(nameof(lines));
        if (format == null) throw new ArgumentNullException(nameof(format));
        if (lineFilter == null) throw new ArgumentNullException(nameof(lineFilter));

        return Iterator(lines, format, lineFilter);

        static async IAsyncEnumerable<TextRow>
            Iterator(IAsyncEnumerable<string> lines, Format format, Func<string, bool> lineFilter,
                     [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var (onLine, onEoi) = Create(format, lineFilter);

            await foreach (var line in lines.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (onLine(line) is { } row)
                    yield return row;
            }

            if (onEoi() is { } e)
                throw e;
        }
    }
}

#endif // !NO_ASYNC_STREAM
