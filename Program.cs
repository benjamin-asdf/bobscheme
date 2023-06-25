using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;

string[] prompts =  {"user> ", "what next?> ", "what is my purpose?> ", "say the next piece of the program> " };


if (args.Length > 0)

{

    // string json = File.ReadAllText(args[0]);
    // var ast = Read(json);
    // var v = Eval(ast);
    // Print(v);

    Repl();


}


else

{

    Repl();
    
}

T RandNth<T>(T[] lst) {
    int seed =  (int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    Random random = new Random(seed);
    int randomIndex = random.Next(0, lst.Count());
    Console.WriteLine(randomIndex);

    var randomElement = lst[randomIndex];
    return randomElement;
}

void Repl() {
    while (true) {
       var prompt = RandNth(prompts);
       Console.Write(prompt);
       var input = Console.ReadLine();
       Print(input);
    }
}


JToken Read(string expr) {
    JToken token = JToken.Parse(expr);
    return token;
}

void Print(Object o) {
    Console.WriteLine(o);
}


bool IsSelfEvaluating(JToken o) {
   
   if (o.Type == JTokenType.Object)
    {
        return false;
    }
    else if (o.Type == JTokenType.Array)
    {
        return false;
    }
    else
    {
        // Process primitive value (string, number, boolean, null)

        return true;
    }
}

Object Eval(JToken ast) {

    if (IsSelfEvaluating(ast)) {
        return ast;
    } else {
        Console.Error.WriteLine("Unknown expression type -- EVAL", ast);
    }

    return null;

}


 

// reader
// read json 

// evaluater
// go through ast
// metacircular evaluator



// printer 
