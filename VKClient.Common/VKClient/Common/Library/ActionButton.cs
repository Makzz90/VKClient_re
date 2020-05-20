namespace VKClient.Common.Library
{
  public class ActionButton
  {
    public string ActionButtonText { get; set; }

    public ActionButtonType ButtonType { get; set; }

    public ActionButton(string text, ActionButtonType buttonType)
    {
      this.ActionButtonText = text;
      this.ButtonType = buttonType;
    }
  }
}
