# Specification for Shimmer

Shimmer is a dynamic-typed, scripting language following the C family of programming languages.

## Language Features

Shimmer supports the built-in types: `number`, `bool`, `string`, `nil`.

### Expressions and Evaluation

|  Operator   | Description                                                                                                                 |
|:-----------:|:----------------------------------------------------------------------------------------------------------------------------|
|   `A + B`   | If both operands are numbers, adds `A` by `B`. If both are strings, gives the concatenation. Otherwise gives runtime error. |
|   `A - B`   | If both operands are numbers, subtracts `A` by `B`. Otherwise gives runtime error.                                          |
|   `A * B`   | If both operands are numbers, multiplies `A` by `B`. Otherwise gives runtime error.                                         |
|   `A / B`   | If both operands are numbers, divides `A` by `B`. `B = 0` or both operands not being numbers gives a runtime error.         |
|   `A % B`   | If both operands are numbers, gives the remainder of `A` divided by `B`. Otherwise gives runtime error.                     |
|   `A < B`   | `TRUE` if `A` is less than `B`. Non-number operands gives runtime error.                                                    |
|  `A <= B`   | `TRUE` if `A` is less than or equal to `B`. Non-number operands gives runtime error.                                        |
|   `A > B`   | `TRUE` if `A` is greater than `B`. Non-number operands gives runtime error.                                                 |
|  `A >= B`   | `TRUE` if `A` is greater than or equal to `B`. Non-number operands gives runtime error.                                     |
|  `A == B`   | `TRUE` if `A` is equal to `B`.                                                                                              |
|  `A != B`   | `TRUE` if `A` is not equal to `B`.                                                                                          |
|  `A && B`   | Logical `AND`. Evaluates to `A` if `A` is falsy, otherwise `B`.                                                             |
| `A \|\| B`  | Logical `OR`. Evaluates to `A` if `A` is truthy, otherwise `B`.                                                             |
|    `!A`     | Logical `NOT`. Evaluates to `TRUE` if `A` is falsy, otherwise `FALSE`.                                                      |
|    `-A`     | Unary negation. Gives runtime error if `A` is not a number.                                                                 |
|  `A, B, C`  | Comma operator, evaluates to `C`. Allows for comma-separated series of expressions where a single expression is expected.   |
| `A ? B : C` | Ternary (conditional), if `A` is truthy then evaluate to `B` otherwise `C`.                                                 |
|   `A = B`   | Assignment. Assign the value `B` to variable `A`.                                                                           |

In conditional contexts (e.g. `if` and `while`), the truthiness of the condition is evaluated, following Ruby's
definition of truthy and falsy values. The values of `false` and `nil` are considered falsy, and all other values are
considered truthy.

