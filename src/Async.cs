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

namespace Dsv
{
    using System;
    using System.Collections.Generic;

    static partial class Parser
    {
        public static IAsyncEnumerable<(T Header, TextRow Row)>
            ParseCsv<T>(this IAsyncEnumerable<string> lines,
                        Func<TextRow, T> headSelector) =>
            lines.ParseDsv(Format.Csv, headSelector, ValueTuple.Create);

        public static IAsyncEnumerable<TRow> ParseCsv<THead, TRow>(this IAsyncEnumerable<string> lines,
            Func<TextRow, THead> headSelector,
            Func<THead, TextRow, TRow> rowSelector) =>
            lines.ParseDsv(Format.Csv, headSelector, rowSelector);

        public static IAsyncEnumerable<(T Header, TextRow Row)>
            ParseDsv<T>(this IAsyncEnumerable<string> lines,
                        Format format,
                        Func<TextRow, T> headSelector) =>
            lines.ParseDsv(format, _ => false, headSelector);

        public static IAsyncEnumerable<(T Header, TextRow Row)>
            ParseDsv<T>(this IAsyncEnumerable<string> lines,
                        Format format,
                        Func<string, bool> lineFilter,
                        Func<TextRow, T> headSelector) =>
            lines.ParseDsv(format, lineFilter, headSelector, ValueTuple.Create);

        public static IAsyncEnumerable<TRow> ParseDsv<THead, TRow>(this IAsyncEnumerable<string> lines,
            Format format,
            Func<TextRow, THead> headSelector,
            Func<THead, TextRow, TRow> rowSelector) =>
            lines.ParseDsv(format, _ => false, headSelector, rowSelector);

        public static IAsyncEnumerable<TRow> ParseDsv<THead, TRow>(this IAsyncEnumerable<string> lines,
            Format format,
            Func<string, bool> lineFilter,
            Func<TextRow, THead> headSelector,
            Func<THead, TextRow, TRow> rowSelector)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            if (format == null) throw new ArgumentNullException(nameof(format));
            if (lineFilter == null) throw new ArgumentNullException(nameof(lineFilter));
            if (headSelector == null) throw new ArgumentNullException(nameof(headSelector));
            if (rowSelector == null) throw new ArgumentNullException(nameof(rowSelector));

            return _(); async IAsyncEnumerable<TRow> _()
            {
                // TODO review CancellationToken.None
                // TODO review await configuration

                await using var row = lines.ParseDsv(format, lineFilter).GetAsyncEnumerator(System.Threading.CancellationToken.None);

                if (!(await row.MoveNextAsync()))
                    yield break;

                var head = headSelector(row.Current);

                while (await row.MoveNextAsync())
                    yield return rowSelector(head, row.Current);
            }
        }

        public static IAsyncEnumerable<TextRow> ParseCsv(this IAsyncEnumerable<string> lines) =>
            lines.ParseDsv(Format.Csv);

        public static IAsyncEnumerable<TextRow> ParseCsv(this IAsyncEnumerable<string> lines, Func<string, bool> lineFilter) =>
            lines.ParseDsv(Format.Csv, lineFilter);

        public static IAsyncEnumerable<TextRow> ParseDsv(this IAsyncEnumerable<string> lines,
            Format format) =>
            lines.ParseDsv(format, (string _) => false);

        public static async IAsyncEnumerable<TextRow> ParseDsv(this IAsyncEnumerable<string> lines,
            Format format, Func<string, bool> lineFilter)
        {
            var (onLine, onEoi) = Create(format, lineFilter);

            // TODO review CancellationToken.None
            // TODO review await configuration

            await foreach (var line in lines)
            {
                if (onLine(line) is TextRow row)
                    yield return row;
            }

            if (onEoi() is Exception e)
                throw e;
        }
    }
}

#endif // !NO_ASYNC_STREAM
