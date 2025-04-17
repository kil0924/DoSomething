using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    public static class Utils
    {
        public static List<T> ToEnumList<T>(this T t) where T : Type
        {
            return Enum.GetValues(t).Cast<T>().ToList();
        }
        
        public static void Reset(this Transform transform, Transform parent = null)
        {
            transform.SetParent(parent);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        
        public static T GetRandom<T>(this List<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }
        
        public static void Shuffle<T>(this List<T> list)
        {
            var rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}