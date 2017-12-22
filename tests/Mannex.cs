#region License, Terms and Author(s)
//
// Mannex - Extension methods for .NET
// Copyright (c) 2009 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      Atif Aziz, http://www.raboof.com
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

namespace Mannex.Text.RegularExpressions
{
    #region Imports

    using System;
    using System.Text.RegularExpressions;

    #endregion

    /// <summary>
    /// Extension methods for <see cref="Match"/> that help with regular
    /// expression matching.
    /// </summary>

    static partial class MatchExtensions
    {
        static readonly Func<Match, string, int, Group> NamedBinder = (match, name, num) => match.Groups[name];
        static readonly Func<Match, string, int, Group> NumberBinder = (match, name, num) => match.Groups[num];

        /// <summary>
        /// Binds match group names to corresponding parameter names of a
        /// method responsible for creating the result of the match.
        /// </summary>
        /// <remarks>
        /// <paramref name="resultSelector"/> is called to generate a
        /// result irrespective of whether the match was successful or not.
        /// </remarks>

        public static TResult Bind<TResult>(this Match match,
            Func<Group, TResult> resultSelector)
        {
            return Bind(match, NamedBinder, resultSelector);
        }

        /// <summary>
        /// Binds match group numbers to corresponding parameter positions
        /// of a method responsible for creating the result of the match.
        /// </summary>
        /// <remarks>
        /// <paramref name="resultSelector"/> is called to generate a
        /// result irrespective of whether the match was successful or not.
        /// </remarks>

        public static TResult BindNum<TResult>(this Match match,
            Func<Group, TResult> resultSelector)
        {
            return Bind(match, NumberBinder, resultSelector);
        }

        /// <summary>
        /// Binds match group names to corresponding parameter names of a
        /// method responsible for creating the result of the match.
        /// </summary>
        /// <remarks>
        /// <paramref name="resultSelector"/> is called to generate a
        /// result irrespective of whether the match was successful or not.
        /// </remarks>

        public static TResult Bind<TResult>(this Match match,
            Func<Group, Group, TResult> resultSelector)
        {
            return Bind(match, NamedBinder, resultSelector);
        }

        /// <summary>
        /// Binds match group numbers to corresponding parameter positions
        /// of a method responsible for creating the result of the match.
        /// </summary>
        /// <remarks>
        /// <paramref name="resultSelector"/> is called to generate a
        /// result irrespective of whether the match was successful or not.
        /// </remarks>

        public static TResult BindNum<TResult>(this Match match,
            Func<Group, Group, TResult> resultSelector)
        {
            return Bind(match, NumberBinder, resultSelector);
        }

        static TResult Bind<TResult>(Match match,
            Func<Match, string, int, Group> groupSelector,
            Func<Group, Group, TResult> resultSelector)
        {
            return Bind(match, groupSelector, groupSelector, resultSelector);
        }

        /// <summary>
        /// Binds match group names to corresponding parameter names of a
        /// method responsible for creating the result of the match.
        /// </summary>
        /// <remarks>
        /// <paramref name="resultSelector"/> is called to generate a
        /// result irrespective of whether the match was successful or not.
        /// </remarks>

        public static TResult Bind<TResult>(this Match match,
            Func<Group, Group, Group, TResult> resultSelector)
        {
            return Bind(match, NamedBinder, resultSelector);
        }

        /// <summary>
        /// Binds match group numbers to corresponding parameter positions
        /// of a method responsible for creating the result of the match.
        /// </summary>
        /// <remarks>
        /// <paramref name="resultSelector"/> is called to generate a
        /// result irrespective of whether the match was successful or not.
        /// </remarks>

        public static TResult BindNum<TResult>(this Match match,
            Func<Group, Group, Group, TResult> resultSelector)
        {
            return Bind(match, NumberBinder, resultSelector);
        }

        static TResult Bind<TResult>(Match match,
            Func<Match, string, int, Group> groupSelector,
            Func<Group, Group, Group, TResult> resultSelector)
        {
            return Bind(match, groupSelector, groupSelector, groupSelector, resultSelector);
        }

        /// <summary>
        /// Binds match group names to corresponding parameter names of a
        /// method responsible for creating the result of the match.
        /// </summary>
        /// <remarks>
        /// <paramref name="resultSelector"/> is called to generate a
        /// result irrespective of whether the match was successful or not.
        /// </remarks>

