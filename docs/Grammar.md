# Grammar

| Notation | Definition                        |
|----------|:----------------------------------|
| `Q*`     | `Q` is repeated one or more times |

```ebnf
(* The grammar respects operator precedence, deeper expression-related rules have higher precedence *)
expression = term ;

equality   = comparison ( ( "==" | "!=" ) comparison )* ;

comparison = term ( ( "<" | "<=" | ">" | ">=" ) term )* ;

term       = factor ( ( "+" | "-" ) factor )* ;
          
factor     = primary ( ( "*" | "/" ) primary )* ;

primary    = number
           | grouping
           | bool
           | identifier ;

(* A number is formed by one or more ASCII digits *)
number     = digit* ;

(* Grouping parenthesis gives a way to raise an expression's precedence to the top *)
grouping   = "(" expression ")" ;

bool       = "true" | "false" ;

identifier = alpha ( alpha | digit )* ;

alpha      = ( "a" ... "z") | "A" ... "Z" | "_" ;

digit      = "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" ;
```
