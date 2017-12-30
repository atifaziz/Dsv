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

namespace Dsv.Tests
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Xunit;

    public sealed class DataTableTests
    {
        enum Choice { Yes, No, Maybe }

        public sealed class ToDataTable
        {
            [Fact]
            public void WithNullRowsThrows()
            {
                var e = Assert.Throws<ArgumentNullException>(() => Dsv.Extensions.ToDataTable(null));
                Assert.Equal("rows", e.ParamName);
            }

            [Fact]
            public void WithNullColumns()
            {
                var e = Assert.Throws<ArgumentNullException>(() => Enumerable.Empty<TextRow>().ToDataTable(null));
                Assert.Equal("builder", e.ParamName);
            }
        }

        public sealed class ParseCsvToDataTable
        {
            DataTable Table { get; }

            public ParseCsvToDataTable()
            {
                const string csv
                    = "col1,col2,col3\n"
                    + "1,foo,yes\n"
                    + "2,bar,no\n"
                    + "3,baz,maybe\n";

                Table = csv.SplitIntoLines().ParseCsv().ToDataTable();
            }

            [Fact]
            public void HasCorrectColumnCount() =>
                Assert.Equal(3, Table.Columns.Count);

            [Fact]
            public void HasCorrectColumnNames() =>
                Assert.Equal(new[] { "col1", "col2", "col3" },
                             from DataColumn dc in Table.Columns
                             select dc.ColumnName);

            [Fact]
            public void HasCorrectColumnDataTypes() =>
                Assert.Equal(Enumerable.Repeat(typeof(string), 3),
                             from DataColumn dc in Table.Columns
                             select dc.DataType);

            [Fact]
            public void HasCorrectRowCount() =>
                Assert.Equal(3, Table.Rows.Count);

            [Fact]
            public void HasCorrectRowData()
            {
                Assert.Equal(new object[] { "1", "foo", "yes"   }, Table.Rows[0].ItemArray);
                Assert.Equal(new object[] { "2", "bar", "no"    }, Table.Rows[1].ItemArray);
                Assert.Equal(new object[] { "3", "baz", "maybe" }, Table.Rows[2].ItemArray);
            }
        }

        public sealed class ParseHeadlessCsvToDataTable
        {
            DataTable Table { get; }

            public ParseHeadlessCsvToDataTable()
            {
                const string csv
                    = "1,foo,yes\n"
                    + "2,bar,no\n"
                    + "3,baz,maybe\n";

                Table = csv.SplitIntoLines().ParseCsv().ToDataTable(headless: true);
            }

            [Fact]
            public void HasCorrectColumnCount() =>
                Assert.Equal(3, Table.Columns.Count);

            [Fact]
            public void HasCorrectColumnNames() =>
                Assert.Equal(new[] { "Column1", "Column2", "Column3" },
                             from DataColumn dc in Table.Columns
                             select dc.ColumnName);

            [Fact]
            public void HasCorrectColumnDataTypes() =>
                Assert.Equal(Enumerable.Repeat(typeof(string), 3),
                             from DataColumn dc in Table.Columns
                             select dc.DataType);

            [Fact]
            public void HasCorrectRowCount() =>
                Assert.Equal(3, Table.Rows.Count);

            [Fact]
            public void HasCorrectRowData()
            {
                Assert.Equal(new object[] { "1", "foo", "yes"   }, Table.Rows[0].ItemArray);
                Assert.Equal(new object[] { "2", "bar", "no"    }, Table.Rows[1].ItemArray);
                Assert.Equal(new object[] { "3", "baz", "maybe" }, Table.Rows[2].ItemArray);
            }
        }

        public sealed class ParseHeadlessCsvToDataTableWithSetup
        {
            DataTable Table { get; }

            public ParseHeadlessCsvToDataTableWithSetup()
            {
                const string csv
                    = "1,foo,yes\n"
                    + "2,bar,no\n"
                    + "3,baz,maybe\n";

                Table = csv.SplitIntoLines()
                           .ParseCsv()
                           .ToDataTable(true,
                               DataTableSetup.Int32Column("num", CultureInfo.InvariantCulture)
                               + DataTableSetup.StringColumn("str")
                               + DataTableSetup.Column("choice", s => Enum.Parse<Choice>(s, ignoreCase: true)));
            }

            [Fact]
            public void HasCorrectColumnCount() =>
                Assert.Equal(3, Table.Columns.Count);

            [Fact]
            public void HasCorrectColumnNames() =>
                Assert.Equal(new[] { "num", "str", "choice" },
                             from DataColumn dc in Table.Columns
                             select dc.ColumnName);

            [Fact]
            public void HasCorrectColumnDataTypes() =>
                Assert.Equal(new[] { typeof(int), typeof(string), typeof(Choice) },
                             from DataColumn dc in Table.Columns
                             select dc.DataType);

            [Fact]
            public void HasCorrectRowCount() =>
                Assert.Equal(3, Table.Rows.Count);

            [Fact]
            public void HasCorrectRowData()
            {
                var i = 0;
                Assert.Equal(new object[] { 1, "foo", (int) Choice.Yes   }, Table.Rows[i++].ItemArray);
                Assert.Equal(new object[] { 2, "bar", (int) Choice.No    }, Table.Rows[i++].ItemArray);
                Assert.Equal(new object[] { 3, "baz", (int) Choice.Maybe }, Table.Rows[i++].ItemArray);
            }
        }

        public sealed class ParseCsvToDataTableWithSetup
        {
            DataTable Table { get; }

            public ParseCsvToDataTableWithSetup()
            {
                const string data
                    = "num,text,ignored,choice,month\n"
                    + "1,foo,NA,yes,Jan-11\n"
                    + "2,bar,NA,no,Mar-15\n"
                    + "3,baz,NA,maybe,Sep-17\n";

                Table =
                    data.SplitIntoLines()
                        .ParseCsv()
                        .ToDataTable(
                            DataTableSetup.Column("choice", s => Enum.Parse<Choice>(s, ignoreCase: true))
                            + DataTableSetup.Int32Column("num", CultureInfo.InvariantCulture)
                            + DataTableSetup.StringColumn("str")
                                            .Header("text")
                            + DataTableSetup.DateTimeColumn("date", "MMM-yy", CultureInfo.InvariantCulture)
                                            .HeaderRegex(@"\b(DATE|MONTH)\b", RegexOptions.IgnoreCase));
            }

            [Fact]
            public void HasCorrectColumnCount() =>
                Assert.Equal(4, Table.Columns.Count);

            [Fact]
            public void HasCorrectColumnNames() =>
                Assert.Equal(new[] { "choice", "num", "str", "date" },
                             from DataColumn dc in Table.Columns
                             select dc.ColumnName);

            [Fact]
            public void HasCorrectColumnDataTypes() =>
                Assert.Equal(new[] { typeof(Choice), typeof(int), typeof(string), typeof(DateTime) },
                             from DataColumn dc in Table.Columns
                             select dc.DataType);

            [Fact]
            public void HasCorrectRowCount() =>
                Assert.Equal(3, Table.Rows.Count);

            [Fact]
            public void HasCorrectRowData()
            {
                var i = 0;
                Assert.Equal(new object[] { (int) Choice.Yes  , 1, "foo", new DateTime(2011, 1, 1) }, Table.Rows[i++].ItemArray);
                Assert.Equal(new object[] { (int) Choice.No   , 2, "bar", new DateTime(2015, 3, 1) }, Table.Rows[i++].ItemArray);
                Assert.Equal(new object[] { (int) Choice.Maybe, 3, "baz", new DateTime(2017, 9, 1) }, Table.Rows[i++].ItemArray);
            }
        }
    }
}
