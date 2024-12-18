A scheme using json data as code, implemented in C#.

Blog: <https://faster-than-light-memes.xyz/bobscheme-1.html>

The basic lambda calculus kinda thing. There are functions, there are values.
It is tail recursive. There is state via reference cells.

There is a local env and lexical binding.

# Syntax

## Symbols

```
"foo"
```

is the symbol `foo`.

# Tail recursion

I use a `trampoline` to achieve tail recursion, even though dotnet does not.


```sh
dotnet run src/fib-iter.json src/fib.json src/call-fib.json
fib-iter
fib-tail-rec
55
```

# Small core

The Bobscheme runtime has a primitive `create-macro`. I flexed the Lisp muscles by implementing `defmacro`
in Bobscheme user land [Defmacro JSON](src/json/core/1-defmacro.json). Chat gpt provided the implementation.

Now the user can express macros like `when` in terms of `defmacro`.
Making it obvious to you, the Bobscheme user, that you can create any kind of syntax yourself.

# Foo Accumulator

Here: Lexical binding and a clojure-like reference type.

This Bobscheme code defines a function called `make-adder`.
This function returns a function object, that accumulates the sum of all the args it was called with.
Paul Graham was describing this function as a small example code snippet to get a feel for the power of a language ([2002](http://www.paulgraham.com/icad.html)).

```json
    [
      "define",
      "make-adder",
      [
        "lambda", ["n"],
        [
          "let",
          [
            "state", ["atom", "n"]
          ],
          [
            "lambda", ["i"],
            ["swap!", "state", "+", "i"]
          ]
        ]
      ]
    ]
```

Def a machine:
```json
    ["define", "machine", ["make-adder", 0]]
```
Use the machine:
```json
    ["machine", 10]
```
=> 10
```json
    ["machine", 15]
```
=> 25

The Bobscheme code is almost the same as the Clojure code:

```clojure
    (defn make-adder [n]
      (let [state (atom n)]
        (fn [i]
          (swap! state + i))))
```

Because the ideas are the same, we get to call that `dialects` of Lisp.

The Bobscheme is just annoyingly bloated with commata and quotation marks, inheriting from its JSON notation heritage.

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


<!-- # Interop -->

<!-- (ideas, ... ) -->

<!-- Interop would be annoying to get right correctly because of dotnet generics and its complicated type names. -->

<!-- ```json -->
<!-- ["new", "System.Object"] -->

<!-- ["func", ["lambda", ["a"] "a"] ] -->

<!-- ["action", ["lambda", []]] -->

<!-- [".", "method", "obj"] -->
<!-- ``` -->
