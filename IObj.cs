using System.Collections.Immutable;
using Microsoft.VisualBasic.CompilerServices;

namespace bobscheme;

public interface IObj : IMeta {
    IObj withMeta(IImmutableDictionary<string,Object> meta);
}
