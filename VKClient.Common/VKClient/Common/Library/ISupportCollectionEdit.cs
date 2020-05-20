using System;

namespace VKClient.Common.Library
{
  public interface ISupportCollectionEdit
  {
    event EventHandler StartedEdit;

    event EventHandler EndedEdit;
  }
}
