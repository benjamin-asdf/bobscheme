using clojure.AtomRef;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

namespace Bobscheme.Lang.RT;

public record PrimitiveProcedure(string name, Func<IEnv<string, Object>, IEnumerable<Object>, Object> proc);

public record Symbol(string name);

public record CompoundProcedure(string name,
    IJEnumerable<JToken> arglist,
    IJEnumerable<JToken> body,
                         IEnv<string, Object> env,
    IImmutableDictionary<string, object> _meta) : IObj
{

    public IImmutableDictionary<string, object> meta()
    {
        return this._meta;
    }

    public IObj withMeta(IImmutableDictionary<string, object> meta)
    {
        return new CompoundProcedure(name, arglist, body, env, meta);
    }

}

public record CompoundProcedureGroup(string name, Dictionary<int, CompoundProcedure> procedures);


public static class RT
{

    public static Random random = new Random();

    public static Dictionary<string, PrimitiveProcedure> primitiveOperators = new();

    public record NilType() : IPrintable
    {
        public string PrintStr()
        {
            return "nil";
        }
    }

    public static Object Nil = new NilType();

    public static IEnv<string, Object> _globalEnv = new ImmutableEnv(ImmutableDictionary<string, object>.Empty);

    static bool IsThunk(Object o)
    {
        return o is Func<Object>;
    }

    static Object Trampoline(Object k)
    {
        while (IsThunk(k))
        {
            k = ((Func<Object>)k)();
        }
        return k;
    }

    public static IImmutableDictionary<string, Object> meta(Object o)
    {
        if (o is IMeta m)
        {
            return m.meta();
        }
        return null;
    }


    // Printer
    public static void Print(Object o)
    {
        Printer.Print(o);
    }


    static IEnv<K, V> Merge<K, V>(IEnv<K, V> env, params IEnv<K, V>[] dictionaries)
    {
        foreach (var dict in dictionaries)
        {
            foreach (var kvp in dict)
            {
                env = env.Extend(kvp.Key, kvp.Value);
            }
        }
        return env;



    }

    public static Object Load(string file)
    {
        string json = File.ReadAllText(file);
        var expr = RT.Read(json);
        var v = RT.Eval(expr, RT._globalEnv);
        return v;
    }

    static RT()

