namespace Dsv.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Xunit;

    public interface IBinder<out T>
    {
        T Bind(TextRow row, DataTable table);
    }

    public static class Column
    {
        public static IBinder<DataColumn> Add(string name) =>
            Binder.Create((hr, t) =>
            {
                var i = hr.GetFirstIndex(name, StringComparison.Ordinal);
                var col = new DataColumn(name) { Caption = hr[i] };
                t.Columns.Add(col);
                return col;
            });

        public static IBinder<DataColumn> AddRegex(string name, string pattern) =>
            Binder.Create((hr, t) =>
            {
                var col = t.Columns.Add(name);
                return col;
            });
    }

    public sealed class BinderTests
    {
        [Xunit.Fact]
        public void Test()
        {
            var q =
                from foo in Column.Add("foo")
                from bar in Column.Add("bar")
                from baz in Column.AddRegex("baz", "[Bb]az")
                select new
                {
                    Foo = foo,
                    Bar = bar,
                    Baz = baz,
                };

            var source = string.Join(Environment.NewLine,
                "foo,baz,bar,quux",
                "a,b,c,d");

            var r =
                from e in new[]
                {
                    source
                    .Split('\n')
                    .ParseDsv(Format.Csv, _ => false, q, (t, cols) => new
                    {
                        Table = t,
                        cols.Foo, cols.Bar, cols.Baz,
                        Rest = t.Columns.Cast<DataColumn>()
                                .Except(new[] { cols.Foo, cols.Bar, cols.Baz })
                                .ToArray(),
                    })
                }
                from DataRow row in e.Table.Rows
                select new
                {
                    Foo = (string) row[e.Foo],
                    Bar = (string) row[e.Bar],
                    Rest = string.Join(",", e.Rest.Select(col => (string) row[col]))
                };

            Assert.Equal(new[]
            {
                new { Foo = "a", Bar = "c", Rest = "d" }
            }, r);
        }
    }

    public static class Binder
    {
        public static TResult ParseDsv<THead, TResult>(this IEnumerable<string> lines,
            Format format,
            Func<string, bool> rowFilter,
            IBinder<THead> headSelector,
            Func<DataTable, THead, TResult> resultSelector)
        {
            var table = new DataTable();

            using (var row = lines.ParseDsv(format, rowFilter).GetEnumerator())
            {
                if (row.MoveNext())
                {
                    var head = headSelector.Bind(row.Current, table);

                    var bindings =
                        table.Columns.Cast<DataColumn>()
                            .Select(column => row.Current.FindFirstIndex(column.ColumnName))
                            .ToArray();

                    while (row.MoveNext())
                    {
                        var dr =
                            bindings.Select(i => i is int ii ? (object) row.Current[ii] : DBNull.Value).ToArray();
                        table.Rows.Add(dr);
                    }

                    return resultSelector(table, head);
                }
                else
                {
                    throw new FormatException();
                }
            }
        }

        public static IBinder<T> Create<T>(Func<TextRow, DataTable, T> binder) =>
            new DelegatingBinder<T>(binder);

        public static IBinder<T> Return<T>(T x) => Create((_, __) => x);

        public static IBinder<R> Bind<T, R>(this IBinder<T> a, Func<T, IBinder<R>> f) =>
            Create((h ,t) => f(a.Bind(h, t)).Bind(h, t));

        public static IBinder<R> Select<T, R>(this IBinder<T> a, Func<T, R> f) =>
            a.Bind(x => Return(f(x)));

        public static IBinder<V> SelectMany<T, U, V>(this IBinder<T> a, Func<T, IBinder<U>> f, Func<T, U, V> g) =>
            a.Bind(x => f(x).Select(y => g(x, y)));

        sealed class DelegatingBinder<T> : IBinder<T>
        {
            readonly Func<TextRow, DataTable, T> _impl;

            public DelegatingBinder(Func<TextRow, DataTable, T> impl) =>
                _impl = impl;

            public T Bind(TextRow row, DataTable table) => _impl(row, table);
        }
    }
}
