using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public static class ComponentTools
    {
        public static bool TryFindComponent<T>(this GameObject gameObject, out T component)
        {
            if (gameObject.TryGetComponent<T>(out component))
                return true;

            component = gameObject.GetComponentInChildren<T>();
            if (component != null)
                return true;

            component = gameObject.GetComponentInParent<T>();
            if (component != null)
                return true;

            return false;
        }

        public static bool TryFindComponent<T>(this Transform transform, out T component)
        {
            return transform.gameObject.TryFindComponent<T>(out component);
        }

        public static bool TryFindComponent<T>(this Collider col, out T component)
        {
            return col.gameObject.TryFindComponent<T>(out component);
        }

        /// <summary>
        /// Returns the component if it is found on the gameobject. If it isn't found, it will add the component.
        /// </summary>
        /// <returns>Always returns component</returns>
        public static T EnsureComponent<T>(this GameObject gameObject)
        {
            if (gameObject.TryGetComponent<T>(out T component))
            {
                return component;
            }
            else
            {
                return (T)(object)gameObject.AddComponent(typeof(T));
            }
        }

        /// <summary>
        /// Returns the component if it is found on the transform. If it isn't found, it will add the component.
        /// </summary>
        /// <returns>Always returns component</returns>
        public static T EnsureComponent<T>(this Transform transform)
        {
            if (transform.TryGetComponent<T>(out T component))
            {
                return component;
            }
            else
            {
                return (T)(object)transform.gameObject.AddComponent(typeof(T));
            }
        }
    }
}
