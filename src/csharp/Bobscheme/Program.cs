using System.Collections.Immutable;
using Bobscheme.Lang.RT;

string[] prompts = { "user> ", "what next?> ", "what is my purpose?> ", "say the next piece of the program> " };


// I would ship this with the release artifact
var coreDir = "src/json/core/";
foreach(var file in new System.IO.DirectoryInfo(coreDir).GetFiles())
{
    RT.Load(file.ToString());

}


var files = args.Where(f => f != "--repl");
var doRepl = args.Contains("--repl");

foreach (var file in files)
{
    RT.Print(RT.Load(file));
}


if (doRepl)
{
    Repl();

}

T RandNth<T>(T[] lst)
{
    int seed = (int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    int randomIndex = RT.random.Next(0, lst.Count());
    var randomElement = lst[randomIndex];
    return randomElement;
}

void Repl() { 
    while (true)
    {
        var prompt = RandNth(prompts);
        Console.Write(prompt);
        var input = Console.ReadLine();
        try
        {
            if (input != "")
            {
                var v = RT.Eval(RT.Read(input), RT._globalEnv);
                RT.Print(v);
            }

        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }
}