    {

        primitiveOperators.Add("+", new PrimitiveProcedure("+", (env, toAdd) =>
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

        primitiveOperators.Add("-", new PrimitiveProcedure("-", (env, list) =>
        {

            var toSubtract = list.ToList();
            if (toSubtract.Count == 0) throw new InvalidOperationException("No arguments supplied to '-'");

            long substractionResult = 0;
            bool isFirst = true;
            foreach (var arg in toSubtract)
            {
                if (arg is long)
                {
                    if (isFirst)
                    {
                        substractionResult = (long)arg;
                        isFirst = false;
                    }
                    else
                    {
                        substractionResult -= (long)arg;
                    }
                }
                else
                    throw new InvalidOperationException($"Expected number, found {arg.GetType()}");
            }
            return substractionResult;
        })
        );

        primitiveOperators.Add("*", new PrimitiveProcedure("*", (env, toMultiply) =>
        {
            if (toMultiply.Any(x => !(x is long)))
                throw new InvalidOperationException($"Expected number(s), found {string.Join(", ", toMultiply.Select(x => x.GetType()))}");

            return toMultiply.Cast<long>().Aggregate((product, next) => product * next);
        }));

        primitiveOperators.Add("printEnv", new PrimitiveProcedure("printEnv", (env, list) =>
        {
            Print(_globalEnv);
            return Nil;
        }));


        primitiveOperators.Add("atom", new PrimitiveProcedure("atom", (env, list) =>
        {
            var v = list.First();
            return new Atom<Object>(v);

        }));

        primitiveOperators.Add("swap!", new PrimitiveProcedure("swap!", (env, list) =>
        {
            var o = list.First();
            if (o is not IAtom<Object> atom)
            {
                throw new Exception("swap expects an atom as the first arg");
            }
            var theFunction = list.Skip(1).First();
            var theArgs = list.Skip(2);

            return atom.swap((v) =>
            {
                var args = theArgs.Prepend(v).ToList();
                return Apply(theFunction, args, env);
            });

        }));


        primitiveOperators.Add("print", new PrimitiveProcedure("print", (env, list) =>
        {
            Print(list);
            return Nil;
        }));

        primitiveOperators.Add("eval", new PrimitiveProcedure("eval", (env, list) =>
        {
            return Eval((JToken)list.First(), env);
        }));

        primitiveOperators.Add("=", new PrimitiveProcedure("=", (env, list) =>
        {
            var first = list.First();
            return list.Skip(1).All(v => Equals(first, v));
        }));

        primitiveOperators.Add("first", new PrimitiveProcedure("first", (env, list) => list.Count() > 0 ? list.First() : Nil));
        primitiveOperators.Add("rest", new PrimitiveProcedure("rest", (env, list) => list.Count() > 0 ? list.Skip(1).ToList() : new List<Object>()));

        // used to make a pair also

        primitiveOperators.Add("list", new PrimitiveProcedure("list", (env, list) => new JArray(list.Select(o =>
        {
            if (o == Nil)
            {
                return "nil";
            }
            else
            {
                return o;
            }
        }))));

        primitiveOperators.Add("prepend-one", new PrimitiveProcedure("prepend", (env, args) =>
        {

            var list = args.ToList();
            if (list.Count != 2 || !(list[1] is IEnumerable<Object>))
                throw new InvalidOperationException("prepend-one expects an item and a list");

            var output = ((IEnumerable<Object>)list[1]).ToList();
            output.Insert(0, list[0]);
            output = (List<object>)output.Select(o =>
            {
                if (o == Nil)
                {
                    return "nil";
                }
                else
                {
                    return o;
                }
            });

            return new JArray(output);

        }));

        primitiveOperators.Add("macroexpand1", new PrimitiveProcedure("macroexpand1", (env, list) =>
        {
            if (list.Count() != 1)
            {
                throw new Exception("Wrong number of args to macroexpand1" + list.Count());
            }
            var expr = list.First();
            return macroexpand1((JToken)expr);

        }));

        // null? 
        // nil? 

        foreach (var kvp in primitiveOperators)
        {
            _globalEnv = _globalEnv.Extend(kvp.Key, kvp.Value); ;
        }
        _globalEnv = _globalEnv.Extend("nil", Nil); ;
        _globalEnv = _globalEnv.Extend("*global-env*", _globalEnv); ;

    }




    // todo: with-meta
    // meta


    // Reader
    public static JToken Read(string expr)
    {
        try
        {
            JToken token = JToken.Parse(expr);
            return token;
        }
        catch (Exception e)
        {
            if (e is Newtonsoft.Json.JsonReaderException)
            {
                Console.Error.WriteLine("DID YOU FORGET A COMMA?");
            }
            Console.Error.WriteLine("Error reading: " + e.Message);
            throw;
        }
    }

    public static bool IsString(JToken expr)
    {
        if (expr.Type == JTokenType.String)
        {
            var s = expr.ToObject<string>();
            return s.FirstOrDefault() == '\'';
        }
        return false;
    }


    public static bool IsSymbol(JToken expr)
    {
        return expr.Type == JTokenType.String && !IsString(expr);
    }

    public static string SymbolName(JToken expr)
    {
        if (expr.Type == JTokenType.String)
        {
            var s = expr.ToObject<string>();
            return s;
        }
        throw new Exception("not a symbol? " + expr);
    }

    public static string StringValue(JToken expr)
    {
        if (expr.Type == JTokenType.String)
        {
            var s = expr.ToObject<string>();
            return s;
            // string substring = s.Substring(1, s.Length - 1);
            // return substring;
        }
        throw new Exception("not a string? " + expr);
    }

    public static bool IsSelfEvaluating(JToken expr)
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

    public static bool IsNil(JToken expr)
    {
        if (expr.Type == JTokenType.Array)
        {
            var objArr = expr.ToObject<JToken[]>();
            if (objArr.Count() == 0)
            {
                return true;
            }
        }
        if (expr == Nil)
        {
            return true;

        }
        return false;
    }

    public static bool IsApplication(JToken expr)
    {
        return expr.Type == JTokenType.Array;
    }

    public static JToken Operator(JToken expr)
    {
        var objArr = expr.ToObject<JToken[]>();
        return objArr.First();
    }

    public static List<Object> ListOfValues(IEnumerable<JToken> expr, IEnv<string, Object> env)
    {
        return expr.Select((e) => Eval(e, env)).ToList();
    }

    public static IEnumerable<JToken> Operands(JToken expr)
    {
        var objArr = expr.ToObject<JToken[]>();
        return objArr.Skip(1);
    }

    public static bool IsOperation(JToken expr, string s)
    {
        if (expr is IJEnumerable<JToken> lst)
        {
            if (lst.Count() > 0)
            {
                if (lst.First().Type == JTokenType.String)
                {
                    return lst.First().ToObject<string>() == s;
                }
            }
        }
        return false;
    }

    public static bool IsAssignment(JToken expr)
    {
        return IsOperation(expr, "define");
    }

    public static bool IsQuote(JToken expr)
    {
        return IsOperation(expr, "quote") || IsOperation(expr, "'");
    }

    // progn, begin, clojure: do
    public static bool IsDo(JToken expr)
    {
        return IsOperation(expr, "do");
    }

    public static Object EvalSequence(IEnumerable<JToken> expressions, IEnv<string, Object> env)
    {
        if (expressions.Count() == 0)
        {
            return Nil;
        }

        List<JToken> seq = expressions.ToList();
        int count = seq.Count;

        for (int i = 0; i < count - 1; i++)
        {
            Eval(seq[i], env);
        }
        return () => Eval1(seq[count - 1], env);

    }

    public static string DefineVariable(string symbol, Object defVal)
    {
        _globalEnv = _globalEnv.Extend(symbol, defVal);
        return symbol;
    }

    // the "foo" in
    // ["define", "foo", 10]
    public static string DefinitionVariable(JToken expr)
    {
        var objArr = expr.ToObject<JToken[]>();
        var symbolExpr = objArr[1];
        if (!IsSymbol(symbolExpr))
        {
            throw new Exception("Expected symbol: " + symbolExpr);
        }
        return symbolExpr.ToObject<string>();
    }

    // the 10 in
    // ["define", "foo", 10]
    public static Object DefinitionValue(JToken expr, IEnv<string, Object> env)
    {
        var objArr = expr.ToObject<JToken[]>();
        var vExpr = objArr[2];
        return Eval(vExpr, env);
    }

    public static string EvalAssignment(JToken expr, IEnv<string, Object> env)
    {
        return DefineVariable(DefinitionVariable(expr), DefinitionValue(expr, env));

    }

    public static bool IsVariable(Object o)
    {
        if (o is JToken jt && IsString(jt))
        {
            return false;
        }
        else if (o is JToken jt1 && IsSymbol(jt1))
        {
            return true;
        }
        if (o is string s)
        {
            return true;
        }
        return false;
    }

    public static Object LookupVariable(JToken expr, IEnv<string, Object> env)
    {
        var s = SymbolName(expr);
        if (!env.TryLookupVariable(s, out var val))
        {
            throw new Exception("Could not resolve symbol: " + s);
        }

        return val;
    }

    public static bool IsIf(JToken expr)
    {
        return IsOperation(expr, "if");
    }

    public static bool IsEmpty(Object o)
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

    public static bool IsFalsy(Object o)
    {
        return o is Boolean b && b == false || o == Nil || IsEmpty(o);
    }


    public static bool IsTruthy(Object o)
    {
        return o is Boolean b && b == true || !IsFalsy(o);
    }

    public static bool IsLambda(JToken expr)
    {
        return IsOperation(expr, "lambda");
    }


    // [ "lambda", [], body2,  body1  ]
    // [ "lambda", [[[], 10] [["a"], ["+", "a", 10]]]]

    // [ "lambda", ["a", "b"], ]
    // [ "lambda", ["a", "&", "b"]]


    public static IJEnumerable<JToken> LambdaArglist(JToken expr)
    {
        return expr.AsJEnumerable().Skip(1).First().AsJEnumerable();
    }

    public static IJEnumerable<JToken> LambdaBody(JToken expr)
    {
        return expr.AsJEnumerable().Skip(2).AsJEnumerable();
    }

    public static CompoundProcedure EvaluateLambda(JToken expr, IEnv<string, Object> env)
    {
        var arglist = LambdaArglist(expr);
        var name = "anonymous-procedure" + "<" + arglist.Count() + ">";
        var body = LambdaBody(expr);
        return new CompoundProcedure(name, arglist, body, env, ImmutableDictionary<string, object>.Empty);
    }

    public static IEnumerable<string> ProcedureParameters(CompoundProcedure proc)
    {
        return proc.arglist.AsEnumerable().Select((JToken e) => e.ToObject<string>());
    }

    // todo &rest

    // 1. enhance the environment with bindings arglist -> arguements
    public static IEnv<string, Object> ExtendEnvironment(
        IEnumerable<string> parameters,
        IEnumerable<Object> arguments,
        IEnv<string, Object> env)
    {
        foreach (var pair in parameters.Zip(arguments))
        {
            env = env.Extend(pair.First, pair.Second);
        }
        return env;
    }

    public static Object ApplyCompountProcedure(CompoundProcedure proc, IEnumerable<Object> arguments, IEnv<string, Object> env)
    {
        var parameters = ProcedureParameters(proc);
        if (parameters.Count() != arguments.Count())
        {
            throw new Exception("Wrong number of arguments. you said " + arguments.Count() + " but " + proc.name + " wants " + parameters.Count());
        }
        var newEnv1 = Merge(env, proc.env);
        var newEnv = ExtendEnvironment(parameters, arguments, newEnv1);
        return EvalSequence(proc.body, newEnv);
    }


    // [ "if", "predicate", "consequence", "alternative" ]

    public static Object EvaluateIf(JToken expr, IEnv<string, Object> env)
    {

        var objArr = expr.ToObject<JToken[]>();
        if (objArr.Count() != 4)
        {
            throw new Exception("Wrong number of args to if, you said " + (objArr.Count() - 1) + " but it needs 3!");
        }
        var predExpr = objArr[1];
        var consequenceExpr = objArr[2];
        var alternativeExpr = objArr[3];
        if (IsTruthy(Eval(predExpr, env)))
        {
            return () => Eval1(consequenceExpr, env);
        }
        else
        {
            return () => Eval1(alternativeExpr, env);
        }
    }


    public static Object EvaluateQuote(JToken expr)
    {
        return expr.AsJEnumerable().Skip(1).First();
    }

    public static bool IsMacroCreation(JToken expr)
    {
        return IsOperation(expr, "create-macro");
    }

    public static Object EvaluateMacro(JToken expr, IEnv<string, Object> env)
    {
        CompoundProcedure proc = EvaluateLambda(expr.Skip(1).First(), env);
        var newMeta = meta(proc);
        newMeta = newMeta.SetItem("macro?", true);
        var macro = proc.withMeta(newMeta);
        return macro;
    }

    public static bool IsMacro(Object o)
    {
        if (o is IMeta m)
        {
            return m.meta().GetValueOrDefault("macro?") is Boolean b && b == true;
        }
        return false;
    }

    // macroexpand
    // - if macro, apply macro
    //   else return expr

    public static JToken macroexpand1(JToken expr)
    {
        if (!IsApplication(expr))
        {
            return expr;
        }
        var op1 = Operator(expr);
        if (!IsSymbol(op1))
        {
            return expr;
        }
        var opSymbol = SymbolName(op1);
        if (!_globalEnv.TryLookupVariable(opSymbol, out var op))
        {
            return expr;
        }
        if (!IsMacro(op))
        {
            return expr;
        }

        var listOfExpr = Operands(expr);
        return (JToken)Trampoline(Apply(op, listOfExpr.Select(o => (Object)o).ToList(), _globalEnv));
    }

    public static JToken macroexpand(JToken expr)
    {
        JToken exf = macroexpand1(expr);
        if (exf != expr)
        {
            return macroexpand(exf);
        }
        return expr;
    }


    // ["let" ["a" 10 "b" ["+" "a" "a"]]]
    // eval first, second etc.

    public static bool IsLet(JToken expr)
    {
        return IsOperation(expr, "let");
    }

    // first build local env
    // eval the body

    public static Object EvalLet(JToken expr, IEnv<string, Object> env)
    {
        var letExpr = expr.Skip(1);
        var bindings = letExpr.First().ToObject<JArray>();
        var localEnv = env;

        for (int i = 0; i < bindings.Count(); i += 2)
        {
            var nameExpr = bindings[i];
            var valueExpr = bindings[i + 1];

            if (!IsSymbol(nameExpr))
            {
                throw new Exception("Expected symbol for binding name in let: " + nameExpr);
            }

            var name = nameExpr.ToObject<string>();
            var v = Eval(valueExpr, localEnv);
            localEnv = localEnv.Extend(name, v);

        }

        var bodyExpressions = letExpr.Skip(1);
        return EvalSequence(bodyExpressions, localEnv);
    }


    // Metacircular evaluator
    public static Object Eval1(JToken expr, IEnv<string, Object> env)
    {
        expr = macroexpand(expr);

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
        if (IsDo(expr))
        {
            return EvalSequence(expr, env);

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
            return EvaluateLambda(expr, env);
        }
        if (IsMacroCreation(expr))
        {
            return EvaluateMacro(expr, env);
        }
        if (IsLet(expr))
        {
            return EvalLet(expr, env);
        }

        if (IsApplication(expr))
        {
            var op = Eval(Operator(expr), env);
            return Apply(op, ListOfValues(Operands(expr), env), env);
        }
        else
        {
            Console.Error.WriteLine("Unknown expression type -- EVAL", expr);
        }

        return null;
    }

    public static Object Eval(JToken expr, IEnv<string, Object> env)
    {
        return Trampoline(Eval1(expr, env));
    }


    // evaluated args
    public static Object Apply(Object procedure, IEnumerable<Object> arguments, IEnv<string, Object> env)
    {
        if (procedure is PrimitiveProcedure proc)
        {
            return proc.proc.Invoke(env, arguments);
        }
        if (procedure is CompoundProcedure cproc)
        {
            return ApplyCompountProcedure(cproc, arguments, env);
        }
        else
        {
            throw new Exception("Unknown procedure type -- APPLY " + procedure);
        }

    }

}
