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
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    sealed partial class Format
    {
        public static readonly Format Csv = new Format(',');
        public static readonly Format UnquotedCsv = Csv.Unquoted();

        public char   Delimiter { get; }
        public char?  Quote     { get; }
        public char   Escape    { get; }
        public string NewLine   { get; }

        public Format(char delimiter) :
            this(delimiter, '\"', '\"', "\n") { }

        Format(char delimiter, char? quote, char escape, string newLine)
        {
            Delimiter    = delimiter;
            Quote        = quote;
            Escape       = escape;
            NewLine      = newLine;
        }

        public Format WithDelimiter(char value) =>
            value == Delimiter ? this : new Format(value, Quote, Escape, NewLine);

        public Format Unquoted() => WithQuote(null).WithEscape('\\');

        public Format WithQuote(char? value) =>
            value == Quote ? this : new Format(Delimiter, value, Escape, NewLine);

        public Format WithEscape(char value) =>
            value == Escape ? this : new Format(Delimiter, Quote, value, NewLine);

        public Format WithNewLine(string value) =>
            value == NewLine ? this : new Format(Delimiter, Quote, Escape, value);
    }

    partial struct TextRow : IList<string>, IReadOnlyList<string>
    {
        readonly string[] _fields;

        internal TextRow(int lineNumber, string[] fields)
        {
            LineNumber = lineNumber;
            _fields = fields ?? throw new ArgumentNullException(nameof(fields));
        }

        public int LineNumber { get; }

        string[] Fields => _fields ??
        #if NETSTANDARD1_0
                           (_zeroFields ?? (_zeroFields = new string[0]));
                           static string[] _zeroFields;
        #else
                           Array.Empty<string>();
        #endif

        public string this[int index] => Fields[index];

        string IList<string>.this[int index]
        {
            get => this[index];
            set => throw ReadOnlyError();
        }

        public IEnumerator<string> GetEnumerator() =>
            ((IEnumerable<string>) Fields).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        public int IndexOf(string item) => Array.IndexOf(Fields, item);
        public bool Contains(string item) => IndexOf(item) >= 0;
        public void CopyTo(string[] array, int arrayIndex) => Fields.CopyTo(array, arrayIndex);

        public int Count => Fields?.Length ?? 0;
        bool ICollection<string>.IsReadOnly => true;

        static Exception ReadOnlyError() =>
            new InvalidOperationException("Collection is read-only.");

        void ICollection<string>.Add(string item)         => throw ReadOnlyError();
        void ICollection<string>.Clear()                  => throw ReadOnlyError();
        bool ICollection<string>.Remove(string item)      => throw ReadOnlyError();
        void IList<string>.Insert(int index, string item) => throw ReadOnlyError();
        void IList<string>.RemoveAt(int index)            => throw ReadOnlyError();
    }

    static partial class Parser
    {
        enum State
        {
            AtFieldStart,
            InQuotedField,
            InField,
            Escaping,
            ExpectingDelimiter,
            QuoteQuote, // when escape char is also quote char
            AwaitNextRow
        }

        public static IEnumerable<TRow> ParseCsv<THead, TRow>(this IEnumerable<string> lines,
            Func<TextRow, THead> headSelector,
            Func<THead, TextRow, TRow> rowSelector) =>
            lines.ParseDsv(Format.Csv, headSelector, rowSelector);

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
        /// Parses CSV data from a sequnce of lines, allowing quoted
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
            ParseDsv(lines, format, _ => false);

        public static IEnumerable<TextRow> ParseDsv(this IEnumerable<string> lines,
            Format format, Func<string, bool> lineFilter)
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

        static (Func<string, TextRow?> OnLine, Func<Exception> OnEoi)
            Create(Format format, Func<string, bool> lineFilter)
        {
            var delimiter = format.Delimiter;
            var quote     = format.Quote;
            var escape    = format.Escape;
            var nl        = format.NewLine;

            var ln     = 0;
            var rln    = 0;
            var col    = 0;
            var sb     = new StringBuilder();
            var size   = 4;
            var fc     = 0;
            var fields = new string[size];
            var state  = State.AtFieldStart;

            void CommitField(State nextState)
            {
                if (fc == size)
                    Array.Resize(ref fields, size += 4);
                fields[fc++] = sb.ToString();
                if (sb.Length > 0)
                    sb.Length = 0;
                state = nextState;
            }

            TextRow? OnLine(string line)
            {
                if (state == State.AwaitNextRow)
                {
                    rln = ln;
                    fields = new string[size];
                    fc = 0;
                    state = State.AtFieldStart;
                }
                ln++;
                if (state != State.InQuotedField && state != State.Escaping)
                {
                    rln++;
                    if (lineFilter(line))
                        return null;
                }
                col = 0;
                foreach (var ch in line)
                {
                    col++;
                    reswitch:
                    switch (state)
                    {
                        case State.AtFieldStart:
                            if (ch == quote)
                            {
                                state = State.InQuotedField;
                            }
                            else if (ch == escape && quote == null)
                            {
                                state = State.Escaping;
                            }
                            else if (ch == delimiter)
                            {
                                CommitField(state);
                            }
                            else
                            {
                                sb.Append(ch);
                                state = State.InField;
                            }
                            break;

                        case State.InField:
                            if (ch == delimiter)
                                CommitField(State.AtFieldStart);
                            else if (ch == escape && quote == null)
                                state = State.Escaping;
                            else
                                sb.Append(ch);
                            break;

                        case State.ExpectingDelimiter:
                            if (char.IsWhiteSpace(ch))
                                break;
                            if (ch != delimiter)
                                throw new FormatException($"Missing delimiter (line #{ln}, col #{col}).");
                            state = State.AtFieldStart;
                            break;

                        case State.Escaping:
                            sb.Append(ch);
                            state = quote != null
                                   ? State.InQuotedField
                                   : State.InField;
                            break;

                        case State.QuoteQuote:
                            if (ch == quote)
                            {
                                sb.Append(ch);
                                state = State.InQuotedField;
                            }
                            else
                            {
                                CommitField(State.ExpectingDelimiter);
                                goto reswitch;
                            }
                            break;

                        case State.InQuotedField:
                            if (ch == quote)
                            {
                                if (quote == escape)
                                    state = State.QuoteQuote;
                                else
                                    CommitField(State.ExpectingDelimiter);
                            }
                            else if (ch == escape)
                                state = State.Escaping;
                            else
                                sb.Append(ch);
                            break;

                        default:
                            throw new Exception("XSV parsing implementation error.");
                    }
                }

                switch (state)
                {
                    case State.Escaping:
                    case State.InQuotedField:
                        sb.Append(nl ?? throw new FormatException($"Unclosed quoted field (line #{ln}, col #{col + 1})."));
                        return null;

                    case State.AtFieldStart:
                    case State.InField:
                    case State.QuoteQuote:
                        CommitField(State.AwaitNextRow);
                        break;

                    default:
                        state = State.AwaitNextRow;
                        break;
                }

                if (fc < size)
                {
                    Array.Resize(ref fields, fc);
                    size = fc;
                }
                return new TextRow(rln, fields);
            }

            Exception OnEoi() =>
                sb.Length > 0
                ? new FormatException($"Unclosed quoted field (line #{ln}, col #{col}).")
                : null;

            return (OnLine, OnEoi);
        }
    }
}
