using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;


string[] prompts =  {"user> ", "what next?> ", "what is my purpose?> ", "say the next piece of the program> " };

Dictionary<String, Func<List<Object>, Object>> primitiveOperators = new ();
var random = new Random();

primitiveOperators.Add("+", (args) =>
{
    long sum = 0;
    foreach (var arg in args)
    {
        if (arg is long)
            sum += (long)arg;
        else
            throw new InvalidOperationException($"Expected number, found {arg.GetType()}");
    }
    return sum;
});

Object Nil = new System.Object();

// env
// { "nil" Nil }

if (args.Length > 0)

{

    string json = File.ReadAllText(args[0]);
    var exp = Read(json);
    var v = Eval(exp);
    Print(v);

}

else

{
    Repl();
    
}

T RandNth<T>(T[] lst) {
    int seed =  (int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    int randomIndex = random.Next(0, lst.Count());
    var randomElement = lst[randomIndex];
    return randomElement;
}

void Repl() {
    while (true) {
       var prompt = RandNth(prompts);
       Console.Write(prompt);
       var input = Console.ReadLine();
       var v = Eval(Read(input));
       Print(v);
    }
}


JToken Read(string expr) {
    JToken token = JToken.Parse(expr);
    return token;
}

void Print(Object o) {
    if (o == Nil) {
        Console.WriteLine("nil");
    } else {
        Console.WriteLine(o);
    }
}


bool IsSelfEvaluating(JToken exp) {
   
   if (exp.Type == JTokenType.Object)
    {
        return false;
    }
    else if (exp.Type == JTokenType.Array)
    {
        return false;
    }
    else
    {
        // Process primitive value (string, number, boolean, null)

        return true;
    }
}

bool IsNil(JToken exp) {
    if (exp.Type == JTokenType.Array) {
    var objArr = exp.ToObject<JToken[]>();
    if (objArr.Count() == 0) {
            return true;
        }
    }
    return false;
}
 
bool TryString(JToken o, out string v) {
    v = "";
    if (o.Type == JTokenType.Object) {
        var objDict = ((JObject)o).ToObject<Dictionary<string, JToken>>();
        if (objDict.TryGetValue("is", out var isV)) {
            if (isV.ToObject<String>() == "string")
            {
                v = objDict["val"].ToObject<String>();
                return true;

            }
        }
    }
    return false;
}

bool IsApplication(JToken exp) {
    return exp.Type == JTokenType.Array;
}

JToken Operator(JToken exp) {
    var objArr = exp.ToObject<JToken[]>();
    return objArr.First();
}

List<Object> EvalSequence(IEnumerable<JToken> exp) {
    return exp.Select(Eval).ToList();
}

IEnumerable<JToken> Operators(JToken exp) {
    var objArr = exp.ToObject<JToken[]>();
    return objArr.Skip(1);
}


Object Eval(JToken exp) {
    if (IsSelfEvaluating(exp)) {
        var token = (JToken)exp;
        var o = token.ToObject<Object>();
        return o;
    } if (IsNil(exp)) {
        return Nil;
    } if (IsApplication(exp)) {
        return
            Apply(Eval(Operator(exp)),
              EvalSequence(Operators(exp)));
    }
    if (TryString(exp, out var s)) {
        return s;
    }
    else {
        Console.Error.WriteLine("Unknown expression type -- EVAL", exp);
    }

    return null;

}


// evaluated args
Object Apply(Object procedure, List<Object> arguments) {
    if (procedure is String proc && primitiveOperators.TryGetValue(proc, out var op)) {
        return op.Invoke(arguments);
    } else {
        throw new Exception("Unknown procedure type -- APPLY " +  procedure);
    }

}


 



