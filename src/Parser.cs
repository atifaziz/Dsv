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
    using static XsvParserState;

    enum XsvParserState
    {
        AtFieldStart,
        InQuotedField,
        InField,
        Escaping,
        ExpectingDelimiter,
        QuoteQuote, // when escape char is also quote char
    }

    static partial class Parser
    {
        /// <summary>
        /// Parses CSV data from a sequnce of lines, allowing quoted
        /// fields and using two quotes to escape quote within a quoted
        /// field.
        /// </summary>

        public static IEnumerable<string[]> ParseCsv(this IEnumerable<string> lines) =>
            lines.ParseXsv(',', '"', '"', "\n");

        /// <summary>
        /// Parses delimiter-separated values, like CSV, given a sequence
        /// of lines.
        /// </summary>

        public static IEnumerable<string[]> ParseXsv(this IEnumerable<string> lines,
                                                     char delimiter,
                                                     char quote,
                                                     char escape,
                                                     string nl)
        {
            var ln = 0;
            var col = 0;
            var sb = new StringBuilder();
            var fields = new List<string>();
            var state = AtFieldStart;

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
                        case AtFieldStart:
                            if (ch == quote)
                            {
                                state = InQuotedField;
                            }
                            else if (ch == delimiter)
                            {
                                fields.Add(string.Empty);
                            }
                            else
                            {
                                sb.Append(ch);
                                state = InField;
                            }
                            break;
                        case InField:
                            if (ch == delimiter)
                            {
                                state = AtFieldStart;
                                fields.Add(sb.ToString());
                                sb.Length = 0;
                            }
                            else
                            {
                                sb.Append(ch);
                            }
                            break;
                        case ExpectingDelimiter:
                            if (char.IsWhiteSpace(ch))
                                break;
                            if (ch != delimiter)
                                throw new FormatException($"Missing delimiter (line #{ln}, col #{col}).");
                            state = AtFieldStart;
                            break;
                        case Escaping:
                            sb.Append(ch);
                            state = InQuotedField;
                            break;
                        case QuoteQuote:
                        {
                            if (ch == quote)
                            {
                                sb.Append(ch);
                                state = InQuotedField;
                            }
                            else
                            {
                                state = ExpectingDelimiter;
                                fields.Add(sb.ToString());
                                sb.Length = 0;
                                goto reswitch;
                            }
                            break;
                        }
                        case InQuotedField:
                            if (ch == quote)
                            {
                                if (quote == escape)
                                {
                                    state = QuoteQuote;
                                }
                                else
                                {
                                    state = ExpectingDelimiter;
                                    fields.Add(sb.ToString());
                                    sb.Length = 0;
                                }
                            }
                            else if (ch == escape)
                            {
                                state = Escaping;
                            }
                            else
                            {
                                sb.Append(ch);
                            }
                            break;
                    }
                }
                if (state != InQuotedField)
                {
                    if (state == AtFieldStart)
                    {
                        fields.Add(string.Empty);
                    }
                    else if (state == InField || state == QuoteQuote)
                    {
                        fields.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    yield return fields.ToArray();
                    fields = new List<string>();
                    state = AtFieldStart;
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
