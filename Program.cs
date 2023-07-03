using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

var random = new Random();

string[] prompts = { "user> ", "what next?> ", "what is my purpose?> ", "say the next piece of the program> " };

Dictionary<String, PrimitiveProcedure> primitiveOperands = new();

Object Nil = new System.Object();

// env
// { "nil" Nil }

// [ "let", [ "a", 10 ] body]
// [ [ "lambda", ["a"] body ], 10 ]

// expand environment
// temp local env

// begin
// form1

var _globalEnv = ImmutableDictionary<String, Object>.Empty;

string PrintStr(Object o)
{
    if (o == Nil)
    {
        return "nil";
    }
    // if (o is IEnumerable<JToken> e) {
    //     return (new JArray(e.ToArray())).ToString();
    // }
    return o.ToString();
}

primitiveOperands.Add("+", new PrimitiveProcedure("+", (toAdd) =>
{
    long sum = 0;
    foreach (var arg in toAdd)
    {
        if (arg is long)
            sum += (long)arg;
        else
            throw new InvalidOperationException($"Expected number, found {arg.GetType()}");
    }
    return sum;
})
);


primitiveOperands.Add("printEnv", new PrimitiveProcedure("printEnv", list =>
{

    Console.WriteLine("{");
    foreach (var kvp in _globalEnv)
    {
        var s = PrintStr(kvp.Value);
        Console.WriteLine(kvp.Key + " - " + s);
    }
    Console.WriteLine("}");
    return Nil;
}));


primitiveOperands.Add("print", new PrimitiveProcedure("print", list =>
{
    Console.Write(String.Join(" ", list.Select(PrintStr)));
    Console.WriteLine();
    return Nil;
}));

// env?
primitiveOperands.Add("eval", new PrimitiveProcedure("eval", list =>
{
    return Eval((JToken)list.First(), _globalEnv);
}));



// null? 
// nil? 

foreach (var kvp in primitiveOperands)
{
    _globalEnv = _globalEnv.SetItem(kvp.Key, kvp.Value);
}


_globalEnv = _globalEnv.SetItem("nil", Nil);


if (args.Length > 0)
{


    // I want to wrap implicity with do but the commas
    string json = File.ReadAllText(args[0]);
    var expr = Read(json);
    foreach (var e in expr.AsEnumerable())
    {
        Console.WriteLine(e);

    }

    var v = Eval(expr, _globalEnv);
    Print(v);

}

else

{
    Repl();

}

