# Specification for Shimmer

Shimmer is a dynamic-typed, scripting language following the C family of programming languages.

## Types, Values and Evaluation

The following lists the Shimmer built-in types:

- `number`
- `bool`
- `nil`

| Operators  | Description                                                                                                         |
|:----------:|:--------------------------------------------------------------------------------------------------------------------|
|  `A + B`   | If both operands are numbers, adds `A` by `B`. Otherwise gives runtime error.                                       |
|  `A - B`   | If both operands are numbers, subtracts `A` by `B`. Otherwise gives runtime error.                                  |
|  `A * B`   | If both operands are numbers, multiplies `A` by `B`. Otherwise gives runtime error.                                 |
|  `A / B`   | If both operands are numbers, divides `A` by `B`. `B = 0` or both operands not being numbers gives a runtime error. |
|  `A < B`   | `TRUE` if `A` is less than `B`. Non-number operands gives runtime error.                                            |
|  `A <= B`  | `TRUE` if `A` is less than or equal to `B`. Non-number operands gives runtime error.                                |
|  `A > B`   | `TRUE` if `A` is greater than `B`. Non-number operands gives runtime error.                                         |
|  `A >= B`  | `TRUE` if `A` is greater than or equal to `B`. Non-number operands gives runtime error.                             |
|  `A == B`  | `TRUE` if `A` is equal to `B`.                                                                                      |
|  `A != B`  | `TRUE` if `A` is not equal to `B`.                                                                                  |
|  `A && B`  | Logical `AND`. Evaluates to `A` if `A` is falsy, otherwise `B`.                                                     |
| `A \|\| B` | Logical `OR`. Evaluates to `A` if `A` is truthy, otherwise `B`.                                                     |
|    `!A`    | Logical `NOT`. Evaluates to `TRUE` if `A` is falsy, otherwise `FALSE`.                                              |
|    `-A`    | Unary negation. Gives runtime error if `A` is not a number.                                                         |

In conditional contexts (e.g. `if` and `while`), the truthiness of the condition is evaluated, following Ruby's
definition of truthy and falsy values. The values of `false` and `nil` are considered falsy, and all other values are
considered truthy.

Operator precedence and associativity follow [C](https://en.cppreference.com/w/c/language/operator_precedence.html), as
implied by the [grammar](#grammar) section below where deeper expression-related rules have higher precedence.

| Precedence | Operator          | Description                                                              | Associativity |
|:----------:|:------------------|--------------------------------------------------------------------------|:-------------:|
|   **1**    | `-`               | Unary negation                                                           |     Right     |     
|            | `!`               | Logical `NOT`                                                            |               |
|   **2**    | `*` `/`           | Multiplication and division                                              |     Left      |
|   **3**    | `+` `-`           | Addition and subtraction                                                 |     Left      |
|   **4**    | `<` `<=` `>` `>=` | Relational: Less than, less than equal, greater than, greater than equal |     Left      |
|   **5**    | `!=` `==`         | Relational: Equal, and not equal                                         |     Left      |
|   **6**    | `&&`              | Logical `AND`                                                            |     Left      |
|   **7**    | `\|\|`            | Logical `OR`                                                             |     Left      |

## Grammar

| Notation | Definition                        |
|:--------:|:----------------------------------|
|   `Q*`   | `Q` is repeated one or more times |

```ebnf
expression = logic_or;

logic_or   = logic_and ( "||" logic_and )* ;

logic_and  = equality ( "&&" equality )* ;

equality   = comparison ( ( "==" | "!=" ) comparison )* ;

comparison = term ( ( "<" | "<=" | ">" | ">=" ) term )* ;

term       = factor ( ( "+" | "-" ) factor )* ;
          
factor     = primary ( ( "*" | "/" ) primary )* ;

unary      = ( "-" | "!" ) unary
           | primary

primary    = number
           | grouping
           | bool
           | identifier 
           | "nil" ;
           
(* A number is formed by one or more ASCII digits *)
number     = digit* ;

(* Grouping parenthesis gives a way to raise an expression's precedence to the top *)
grouping   = "(" expression ")" ;

bool       = "true" | "false" ;

identifier = alpha ( alpha | digit )* ;

alpha      = ( "a" ... "z") | "A" ... "Z" | "_" ;

digit      = "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" ;
```

