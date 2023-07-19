namespace bobscheme.csharp.Bobscheme.Lang;

public interface IFn
{
    public Object apply(IEnumerable<Object> args);
}