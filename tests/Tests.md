# Parser Tests

## Single Row

Suppose:

- delimiter is `,`
- quote is `"`
- escape is `"`
- newline is `\n`

Source:

```
1,2,3
```

Expected:

```
[
    ["1", "2", "3"]
]
```

## Many Rows

Suppose:

- delimiter is `,`
- quote is `"`
- escape is `"`
- newline is `\n`

```
1,2,3
4,5,6
7,8,9
```

```
[
    ["1", "2", "3"],
    ["4", "5", "6"],
    ["7", "8", "9"],
]
```

## Uneven Rows

### Unquoted Fields

Suppose:

- delimiter is `,`
- quote is `"`
- escape is `"`
- newline is `\n`

```
1,2,3
4,5
6
7,8,9
```

```
[
    ["1", "2", "3"],
    ["4", "5"],
    ["6"],
    ["7", "8", "9"],
]
```
