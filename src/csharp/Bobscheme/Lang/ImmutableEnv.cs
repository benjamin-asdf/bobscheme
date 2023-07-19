using System.Collections.Immutable;
using Bobscheme.Lang;

namespace Bobscheme.Lang;

public record ImmutableEnv(ImmutableDictionary<String,Object> dict) : IEnv<String,Object>
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
}