#region Copyright 2018 Atif Aziz. All rights reserved.
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
    using System.Linq;
    using System.Text.RegularExpressions;

    static partial class TextRowExtensions
    {
        // Methods that get the index of first match; otherwise throw

        public static int GetFirstIndex(this TextRow row, string sought) =>
            row.GetFirstIndex(sought, StringComparison.Ordinal);

        public static int GetFirstIndex(this TextRow row,
            string sought, StringComparison comparison) =>
            row.GetFirstIndex(s => string.Equals(s, sought, comparison));

        public static int GetFirstIndex(this TextRow row, Func<string, bool> predicate) =>
            row.FindFirstIndex(predicate) ?? throw new InvalidOperationException("Sequence contains no elements.");

        // Methods that get the index of first match; otherwise return null

        public static int? FindFirstIndex(this TextRow row, string sought) =>
            row.FindFirstIndex(sought, StringComparison.Ordinal);

        public static int? FindFirstIndex(this TextRow row,
            string sought, StringComparison comparison) =>
            row.FindFirstIndex(s => string.Equals(s, sought, comparison));

        public static int? FindFirstIndex(this TextRow row, Func<string, bool> predicate)
        {
            for (var i = 0; i < row.Count; i++)
            {
                if (predicate(row[i]))
                    return i;
            }

            return null;
        }

        // Method to find index of fields matching a condition

        public static IEnumerable<int> FindIndex(this TextRow row, Func<string, bool> predicate) =>
            row.Find((s, i) => predicate(s) ? (true, i) : (false, default));

        // Methods to find the first field matching a condition; otherwise
        // return an alternative when none match.

        public static T FindFirstField<T>(this TextRow row, string sought,
            Func<string, int, T> someSelector) =>
            row.FindFirstField(sought, someSelector, default(T));

        public static T FindFirstField<T>(this TextRow row, string sought,
            Func<string, int, T> someSelector, T none) =>
            row.FindFirstField(sought, someSelector, () => none);

        public static T FindFirstField<T>(this TextRow row, string sought,
            Func<string, int, T> someSelector, Func<T> noneSelector) =>
            row.FindFirstField(sought, StringComparison.Ordinal, someSelector, noneSelector);

        public static T FindFirstField<T>(this TextRow row,
            string sought, StringComparison comparison,
            Func<string, int, T> someSelector) =>
            row.FindFirstField(sought, comparison, someSelector, default(T));

        public static T FindFirstField<T>(this TextRow row,
            string sought, StringComparison comparison,
            Func<string, int, T> someSelector, T none) =>
            row.FindFirstField(sought, comparison, someSelector, () => none);

        public static T FindFirstField<T>(this TextRow row,
            string sought, StringComparison comparison,
            Func<string, int, T> someSelector, Func<T> noneSelector)
        {
            using var e = row.Find((s, i) => string.Equals(sought, s, comparison)
                                           ? (true, someSelector(s, i))
                                           : default)
                             .GetEnumerator();
            return e.MoveNext() ? e.Current : noneSelector();
        }

        // Methods to get the first matching field; otherwise throw

        public static (string Field, int Index) GetFirstField(this TextRow row,
            string sought, StringComparison comparison) =>
            row.GetFirstField(sought, comparison, ValueTuple.Create);

        public static T GetFirstField<T>(this TextRow row,
            string sought, StringComparison comparison, Func<string, int, T> resultSelector) =>
            row.Find((s, i) => string.Equals(sought, s, comparison)
                             ? (true, resultSelector(s, i))
                             : default)
               .First();

        // Methods that find the first field matching a pattern; otherwise
        // return an alternative when none match.

        public static T FindFirstMatch<T>(this TextRow row,
            string pattern, Func<string, int, Match, T> someSelector) =>
            row.FindFirstMatch(pattern, someSelector, default(T));

        public static T FindFirstMatch<T>(this TextRow row,
            string pattern, Func<string, int, Match, T> someSelector, T none) =>
            row.FindFirstMatch(pattern, RegexOptions.None, someSelector, none);

        public static T FindFirstMatch<T>(this TextRow row,
            string pattern, RegexOptions options,
            Func<string, int, Match, T> someSelector) =>
            row.FindFirstMatch(pattern, options, someSelector, default(T));

        public static T FindFirstMatch<T>(this TextRow row,
            string pattern, RegexOptions options,
            Func<string, int, Match, T> someSelector, T none) =>
            row.FindFirstMatch(pattern, options, someSelector, () => none);

        public static T FindFirstMatch<T>(this TextRow row, string pattern,
            Func<string, int, Match, T> someSelector, Func<T> noneSelector) =>
            row.FindFirstMatch(pattern, RegexOptions.None, someSelector, noneSelector);

        public static T FindFirstMatch<T>(this TextRow row,
            string pattern, RegexOptions options,
            Func<string, int, Match, T> someSelector, Func<T> noneSelector)
        {
            using var e = row.Match(pattern, options, someSelector).GetEnumerator();
            return e.MoveNext() ? e.Current : noneSelector();
        }

        // Methods that get the first field matching a pattern; otherwise throw

        public static (string Field, int Index)
            GetFirstMatch(this TextRow row, string pattern) =>
            GetFirstMatch(row, pattern, RegexOptions.None);

        public static (string Field, int Index)
            GetFirstMatch(this TextRow row, string pattern, RegexOptions options) =>
            row.GetFirstMatch(pattern, options, (s, i, _) => (s, i));

        public static T GetFirstMatch<T>(this TextRow row,
            string pattern, Func<string, int, Match, T> resultSelector) =>
            row.GetFirstMatch(pattern, RegexOptions.None, resultSelector);

        public static T GetFirstMatch<T>(this TextRow row,
            string pattern, RegexOptions options,
            Func<string, int, Match, T> resultSelector) =>
            row.Match(pattern, options, resultSelector).First();

        // Methods that all fields matching a pattern

        public static IEnumerable<(string Field, int Index)>
            Match(this TextRow row, string pattern) =>
            Match(row, pattern, RegexOptions.None);

        public static IEnumerable<(string Field, int Index)>
            Match(this TextRow row, string pattern, RegexOptions options) =>
            row.Match(pattern, options, (s, i, _) => (s, i));

        public static IEnumerable<T> Match<T>(this TextRow row,
            string pattern, Func<string, int, Match, T> resultSelector) =>
            Match(row, pattern, RegexOptions.None, resultSelector);

        public static IEnumerable<T> Match<T>(this TextRow row,
            string pattern, RegexOptions options,
            Func<string, int, Match, T> resultSelector) =>
            from e in row.Find((s, i) =>
            {
                var m = Regex.Match(s, pattern, options);
                return (m.Success, (Input: s, Index: i, Match: m));
            })
            select resultSelector(e.Input, e.Index, e.Match);

        // Methods that find all fields matching a condition

        public static IEnumerable<(string Field, int Index)>
            Find(this TextRow row, Func<string, int, bool> predicate) =>
            row.Find((s, i) => predicate(s, i) ? (true, (s, i)) : default);

        public static IEnumerable<T> Find<T>(this TextRow row,
            Func<string, int, (bool, T)> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return _(); IEnumerable<T> _()
            {
                for (var i = 0; i < row.Count; i++)
                {
                    var (matched, result) = predicate(row[i], i);
                    if (matched)
                        yield return result;
                }
            }
        }
    }
}
