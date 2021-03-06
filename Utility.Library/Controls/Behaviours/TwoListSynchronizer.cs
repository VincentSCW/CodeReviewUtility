﻿using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;

using System.Collections.Generic;

namespace Utility.Library.Controls.Behaviours
{
    /// <summary>
    /// Converts items in the Master list to Items in the target list, and back again.
    /// </summary>
    public interface IListItemConverter
    {
        /// <summary>
        /// Converts the specified master list item.
        /// </summary>
        /// <param name="masterListItem">The master list item.</param>
        /// <returns>The result of the conversion.</returns>
        object Convert(object masterListItem);

        /// <summary>
        /// Converts the specified target list item.
        /// </summary>
        /// <param name="targetListItem">The target list item.</param>
        /// <returns>The result of the conversion.</returns>
        object ConvertBack(object targetListItem);
    }

    /// <summary>
    /// Keeps two lists synchronized. 
    /// </summary>
    public class TwoListSynchronizer : IWeakEventListener
    {
        private static readonly IListItemConverter DefaultConverter = new DoNothingListItemConverter();
        private readonly IList masterList;
        private readonly IListItemConverter masterTargetConverter;
        private readonly IList targetList;
        private readonly Selector selector;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoListSynchronizer"/> class.
        /// </summary>
        /// <param name="masterList">The master list.</param>
        /// <param name="targetList">The target list.</param>
        /// <param name="masterTargetConverter">The master-target converter.</param>
        public TwoListSynchronizer(Selector selector, IList masterList, IList targetList, IListItemConverter masterTargetConverter)
        {
            this.selector = selector;
            this.masterList = masterList;
            this.targetList = targetList;

            if (masterTargetConverter == null)
                this.masterTargetConverter = DefaultConverter;
            else
                this.masterTargetConverter = masterTargetConverter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoListSynchronizer"/> class.
        /// </summary>
        /// <param name="masterList">The master list.</param>
        /// <param name="targetList">The target list.</param>
        public TwoListSynchronizer(Selector selector, IList masterList, IList targetList)
            : this(selector, masterList, targetList, DefaultConverter)
        {
        }

        private delegate void ChangeListAction(IList list, NotifyCollectionChangedEventArgs e, Converter<object, object> converter);

        /// <summary>
        /// Starts synchronizing the lists.
        /// </summary>
        public void StartSynchronizing()
        {
            ListenForChangeEvents(masterList);
            ListenForChangeEvents(targetList);

            // Update the Target list from the Master list
            SetListValuesFromSource(masterList, targetList, ConvertFromMasterToTarget);

            // In some cases the target list might have its own view on which items should included:
            // so update the master list from the target list
            // (This is the case with a ListBox SelectedItems collection: only items from the ItemsSource can be included in SelectedItems)
            if (!TargetAndMasterCollectionsAreEqual())
            {
                SetListValuesFromSource(targetList, masterList, ConvertFromTargetToMaster);
            }
        }

        /// <summary>
        /// Stop synchronizing the lists.
        /// </summary>
        public void StopSynchronizing()
        {
            StopListeningForChangeEvents(masterList);
            StopListeningForChangeEvents(targetList);
        }

        /// <summary>
        /// Receives events from the centralized event manager.
        /// </summary>
        /// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager"/> calling this method.</param>
        /// <param name="sender">Object that originated the event.</param>
        /// <param name="e">Event data.</param>
        /// <returns>
        /// true if the listener handled the event. It is considered an error by the <see cref="T:System.Windows.WeakEventManager"/> handling in WPF to register a listener for an event that the listener does not handle. Regardless, the method should return false if it receives an event that it does not recognize or handle.
        /// </returns>
        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            HandleCollectionChanged(sender as IList, e as NotifyCollectionChangedEventArgs);

            return true;
        }

        /// <summary>
        /// Listens for change events on a list.
        /// </summary>
        /// <param name="list">The list to listen to.</param>
        protected void ListenForChangeEvents(IList list)
        {
            if (list is INotifyCollectionChanged)
            {
                CollectionChangedEventManager.AddListener(list as INotifyCollectionChanged, this);
            }
        }

        /// <summary>
        /// Stops listening for change events.
        /// </summary>
        /// <param name="list">The list to stop listening to.</param>
        protected void StopListeningForChangeEvents(IList list)
        {
            if (list is INotifyCollectionChanged)
            {
                CollectionChangedEventManager.RemoveListener(list as INotifyCollectionChanged, this);
            }
        }

