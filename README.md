# Dsv

Dsv is a [.NET Standard][netstd] Library for parsing [delimiter-separated
values][dsv], like [CSV (comma-separated values)][csv] data. It has a
functional design in that most data structures are immutable and there is no
shared state.

The goal of the project is to support common formats. It is a _non-goal_ to
try and support malformed cases of a DSV format and recover from errors.

Features:

- Custom delimiter character
- Quoting character, e.g. `"`
- Escape character
- Multi-line rows
- Error detection (missing delimiter or end-quote)
- Line and column information on error
- Uneven rows
- Lazy (in consuming a source and producing rows)
- Parse a DSV format to good old friends like `DataTable`,
  including header-less support
- Pull (enumerables) & push (observables) parsing

See the [parser test cases](tests/Tests.md) for how DSV data is handled.


[dsv]: https://en.wikipedia.org/wiki/Delimiter-separated_values
[csv]: https://en.wikipedia.org/wiki/Comma-separated_values
[netstd]: https://docs.microsoft.com/en-us/dotnet/standard/net-standard
