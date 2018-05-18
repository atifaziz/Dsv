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
        public static DataColumnSetup Column(string name, Type dataType) =>
            Column(name, dataType, null);

        public static DataColumnSetup Column(string name, Type dataType, IFormatProvider provider)
        {
            var setup = new DataColumnSetup(() => new DataColumn(name, dataType));
            return provider != null ? setup.AutoConvert(provider) : setup;
        }

        public static DataColumnSetup Column<T>(string name, Func<string, T> converter) =>
            Column(name, typeof(T)).Converter(s => converter(s));

        public static DataColumnSetup StringColumn(string name) =>
            Column(name, typeof(string));

        public static DataColumnSetup Int32Column(string name, IFormatProvider provider) =>
            Int32Column(name, NumberStyles.Integer, provider);

        public static DataColumnSetup Int32Column(string name, NumberStyles styles,
            IFormatProvider provider) =>
            Column(name, s => int.Parse(s, styles, provider));

        public static DataColumnSetup Int64Column(string name, IFormatProvider provider) =>
            Int64Column(name, NumberStyles.Integer, provider);

        public static DataColumnSetup Int64Column(string name, NumberStyles styles,
            IFormatProvider provider) =>
            Column(name, s => long.Parse(s, styles, provider));

        public static DataColumnSetup SingleColumn(string name, IFormatProvider provider) =>
            SingleColumn(name, NumberStyles.Float | NumberStyles.AllowThousands, provider);

        public static DataColumnSetup SingleColumn(string name, NumberStyles styles,
            IFormatProvider provider) =>
            Column(name, s => float.Parse(s, styles, provider));

        public static DataColumnSetup DoubleColumn(string name, IFormatProvider provider) =>
            DoubleColumn(name, NumberStyles.Float | NumberStyles.AllowThousands, provider);

        public static DataColumnSetup DoubleColumn(string name, NumberStyles styles,
            IFormatProvider provider) =>
            Column(name, s => double.Parse(s, styles, provider));

        public static DataColumnSetup DecimalColumn(string name, IFormatProvider provider) =>
            DoubleColumn(name, NumberStyles.Number, provider);

        public static DataColumnSetup DecimalColumn(string name, NumberStyles styles,
            IFormatProvider provider) =>
            Column(name, s => decimal.Parse(s, styles, provider));

        public static DataColumnSetup DateTimeColumn(string name, IFormatProvider provider) =>
            DateTimeColumn(name, DateTimeStyles.None, provider);

        public static DataColumnSetup DateTimeColumn(string name, DateTimeStyles styles,
            IFormatProvider provider) =>
            Column(name, s => DateTime.Parse(s, provider, styles));

        public static DataColumnSetup DateTimeColumn(string name, string format,
            IFormatProvider provider) =>
            DateTimeColumn(name, format, DateTimeStyles.None, provider);

        public static DataColumnSetup DateTimeColumn(string name, string format,
            DateTimeStyles styles, IFormatProvider provider) =>
            Column(name, s => DateTime.ParseExact(s, format, provider, styles));

        public static DataColumnSetup DateTimeColumn(string name, string[] formats,
            IFormatProvider provider) =>
            DateTimeColumn(name, formats, DateTimeStyles.None, provider);

        public static DataColumnSetup DateTimeColumn(string name, string[] formats,
            DateTimeStyles styles, IFormatProvider provider) =>
            Column(name, s => DateTime.ParseExact(s, formats, provider, styles));

        public static DataColumnSetup
            DateTimeOffsetColumn(string name, IFormatProvider provider) =>
            DateTimeOffsetColumn(name, DateTimeStyles.None, provider);

        public static DataColumnSetup DateTimeOffsetColumn(string name, DateTimeStyles styles,
            IFormatProvider provider) =>
            Column(name, s => DateTimeOffset.Parse(s, provider, styles));

        public static DataColumnSetup DateTimeOffsetColumn(string name, string format,
            IFormatProvider provider) =>
            DateTimeOffsetColumn(name, format, DateTimeStyles.None, provider);

        public static DataColumnSetup DateTimeOffsetColumn(string name, string format,
            DateTimeStyles styles, IFormatProvider provider) =>
            Column(name, s => DateTimeOffset.ParseExact(s, format, provider, styles));

        public static DataColumnSetup DateTimeOffsetColumn(string name, string[] formats,
            IFormatProvider provider) =>
            DateTimeOffsetColumn(name, formats, DateTimeStyles.None, provider);

        public static DataColumnSetup DateTimeOffsetColumn(string name, string[] formats,
            DateTimeStyles styles, IFormatProvider provider) =>
            Column(name, s => DateTimeOffset.ParseExact(s, formats, provider, styles));

        public static IDataTableBuilder[] StringColumns(params string[] columns) =>
            columns.Select(name => (IDataTableBuilder) StringColumn(name)).ToArray();
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

    sealed partial class DataColumnSetup : IDataTableBuilder, IDataColumnBuilder
    {
        readonly Func<DataColumn> _factory;
        readonly IDataColumnBuilder _builder;

        public DataColumnSetup(Func<DataColumn> factory) :
            this(factory, DataColumnBuilder.Nop, DataColumnOptions.Default) {}

        public DataColumnSetup(Func<DataColumn> factory,
                               IDataColumnBuilder builder,
                               DataColumnOptions options)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Options  = options ?? throw new ArgumentNullException(nameof(options));
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public DataColumnOptions Options { get; }

        public DataColumnSetup WithOptions(DataColumnOptions options) =>
            new DataColumnSetup(_factory, _builder, options);

        public DataColumnSetup AddBuilder(IDataColumnBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return new DataColumnSetup(_factory,
                                       new DataColumnBuilder(dc =>
                                       {
                                           _builder.Build(dc);
                                           builder.Build(dc);
                                       }),
                                       Options);
        }

        void IDataColumnBuilder.Build(DataColumn column) =>
            _builder.Build(column ?? throw new ArgumentNullException(nameof(column)));

        void IDataTableBuilder.Build(DataTable table, ICollection<DataColumnOptions> columnOptions)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (columnOptions == null) throw new ArgumentNullException(nameof(columnOptions));

            var column = _factory();
            _builder.Build(column);
            table.Columns.Add(column);
            columnOptions.Add(Options);
        }

        public static DataTableBuilder operator +(DataColumnSetup a, IDataTableBuilder b) =>
            new DataTableBuilder((t, cos) =>
            {
                IDataTableBuilder dtba = a;
                dtba.Build(t, cos);
                b.Build(t, cos);
            });

        public DataColumnSetup Name(string value) =>
            AddBuilder(new DataColumnBuilder(dc => dc.ColumnName = value));

        public DataColumnSetup DataType(Type value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return AddBuilder(new DataColumnBuilder(dc => dc.DataType = value));
        }

        public DataColumnSetup Converter(Func<string, object> converter)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            return Converter((_, row, i) => converter(row[i]));
        }

        public DataColumnSetup Converter(Func<DataColumn, TextRow, int, object> converter)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            return WithOptions(Options.WithConverter(converter));
        }

        public DataColumnSetup AutoConvert(IFormatProvider provider) =>
            WithOptions(Options.UseAutoConversion(provider));

        public DataColumnSetup Binder(Func<DataColumn, TextRow, int> binder)
        {
            if (binder == null) throw new ArgumentNullException(nameof(binder));
            return WithOptions(Options.WithBinder(binder));
        }

        public DataColumnSetup Header(string name) =>
            Header(name, StringComparison.OrdinalIgnoreCase);

        public DataColumnSetup Header(string name, StringComparison comparison) =>
            Binder((_, hs) => hs.FindFirstIndex(name, comparison) ?? -1);

        public DataColumnSetup ColumnNameComparison(StringComparison comparison) =>
            Binder((dc, hs) => hs.FindFirstIndex(dc.ColumnName, comparison) ?? -1);

        public DataColumnSetup HeaderRegex(Regex regex) =>
            Binder((dc, hs) => hs.FindFirstIndex(regex.IsMatch) ?? -1);

        public DataColumnSetup HeaderRegex(string pattern) =>
            HeaderRegex(pattern, RegexOptions.None);

        public DataColumnSetup HeaderRegex(string pattern, RegexOptions options) =>
            Binder((dc, hs) => hs.FindFirstIndex(h => Regex.IsMatch(h, pattern, options)) ?? -1);
    }

    partial interface IDataTableBuilder
    {
        void Build(DataTable table, ICollection<DataColumnOptions> columnOptions);
    }

    partial class DataTableBuilder : IDataTableBuilder
    {
        public static readonly IDataTableBuilder Nop = new DataTableBuilder(delegate {});

        readonly Action<DataTable, ICollection<DataColumnOptions>> _builder;

        public DataTableBuilder(Action<DataTable, ICollection<DataColumnOptions>> builder) =>
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));

        public void Build(DataTable table, ICollection<DataColumnOptions> columnOptions)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (columnOptions == null) throw new ArgumentNullException(nameof(columnOptions));
            _builder(table, columnOptions);
        }

        public static DataTableBuilder operator +(DataTableBuilder a, DataTableBuilder b) =>
              b == null ? a
            : a == null ? b
            : new DataTableBuilder((t, cos) => { a.Build(t, cos); b.Build(t, cos); });

        public static DataTableBuilder operator +(DataTableBuilder a, IDataTableBuilder b) =>
              b == null ? a
            : a == null ? new DataTableBuilder(b.Build)
            : new DataTableBuilder((t, cos) => { a.Build(t, cos); b.Build(t, cos); });
    }

    partial interface IDataColumnBuilder
    {
        void Build(DataColumn column);
    }

    sealed partial class DataColumnBuilder : IDataColumnBuilder
    {
        internal static readonly IDataColumnBuilder Nop = new DataColumnBuilder(delegate {});

        readonly Action<DataColumn> _builder;

        public DataColumnBuilder(Action<DataColumn> builder) =>
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));

        public void Build(DataColumn column) =>
            _builder(column ?? throw new ArgumentNullException(nameof(column)));
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
                                    ?? row.Current.FindFirstIndex(h => string.Equals(h, e.Column.ColumnName, StringComparison.OrdinalIgnoreCase))
                                    ?? -1,
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
