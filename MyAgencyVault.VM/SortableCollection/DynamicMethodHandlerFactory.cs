//
//  This code is Copyright 2009 Marimer LLC
//  and is part of the CSLA framework found at http://www.lhotka.net
//

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace MyAgencyVault.VM.SortableCollection
{
    /// <summary>
    /// Delegate for getting a value.
    /// </summary>
    /// <param name="target">Target object.</param>
    /// <returns></returns>
    public delegate object DynamicMemberGetDelegate(object target);

    internal static class DynamicMethodHandlerFactory
    {
        public static DynamicMemberGetDelegate CreatePropertyGetter(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            if (!property.CanRead) return null;

            MethodInfo getMethod = property.GetGetMethod();
            if (getMethod == null)   //maybe is private
                getMethod = property.GetGetMethod(true);

            DynamicMethod dm = new DynamicMethod("propg", typeof(object),
                new Type[] { typeof(object) },
                property.DeclaringType, true);

            ILGenerator il = dm.GetILGenerator();

            if (!getMethod.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Callvirt, getMethod, null);
            }
            else
                il.EmitCall(OpCodes.Call, getMethod, null);

            if (property.PropertyType.IsValueType)
                il.Emit(OpCodes.Box, property.PropertyType);

            il.Emit(OpCodes.Ret);

            return (DynamicMemberGetDelegate)dm.CreateDelegate(typeof(DynamicMemberGetDelegate));
        }
    }
}
