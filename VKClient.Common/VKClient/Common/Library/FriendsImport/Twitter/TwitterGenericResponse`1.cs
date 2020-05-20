namespace VKClient.Common.Library.FriendsImport.Twitter
{
  public class TwitterGenericResponse<T> where T : class
  {
    public T Data { get; set; }

    public TwitterError Error { get; set; }
  }
}