T RandNth<T>(T[] lst)
{
    int seed = (int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    int randomIndex = random.Next(0, lst.Count());
    var randomElement = lst[randomIndex];
    return randomElement;
}

void Repl()
{
    while (true)
    {
        var prompt = RandNth(prompts);
        Console.Write(prompt);
        var input = Console.ReadLine();
        try
        {
            if (input != "")
            {
                var v = Eval(Read(input), _globalEnv);
                Print(v);
            }

        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }
}

// Reader
JToken Read(string expr)
{
    try
    {
        JToken token = JToken.Parse(expr);
        return token;
    }
    catch (Exception e)
    {
        Console.Error.WriteLine("Error reading: " + e.Message);
        throw;
    }
}


// Printer
void Print(Object o)
{
    if (o == Nil)
    {
        Console.WriteLine("nil");
    }
    else
    {
        Console.WriteLine(o);
    }
}

bool IsString(JToken expr)
{
    if (expr.Type == JTokenType.String)
    {
        var s = expr.ToObject<String>();
        return s.FirstOrDefault() == '\'';
    }
    return false;
}


bool IsSymbol(JToken expr)
{
    return expr.Type == JTokenType.String && !IsString(expr);
}

string SymbolName(JToken expr)
{
    if (expr.Type == JTokenType.String)
    {
        var s = expr.ToObject<String>();
        return s;
    }
    throw new Exception("not a symbol? " + expr);
}

string StringValue(JToken expr)
{
    if (expr.Type == JTokenType.String)
    {
        var s = expr.ToObject<String>();
        return s;
        // string substring = s.Substring(1, s.Length - 1);
        // return substring;
    }
    throw new Exception("not a string? " + expr);
}

bool IsSelfEvaluating(JToken expr)
{


    if (expr.Type == JTokenType.Object)
    {
        return false;
    }
    else if (expr.Type == JTokenType.Array)
    {
        return false;
    }
    else if (IsString(expr))
    {
        return true;
    }

    else if (IsSymbol(expr))
    {
        return false;
    }
    else
    {
        // Process primitive value (string, number, boolean, null)

        return true;
    }
}

bool IsNil(JToken expr)
{
    if (expr.Type == JTokenType.Array)
    {
        var objArr = expr.ToObject<JToken[]>();
        if (objArr.Count() == 0)
        {
            return true;
        }
    }
    return false;
}

bool IsApplication(JToken expr)
{
    return expr.Type == JTokenType.Array;
}

JToken Operator(JToken expr)
{
    var objArr = expr.ToObject<JToken[]>();
    return objArr.First();
}

List<Object> ListOfValues(IEnumerable<JToken> expr, ImmutableDictionary<String, Object> env)
{
    return expr.Select((e) => Eval(e, env)).ToList();
}

IEnumerable<JToken> Operands(JToken expr)
{
    var objArr = expr.ToObject<JToken[]>();
    return objArr.Skip(1);
}

bool IsOperation(JToken expr, String s)
{
    if (expr.Type == JTokenType.Array)
    {
        var objArr = expr.ToObject<JToken[]>();
        var op = objArr.First();
        if (op.ToObject<String>() == s)
        {

            return true;
        }

    }
    return false;
}

bool IsAssignment(JToken expr)
{
    return IsOperation(expr, "define");
}

bool IsQuote(JToken expr)
{
    return IsOperation(expr, "quote") || IsOperation(expr, "'");
}

// progn, begin, clojure: do
bool IsDo(JToken expr)
{
    return IsOperation(expr, "do");
}

Object EvalSequencially(JToken expr, ImmutableDictionary<String, Object> env)
{

    Object ret = Nil;
    foreach (var e in expr.Skip(1).ToArray())
    {
        var newEnv = _globalEnv;
        foreach (var kvp in env) {
            newEnv = newEnv.SetItem(kvp.Key,kvp.Value);
        }
        ret = Eval(e, newEnv);
    }
    return ret;

}

Object EvalSequence(IEnumerable<JToken> expressions, ImmutableDictionary<String, Object> env)
{

    Object ret = Nil;
    foreach (var e in expressions)
    {
        ret = Eval(e, env);
    }
    return ret;

}


String DefineVariable(String symbol, Object defVal, ImmutableDictionary<String, Object> env)
{
    _globalEnv = _globalEnv.SetItem(symbol, defVal);
    return symbol;
}

// the "foo" in
// ["define", "foo", 10]
String DefinitionVariable(JToken expr)
{
    var objArr = expr.ToObject<JToken[]>();
    var symbolExpr = objArr[1];
    if (!IsSymbol(symbolExpr))
    {
        throw new Exception("Expected symbol: " + symbolExpr);
    }
    return symbolExpr.ToObject<String>();
}

// the "foo" in
// ["define", "foo", 10]
Object DefinitionValue(JToken expr, ImmutableDictionary<String, Object> env)
{
    var objArr = expr.ToObject<JToken[]>();
    var vExpr = objArr[2];
    return Eval(vExpr, env);
}

String EvalAssignment(JToken expr, ImmutableDictionary<String, Object> env)
{
    return DefineVariable(DefinitionVariable(expr), DefinitionValue(expr, env), env);

}

bool IsVariable(Object o)
{
    if (o is JToken jt && IsString(jt))
    {
        return false;
    }
    else if (o is JToken jt1 && IsSymbol(jt1))
    {
        return true;
    }
    if (o is String s)
    {
        return true;
    }
    return false;
}

Object LookupVariable(JToken expr, ImmutableDictionary<String, Object> env)
{
    var s = SymbolName(expr);
    if (!env.ContainsKey(s))
    {
        throw new Exception("Could not resolve symbol: " + s);
    }
    return env[SymbolName(expr)];
}

bool IsIf(JToken expr)
{
    return IsOperation(expr, "if");
}

bool IsEmpty(Object o)
{
    if (o is JToken jt && jt.Type == JTokenType.Array)
    {
        var objArr = jt.ToObject<JToken[]>();
        return objArr.Count() == 0;
    }
    else if (o is IEnumerable<Object> e)
    {
        return e.Count() == 0;
    }
    return false;
}

bool IsFalsy(Object o)
{
    return o is Boolean b && b == false || o == Nil || IsEmpty(o);
}


bool IsTruthy(Object o)
{
    return !IsFalsy(o);
}

bool IsLambda(JToken expr)
{
    return IsOperation(expr, "lambda");
}


// [ "lambda", [], body2,  body1  ]
// [ "lambda", [[[], 10] [["a"], ["+", "a", 10]]]]

// [ "lambda", ["a", "b"], ]
// [ "lambda", ["a", "&", "b"]]


IJEnumerable<JToken> LambdaArglist(JToken expr)
{
    return expr.AsJEnumerable().Skip(1).First().AsJEnumerable();
}

IJEnumerable<JToken> LambdaBody(JToken expr)
{
    return expr.AsJEnumerable().Skip(2).AsJEnumerable();
}

Object EvaluateLambda(JToken expr,  ImmutableDictionary<String, Object> env)
{
    var arglist = LambdaArglist(expr);
    var name = "anonymous-procedure" + "<" + arglist.Count() + ">";
    var body = LambdaBody(expr);
    return new CompoundProcedure(name, arglist, body, env);
    // return new CompoundProcedureGroup("anon");
}


ImmutableDictionary<String, Object> ExpandEnv(ImmutableDictionary<String, Object> env, String k, Object v) {
    Console.WriteLine("ExpandEnv: " + k + " ( " + k.GetType().Name + ")" + " " + v);
    return env.SetItem(k,v);
}

IEnumerable<String> ProcedureParameters(CompoundProcedure proc) {
    return proc.arglist.AsEnumerable().Select((JToken e) => e.ToObject<String>());
}

// todo &rest

// 1. enhance the environment with bindings arglist -> arguements
ImmutableDictionary<String, Object> ExtendEnvironment(
    IEnumerable<String> parameters,
    List<Object> arguments,
    ImmutableDictionary<String, Object> env) {
    foreach (var pair in parameters.Zip(arguments)) {
        env = env.Add(pair.First, pair.Second);
    }

    return env;
}



Object ApplyCompountProcedure(CompoundProcedure proc, List<Object> arguments, ImmutableDictionary<String, Object> env) {
    // Console.WriteLine(arguments.First());
    var newEnv = ExtendEnvironment(ProcedureParameters(proc), arguments, env);
    return EvalSequence(proc.body, newEnv);
}


// [ "if", "predicate", "consequence", "alternative" ]

Object EvaluateIf(JToken expr, ImmutableDictionary<String, Object> env)
{
    var objArr = expr.ToObject<JToken[]>();
    var predExpr = objArr[1];
    var consequenceExpr = objArr[2];
    var alternativeExpr = objArr[3];
    if (IsTruthy(Eval(predExpr, env)))
    {
        return Eval(consequenceExpr, env);
    }
    else
    {
        return Eval(alternativeExpr, env);
    }
}


Object EvaluateQuote(JToken expr)
{
    return expr.AsJEnumerable().Skip(1).First();
}


// Metacircular evaluator
Object Eval(JToken expr, ImmutableDictionary<String, Object> env)
{
    if (IsSelfEvaluating(expr))
    {
        var token = (JToken)expr;
        if (IsString(expr))
        {
            return StringValue(expr);
        }
        var o = token.ToObject<Object>();
        return o;
    }
    if (IsQuote(expr))
    {
        return EvaluateQuote(expr);
    }
    if (IsNil(expr))
    {
        return Nil;
    }
    if (IsAssignment(expr))
    {
        return EvalAssignment(expr, env);
    }
    if (IsDo(expr)) {
        return EvalSequencially(expr, env);
    }
         
    if (IsVariable(expr))
    {
        return LookupVariable(expr, env);
    }
    if (IsIf(expr))
    {
        return EvaluateIf(expr, env);
    }
    if (IsLambda(expr))
    {
        return EvaluateLambda(expr,env);
    }
    if (IsApplication(expr))
    {
        return
            Apply(Eval(Operator(expr), env),
                  ListOfValues(Operands(expr), env),
                  env);
    }
    else
    {
        Console.Error.WriteLine("Unknown expression type -- EVAL", expr);
    }

    return null;
}


// evaluated args
Object Apply(Object procedure, List<Object> arguments, ImmutableDictionary<String, Object> env)
{
    if (procedure is PrimitiveProcedure proc)
    {
        return proc.proc.Invoke(arguments);
    }
    if (procedure is CompoundProcedure cproc) {

        return ApplyCompountProcedure(cproc, arguments, env);
    }
    else
    {
        throw new Exception("Unknown procedure type -- APPLY " + procedure);
    }

}

// env local
// lambda

record PrimitiveProcedure(String name, Func<List<Object>, Object> proc);

record Symbol(String name);

record CompoundProcedure(String name, IJEnumerable<JToken> arglist, IJEnumerable<JToken> body,
                         ImmutableDictionary<String, Object> env);

record CompoundProcedureGroup(String name, Dictionary<int, CompoundProcedure> procedures);

