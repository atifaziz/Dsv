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

namespace Yax
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    static partial class DataColumnSetup
    {
        static readonly DataColumnBuilder Nop = DataColumnBuilder.ByVal(delegate { });

        public static readonly DataColumnBuilder Default =
            DataColumnBuilder.ByRef(dc => dc[0] = dc[0] ?? new DataColumn());

        public static DataColumnBuilder Of(string name, Type dataType) =>
            Of(name, dataType, null);

        public static DataColumnBuilder Of(string name, Type dataType, IFormatProvider provider) =>
            DataColumnBuilder.ByRef(dc => dc[0] = new DataColumn(name, dataType))
            + FormatProvider(provider);

        public static DataColumnBuilder Of<T>(string name, Func<string, T> converter) =>
            Of(name, typeof(T)) + Converter(s => converter(s));

        public static DataColumnBuilder String(string name) =>
            Of(name, typeof(string));

        public static DataColumnBuilder Int32(string name, IFormatProvider provider) =>
            Int32(name, NumberStyles.Integer, provider);

        public static DataColumnBuilder Int32(string name, NumberStyles styles, IFormatProvider provider) =>
            Of(name, s => int.Parse(s, styles, provider));

        public static DataColumnBuilder Single(string name, IFormatProvider provider) =>
            Single(name, NumberStyles.Float | NumberStyles.AllowThousands, provider);

        public static DataColumnBuilder Single(string name, NumberStyles styles, IFormatProvider provider) =>
            Of(name, s => float.Parse(s, styles, provider));

        public static DataColumnBuilder Double(string name, IFormatProvider provider) =>
            Double(name, NumberStyles.Float | NumberStyles.AllowThousands, provider);

        public static DataColumnBuilder Double(string name, NumberStyles styles, IFormatProvider provider) =>
            Of(name, s => double.Parse(s, styles, provider));

        public static DataColumnBuilder Decimal(string name, IFormatProvider provider) =>
            Double(name, NumberStyles.Number, provider);

        public static DataColumnBuilder Decimal(string name, NumberStyles styles, IFormatProvider provider) =>
            Of(name, s => decimal.Parse(s, styles, provider));

        public static DataColumnBuilder DateTime(string name, IFormatProvider provider) =>
            DateTime(name, DateTimeStyles.None, provider);

        public static DataColumnBuilder DateTime(string name, DateTimeStyles styles, IFormatProvider provider) =>
            Of(name, s => System.DateTime.Parse(s, provider, styles));

        public static DataColumnBuilder DateTime(string name, string format, IFormatProvider provider) =>
            DateTime(name, format, DateTimeStyles.None, provider);

        public static DataColumnBuilder DateTime(string name, string format, DateTimeStyles styles, IFormatProvider provider) =>
            Of(name, s => System.DateTime.ParseExact(s, format, provider, styles));

        public static DataColumnBuilder DateTime(string name, string[] formats, IFormatProvider provider) =>
            DateTime(name, formats, DateTimeStyles.None, provider);

        public static DataColumnBuilder DateTime(string name, string[] formats, DateTimeStyles styles, IFormatProvider provider) =>
            Of(name, s => System.DateTime.ParseExact(s, formats, provider, styles));

        public static DataColumnBuilder DateTimeOffset(string name, IFormatProvider provider) =>
            DateTimeOffset(name, DateTimeStyles.None, provider);

        public static DataColumnBuilder DateTimeOffset(string name, DateTimeStyles styles, IFormatProvider provider) =>
            Of(name, s => System.DateTimeOffset.Parse(s, provider, styles));

        public static DataColumnBuilder DateTimeOffset(string name, string format, IFormatProvider provider) =>
            DateTimeOffset(name, format, DateTimeStyles.None, provider);

        public static DataColumnBuilder DateTimeOffset(string name, string format, DateTimeStyles styles, IFormatProvider provider) =>
            Of(name, s => System.DateTimeOffset.ParseExact(s, format, provider, styles));

        public static DataColumnBuilder DateTimeOffset(string name, string[] formats, IFormatProvider provider) =>
            DateTimeOffset(name, formats, DateTimeStyles.None, provider);

        public static DataColumnBuilder DateTimeOffset(string name, string[] formats, DateTimeStyles styles, IFormatProvider provider) =>
            Of(name, s => System.DateTimeOffset.ParseExact(s, formats, provider, styles));

        public static DataColumnBuilder Name(string value) =>
            Default + DataColumnBuilder.ByVal(dc => dc.ColumnName = value);

        public static DataColumnBuilder DataType(Type value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return Default + DataColumnBuilder.ByVal(dc => dc.DataType = value);
        }

        public static IDataColumnBuilder[] Strings(params string[] columns) =>
            columns.Select(name => (IDataColumnBuilder) String(name)).ToArray();

        static DataColumnBuilder Options(Func<DataColumnOptions, DataColumnOptions> modifier) =>
            Default + DataColumnBuilder.Options(modifier);

        public static DataColumnBuilder FormatProvider(IFormatProvider provider) =>
            provider == null ? Nop : Options(o => o.WithFormatProvider(provider));

        public static DataColumnBuilder Converter(Func<string, object> converter)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            return Options(o => o.WithConverter((_, row, i) => converter(row[i])));
        }

        public static DataColumnBuilder Converter(Func<DataColumn, TextRow, int, object> converter)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            return Options(o => o.WithConverter(converter));
        }

        public static DataColumnBuilder Binder(Func<DataColumn, TextRow, int> binder)
        {
            if (binder == null) throw new ArgumentNullException(nameof(binder));
            return Options(o => o.WithBinder(binder));
        }

        public static DataColumnBuilder Header(string name) =>
            Header(name, StringComparison.OrdinalIgnoreCase);

        public static DataColumnBuilder Header(string name, StringComparison comparison) =>
            Binder((_, hs) => hs.FindIndex(h => string.Equals(h, name, comparison)));

        public static DataColumnBuilder ColumnNameHeader(StringComparison comparison) =>
            Binder((dc, hs) => hs.FindIndex(h => string.Equals(h, dc.ColumnName, comparison)));

        public static DataColumnBuilder HeaderRegex(Regex regex) =>
            Binder((dc, hs) => hs.FindIndex(regex.IsMatch));

        public static DataColumnBuilder HeaderRegex(string pattern) =>
            HeaderRegex(pattern, RegexOptions.None);

        public static DataColumnBuilder HeaderRegex(string pattern, RegexOptions options) =>
            Binder((dc, hs) => hs.FindIndex(h => Regex.IsMatch(h, pattern, options)));
    }

    sealed partial class DataColumnOptions
    {
        public static readonly DataColumnOptions Default = new DataColumnOptions(null, null, null);

        public Func<DataColumn, TextRow, int> Binder { get; }
        public Func<DataColumn, TextRow, int, object> Converter { get; }
        public IFormatProvider FormatProvider { get; }

        DataColumnOptions(Func<DataColumn, TextRow, int> binder,
                          Func<DataColumn, TextRow, int, object> converter,
                          IFormatProvider formatProvider)
        {
            Converter      = converter;
            Binder         = binder;
            FormatProvider = formatProvider;
        }

        public DataColumnOptions WithConverter(Func<DataColumn, TextRow, int, object> value) =>
            value == Converter ? this : new DataColumnOptions(Binder, value, FormatProvider);

        public DataColumnOptions WithFormatProvider(IFormatProvider value) =>
            value == FormatProvider ? this : new DataColumnOptions(Binder, Converter, value);

        public DataColumnOptions WithBinder(Func<DataColumn, TextRow, int> value) =>
            value == Binder ? this : new DataColumnOptions(value, Converter, FormatProvider);
    }

    sealed partial class DataColumnBuildResult : IEquatable<DataColumnBuildResult>
    {
        public DataColumn Column { get; }
        public DataColumnOptions Options { get; }

        internal DataColumnBuildResult(DataColumn column, DataColumnOptions options)
        {
            Column  = column  ?? throw new ArgumentNullException(nameof(column));
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public bool Equals(DataColumnBuildResult other) =>
            !ReferenceEquals(null, other)
            && (ReferenceEquals(this, other)
                || Equals(Column, other.Column)
                && Equals(Options, other.Options));

        public override bool Equals(object obj) =>
            Equals(obj as DataColumnBuildResult);

        public override int GetHashCode() =>
            unchecked((Column.GetHashCode() * 397) ^ Options.GetHashCode().GetHashCode());

        public void Deconstruct(out DataColumn column, out DataColumnOptions options)
        {
            column  = Column;
            options = Options;
        }
    }

    partial interface IDataColumnBuilder
    {
        DataColumnBuildResult Build();
    }

    sealed partial class DataColumnBuilder : IDataColumnBuilder
    {
        static readonly Action<DataColumn[]> NonColumnBuilder = delegate { };

        readonly Action<DataColumn[]> _builder;
        readonly Func<DataColumnOptions, DataColumnOptions> _optionsBuilder;

        internal static DataColumnBuilder ByRef(Action<DataColumn[]> builder) =>
            new DataColumnBuilder(builder);

        internal static DataColumnBuilder ByVal(Action<DataColumn> builder) =>
            new DataColumnBuilder(builder ?? throw new ArgumentNullException(nameof(builder)));

        internal static DataColumnBuilder Options(Func<DataColumnOptions, DataColumnOptions> optionsBuilder) =>
            new DataColumnBuilder(NonColumnBuilder,
                                  optionsBuilder ?? throw new ArgumentNullException(nameof(optionsBuilder)));

        DataColumnBuilder(Action<DataColumn> builder, Func<DataColumnOptions, DataColumnOptions> optionsBuilder = null) :
            this(dc => builder(dc[0]), optionsBuilder) {}

        DataColumnBuilder(Action<DataColumn[]> builder, Func<DataColumnOptions, DataColumnOptions> optionsBuilder = null)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _optionsBuilder = optionsBuilder ?? (options => options);
        }

        public DataColumnBuildResult Build()
        {
            var column = new DataColumn[1];
            _builder(column);
            return new DataColumnBuildResult(column[0], _optionsBuilder(DataColumnOptions.Default));
        }

        public static DataColumnBuilder operator +(DataColumnBuilder a, DataColumnBuilder b) =>
            new DataColumnBuilder(dc => { a._builder(dc); b._builder(dc); },
                                  options => b._optionsBuilder(a._optionsBuilder(options)));
    }

    static partial class Extensions
    {
        public static DataTable ToDataTable(this IEnumerable<TextRow> rows, params IDataColumnBuilder[] columns)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            if (columns == null) throw new ArgumentNullException(nameof(columns));

            var builtColumns = columns.Select(c => c.Build()).ToArray();

            var table = new DataTable();
            foreach (var (column, _) in builtColumns)
                table.Columns.Add(column);

            using (var row = rows.GetEnumerator())
            {
                if (!row.MoveNext())
                    return table;

                var bindingz =
                    columns.Length > 0
                    ? from e in builtColumns
                      where string.IsNullOrWhiteSpace(e.Column.Expression)
                      select new
                      {
                          e.Column,
                          Index = e.Options.Binder?.Invoke(e.Column, row.Current)
                                  ?? row.Current.FindIndex(h => string.Equals(h, e.Column.ColumnName, StringComparison.OrdinalIgnoreCase)),
                          e.Options.Converter,
                          e.Options.FormatProvider,
                      }
                      into e
                      where e.Index >= 0
                      select e
                    : from i in Enumerable.Range(0, row.Current.Count)
                      select new
                      {
                          Column = new DataColumn(row.Current[i], typeof(string)),
                          Index = i,
                          DataColumnOptions.Default.Converter,
                          DataColumnOptions.Default.FormatProvider,
                      };

                var bindings = bindingz.ToArray();

                if (columns.Length == 0)
                {
                    foreach (var binding in bindings)
                        table.Columns.Add(binding.Column);
                }

                while (row.MoveNext())
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
                                : Convert.ChangeType(row.Current[binding.Index], binding.Column.DataType, binding.FormatProvider);
                        }
                    }

                    table.Rows.Add(dr);
                }
            }

            return table;
        }
    }
}
