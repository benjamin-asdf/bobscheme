using System.Collections;
using System.Collections.Immutable;
using bobscheme.csharp.Bobscheme.Lang;
using Bobscheme.Lang;

namespace Bobscheme.Lang;

public record ImmutableEnv(ImmutableDictionary<string,Object> dict) : IEnv<string,Object>, IPrintable
{
    public IEnv<string, object> Extend(string key, object value)
    {
        return new ImmutableEnv(dict.SetItem(key,value));
    }

    public bool TryLookupVariable(string key, out object var)
    
    {
        if (dict.ContainsKey(key))
        {
            var = dict[key];
            return true;
        }

        var = null;
        return false;
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        return dict.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public string PrintStr()
    {
        var s = "{";
        foreach (var kvp in dict)
        {
            s = s + kvp.Key + ":" + kvp.Value;
            
        }

        s = s + "}";
        return s;
    }
}
