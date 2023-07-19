using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

namespace Bobscheme.Lang.Env;

public class Environment {

    var _dict;
    
    public Environment() {
        _dict = ImmutableDictionary<String, Object>.Empty;
    }


    

}
