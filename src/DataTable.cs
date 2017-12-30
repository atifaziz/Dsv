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
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    static partial class DataTableSetup
    {
        static readonly DataTableBuilder Nop = new DataTableBuilder(delegate { });

        public static readonly DataTableBuilder DefaultColumn =
            new DataTableBuilder((t, cos) =>
            {
                if (t.Columns.Count != 0)
                    return;
                AddDataColumn(t, cos, new DataColumn());
            });

        static void AddDataColumn(DataTable table,
                                  ICollection<DataColumnOptions> columnOptionsList,
                                  DataColumn column)
        {
            table.Columns.Add(column);
            columnOptionsList.Add(DataColumnOptions.Default);
        }

        public static DataTableBuilder Column(string name, Type dataType) =>
            Column(name, dataType, null);

        public static DataTableBuilder Column(string name, Type dataType, IFormatProvider provider) =>
            new DataTableBuilder((t, cos) => AddDataColumn(t, cos, new DataColumn(name, dataType)))
            + (provider != null ? AutoConvert(provider) : Nop);

        public static DataTableBuilder Column<T>(string name, Func<string, T> converter) =>
            Column(name, typeof(T)) + Converter(s => converter(s));

        public static DataTableBuilder StringColumn(string name) =>
            Column(name, typeof(string));

        public static DataTableBuilder Int32Column(string name, IFormatProvider provider) =>
            Int32Column(name, NumberStyles.Integer, provider);

        public static DataTableBuilder Int32Column(string name, NumberStyles styles, IFormatProvider provider) =>
            Column(name, s => int.Parse(s, styles, provider));

        public static DataTableBuilder SingleColumn(string name, IFormatProvider provider) =>
            SingleColumn(name, NumberStyles.Float | NumberStyles.AllowThousands, provider);

        public static DataTableBuilder SingleColumn(string name, NumberStyles styles, IFormatProvider provider) =>
            Column(name, s => float.Parse(s, styles, provider));

        public static DataTableBuilder DoubleColumn(string name, IFormatProvider provider) =>
            DoubleColumn(name, NumberStyles.Float | NumberStyles.AllowThousands, provider);

        public static DataTableBuilder DoubleColumn(string name, NumberStyles styles, IFormatProvider provider) =>
            Column(name, s => double.Parse(s, styles, provider));

        public static DataTableBuilder DecimalColumn(string name, IFormatProvider provider) =>
            DoubleColumn(name, NumberStyles.Number, provider);

        public static DataTableBuilder DecimalColumn(string name, NumberStyles styles, IFormatProvider provider) =>
            Column(name, s => decimal.Parse(s, styles, provider));

        public static DataTableBuilder DateTimeColumn(string name, IFormatProvider provider) =>
            DateTimeColumn(name, DateTimeStyles.None, provider);

        public static DataTableBuilder DateTimeColumn(string name, DateTimeStyles styles, IFormatProvider provider) =>
            Column(name, s => DateTime.Parse(s, provider, styles));

        public static DataTableBuilder DateTimeColumn(string name, string format, IFormatProvider provider) =>
            DateTimeColumn(name, format, DateTimeStyles.None, provider);

        public static DataTableBuilder DateTimeColumn(string name, string format, DateTimeStyles styles, IFormatProvider provider) =>
            Column(name, s => DateTime.ParseExact(s, format, provider, styles));

        public static DataTableBuilder DateTimeColumn(string name, string[] formats, IFormatProvider provider) =>
            DateTimeColumn(name, formats, DateTimeStyles.None, provider);

        public static DataTableBuilder DateTimeColumn(string name, string[] formats, DateTimeStyles styles, IFormatProvider provider) =>
            Column(name, s => DateTime.ParseExact(s, formats, provider, styles));

        public static DataTableBuilder DateTimeOffsetColumn(string name, IFormatProvider provider) =>
            DateTimeOffsetColumn(name, DateTimeStyles.None, provider);

        public static DataTableBuilder DateTimeOffsetColumn(string name, DateTimeStyles styles, IFormatProvider provider) =>
            Column(name, s => DateTimeOffset.Parse(s, provider, styles));

        public static DataTableBuilder DateTimeOffsetColumn(string name, string format, IFormatProvider provider) =>
            DateTimeOffsetColumn(name, format, DateTimeStyles.None, provider);

        public static DataTableBuilder DateTimeOffsetColumn(string name, string format, DateTimeStyles styles, IFormatProvider provider) =>
            Column(name, s => DateTimeOffset.ParseExact(s, format, provider, styles));

        public static DataTableBuilder DateTimeOffsetColumn(string name, string[] formats, IFormatProvider provider) =>
            DateTimeOffsetColumn(name, formats, DateTimeStyles.None, provider);

        public static DataTableBuilder DateTimeOffsetColumn(string name, string[] formats, DateTimeStyles styles, IFormatProvider provider) =>
            Column(name, s => DateTimeOffset.ParseExact(s, formats, provider, styles));

        public static DataTableBuilder Name(string value) =>
            DefaultColumn + DataTableBuilder.UpdateLastColumn(dc => dc.ColumnName = value);

        public static DataTableBuilder DataType(Type value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return DefaultColumn + DataTableBuilder.UpdateLastColumn(dc => dc.DataType = value);
        }

        public static IDataTableBuilder[] StringColumns(params string[] columns) =>
            columns.Select(name => (IDataTableBuilder) StringColumn(name)).ToArray();

        static DataTableBuilder Options(Func<DataColumnOptions, DataColumnOptions> modifier) =>
            DefaultColumn + DataTableBuilder.UpdateLastColumnOptions(modifier);

        public static DataTableBuilder AutoConvert(IFormatProvider provider) =>
            Options(o => o.UseAutoConversion(provider));

        public static DataTableBuilder Converter(Func<string, object> converter)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            return Options(o => o.WithConverter((_, row, i) => converter(row[i])));
        }

        public static DataTableBuilder Converter(Func<DataColumn, TextRow, int, object> converter)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            return Options(o => o.WithConverter(converter));
        }

        public static DataTableBuilder Binder(Func<DataColumn, TextRow, int> binder)
        {
            if (binder == null) throw new ArgumentNullException(nameof(binder));
            return Options(o => o.WithBinder(binder));
        }

        public static DataTableBuilder Header(string name) =>
            Header(name, StringComparison.OrdinalIgnoreCase);

        public static DataTableBuilder Header(string name, StringComparison comparison) =>
            Binder((_, hs) => hs.FindIndex(h => string.Equals(h, name, comparison)));

        public static DataTableBuilder ColumnNameComparison(StringComparison comparison) =>
            Binder((dc, hs) => hs.FindIndex(h => string.Equals(h, dc.ColumnName, comparison)));

        public static DataTableBuilder HeaderRegex(Regex regex) =>
            Binder((dc, hs) => hs.FindIndex(regex.IsMatch));

        public static DataTableBuilder HeaderRegex(string pattern) =>
            HeaderRegex(pattern, RegexOptions.None);

        public static DataTableBuilder HeaderRegex(string pattern, RegexOptions options) =>
            Binder((dc, hs) => hs.FindIndex(h => Regex.IsMatch(h, pattern, options)));
    }

    sealed partial class DataColumnOptions
    {
        public static readonly DataColumnOptions Default = new DataColumnOptions(null, null);

        public Func<DataColumn, TextRow, int> Binder { get; }
        public Func<DataColumn, TextRow, int, object> Converter { get; }

        DataColumnOptions(Func<DataColumn, TextRow, int> binder,
                          Func<DataColumn, TextRow, int, object> converter)
        {
            Converter = converter;
            Binder    = binder;
        }

        public DataColumnOptions WithConverter(Func<DataColumn, TextRow, int, object> value) =>
            value == Converter ? this : new DataColumnOptions(Binder, value);

        public DataColumnOptions UseAutoConversion(IFormatProvider provider) =>
            WithConverter((dc, row, i) => Convert.ChangeType(row[i], dc.DataType, provider));

        public DataColumnOptions WithBinder(Func<DataColumn, TextRow, int> value) =>
            value == Binder ? this : new DataColumnOptions(value, Converter);
    }

    partial interface IDataTableBuilder
    {
        void Build(DataTable table, IList<DataColumnOptions> columnOptions);
    }

    sealed partial class DataTableBuilder : IDataTableBuilder
    {
        public static readonly IDataTableBuilder Nop = new DataTableBuilder(delegate {});

        readonly Action<DataTable, IList<DataColumnOptions>> _builder;

        public static DataTableBuilder UpdateLastColumn(Action<DataColumn> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return new DataTableBuilder((t, _) => action(t.Columns[t.Columns.Count - 1]));
        }

        public static DataTableBuilder UpdateLastColumnOptions(Func<DataColumnOptions, DataColumnOptions> modifier)
        {
            if (modifier == null) throw new ArgumentNullException(nameof(modifier));
            return new DataTableBuilder((_, cos) =>
            {
                var index = cos.Count - 1;
                cos[index] = modifier(cos[index]);
            });
        }

        public DataTableBuilder(Action<DataTable, IList<DataColumnOptions>> builder) =>
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));

        public void Build(DataTable table, IList<DataColumnOptions> columnOptions) =>
            _builder(table, columnOptions);

        public static DataTableBuilder operator +(DataTableBuilder a, DataTableBuilder b) =>
            new DataTableBuilder((t, cos) => { a._builder(t, cos); b._builder(t, cos); });
    }

    static partial class Extensions
    {
        public static DataTable ToDataTable(this IEnumerable<TextRow> rows) =>
            ToDataTable(rows, DataTableBuilder.Nop);

        public static DataTable ToDataTable(this IEnumerable<TextRow> rows, IDataTableBuilder builder) =>
            rows.ToDataTable(false, builder);

        public static DataTable ToDataTable(this IEnumerable<TextRow> rows, bool headless) =>
            ToDataTable(rows, headless, DataTableBuilder.Nop);

        public static DataTable ToDataTable(this IEnumerable<TextRow> rows,
                                            bool headless,
                                            IDataTableBuilder builder)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            var table = new DataTable();
            var columnOptionsList = new List<DataColumnOptions>();

            builder.Build(table, columnOptionsList);

            var builtColumns =
                table.Columns.Cast<DataColumn>()
                             .Zip(columnOptionsList, (c, o) => (Column: c, Options: o))
                             .ToArray();

            using (var row = rows.GetEnumerator())
            {
                if (!row.MoveNext())
                    return table;

                var bindingz =
                    table.Columns.Count > 0
                    ? headless
                      ? builtColumns.Where(e => string.IsNullOrWhiteSpace(e.Column.Expression))
                                    .Select((e, i) => new
                                    {
                                        e.Column,
                                        Index = i,
                                        e.Options.Converter,
                                    })
                      : from e in builtColumns
                        where string.IsNullOrWhiteSpace(e.Column.Expression)
                        select new
                        {
                            e.Column,
                            Index = e.Options.Binder?.Invoke(e.Column, row.Current)
                                    ?? row.Current.FindIndex(h => string.Equals(h, e.Column.ColumnName, StringComparison.OrdinalIgnoreCase)),
                            e.Options.Converter,
                        }
                        into e
                        where e.Index >= 0
                        select e
                    : from i in Enumerable.Range(0, row.Current.Count)
                      select new
                      {
                          Column = new DataColumn(headless ? "Column" + (i + 1).ToString(CultureInfo.InvariantCulture)
                                                           : row.Current[i],
                                                  typeof(string)),
                          Index = i,
                          DataColumnOptions.Default.Converter,
                      };

                var bindings = bindingz.ToArray();

                if (table.Columns.Count == 0)
                {
                    foreach (var binding in bindings)
                        table.Columns.Add(binding.Column);
                }

                if (headless || row.MoveNext())
                {
                    do
                    {
                        var dr = table.NewRow();

                        foreach (var binding in bindings)
                        {
                            if (binding.Index < row.Current.Count)
                            {
                                dr[binding.Column]
                                    = binding.Converter != null
                                    ? binding.Converter(binding.Column, row.Current, binding.Index)
                                    : binding.Column.DataType == typeof(string)
                                    ? row.Current[binding.Index]
                                    : Convert.ChangeType(row.Current[binding.Index], binding.Column.DataType);
                            }
                        }

                        table.Rows.Add(dr);
                    }
                    while (row.MoveNext());
                }
            }

            return table;
        }
    }
}
