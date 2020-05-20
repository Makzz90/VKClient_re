using System;
using System.Windows;
using VKClient.Common.Framework;
using VKClient.Common.UC;

namespace VKClient.Common.Library.VirtItems
{
  public class NewsFeedHideActionsItem : VirtualizableItemBase
  {
    private readonly string _type;
    private readonly long _ownerId;
    private readonly long _itemId;
    private readonly Action<long> _hideSourceItemsCallback;
    private readonly Action _cancelCallback;
    private NewsFeedHideActionsUC _actionsUC;
    private double _fixedHeight;

    public override double FixedHeight
    {
      get
      {
        return this._fixedHeight;
      }
    }

    public NewsFeedHideActionsItem(double width, Thickness margin, string type, long ownerId, long itemId, Action<long> hideSourceItemsCallback, Action cancelCallback)
      : base(width, margin, new Thickness())
    {
      this._type = type;
      this._ownerId = ownerId;
      this._itemId = itemId;
      this._hideSourceItemsCallback = hideSourceItemsCallback;
      this._cancelCallback = cancelCallback;
      this.CreateLayout();
    }

    private void CreateLayout()
    {
      if (this._actionsUC == null)
      {
        NewsFeedHideActionsUC feedHideActionsUc = new NewsFeedHideActionsUC();
        double width = this.Width;
        feedHideActionsUc.Width = width;
        this._actionsUC = feedHideActionsUc;
        this._actionsUC.Initialize(new NewsFeedHideActionsViewModel(this._type, this._ownerId, this._itemId), this._hideSourceItemsCallback, this._cancelCallback);
        this.VirtualizableChildren.Add((IVirtualizable) new UCItem(this.Width, this.Margin, (Func<UserControlVirtualizable>) (() => (UserControlVirtualizable) this._actionsUC), (Func<double>) (() => this.FixedHeight), (Action<UserControlVirtualizable>) null, 0.0, false));
      }
      this._fixedHeight = 80.0;
    }
  }
}
