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
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    sealed partial class Dialect
    {
        public static readonly Dialect Csv = new Dialect(',');

        public char   Delimiter { get; }
        public char   Quote     { get; }
        public char   Escape    { get; }
        public string NewLine   { get; }

        public Func<string, bool> RowFilter { get; }

        public Dialect(char delimiter) :
            this(delimiter, '\"', '\"', "\n", null) { }

        Dialect(char delimiter, char quote, char escape, string newLine,
                Func<string, bool> rowFilter)
        {
            Delimiter    = delimiter;
            Quote        = quote;
            Escape       = escape;
            NewLine      = newLine;
            RowFilter = rowFilter;
        }

        public Dialect WithDelimiter(char value) =>
            value == Delimiter ? this : new Dialect(value, Quote, Escape, NewLine, RowFilter);

        public Dialect WithQuote(char value) =>
            value == Quote ? this : new Dialect(Delimiter, value, Escape, NewLine, RowFilter);

        public Dialect WithEscape(char value) =>
            value == Escape ? this : new Dialect(Delimiter, Quote, value, NewLine, RowFilter);

        public Dialect WithNewLine(string value) =>
            value == NewLine ? this : new Dialect(Delimiter, Quote, Escape, value, RowFilter);

        public Dialect WithRowFilter(Func<string, bool> value) =>
            value == RowFilter ? this : new Dialect(Delimiter, Quote, Escape, NewLine, value);

        public Dialect OrWithRowFilter(Func<string, bool> value) =>
            value == RowFilter ? this : new Dialect(Delimiter, Quote, Escape, NewLine,
                                                    RowFilter == null
                                                    ? value
                                                    : (s => RowFilter(s) || value(s)));

        public Dialect SkipBlankRows() =>
            OrWithRowFilter(string.IsNullOrWhiteSpace);
    }

    partial struct TextRow : IList<string>, IReadOnlyList<string>
    {
        readonly string[] _fields;

        internal TextRow(string[] fields) =>
            _fields = fields ?? throw new ArgumentNullException(nameof(fields));

        public string[] Fields => _fields ?? Array.Empty<string>();

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
        }

        /// <summary>
        /// Parses CSV data from a sequnce of lines, allowing quoted
        /// fields and using two quotes to escape quote within a quoted
        /// field.
        /// </summary>

        public static IEnumerable<TextRow> ParseCsv(this IEnumerable<string> lines) =>
            lines.ParseXsv(Dialect.Csv);

        /// <summary>
        /// Parses delimiter-separated values, like CSV, given a sequence
        /// of lines.
        /// </summary>

        public static IEnumerable<TextRow> ParseXsv(this IEnumerable<string> lines,
            Dialect dialect) =>
            ParseXsv(lines, dialect.Delimiter,
                            dialect.Quote,
                            dialect.Escape,
                            dialect.NewLine,
                            dialect.RowFilter ?? (_ => false));

        static IEnumerable<TextRow> ParseXsv(IEnumerable<string> lines,
                                              char delimiter,
                                              char quote,
                                              char escape,
                                              string nl,
                                              Func<string, bool> rowFilter)
        {
            var ln = 0;
            var col = 0;
            var sb = new StringBuilder();
            var fields = new List<string>();
            var state = State.AtFieldStart;

            foreach (var line in lines)
            {
                if (state != State.InQuotedField && rowFilter(line))
                    continue;
                ln++;
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
                    yield return new TextRow(fields.ToArray());
                    fields = new List<string>();
                    state = State.AtFieldStart;
                }
                else
                {
                    sb.Append(nl);
                }
            }
            if (sb.Length > 0)
                throw new FormatException($"Unclosed quoted field (line #{ln}, col #{col}).");
        }
    }
}
