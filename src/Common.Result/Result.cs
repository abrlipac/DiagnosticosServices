using System.Collections.Generic;

namespace Common.Result
{
  public class Result<T>
  {
    public bool Succeeded { get; set; }
    public T Content { get; set; }
    public IList<string> Errors { get; set; }
  }
}