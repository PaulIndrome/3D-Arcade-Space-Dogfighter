#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;

namespace ExtensionTools.Editor
{
    public static class SerializedPropertyExtensions
    {
        public static void Initialize(this SerializedProperty property)
        {
            var targetObject = property.serializedObject.targetObject;
            var targetObjectClassType = targetObject.GetType();
            var field = targetObjectClassType.GetField(property.propertyPath,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);

            var Object = Activator.CreateInstance(field.GetType());

            property.SetValue(Object);
        }

        public static void SetValue(this SerializedProperty property,object value)
        {
            var targetObject = property.serializedObject.targetObject;
            var targetObjectClassType = targetObject.GetType();
            var field = targetObjectClassType.GetField(property.propertyPath, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (field != null)
            {
                field.SetValue(targetObject,value);
            }
        }

        public static object GetValue(this SerializedProperty property)
        {
            if (property == null) return null;

            var path = property.propertyPath.Replace(".Array.data[", "[");
            object targetObject = property.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    targetObject = GetValueFromObject(targetObject, elementName, index);
                }
                else
                {
                    targetObject = GetValueFromObject(targetObject, element);
                }
            }
            return targetObject;
        }

        private static object GetValueFromObject(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private static object GetValueFromObject(object source, string name, int index)
        {
            var enumerable = GetValueFromObject(source, name) as IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }
    }
}
#endif