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
    using System.Collections.Generic;
    using System.Text;

    sealed partial class Dialect
    {
        public static readonly Dialect Csv = new Dialect(',');

        public char   Delimiter { get; }
        public char   Quote     { get; }
        public char   Escape    { get; }
        public string NewLine   { get; }

        public Dialect(char delimiter) :
            this(delimiter, '\"', '\"', "\n") { }

        Dialect(char delimiter, char quote, char escape, string newLine)
        {
            Delimiter = delimiter;
            Quote     = quote;
            Escape    = escape;
            NewLine   = newLine;
        }

        public Dialect WithDelimiter(char value) =>
            value == Delimiter ? this : new Dialect(value, Quote, Escape, NewLine);

        public Dialect WithQuote(char value) =>
            value == Quote ? this : new Dialect(Delimiter, value, Escape, NewLine);

        public Dialect WithEscape(char value) =>
            value == Escape ? this : new Dialect(Delimiter, Quote, value, NewLine);

        public Dialect WithNewLine(string value) =>
            value == NewLine ? this : new Dialect(Delimiter, Quote, Escape, value);
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

        public static IEnumerable<string[]> ParseCsv(this IEnumerable<string> lines) =>
            lines.ParseXsv(Dialect.Csv);

        /// <summary>
        /// Parses delimiter-separated values, like CSV, given a sequence
        /// of lines.
        /// </summary>

        public static IEnumerable<string[]> ParseXsv(this IEnumerable<string> lines,
            Dialect dialect) =>
            ParseXsv(lines, dialect.Delimiter,
                            dialect.Quote,
                            dialect.Escape,
                            dialect.NewLine);

        static IEnumerable<string[]> ParseXsv(IEnumerable<string> lines,
                                              char delimiter,
                                              char quote,
                                              char escape,
                                              string nl)
        {
            var ln = 0;
            var col = 0;
            var sb = new StringBuilder();
            var fields = new List<string>();
            var state = State.AtFieldStart;

            foreach (var line in lines)
            {
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
                    yield return fields.ToArray();
                    fields = new List<string>();
                    state = State.AtFieldStart;
                }
                else
                {
                    sb.Append(nl);
                }
            }
            if (sb.Length > 0)
                throw new FormatException($"Missing delimiter (line #{ln}, col #{col}).");
        }
    }
}
