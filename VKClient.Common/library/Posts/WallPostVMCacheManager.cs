using VKClient.Common.Framework;

namespace VKClient.Common.Library.Posts
{
  public static class WallPostVMCacheManager
  {
    private static readonly string _wallPostKey = "WallPost";
    private static WallPostViewModel _instance;

    public static void RegisterForDelayedSerialization(WallPostViewModel vm)
    {
      WallPostVMCacheManager._instance = vm;
    }

    public static void TrySerializeVM(WallPostViewModel vm)
    {
      string key = WallPostVMCacheManager.GetKey(vm);
      CacheManager.TrySerialize((IBinarySerializable) vm, key, false, CacheManager.DataType.CachedData);
    }

    public static void TryDeserializeVM(WallPostViewModel vm)
    {
      string key = WallPostVMCacheManager.GetKey(vm);
      CacheManager.TryDeserialize((IBinarySerializable) vm, key, CacheManager.DataType.CachedData);
    }

    private static string GetKey(WallPostViewModel vm)
    {
      return "WallPost_" + AppGlobalStateManager.Current.GlobalState.LoggedInUserId + "_" + vm.UniqueId;
    }

    public static void ResetVM(WallPostViewModel vm)
    {
      CacheManager.TryDelete(WallPostVMCacheManager.GetKey(vm), CacheManager.DataType.CachedData);
    }

    public static void TryDeserializeInstance(WallPostViewModel vm)
    {
      CacheManager.TryDeserialize((IBinarySerializable) vm, WallPostVMCacheManager._wallPostKey, CacheManager.DataType.CachedData);
    }

    public static void TrySerializeInstance()
    {
      if (WallPostVMCacheManager._instance == null)
        return;
      CacheManager.TrySerialize((IBinarySerializable) WallPostVMCacheManager._instance, WallPostVMCacheManager._wallPostKey, false, CacheManager.DataType.CachedData);
    }

    public static void ResetInstance()
    {
      WallPostVMCacheManager._instance =  null;
      CacheManager.TryDelete(WallPostVMCacheManager._wallPostKey, CacheManager.DataType.CachedData);
    }
  }
}