        private void AddItems(IList list, NotifyCollectionChangedEventArgs e, Converter<object, object> converter)
        {
            
                int itemCount = e.NewItems.Count;

                for (int i = 0; i < itemCount; i++)
                {
                    int insertionPoint = e.NewStartingIndex + i;

                    if (insertionPoint > list.Count)
                    {
                        list.Add(converter(e.NewItems[i]));
                    }
                    else
                    {
                        list.Insert(insertionPoint, converter(e.NewItems[i]));
                    }
                }
            
        }

        private object ConvertFromMasterToTarget(object masterListItem)
        {
            return masterTargetConverter == null ? masterListItem : masterTargetConverter.Convert(masterListItem);
        }

        private object ConvertFromTargetToMaster(object targetListItem)
        {
            return masterTargetConverter == null ? targetListItem : masterTargetConverter.ConvertBack(targetListItem);
        }

        private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IList sourceList = sender as IList;

            this.selector.Dispatcher.BeginInvoke((System.Threading.ThreadStart)delegate()
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        PerformActionOnAllLists(AddItems, sourceList, e);
                        break;
                    case NotifyCollectionChangedAction.Move:
                        PerformActionOnAllLists(MoveItems, sourceList, e);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        PerformActionOnAllLists(RemoveItems, sourceList, e);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        PerformActionOnAllLists(ReplaceItems, sourceList, e);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        UpdateListsFromSource(sender as IList);
                        break;
                    default:
                        break;
                }
            });
        }

        private void MoveItems(IList list, NotifyCollectionChangedEventArgs e, Converter<object, object> converter)
        {
            RemoveItems(list, e, converter);
            AddItems(list, e, converter);
        }

        private void PerformActionOnAllLists(ChangeListAction action, IList sourceList, NotifyCollectionChangedEventArgs collectionChangedArgs)
        {
            if (sourceList == masterList)
            {
                PerformActionOnList(targetList, action, collectionChangedArgs, ConvertFromMasterToTarget);
            }
            else
            {
                PerformActionOnList(masterList, action, collectionChangedArgs, ConvertFromTargetToMaster);
            }
        }

        private void PerformActionOnList(IList list, ChangeListAction action, NotifyCollectionChangedEventArgs collectionChangedArgs, Converter<object, object> converter)
        {
            StopListeningForChangeEvents(list);
            action(list, collectionChangedArgs, converter);
            ListenForChangeEvents(list);
        }

        private void RemoveItems(IList list, NotifyCollectionChangedEventArgs e, Converter<object, object> converter)
        {
            if (list.Count > 0)
            {
                int itemCount = e.OldItems.Count;

                // for the number of items being removed, remove the item from the Old Starting Index
                // (this will cause following items to be shifted down to fill the hole).
                for (int i = 0; i < itemCount; i++)
                {
                    list.RemoveAt(e.OldStartingIndex);
                }
            }
        }

        private void ReplaceItems(IList list, NotifyCollectionChangedEventArgs e, Converter<object, object> converter)
        {
            RemoveItems(list, e, converter);
            AddItems(list, e, converter);
        }

        private void SetListValuesFromSource(IList sourceList, IList targetList, Converter<object, object> converter)
        {
            StopListeningForChangeEvents(targetList);

            targetList.Clear();

            foreach (object o in sourceList)
            {
                object item = this.selector.Items.Cast<object>().FirstOrDefault(e => ConvertFromMasterToTarget(e) == o);
                if (item != null)
                    targetList.Add(item);
                else
                    targetList.Add(converter(o));

            }

            ListenForChangeEvents(targetList);
        }

        private bool TargetAndMasterCollectionsAreEqual()
        {
            return masterList.Cast<object>().SequenceEqual(targetList.Cast<object>().Select(item => ConvertFromTargetToMaster(item)));
        }

        /// <summary>
        /// Makes sure that all synchronized lists have the same values as the source list.
        /// </summary>
        /// <param name="sourceList">The source list.</param>
        private void UpdateListsFromSource(IList sourceList)
        {
            if (sourceList == masterList)
            {
                SetListValuesFromSource(masterList, targetList, ConvertFromMasterToTarget);
            }
            else
            {
                SetListValuesFromSource(targetList, masterList, ConvertFromTargetToMaster);
            }
        }

        /// <summary>
        /// An implementation that does nothing in the conversions.
        /// </summary>
        internal class DoNothingListItemConverter : IListItemConverter
        {
            /// <summary>
            /// Converts the specified master list item.
            /// </summary>
            /// <param name="masterListItem">The master list item.</param>
            /// <returns>The result of the conversion.</returns>
            public object Convert(object masterListItem)
            {
                return masterListItem;
            }

            /// <summary>
            /// Converts the specified target list item.
            /// </summary>
            /// <param name="targetListItem">The target list item.</param>
            /// <returns>The result of the conversion.</returns>
            public object ConvertBack(object targetListItem)
            {
                return targetListItem;
            }
        }
    }
}
