A scheme using json data as code, implemented in C#.

# Chat GPT examples

Here are some sample BobScheme code snippets in the form of JSON:

1. Defining a constant value:
```json
["define", "pi", 3.14159]
```

2. Defining a function to compute the square of a number:
```json
["define", "square", ["lambda", ["x"], ["*", "x", "x"]]]
```

3. Computing the sum of squares of two numbers using the square function:
```json
["define", "sum-of-squares", ["lambda", ["a", "b"], ["+", ["square", "a"], ["square", "b"]]]]
```

4. Using the defined sum-of-squares function with example arguments:
```json
["sum-of-squares", 3, 4]
```

Please keep in mind that this is just an example of BobScheme code and may need to be adjusted according to your specific interpreter or syntax requirements.
