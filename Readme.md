A scheme using json data as code, implemented in C#.

# Syntax

## Symbols

```
"foo"
```

is the symbol `foo`.

# tailr recursive fib

```sh
dotnet run src/fib-iter.json src/fib.json src/call-fib.json
fib-iter
fib-tail-rec
55
```

# Chat GPT examples

Here are some sample BobScheme code snippets in the form of JSON:

1. Defining a constant value:

```json
["define", "pi", 3.14159]
```

2. Defining a function to compute the square of a number
```json
["define", "square", ["lambda", ["x"], ["*", "x", "x"]]]
```

3. Computing the sum of squares of two numbers using the square function
```json
["define", "sum-of-squares", ["lambda", ["a", "b"], ["+", ["square", "a"], ["square", "b"]]]]
```

4. Using the defined sum-of-squares function with example arguments
```json
["sum-of-squares", 3, 4]
```

# Atom (like Clojure atom)

```json
["define", "make-adder", ["lambda", ["n"], ["let", ["state", ["atom", "n"]], ["lambda", ["i"], ["swap!", "state", "+",  "i"]]]]]

["define", "machine", ["make-adder", 0]]

["machine", 10]
```


# Interop

(ideas)

```json
["new", "System.Object"]

["func", ["lambda", ["a"] "a"] ]

["action", ["lambda", []]]

[".", "method", "obj"]
```


# GPT factorial

```json
["define", "fact", 
   ["lambda", ["n"], 
      ["if", ["=", "n", 0], 
         1, 
         ["*", "n", ["fact", ["-", "n", 1]]]
      ]
   ]
]
```
