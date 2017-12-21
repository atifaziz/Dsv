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

namespace Yax.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using MoreLinq;
    using Xunit;

    public class ParserTests
    {
        [Theory]
        [MemberData(nameof(GetData))]
        public void Parse(char delimiter, char quote, char escape, string newline, bool skipBlanks,
                          IEnumerable<string> lines, IEnumerable<string[]> rows)
        {
            using (var row = rows.GetEnumerator())
            {
                var dialect = new Dialect(delimiter).WithQuote(quote)
                                                    .WithEscape(escape)
                                                    .WithNewLine(newline);

                if (skipBlanks)
                    dialect = dialect.SkipBlankRows();

                foreach (var fields in lines.ParseXsv(dialect))
                {
                    Assert.True(row.MoveNext(), "Source has too many rows.");
                    Assert.Equal(row.Current, fields);
                }
                Assert.False(row.MoveNext(), "Source has too few rows.");
            }
        }

        public static TheoryData<char, char, char, string, bool, IEnumerable<string>, IEnumerable<string[]>> GetData()
        {
            var type = MethodBase.GetCurrentMethod().DeclaringType;

            var config = new[] { "delimiter", "quote", "escape", "newline", "blanks" };

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
                select
                    config
                        .Select(p => Regex.Match(e.Suppositions, $@"(?<=\b{Regex.Escape(p)}( +is| *[=:]) *`)[^`]+(?=`)", RegexOptions.ExplicitCapture))
                        .Select(m => m.Success ? m.Value : null)
                        .Fold((d, q, esc, nl, blanks) => new
                        {
                            Delimiter  = d?[0] ?? ',',
                            Quote      = q?[0] ?? '"',
                            Escape     = esc?[0] ?? '"',
                            NewLine    = nl != null
                                       ? Regex.Replace(nl, @"\\[rn]", m => m.Value[1] == 'r' ? "\r"
                                                                         : m.Value[1] == 'n' ? "\n"
                                                                         : throw new FormatException())
                                       : "\n",
                            SkipBlanks = "skip".Equals(blanks, StringComparison.OrdinalIgnoreCase),
                            e.Input,
                            Expected   = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<string[]>>(string.Join(Environment.NewLine, e.Expected))
                        })
                into e
                select (e.Delimiter, e.Quote, e.Escape, e.NewLine, e.SkipBlanks, e.Input, e.Expected);

            return data.ToTheoryData();
        }

    }
}
