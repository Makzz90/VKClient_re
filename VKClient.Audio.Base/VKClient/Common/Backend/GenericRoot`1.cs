using System.Collections.Generic;

namespace VKClient.Common.Backend
{
  public class GenericRoot<T> where T : class
  {
    public T response { get; set; }

    public List<ExecuteError> execute_errors { get; set; }
  }
}
