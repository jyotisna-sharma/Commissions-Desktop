using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics;

namespace MyAgencyVault.VM.SortableCollection
{
    /// <summary>
    /// ObservableCollection class providing sorting functionality
    /// </summary>
    /// <typeparam name="T">Type of object contained in the collection</typeparam>
    public class SortableObservableCollection<T> : ObservableCollection<T>
    {
        #region constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SortableObservableCollection()
        {
            //instantiate the Sort Descriptions collection
            SortDescriptions = new ObservableCollection<SortDescription>();

            //default to providing collection changed notifications
            ExecuteNotifyCollectionChanged = true;
        }

        public SortableObservableCollection(List<T> collection) : base(collection)
        {
            //instantiate the Sort Descriptions collection
            SortDescriptions = new ObservableCollection<SortDescription>();

            //default to providing collection changed notifications
            ExecuteNotifyCollectionChanged = true;
        }

        #endregion

        #region properties

        /// <summary>
        /// Collection of Sort Descriptions
        /// </summary>
        public ObservableCollection<SortDescription> SortDescriptions { get; protected set; }

        /// <summary>
        /// Property to determine if notifiation of collection changed should run
        /// </summary>
        protected bool ExecuteNotifyCollectionChanged { get; set; }

        #endregion

        #region methods

        /// <summary>
        /// Moves a sort description to a new position when calculating order bys
        /// </summary>
        /// <param name="item">Sort description to move</param>
        /// <param name="newIndex">Index to move the sort description to</param>
        public void SetSortDescriptionIndex(SortDescription item, int newIndex)
        {
            SortDescriptions.Move(SortDescriptions.IndexOf(item), newIndex);
        }

        /// <summary>
        /// Method that performs the sort based on sort descriptions
        /// </summary>
        public void RefreshSort()
        {
            var orderQuery = this.BuildSorts(SortDescriptions);

            //if an orderquery was created
            if (orderQuery != null)
            {
                
#if DEBUG
                //store the current time
                DateTime startTime = DateTime.Now;
#endif
                //perform the actual processing of the query and store it
                List<T> orderedItems = orderQuery.ToList();

#if DEBUG
                //show how long in milliseconds it took to perform the query
                DateTime endTime = DateTime.Now;
                Debug.WriteLine(String.Format("Time in milliseconds to perform the sort: {0}", (endTime - startTime).TotalMilliseconds));
#endif

                //turn off collection changed notification
                ExecuteNotifyCollectionChanged = false;

                //clear all current items
                this.Clear();

                //iterate sorted items and add them
                foreach (T item in orderedItems)
                {
                    this.Add(item);
                }

                //turn collection changed notifications back on
                ExecuteNotifyCollectionChanged = true;

                //raise the reset notification to cause rebinding of the sorted list
                //after changes to the collection have been made
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Override of OnCollectionChanged to look for disabling of notifications
        /// </summary>
        /// <param name="e">Args representing the type of notification to raise</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            //if allowed to execute collection changed notifications then execute
            if (ExecuteNotifyCollectionChanged)
                base.OnCollectionChanged(e);
        }

        #endregion
    }
}