Operator precedence and associativity follow [C](https://en.cppreference.com/w/c/language/operator_precedence.html), as
implied by the [grammar](#grammar) section below where deeper expression-related rules have higher precedence.

| Precedence | Operator          | Description                                                              | Associativity |
|:----------:|:------------------|--------------------------------------------------------------------------|:-------------:|
|   **1**    | `-`               | Unary negation                                                           | Right to left |     
|            | `!`               | Logical `NOT`                                                            |               |
|   **2**    | `*` `/`           | Multiplication and division                                              | Left to right |
|   **3**    | `+` `-` `%`       | Addition, subtraction and remainder                                      | Left to right |
|   **4**    | `<` `<=` `>` `>=` | Relational: Less than, less than equal, greater than, greater than equal | Left to right |
|   **5**    | `!=` `==`         | Relational: Equal, and not equal                                         | Left to right |
|   **6**    | `&&`              | Logical `AND`                                                            | Left to right |
|   **7**    | `\|\|`            | Logical `OR`                                                             | Left to right |
|   **8**    | `? :`             | Ternary conditional                                                      | Right to left |
|   **9**    | `=`               | Assignment                                                               | Right to left |
|   **10**   | `,`               | Comma operator                                                           | Left to right |

### Comments

Shimmer supports both `//` inline comments and `/**/` block comments. Nested block comments are not supported.

### Variables

When declaring variables, they can be optionally given an initializer. If omitted, the variable is implicitly
initialized with `nil`.

```shimmer
var x;
print x;  // nil
```

Variables cannot be redefined if the variable name has already been defined in the same scope. 

```shimmer
var x = 10;
var x = 20; // Runtime error: "Variable 'x' already defined in this scope."
```

Though, a variable can *shadow* an existing variable with the same name from an outer scope.

```shimmer
var x = "global";
{
  var x = "inner";
  print x; // "inner"
}
```

### Control Flow

Shimmer's control flow constructs contain no surprises, similar to other programming languages:
- `if` and `if-else` statements
- `while`, `for`, and `do-while` loops

Examples:
```shimmer
if (true) { print "1"; }

if (true) { print "1"; } else { print "2"; }

while (true) { print "Infinite Loop"; }

for (var i = 0; i < 10; i = i + 1) { print i + 1; }

do { print "Infinite Loop"; } while (true);
```

The statements to the execution branches don't necessarily have to be blocks:

```shimmer
if (true) print "1";
```

For the [Dangling Else](https://en.wikipedia.org/wiki/Dangling_else) problem, the `else` is bound to the nearest
`if` that precedes it. For example, `if (cond1) if (cond2) s1; else s2;` - `s2` is bound to `if (cond2) s1`.

## Grammar

We use EBNF-like notation to describe the lexical and syntax grammars which the scanner and parser process respectively.

| Notation | Definition                                      |
|:--------:|:------------------------------------------------|
|   `Q*`   | `Q` is repeated zero or more times.             |
|   `Q?`   | `Q` is repeated zero or one time, but not more. |

### Lexical Grammar

```ebnf
(* A number is formed by one or more ASCII digits - for now *)
NUMBER     = DIGIT* ;

(* i.e. any set of characters but quote surround by quote characters *)
STRING     = " \" { all characters but " } \" " ;

IDENTIFIER = ALPHA ( ALPHA | DIGIT )* ;

ALPHA      = ( "a" ... "z") | "A" ... "Z" | "_" ;

DIGIT      = "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" ;
```

### Syntax Grammar

```ebnf
program     = declaration* EOF;

declaration = varDecl
            | statement ;
            
varDecl     = "var" IDENTIFIER ( "=" expression )? ";" ;

statement   = printStmt
            | exprStmt
            | ifStmt
            | whileStmt
            | forStmt
            | doWhileStmt
            | breakStmt
            | contStmt
            | block ;
            
printStmt   = "print" expression ";" ;

exprStmt    = expression ";" ;

ifStmt      = "if" "(" expression ")" statement ( "else" statement )? ;

whileStmt   = "while" "(" expression ")" statement ;

forStmt     = "for" "("
              ( varDecl | exprStmt | ";" )    (* initializer *)
              expression? ";"                 (* condition *)
              expression? ")"                 (* increment *)
              statement ;                     (* loop body *)
              
doWhileStmt = "do" statement "while" "(" expression ")" ";" ;
              
breakStmt   = "break" ";" ;

contStmt    = "continue" ";" ;

block       = "{" declaration* "}" ;

expression  = comma ;

comma       = assignment ( "," assignment )* ;

assignment  = IDENTIFIER "=" assignment
            | conditional ;

conditional = logic_or ( "?" expression ":" conditional )? ;

logic_or    = logic_and ( "||" logic_and )* ;

logic_and   = equality ( "&&" equality )* ;

equality    = comparison ( ( "==" | "!=" ) comparison )* ;

comparison  = term ( ( "<" | "<=" | ">" | ">=" ) term )* ;

term        = factor ( ( "+" | "-" | "%" ) factor )* ;
          
factor      = primary ( ( "*" | "/" ) primary )* ;

unary       = ( "-" | "!" ) unary
            | primary

primary     = NUMBER
            | grouping
            | "true"
            | "false"
            | IDENTIFIER
            | "nil" ;

(* Grouping parenthesis gives a way to raise an expression's precedence to the top *)
grouping    = "(" expression ")" ;
```

