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
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using Xunit;

    public sealed class DataTableSetupTests
    {
        static void AssertEqual(DataColumn expected, DataColumn actual, int ordinal = 0)
        {
            Assert.Equal(expected.ColumnName        , actual.ColumnName        );
            Assert.Equal(expected.DataType          , actual.DataType          );
            Assert.Equal(expected.AllowDBNull       , actual.AllowDBNull       );
            Assert.Equal(expected.AutoIncrement     , actual.AutoIncrement     );
            Assert.Equal(expected.AutoIncrementSeed , actual.AutoIncrementSeed );
            Assert.Equal(expected.AutoIncrementStep , actual.AutoIncrementStep );
            Assert.Equal(expected.Caption           , actual.Caption           );
            Assert.Equal(expected.ColumnMapping     , actual.ColumnMapping     );
            Assert.Equal(expected.DateTimeMode      , actual.DateTimeMode      );
            Assert.Equal(expected.DefaultValue      , actual.DefaultValue      );
            Assert.Equal(expected.Expression        , actual.Expression        );
            Assert.Equal(expected.MaxLength         , actual.MaxLength         );
            Assert.Equal(expected.Namespace         , actual.Namespace         );
            Assert.Equal(expected.Prefix            , actual.Prefix            );
            Assert.Equal(expected.ReadOnly          , actual.ReadOnly          );
            Assert.Equal(expected.Unique            , actual.Unique            );

            Assert.Equal(ordinal, actual.Ordinal);
            Assert.NotNull(actual.Table);
        }

        static (DataColumn Column, DataColumnOptions Options) Build(IDataTableBuilder builder)
        {
            var table = new DataTable();
            var columnOptionsList = new List<DataColumnOptions>();
            builder.Build(table, columnOptionsList);
            return (table.Columns.Cast<DataColumn>().Single(),
                    columnOptionsList.Single());
        }

        public sealed class Column
        {
            [Fact]
            public void WithNameWithTypeReturnsSpecifiedDataColumnBuilder()
            {
                const string name = "foo";
                var type = typeof(int);
                var (column, options) = Build(DataTableSetup.Column(name, type));
                AssertEqual(new DataColumn(name, type), column);
                Assert.Same(DataColumnOptions.Default, options);
            }

            [Fact]
            public void WithNameWithTypeWithFormatProviderReturnsSpecifiedDataColumnBuilder()
            {
                const string name = "foo";
                var type = typeof(decimal);
                var culture = new CultureInfo(string.Empty) { NumberFormat = { NumberGroupSeparator = "_" } };
                var (column, options) = Build(DataTableSetup.Column(name, type, culture));
                AssertEqual(new DataColumn(name, type), column);
                var n = options.Converter(column, "1_234_567".SplitIntoLines().ParseCsv().Single(), 0);
                Assert.Equal(1_234_567m, n);
            }

            [Fact]
            public void WithNameWithTypeWithConverterReturnsSpecifiedDataColumnBuilder()
            {
                const string name = "foo";
                var called = false;
                int Converter(string s)
                {
                    called = true;
                    return int.Parse(s);
                }
                var (column, options) = Build(DataTableSetup.Column(name, Converter));
                AssertEqual(new DataColumn(name, typeof(int)), column);
                var converter = options.Converter;
                Assert.NotNull(converter);
                Assert.Equal(42, converter(column, "42".SplitIntoLines().ParseCsv().Single(), 0));
                Assert.True(called);
            }
        }

        [Fact]
        public void StringReturnsSpecifiedDataColumnBuilder()
        {
            const string name = "foo";
            var (column, options) = Build(DataTableSetup.StringColumn(name));
            AssertEqual(new DataColumn(name), column);
            Assert.Same(DataColumnOptions.Default, options);
        }
    }
}
