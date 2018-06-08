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
    using System.Linq;
    using System.Text.RegularExpressions;
    using Xunit;

    public sealed class TextRowTests
    {
        static readonly TextRow Row = new[] { @"foo,bar,baz,Foo,Bar,Baz,FOO,BAR,BAZ" }.ParseCsv().Single();

        [Fact]
        public void Find()
        {
            var result = Row.Find((s, i) => s[0] == 'f' || s[0] == 'F');
            Assert.Equal(new[] { 0, 3, 6 }, from e in result select e.Index);
            Assert.Equal(new[] { "foo", "Foo", "FOO" }, from e in result select e.Field);
        }

        [Fact]
        public void FindCustom()
        {
            var result = Row.Find((s, i) => s[0] == 'f' || s[0] == 'F' ? (true, new { Field = s, Index = i }) : default);
            Assert.Equal(new[] { 0, 3, 6 }, from e in result select e.Index);
            Assert.Equal(new[] { "foo", "Foo", "FOO" }, from e in result select e.Field);
        }

        [Fact]
        public void FindIndex()
        {
            Assert.Equal(new[] { 0, 3, 6 }, Row.FindIndex(s => s[0] == 'f' || s[0] == 'F'));
        }

        [Fact]
        public void Match()
        {
            var result = Row.Match("^[fB]..", (s, i, m) => new { Field = s, Index = i, Match = m.Value });
            Assert.Equal(new[] { 0, 4, 5, 7, 8 }, from e in result select e.Index);
            var hit = new[] { "foo", "Bar", "Baz", "BAR", "BAZ" };
            Assert.Equal(hit, from e in result select e.Field);
            Assert.Equal(hit, from e in result select e.Match);
        }

        [Fact]
        public void MatchIgnoringCase()
        {
            var result = Row.Match("^.a.$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                                   (s, i, m) => new { Field = s, Index = i, Match = m.Value });
            Assert.Equal(new[] { 1, 2, 4, 5, 7, 8 }, from e in result select e.Index);
            var hit = new[] { "bar", "baz", "Bar", "Baz", "BAR", "BAZ" };
            Assert.Equal(hit, from e in result select e.Field);
            Assert.Equal(hit, from e in result select e.Match);
        }

        public sealed class GetFirstIndex
        {
            [Theory]
            [InlineData(0, "foo")]
            [InlineData(1, "bar")]
            [InlineData(2, "baz")]
            [InlineData(3, "Foo")]
            [InlineData(4, "Bar")]
            [InlineData(5, "Baz")]
            [InlineData(6, "FOO")]
            [InlineData(7, "BAR")]
            [InlineData(8, "BAZ")]
            public void ReturnsIndexOfSoughtString(int index, string sought) =>
                Assert.Equal(index, Row.GetFirstIndex(sought));

            [Fact]
            public void ThrowsOnNoMatch() =>
                Assert.Throws<InvalidOperationException>(() => Row.GetFirstIndex("?"));

            [Theory]
            [InlineData(0, "foo", StringComparison.Ordinal)]
            [InlineData(1, "bar", StringComparison.Ordinal)]
            [InlineData(2, "baz", StringComparison.Ordinal)]
            [InlineData(3, "Foo", StringComparison.Ordinal)]
            [InlineData(4, "Bar", StringComparison.Ordinal)]
            [InlineData(5, "Baz", StringComparison.Ordinal)]
            [InlineData(6, "FOO", StringComparison.Ordinal)]
            [InlineData(7, "BAR", StringComparison.Ordinal)]
            [InlineData(8, "BAZ", StringComparison.Ordinal)]
            [InlineData(0, "FoO", StringComparison.OrdinalIgnoreCase)]
            [InlineData(1, "BaR", StringComparison.OrdinalIgnoreCase)]
            [InlineData(2, "BaZ", StringComparison.OrdinalIgnoreCase)]
            public void WithComparisonReturnsIndexOfSoughtString(int index, string sought, StringComparison comparison) =>
                Assert.Equal(index, Row.GetFirstIndex(sought, comparison));

            [Fact]
            public void WithComparisonThrowsOnNoMatch() =>
                Assert.Throws<InvalidOperationException>(() =>
                    Row.GetFirstIndex("F00", StringComparison.OrdinalIgnoreCase));

            [Fact]
            public void WithMatchingPredicateReturnsIndex() =>
                Assert.Equal(1, Row.GetFirstIndex(s => s[1] == 'a'));

            [Fact]
            public void WithNonMatchingPredicateThrows() =>
                Assert.Throws<InvalidOperationException>(() =>
                    Row.GetFirstIndex(s => s[0] == 'z'));
        }

        public sealed class FindFirstIndex
        {
            [Theory]
            [InlineData( 0, "foo")]
            [InlineData( 1, "bar")]
            [InlineData( 2, "baz")]
            [InlineData( 3, "Foo")]
            [InlineData( 4, "Bar")]
            [InlineData( 5, "Baz")]
            [InlineData( 6, "FOO")]
            [InlineData( 7, "BAR")]
            [InlineData( 8, "BAZ")]
            [InlineData(-1, "-")]
            [InlineData(-1, "?")]
            public void ReturnsIndexOfSoughtString(int index, string sought) =>
                Assert.Equal(index, Row.FindFirstIndex(sought) ?? -1);

            [Theory]
            [InlineData( 0, "foo", StringComparison.Ordinal)]
            [InlineData( 1, "bar", StringComparison.Ordinal)]
            [InlineData( 2, "baz", StringComparison.Ordinal)]
            [InlineData( 3, "Foo", StringComparison.Ordinal)]
            [InlineData( 4, "Bar", StringComparison.Ordinal)]
            [InlineData( 5, "Baz", StringComparison.Ordinal)]
            [InlineData( 6, "FOO", StringComparison.Ordinal)]
            [InlineData( 7, "BAR", StringComparison.Ordinal)]
            [InlineData( 8, "BAZ", StringComparison.Ordinal)]
            [InlineData(-1, "-"  , StringComparison.Ordinal)]
            [InlineData(-1, "?"  , StringComparison.Ordinal)]
            [InlineData( 0, "FoO", StringComparison.OrdinalIgnoreCase)]
            [InlineData( 1, "BaR", StringComparison.OrdinalIgnoreCase)]
            [InlineData( 2, "BaZ", StringComparison.OrdinalIgnoreCase)]
            [InlineData(-1, "-"  , StringComparison.OrdinalIgnoreCase)]
            [InlineData(-1, "?"  , StringComparison.OrdinalIgnoreCase)]
            public void WithComparisonReturnsIndexOfSoughtString(int index, string sought, StringComparison comparison) =>
                Assert.Equal(index, Row.FindFirstIndex(sought, comparison) ?? -1);

            [Fact]
            public void WithMatchingPredicateReturnsIndex() =>
                Assert.Equal(1, Row.FindFirstIndex(s => s[1] == 'a'));

            [Fact]
            public void WithNonMatchingPredicateReturnsNull() =>
                Assert.Null(Row.FindFirstIndex(s => s[1] == 'x'));
        }
    }
}
