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

## Empty

Suppose:

```
```

Expected:

```
[]
```

## Blank Rows

### At the Beginning

Suppose:

```


1,2,3
4,5,6
```

Expected:

```
[
    [],
    [],
    ["1", "2", "3"],
    ["4", "5", "6"],
]
```

### In the Middle

Suppose:

```
1,2,3


4,5,6
```

Expected:

```
[
    ["1", "2", "3"],
    [],
    [],
    ["4", "5", "6"],
]
```

### In the Middle

Suppose:

```
1,2,3
4,5,6


```

Expected:

```
[
    ["1", "2", "3"],
    ["4", "5", "6"],
    [],
    [],
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

Expected:

```
[
    ["1", "2", "3"],
    ["4", "5"],
    ["6"],
    ["7", "8", "9"],
]
```

### Quoted Fields

Suppose:

- delimiter is `,`
- quote is `"`
- escape is `"`
- newline is `\n`

```
"foo",bar,baz
"foo,bar",baz
"foo,bar,baz"
foo,"bar,baz"
foo,bar,"baz"
"foo","bar","baz"
```

Expected:

```
[
    ["foo", "bar", "baz"],
    ["foo,bar", "baz"],
    ["foo,bar,baz"],
    ["foo", "bar,baz"],
    ["foo", "bar", "baz"],
    ["foo", "bar", "baz"],
]
```
