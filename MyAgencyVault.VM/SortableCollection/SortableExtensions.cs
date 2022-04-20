using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace MyAgencyVault.VM.SortableCollection
{
    public static class SortableExtensions
    {
        public static IEnumerable<TSource> BuildSorts<TSource>(this IEnumerable<TSource> source, IEnumerable<SortDescription> sortDescriptions)
        {
           
            //this is our order query we use at the end
            IOrderedEnumerable<TSource> orderQuery = null;

            //count for iteration process
            int count = 0;

            //iterate the sort descriptions
            foreach (SortDescription description in sortDescriptions)
            {
                #region build dot notation property filtering logic

                //get the collection of properties in the dot notation format
                string[] propPaths = description.PropertyName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                //create a collection of dynamic property getters
                List<DynamicMemberGetDelegate> propertyMethods = new List<DynamicMemberGetDelegate>();

                for (int i = 0; i < propPaths.Length; i++)
                {
                    string propPath = propPaths[i];

                    //if first item
                    if (i == 0)
                    {
                        //get the property of the first property in the dot notation from the initial object
                        //and add the generated dynamic delegate to the collection
                        PropertyInfo propInfo = typeof(TSource).GetProperty(propPath);

                        propertyMethods.Add(DynamicMethodHandlerFactory.CreatePropertyGetter(propInfo));
                    }
                    else
                    {
                        //get the previously stored dynamice delegate info
                        DynamicMemberGetDelegate previousMethod = propertyMethods[i - 1];

                        //use the return type of the previous dynamic delegate info to get
                        //the property info of the next property in the dot notation
                        //based on the type returned from the previous property
                        PropertyInfo propInfo = previousMethod.Method.ReturnType.GetProperty(propPath);

                        propertyMethods.Add(DynamicMethodHandlerFactory.CreatePropertyGetter(propInfo));
                    }
                }

                //create the function that returns the value to compare
                //for the current sort description
                Func<TSource, object> sortFunc = (item) =>
                {
                    //object store for the return value
                    object ret = null;

                    //iterate the dynamic delegates found
                    for (int i = 0; i < propertyMethods.Count; i++)
                    {
                        DynamicMemberGetDelegate propertyMethod = propertyMethods[i];

                        //if first item
                        if (i == 0)
                        {
                            //invoke the property dynamice delegate on the initial item
                            //and store it
                            ret = propertyMethod.Invoke(item);
                        }
                        else
                        {
                            //invote the property dynamice delegate on the previously stored item
                            //and store it
                            ret = propertyMethod.Invoke(ret);
                        }
                    }

                    //return the final value out of the last property in the dot notation
                    return ret;
                };

                #endregion

                //if first item create the initital order by
                if (count == 0)
                {
                    if (description.Direction == SortDirection.Ascending)
                    {
                        orderQuery = source.OrderBy(sortFunc);
                    }
                    else
                    {
                        orderQuery = source.OrderByDescending(sortFunc);
                    }
                }
                //if not first item, append ThenBy for the current sort description
                else
                {
                    if (description.Direction == SortDirection.Ascending)
                    {
                        orderQuery = orderQuery.ThenBy(sortFunc);
                    }
                    else
                    {
                        orderQuery = orderQuery.ThenByDescending(sortFunc);
                    }
                }

                count++;
            }

            if (orderQuery != null)
                return orderQuery.AsEnumerable<TSource>();
            else
                return source;
        }
    }
}
