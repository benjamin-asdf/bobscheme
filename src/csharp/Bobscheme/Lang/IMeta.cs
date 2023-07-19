using System.Collections.Immutable;

namespace Bobscheme.Lang; 

public interface IMeta {
   IImmutableDictionary<string, Object> meta();
}
