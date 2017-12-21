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
    using System.Linq;

    static partial class Extensions
    {
        public static DataTable ToDataTable(this IEnumerable<TextRow> rows, params Func<DataColumn>[] columns)
        {
            var table = new DataTable();
            foreach (var column in columns)
                table.Columns.Add(column());

            using (var row = rows.GetEnumerator())
            {
                if (!row.MoveNext())
                    return table;

                var bindingz =
                    from DataColumn column in table.Columns
                    select (Column: column,
                            Index : row.Current.FindIndex(h => string.Equals(h, column.ColumnName, StringComparison.OrdinalIgnoreCase)))
                    into e
                    where e.Index >= 0
                    select e;

                var bindings = bindingz.ToArray();

                while (row.MoveNext())
                {
                    var dr = table.NewRow();
                    foreach (var binding in bindings)
                    {
                        var (column, index) = binding;
                        if (index < row.Current.Count)
                            dr[column] = Convert.ChangeType(row.Current[index], column.DataType);
                    }
                    table.Rows.Add(dr);
                }
            }

            return table;
        }
    }
}
