namespace Bobscheme.Lang;

public interface IEnv<K,V>
{
    IEnv<K, V> Extend(K key, V value);
    bool TryLookupVariable(K key, out V var);

}