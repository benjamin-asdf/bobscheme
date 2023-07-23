using Newtonsoft.Json.Linq;

namespace Bobscheme.Lang;

public static class Printer
{

    public static string PrintStr(Object o)
    {
        if (o is IPrintable printable)
        {
            return printable.PrintStr();
        }
        if (o is IJEnumerable<JToken> je)
        {
            return o.ToString();
        }

        if (o is IEnumerable<Object> lst) {
            return String.Join(", ", lst.Select(PrintStr));
        }
        
        return o.ToString();
    }

    public static void Print(Object o)
    {
        Console.WriteLine(PrintStr(o));
    }


}
