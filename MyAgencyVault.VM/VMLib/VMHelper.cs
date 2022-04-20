using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace MyAgencyVault.ViewModel
{
    public static class VMHelper
    {
        public static string getPolicyMode(int value)
        {
            string PolicyMode = string.Empty;
            
            if (value == 0)
                PolicyMode = "M";
            else if (value == 1)
                PolicyMode = "Q";
            else if (value == 2)
                PolicyMode = "S";
            else if (value == 3)
                PolicyMode = "A";
            else if (value == 4)
                PolicyMode = "0";
            else
                PolicyMode = "";

            return PolicyMode;
        }

        public static string getCompType(int value)
        {
            string  CompType = string.Empty;

            if (value == 1)
                CompType = "C";
            else if (value == 2)
                CompType = "O";
            else if (value == 3)
                CompType = "B";
            else if (value == 4)
                CompType = "F";

            return CompType;
        }

        public static string CorrectPolicyNo(string value)
        {
            value = value.Trim();
            value = value.Replace(" ", "");

            bool IsCharAddedToStringBuilder = false;
            StringBuilder stringBuilder = new StringBuilder(50);

            foreach (char c in value)
            {
                if (!IsCharAddedToStringBuilder && c == '0')
                {
                    continue;
                }

                if (char.IsLetterOrDigit(c))
                {
                    stringBuilder.Append(c);
                    IsCharAddedToStringBuilder = true;
                }
            }

            return stringBuilder.ToString();
        }

        public static string CorrectBrokerCode(string value)
        {
            value = value.Trim();
            value = value.Replace(" ", "");
            StringBuilder stringBuilder = new StringBuilder(100);
            foreach (char c in value)
            {
                if (char.IsLetterOrDigit(c))
                {
                    stringBuilder.Append(c);                    
                }
            }

            return stringBuilder.ToString();
        }

        public static void AddRange<TItem, TElement>(this ICollection<TElement> collection, IEnumerable<TItem> items) where TItem : TElement
        {
            if (collection == null) throw new ArgumentNullException("collection");
            if (items != null)
            {
                foreach (var item in items)
                    collection.Add(item);
            }
        }

        public static bool Exist<TElement>(this ObservableCollection<TElement> collection,Predicate<TElement> predicate)
        {
            if (collection == null) return false;
            else
            {
                return collection.ToList().Exists(predicate);
            }
        }
    }
}
