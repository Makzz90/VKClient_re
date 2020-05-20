namespace VKClient.Common.Library
{
  public class AgeType
  {
    private readonly string _resultStr;

    public int Age { get; set; }

    public string Prefix { get; set; }

    public AgeType(string prefix, int age)
    {
      this.Prefix = prefix;
      this.Age = age;
      this._resultStr = this.Age.ToString();
    }

    public override string ToString()
    {
      return this._resultStr;
    }
  }
}
