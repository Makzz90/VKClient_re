namespace VKClient.Common.UC
{
  public sealed class CustomListPickerItem
  {
    public string Name { get; set; }

    public long Id { get; set; }

    public bool IsUnknown { get; set; }

    public override string ToString()
    {
      return this.Name;
    }

      public CustomListPickerItem()
      {
          this.Name = "";
      }
  }
}
