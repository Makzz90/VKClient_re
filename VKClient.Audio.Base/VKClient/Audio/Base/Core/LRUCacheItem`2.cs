namespace VKClient.Audio.Base.Core
{
  public class LRUCacheItem<K, V>
  {
    public K key;
    public V value;

    public LRUCacheItem(K k, V v)
    {
      this.key = k;
      this.value = v;
    }
  }
}
