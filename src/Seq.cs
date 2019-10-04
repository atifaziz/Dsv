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

    static partial class Parser
    {
        public static IEnumerable<(T Header, TextRow Row)>
            ParseCsv<T>(this IEnumerable<string> lines,
                        Func<TextRow, T> headSelector) =>
            lines.ParseDsv(Format.Csv, headSelector, ValueTuple.Create);

        public static IEnumerable<TRow> ParseCsv<THead, TRow>(this IEnumerable<string> lines,
            Func<TextRow, THead> headSelector,
            Func<THead, TextRow, TRow> rowSelector) =>
            lines.ParseDsv(Format.Csv, headSelector, rowSelector);

        public static IEnumerable<(T Header, TextRow Row)>
            ParseDsv<T>(this IEnumerable<string> lines,
                        Format format,
                        Func<TextRow, T> headSelector) =>
            lines.ParseDsv(format, _ => false, headSelector);

        public static IEnumerable<(T Header, TextRow Row)>
            ParseDsv<T>(this IEnumerable<string> lines,
                        Format format,
                        Func<string, bool> lineFilter,
                        Func<TextRow, T> headSelector) =>
            lines.ParseDsv(format, lineFilter, headSelector, ValueTuple.Create);

        public static IEnumerable<TRow> ParseDsv<THead, TRow>(this IEnumerable<string> lines,
            Format format,
            Func<TextRow, THead> headSelector,
            Func<THead, TextRow, TRow> rowSelector) =>
            lines.ParseDsv(format, _ => false, headSelector, rowSelector);

        public static IEnumerable<TRow> ParseDsv<THead, TRow>(this IEnumerable<string> lines,
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

            return _(); IEnumerable<TRow> _()
            {
                using (var row = lines.ParseDsv(format, lineFilter).GetEnumerator())
                {
                    if (!row.MoveNext())
                        yield break;

                    var head = headSelector(row.Current);

                    while (row.MoveNext())
                        yield return rowSelector(head, row.Current);
                }
            }
        }

        /// <summary>
        /// Parses CSV data from a sequence of lines, allowing quoted
        /// fields and using two quotes to escape quote within a quoted
        /// field.
        /// </summary>

        public static IEnumerable<TextRow> ParseCsv(this IEnumerable<string> lines) =>
            lines.ParseDsv(Format.Csv);

        public static IEnumerable<TextRow> ParseCsv(this IEnumerable<string> lines, Func<string, bool> lineFilter) =>
            lines.ParseDsv(Format.Csv, lineFilter);

        /// <summary>
        /// Parses delimiter-separated values, like CSV, given a sequence
        /// of lines.
        /// </summary>

        public static IEnumerable<TextRow> ParseDsv(this IEnumerable<string> lines,
            Format format) =>
            lines.ParseDsv(format, (string _) => false);

        public static IEnumerable<TextRow> ParseDsv(this IEnumerable<string> lines,
            Format format, Func<string, bool> lineFilter)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            if (format == null) throw new ArgumentNullException(nameof(format));
            if (lineFilter == null) throw new ArgumentNullException(nameof(lineFilter));

            return _(); IEnumerable<TextRow> _()
            {
                var (onLine, onEoi) = Create(format, lineFilter);

                foreach (var line in lines)
                {
                    if (onLine(line) is TextRow row)
                        yield return row;
                }

                if (onEoi() is Exception e)
                    throw e;
            }
        }
    }
}
