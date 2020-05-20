using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.VirtItems;

namespace VKMessenger.Library.VirtItems
{
    public class ConversationItems : ViewModelBase, IHandle<VoiceMessagePlayEndedEvent>, IHandle
    {
        private ObservableCollection<IVirtualizable> _messages;
        private ConversationViewModel _cvm;
        private ObservableCollection<MessageViewModel> _messagesList;

        public ObservableCollection<IVirtualizable> Messages
        {
            get
            {
                return this._messages;
            }
            set
            {
                this._messages = value;
                this.NotifyPropertyChanged<ObservableCollection<IVirtualizable>>((Expression<Func<ObservableCollection<IVirtualizable>>>)(() => this.Messages));
            }
        }

        public ConversationItems(ConversationViewModel cvm)
        {
            this._cvm = cvm;
            this._cvm.PropertyChanged += new PropertyChangedEventHandler(this._cvm_PropertyChanged);
            this.Messages = new ObservableCollection<IVirtualizable>();
            this.Initialize();
            EventAggregator.Current.Subscribe((object)this);
        }

        private void _cvm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "Messages"))
                return;
            this.Initialize();
        }

        internal void Initialize()
        {
            if (this._messagesList != null)
                this._messagesList.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.Messages_CollectionChanged);
            this._messagesList = this._cvm.Messages;
            this._messagesList.CollectionChanged += new NotifyCollectionChangedEventHandler(this.Messages_CollectionChanged);
            this._messages.Clear();
            foreach (MessageViewModel mvm in this._messagesList.Reverse<MessageViewModel>())
                this._messages.Add((IVirtualizable)new MessageItem(mvm, this._cvm.Scroll != null && this._cvm.Scroll.IsHorizontalOrientation));
        }

        private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
            if (e.NewItems != null)
            {
                foreach (object newItem in (IEnumerable)e.NewItems)
                {
                    if (newItem is MessageViewModel)
                    {
                        MessageItem messageItem = new MessageItem(newItem as MessageViewModel, this._cvm.Scroll != null && this._cvm.Scroll.IsHorizontalOrientation);
                        virtualizableList.Add((IVirtualizable)messageItem);
                    }
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                int index = this._messages.Count - e.NewStartingIndex;
                if (index < 0 || index > this._messages.Count)
                    return;
                foreach (IVirtualizable virtualizable in virtualizableList)
                    this._messages.Insert(index, virtualizable);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this._messages.Clear();
            }
            else
            {
                if (e.Action != NotifyCollectionChangedAction.Remove || e.OldItems.Count <= 0)
                    return;
                int index = this._messages.Count - e.OldStartingIndex - 1;
                if (index < 0 || index >= this._messages.Count)
                    return;
                this._messages.RemoveAt(index);
            }
        }

        internal void Cleanup()
        {
            this._cvm.PropertyChanged -= new PropertyChangedEventHandler(this._cvm_PropertyChanged);
        }

        public void Handle(VoiceMessagePlayEndedEvent message)
        {
            if (this._messages == null || this._messages.Count == 0)
                return;
            Doc doc1 = message.Doc;
            if (doc1 == null || doc1.owner_id == 0L || doc1.id == 0L)
                return;
            int index1 = -1;
            for (int index2 = this._messages.Count - 1; index2 >= 0; --index2)
            {
                VoiceMessageItem voiceMessageItem = ConversationItems.GetLoadedVoiceMessageItem((IVirtualizable)(this._messages[index2] as MessageItem));
                Doc doc2 = voiceMessageItem != null ? voiceMessageItem.Doc : (Doc)null;
                if (voiceMessageItem != null && doc2 != null && (doc2.owner_id == doc1.owner_id && doc2.id == doc1.id))
                {
                    index1 = index2 - 1;
                    break;
                }
            }
            if (index1 < 0)
                return;
            VoiceMessageItem voiceMessageItem1 = ConversationItems.GetLoadedVoiceMessageItem((IVirtualizable)(this._messages[index1] as MessageItem));
            if (voiceMessageItem1 == null)
                return;
            voiceMessageItem1.PlayPause();
        }

        private static VoiceMessageItem GetLoadedVoiceMessageItem(IVirtualizable item)
        {
            VoiceMessageItem voiceMessageItem = ConversationItems.GetVoiceMessageItem(item);
            if (voiceMessageItem != null && voiceMessageItem.CurrentState != VirtualizableState.Unloaded)
                return voiceMessageItem;
            return (VoiceMessageItem)null;
        }

        private static VoiceMessageItem GetVoiceMessageItem(IVirtualizable item)
        {
            MessageItem messageItem = item as MessageItem;
            MessageContentItem messageContentItem = messageItem == null ? item as MessageContentItem : messageItem.VirtualizableChildren.FirstOrDefault<IVirtualizable>((Func<IVirtualizable, bool>)(i => i is MessageContentItem)) as MessageContentItem;
            if (messageContentItem != null)
            {
                if (messageContentItem.ForwardedList != null && messageContentItem.ForwardedList.Count > 0)
                    return (VoiceMessageItem)null;
                AttachmentsItem attachmentsItem = messageContentItem.VirtualizableChildren.FirstOrDefault<IVirtualizable>((Func<IVirtualizable, bool>)(i => i is AttachmentsItem)) as AttachmentsItem;
                VoiceMessageItem voiceMessageItem = (attachmentsItem != null ? attachmentsItem.VirtualizableChildren.FirstOrDefault<IVirtualizable>((Func<IVirtualizable, bool>)(i => i is VoiceMessageItem)) : (IVirtualizable)null) as VoiceMessageItem;
                if (voiceMessageItem != null)
                    return voiceMessageItem;
            }
            return (VoiceMessageItem)null;
        }
    }
}
