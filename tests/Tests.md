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

```json
[
    { ln: 1, row: ["1", "2", "3"] }
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

Expected:

```json
[
    { ln: 1, row: ["1", "2", "3"] },
    { ln: 2, row: ["4", "5", "6"] },
    { ln: 3, row: ["7", "8", "9"] },
]
```

## Empty

Suppose:

```
```

Expected:

```json
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

```json
[
    { ln: 1, row: [""] },
    { ln: 2, row: ["", ""] },
    { ln: 3, row: ["", "", ""] },
    { ln: 4, row: ["", "", "", ""] },
    { ln: 5, row: ["foo", ""] },
    { ln: 6, row: ["", "foo"] },
    { ln: 7, row: ["foo", "", ""] },
    { ln: 8, row: ["", "foo", ""] },
    { ln: 9, row: ["", "", "foo"] },
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

```json
[
    { ln: 1, row: [""] },
    { ln: 2, row: [""] },
    { ln: 3, row: ["1", "2", "3"] },
    { ln: 4, row: ["4", "5", "6"] },
]
```

### In the Middle

Suppose:

```
1,2,3


4,5,6
```

Expected:

```json
[
    { ln: 1, row: ["1", "2", "3"] },
    { ln: 2, row: [""] },
    { ln: 3, row: [""] },
    { ln: 4, row: ["4", "5", "6"] },
]
```

### In the Middle

Suppose:

```
1,2,3
4,5,6


```

Expected:

```json
[
    { ln: 1, row: ["1", "2", "3"] },
    { ln: 2, row: ["4", "5", "6"] },
    { ln: 3, row: [""] },
    { ln: 4, row: [""] },
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

```json
[
    { ln: 1, row: ["1", "2", "3"] },
    { ln: 2, row: ["4", "5"] },
    { ln: 3, row: ["6"] },
    { ln: 4, row: ["7", "8", "9"] },
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

```json
[
    { ln: 1, row: ["foo", "bar", "baz"] },
    { ln: 2, row: ["foo,bar", "baz"] },
    { ln: 3, row: ["foo,bar,baz"] },
    { ln: 4, row: ["foo", "bar,baz"] },
    { ln: 5, row: ["foo", "bar", "baz"] },
    { ln: 6, row: ["foo", "bar", "baz"] },
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

```json
[
    { ln:  1, row: ["Name", "Street", "Postal"] },
    { ln:  2, row: ["Axel Burns", "nunc@example.com", "P.O. Box 648\n7266 Ipsum Street", "TJ7 4LC"] },
    { ln:  4, row: ["Akeem Oneill", "penatibus@example.com", "880-5079 Ipsum St.", "IJ44 7TH"] },
    { ln:  5, row: ["Bruce Drake", "felis@example.com", "P.O. Box 968\n6765 Aliquam Ave", "A4 8SZ"] },
    { ln:  7, row: ["Kermit Carr", "leo@example.com", "Ap #922-804 Urna Rd.", "NR09 2LM"] },
    { ln:  8, row: ["Fitzgerald Allison", "ullamcorper@example.com", "P.O. Box 117\n4098 Erat Street", "W1 5US"] },
    { ln: 10, row: ["Zeus Shannon", "fusce@example.com", "261-3752 Turpis. Rd.", "FG69 4CF"] },
]
```

## Disallow Rows on Multiple Lines

Suppose:

- newline is `null`

```
Name,Street,Postal
Axel Burns,nunc@example.com,"P.O. Box 648
7266 Ipsum Street",TJ7 4LC
Akeem Oneill,penatibus@example.com,880-5079 Ipsum St.,IJ44 7TH
```

Throws:

```
System.FormatException: Unclosed quoted field (line #2, col #42).
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
"""foo""",bar,baz
foo,"""bar""",baz
foo,bar,"""baz"""
""""""
"""""",""""""
"""""","""""",""""""
"""""",
,""""""
,"""""",
```

Expected:

```json
[
    { ln:  1, row: ["foo,\"bar\",baz"] },
    { ln:  2, row: ["\"foo\",bar,baz"] },
    { ln:  3, row: ["foo,bar,\"baz\""] },
    { ln:  4, row: ["\"foo\"", "bar", "baz"] },
    { ln:  5, row: ["foo", "\"bar\"", "baz"] },
    { ln:  6, row: ["foo", "bar", "\"baz\""] },
    { ln:  7, row: ["\"\""] },
    { ln:  8, row: ["\"\"", "\"\""] },
    { ln:  9, row: ["\"\"", "\"\"", "\"\""] },
    { ln: 10, row: ["\"\"", ""] },
    { ln: 11, row: ["", "\"\""] },
    { ln: 12, row: ["", "\"\"", ""] },
]
```

## Skip Blanks

Suppose:

- blanks = `skip`

```

1,2,3

4,5,6

7,8,9

```

Expected:

```json
[
    { ln: 2, row: ["1", "2", "3"] },
    { ln: 4, row: ["4", "5", "6"] },
    { ln: 6, row: ["7", "8", "9"] },
]
```

## Without Quoting

Suppose:

- quote is `null`

```
"foo",bar,baz
"foo,bar",baz
"foo
bar
baz"
"foo, ""bar"", baz"
```

Expected:

```json
[
    { ln: 1, row: ["\"foo\"", "bar", "baz" ] },
    { ln: 2, row: ["\"foo", "bar\"", "baz" ] },
    { ln: 3, row: ["\"foo"] },
    { ln: 4, row: ["bar"] },
    { ln: 5, row: ["baz\""] },
    { ln: 6, row: ["\"foo", " \"\"bar\"\"", " baz\""] },
]
```

## Errors

### Unclosed Blank Quoted Field

Suppose:

```
"
```

Throws:

```
System.FormatException: Unclosed quoted field (line #1, col #1).
```

### Unclosed Non-Blank Quoted Field

Suppose:

```
foo,bar
"foo,bar
```

Throws:

```
System.FormatException: Unclosed quoted field (line #2, col #8).
```

### Missing Delimiter

Suppose:

```
"foo" bar
```

Throws:

```
System.FormatException: Missing delimiter (line #1, col #7).
```
