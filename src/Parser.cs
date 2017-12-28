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

        public Format Unquoted() => WithQuote(null).WithEscape('\0');

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

        string[] Fields => _fields ?? Array.Empty<string>();

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
        public int FindIndex(Predicate<string> predicate) => Array.FindIndex(Fields, predicate);
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

        /// <summary>
        /// Parses CSV data from a sequnce of lines, allowing quoted
        /// fields and using two quotes to escape quote within a quoted
        /// field.
        /// </summary>

        public static IEnumerable<TextRow> ParseCsv(this IEnumerable<string> lines) =>
            lines.ParseDsv(Format.Csv);

        public static IEnumerable<TextRow> ParseCsv(this IEnumerable<string> lines, Func<string, bool> rowFilter) =>
            lines.ParseDsv(Format.Csv, rowFilter);

        /// <summary>
        /// Parses delimiter-separated values, like CSV, given a sequence
        /// of lines.
        /// </summary>

        public static IEnumerable<TextRow> ParseDsv(this IEnumerable<string> lines,
            Format format) =>
            ParseDsv(lines, format, _ => false);

        public static IEnumerable<TextRow> ParseDsv(this IEnumerable<string> lines,
            Format format, Func<string, bool> rowFilter)
        {
            var (onLine, onEoi) = Create(format, rowFilter);

            foreach (var line in lines)
            {
                if (onLine(line) is TextRow row)
                    yield return row;
            }

            if (onEoi() is Exception e)
                throw e;
        }

        static (Func<string, TextRow?> OnLine, Func<Exception> OnEoi)
            Create(Format format, Func<string, bool> rowFilter)
        {
            var delimiter = format.Delimiter;
            var quote     = format.Quote;
            var escape    = format.Escape;
            var nl        = format.NewLine;

            var ln     = 0;
            var rln    = 0;
            var col    = 0;
            var sb     = new StringBuilder();
            var fields = new List<string>();
            var state  = State.AtFieldStart;

            TextRow? OnLine(string line)
            {
                if (state == State.AwaitNextRow)
                {
                    rln = ln;
                    fields = new List<string>();
                    state = State.AtFieldStart;
                }
                ln++;
                if (state != State.InQuotedField)
                {
                    rln++;
                    if (rowFilter(line))
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
                            else if (ch == delimiter)
                            {
                                fields.Add(string.Empty);
                            }
                            else
                            {
                                sb.Append(ch);
                                state = State.InField;
                            }
                            break;
                        case State.InField:
                            if (ch == delimiter)
                            {
                                state = State.AtFieldStart;
                                fields.Add(sb.ToString());
                                sb.Length = 0;
                            }
                            else
                            {
                                sb.Append(ch);
                            }
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
                            state = State.InQuotedField;
                            break;
                        case State.QuoteQuote:
                            {
                                if (ch == quote)
                                {
                                    sb.Append(ch);
                                    state = State.InQuotedField;
                                }
                                else
                                {
                                    state = State.ExpectingDelimiter;
                                    fields.Add(sb.ToString());
                                    sb.Length = 0;
                                    goto reswitch;
                                }
                                break;
                            }
                        case State.InQuotedField:
                            if (ch == quote)
                            {
                                if (quote == escape)
                                {
                                    state = State.QuoteQuote;
                                }
                                else
                                {
                                    state = State.ExpectingDelimiter;
                                    fields.Add(sb.ToString());
                                    sb.Length = 0;
                                }
                            }
                            else if (ch == escape)
                            {
                                state = State.Escaping;
                            }
                            else
                            {
                                sb.Append(ch);
                            }
                            break;
                        default:
                            throw new Exception("XSV parsing implementation error.");
                    }
                }

                if (state != State.InQuotedField)
                {
                    if (state == State.AtFieldStart)
                    {
                        fields.Add(string.Empty);
                    }
                    else if (state == State.InField || state == State.QuoteQuote)
                    {
                        fields.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    state = State.AwaitNextRow;
                    return new TextRow(rln, fields.ToArray());
                }

                sb.Append(nl);
                return null;
            }

            Exception OnEoi() =>
                sb.Length > 0
                ? new FormatException($"Unclosed quoted field (line #{ln}, col #{col}).")
                : null;

            return (OnLine, OnEoi);
        }
    }
}
