using System.Collections.Immutable;

namespace bobscheme; 

public interface IMeta {
   IImmutableDictionary<string, Object> meta();
}
