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
    using System.IO;
    using System.Text;
    using Xunit;

    static class Extensions
    {
        public static TheoryData<T1, T2, T3, T4, T5, T6, T7> ToTheoryData<T1, T2, T3, T4, T5, T6, T7>(
            this IEnumerable<(T1, T2, T3, T4, T5, T6, T7)> rows)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            var data = new TheoryData<T1, T2, T3, T4, T5, T6, T7>();
            foreach (var row in rows)
                data.Add(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7);
            return data;
        }

        public static Stream GetManifestResourceStream(this Type type, string resourceName)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return type.Assembly.GetManifestResourceStream(type, resourceName);
        }
    }
}
