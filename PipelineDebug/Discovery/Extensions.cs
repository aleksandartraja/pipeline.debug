﻿using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace PipelineDebug.Discovery
{
    public static class Extensions
    {
        public static bool IsPrimitive(this Type type)
        {
            var checkType = type.EnumerableType() ?? type;
            return checkType == null || checkType.IsPrimitive || checkType.Equals(typeof(string)) || checkType.Equals(typeof(object));
        }

        public static bool HasToString(this Type type)
        {
            try
            {
                var checkType = type.EnumerableType() ?? type;
                var toString = checkType.GetMethod("ToString", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[0], null);
                return (toString != null && toString.DeclaringType != typeof(object));
            }
            catch (AmbiguousMatchException)
            {
                //This should not happen - but if it does it means that there's at least 2 ToString methods, and thus it's viable, we just return true;
                return true;
            }
        }

        public static string AsString(this Type type)
        {
            if (type.GenericTypeArguments != null && type.GenericTypeArguments.Length > 0)
            {
                return string.Format("{0}<{1}>",
                    type.Name.Substring(0, type.Name.IndexOf('`')),
                    string.Join(", ", type.GenericTypeArguments.Select(t => t.Name)));
            }
            else
            {
                return type.Name;
            }
        }

        public static Type EnumerableType(this Type type)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                //Classic enumerables will just have 1 argument, we discover that.
                if (type.GenericTypeArguments.Length == 1)
                {
                    return type.GenericTypeArguments[0];
                }
                //Dictionaries (among others) have 2 type arguments, but usually implement IEnumerable<T> - in the case of dictionaries T is KeyValuePair.
                //Try to find T and discover that.
                foreach (var ti in type.GetInterfaces())
                {
                    if (typeof(IEnumerable).IsAssignableFrom(ti) && ti.IsGenericType && ti.GenericTypeArguments.Length == 1)
                    {
                        return ti.GenericTypeArguments[0];
                    }
                }
                //If it wasn't IEnumberable<T> or inherited from IEnumerable<T> then it's unhandled right now
                return null;
            }
            return null;
        }
    }
}