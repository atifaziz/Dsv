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

## Empty Fields

Suppose:

```

,
,,
,,,
foo,
,foo
foo,,
,foo,
,,foo
```

Expected:

```
[
    [""],
    ["", ""],
    ["", "", ""],
    ["", "", "", ""],
    ["foo", ""],
    ["", "foo"],
    ["foo", "", ""],
    ["", "foo", ""],
    ["", "", "foo"],
]
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
    [""],
    [""],
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
    [""],
    [""],
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
    [""],
    [""],
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

## Some Fields on Multiple Lines

Suppose:

- delimiter is `,`
- quote is `"`
- escape is `"`
- newline is `\n`

```
Name,Street,Postal
Axel Burns,nunc@example.com,"P.O. Box 648
7266 Ipsum Street",TJ7 4LC
Akeem Oneill,penatibus@example.com,880-5079 Ipsum St.,IJ44 7TH
Bruce Drake,felis@example.com,"P.O. Box 968
6765 Aliquam Ave",A4 8SZ
Kermit Carr,leo@example.com,Ap #922-804 Urna Rd.,NR09 2LM
Fitzgerald Allison,ullamcorper@example.com,"P.O. Box 117
4098 Erat Street",W1 5US
Zeus Shannon,fusce@example.com,261-3752 Turpis. Rd.,FG69 4CF
```

Expected:

```
[
    ["Name", "Street", "Postal"],
    ["Axel Burns", "nunc@example.com", "P.O. Box 648\n7266 Ipsum Street", "TJ7 4LC"],
    ["Akeem Oneill", "penatibus@example.com", "880-5079 Ipsum St.", "IJ44 7TH"],
    ["Bruce Drake", "felis@example.com", "P.O. Box 968\n6765 Aliquam Ave", "A4 8SZ"],
    ["Kermit Carr", "leo@example.com", "Ap #922-804 Urna Rd.", "NR09 2LM"],
    ["Fitzgerald Allison", "ullamcorper@example.com", "P.O. Box 117\n4098 Erat Street", "W1 5US"],
    ["Zeus Shannon", "fusce@example.com", "261-3752 Turpis. Rd.", "FG69 4CF"],
]
```

## Quotes in Fields

Suppose:

- delimiter is `,`
- quote is `"`
- escape is `"`
- newline is `\n`

```
"foo,""bar"",baz"
"""foo"",bar,baz"
"foo,bar,""baz"""
```

Expected:

```
[
    ["foo,\"bar\",baz"],
    ["\"foo\",bar,baz"],
    ["foo,bar,\"baz\""],
]
```
