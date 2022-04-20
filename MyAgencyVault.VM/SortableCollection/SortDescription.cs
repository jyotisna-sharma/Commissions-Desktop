using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.VM.SortableCollection
{
    /// <summary>
    /// Enum describing the directions that can be sorted
    /// </summary>
    public enum SortDirection { Ascending, Descending }

    /// <summary>
    /// Class representing values needed for a sort description
    /// </summary>
    [Serializable]
    public class SortDescription
    {
        /// <summary>
        /// Name of the property to sort against, can use dot notation
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Direction the sorting needs to occur on
        /// </summary>
        public SortDirection Direction { get; set; }
    }
}
