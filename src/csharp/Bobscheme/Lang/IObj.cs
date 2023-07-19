using System.Collections.Immutable;
using Microsoft.VisualBasic.CompilerServices;

namespace Bobscheme.Lang; 

public interface IObj : IMeta {
    IObj withMeta(IImmutableDictionary<string,Object> meta);
}
