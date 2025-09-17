using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SketchRenderer.Editor.Utils
{
    internal static class PropertyAttributeWrapper
    {
        internal static bool CheckHasRangeAttribute(SerializedProperty property, out float min, out float max)
        {
            min = 0f;
            max = 0f;

            if (TryGetPropertyField(property, out FieldInfo fieldInfo))
            {
                if (TryGetFieldAttribute(fieldInfo, out RangeAttribute range))
                {
                    min = range.min;
                    max = range.max;
                    return true;
                }
            }
            
            return false;
        }
        
        private static bool TryGetPropertyField(SerializedProperty property, out FieldInfo field)
        {
            if(property == null)
                throw new ArgumentNullException(nameof(property));

            field = null;
            
            Type type = property.serializedObject.targetObject.GetType();
            string[] pathParts = property.propertyPath.Split('.');
            for (int i = 0; i < pathParts.Length; i++)
            {
                string part = pathParts[i];
                field = type.GetField(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if(field == null)
                    return false;
                
                type = field.FieldType;
            }
            return field != null;
        }

        private static bool TryGetFieldAttribute<T>(FieldInfo field, out T fieldAttribute) where T : PropertyAttribute
        {
            if(field == null)
                throw new ArgumentNullException(nameof(field));
            
            fieldAttribute = field.GetCustomAttribute<T>();
            return fieldAttribute != null;
        }
    }
}