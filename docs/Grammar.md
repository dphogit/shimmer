# Grammar

| Notation | Definition                        |
|----------|:----------------------------------|
| `Q*`     | `Q` is repeated one or more times |

```ebnf
expression = term

term       = primary (("+" | "-" ) primary 
           | primary ;

primary    = number

(* A number is formed by one or more ASCII digits *)
number     = digit* ;

digit      = "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" ;
```