        public static TResult Bind<TResult>(this Match match,
            Func<Group, Group, Group, Group, TResult> resultSelector)
        {
            return Bind(match, NamedBinder, resultSelector);
        }

        /// <summary>
        /// Binds match group numbers to corresponding parameter positions
        /// of a method responsible for creating the result of the match.
        /// </summary>
        /// <remarks>
        /// <paramref name="resultSelector"/> is called to generate a
        /// result irrespective of whether the match was successful or not.
        /// </remarks>

        public static TResult BindNum<TResult>(this Match match,
            Func<Group, Group, Group, Group, TResult> resultSelector)
        {
            return Bind(match, NumberBinder, resultSelector);
        }

        static TResult Bind<TResult>(Match match,
            Func<Match, string, int, Group> groupSelector,
            Func<Group, Group, Group, Group, TResult> resultSelector)
        {
            return Bind(match, groupSelector, groupSelector, groupSelector, groupSelector, resultSelector);
        }

        static TResult Bind<T1, TResult>(Match match,
            Func<Match, string, int, T1> selector,
            Func<T1, TResult> resultSelector)
        {
            return Bind<T1, object, object, object, TResult>(match,
                       selector, null, null, null,
                       resultSelector, null, null, null);
        }

        static TResult Bind<T1, T2, TResult>(Match match,
            Func<Match, string, int, T1> selector1,
            Func<Match, string, int, T2> selector2,
            Func<T1, T2, TResult> resultSelector)
        {
            return Bind<T1, T2, object, object, TResult>(match,
                       selector1, selector2, null, null,
                       null, resultSelector, null, null);
        }

        static TResult Bind<T1, T2, T3, TResult>(Match match,
            Func<Match, string, int, T1> selector1,
            Func<Match, string, int, T2> selector2,
            Func<Match, string, int, T3> selector3,
            Func<T1, T2, T3, TResult> resultSelector)
        {
            return Bind<T1, T2, T3, object, TResult>(match,
                       selector1, selector2, selector3, null,
                       null, null, resultSelector, null);
        }

        static TResult Bind<T1, T2, T3, T4, TResult>(Match match,
            Func<Match, string, int, T1> selector1,
            Func<Match, string, int, T2> selector2,
            Func<Match, string, int, T3> selector3,
            Func<Match, string, int, T4> selector4,
            Func<T1, T2, T3, T4, TResult> resultSelector)
        {
            return Bind<T1, T2, T3, T4, TResult>(match,
                       selector1, selector2, selector3, selector4,
                       null, null, null, resultSelector);
        }

        internal static TResult Bind<T1, T2, T3, T4, TResult>(
            Match match,
            Func<Match, string, int, T1> s1,
            Func<Match, string, int, T2> s2,
            Func<Match, string, int, T3> s3,
            Func<Match, string, int, T4> s4,
            Func<T1, TResult> r1,
            Func<T1, T2, TResult> r2,
            Func<T1, T2, T3, TResult> r3,
            Func<T1, T2, T3, T4, TResult> r4)
        {
            if (match == null) throw new ArgumentNullException("match");

            var d = r1 ?? r2 ?? r3 ?? (Delegate)r4;
            if (d == null)
                throw new ArgumentNullException("resultSelector");

            var ps = d.Method.GetParameters();
            var count = ps.Length;

            if (s1 == null) throw new ArgumentNullException(count == 1 ? "selector" : "selector1");
            if (count > 1 && s2 == null) throw new ArgumentNullException("selector2");
            if (count > 2 && s3 == null) throw new ArgumentNullException("selector3");
            if (count > 3 && s4 == null) throw new ArgumentNullException("selector4");

            return r1 != null
                 ? r1(s1(match, ps[0].Name, ps[0].Position + 1))

                 : r2 != null
                 ? r2(s1(match, ps[0].Name, ps[0].Position + 1),
                      s2(match, ps[1].Name, ps[1].Position + 1))

                 : r3 != null
                 ? r3(s1(match, ps[0].Name, ps[0].Position + 1),
                      s2(match, ps[1].Name, ps[1].Position + 1),
                      s3(match, ps[2].Name, ps[2].Position + 1))

                 : r4(s1(match, ps[0].Name, ps[0].Position + 1),
                      s2(match, ps[1].Name, ps[1].Position + 1),
                      s3(match, ps[2].Name, ps[2].Position + 1),
                      s4(match, ps[3].Name, ps[3].Position + 1));
        }
    }
}
