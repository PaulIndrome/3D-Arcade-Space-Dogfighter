using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace ExtensionTools
{
    public static class UnityCollectionExtensions
    {
        static System.Random random = new System.Random();

        //Shuffle List
        public static List<T> Shuffle<T>(this List<T> collection)
        {
            var newcollection = new List<T>(collection);
            int n = newcollection.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = newcollection[k];
                newcollection[k] = newcollection[n];
                newcollection[n] = value;
            }

            return newcollection;
        }
        //Shuffle Array
        public static T[] Shuffle<T>(this T[] collection)
        {

            var newcollection = new T[collection.Length];
            Array.Copy(collection, newcollection, collection.Length);

            int n = newcollection.Length;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = newcollection[k];
                newcollection[k] = newcollection[n];
                newcollection[n] = value;
            }

            return newcollection;
        }

        /* We check if this array is identical to the other array*/
        public static bool IsArrayIdentical<T>(this T[] first, T[] second)
        {
            if (first == second)
                return true;
            if (first == null || second == null)
                return false;
            if (first.Length != second.Length)
                return false;
            for (var i = 0; i < first.Length; i++)
            {
                if (!first[i].Equals(second[i]))
                    return false;
            }
            return true;
        }

        public static IList<T> Union<T>(this IList<T> collection, IList<T> othercollection)
        {
            return collection.AsEnumerable().Union(othercollection).ToList();
        }
        public static IList<T> Intersect<T>(this IList<T> collection, IList<T> othercollection)
        {
            return collection.AsEnumerable().Intersect(othercollection).ToList();
        }
        public static IList<T> Difference<T>(this IList<T> collection, IList<T> othercollection)
        {
            var a = collection.Except(othercollection);
            var b = othercollection.Except(collection);
            return a.Concat(b).ToList();
        }


        /* We add a value to the collection but only if it doesn't exist yet. Returns whether the add was succesful or not*/
        public static bool AddUnique<T>(this IList<T> collection,T value)
        {
            if (!collection.Contains(value))
            {
                collection.Add(value);
                return true;
            }
            return false;

        }

        public static T GetRandom<T>(this IList<T> collection)
        {
            int n = collection.Count;
            return collection[random.Next(n)];
        }

        public static void RemoveElements<T>(this IList<T> collection, T value)
        {
            int count = 0;
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (collection[i].Equals(value))
                {
                    ++count;
                    collection.RemoveAt(i);
                }
            }
        }

        public static bool IsIndexValid(this IList collection, int index)
        {
            return (index >= 0 && index < collection.Count);
        }


        /* Resize the list*/
        public static void Resize<T>(this List<T> list, int size, T element = default(T))
        {
            int count = list.Count;

            if (size < count)
            {
                list.RemoveRange(size, count - size);
            }
            else if (size > count)
            {
                if (size > list.Capacity)
                    list.Capacity = size;

                list.AddRange(Enumerable.Repeat(element, size - count));
            }
        }

        /* Resize the array*/
        public static void Resize<T>(this T[] array, int size)
        {
            Array.Resize(ref array, size);
        }

        /* Get Closest GameObject from list to a certain point*/
        public static IList<GameObject> SortByDistanceFromTarget(this IList<GameObject> gameObjects, Vector3 target)
        {
            return gameObjects.OrderBy((GameObject gameobject) => { return (gameobject.transform.position - target).sqrMagnitude; }).ToList();
        }
        public static GameObject GetClosestToTarget(this IList<GameObject> gameObjects, Vector3 target)
        {
            return gameObjects.SortByDistanceFromTarget(target)[0];
        }
        public static GameObject GetFurthestFromTarget(this IList<GameObject> gameObjects, Vector3 target)
        {
            var sorted = gameObjects.SortByDistanceFromTarget(target);
            return sorted[sorted.Count-1];
        }
        public static IList<GameObject> GetGameObjectsInRange(this IList<GameObject> gameObjects, Vector3 target, float range)
        {
            List<GameObject> GameObjectsInRange = new List<GameObject>();

            foreach (GameObject gameObject in gameObjects)
            {
                if ((gameObject.transform.position - target).sqrMagnitude <= (range * range))
                    GameObjectsInRange.Add(gameObject);
            }

            return GameObjectsInRange;
        }

        /* Get Closest Transform from list to a certain point*/
        public static IList<Transform> SortByDistanceFromTarget(this IList<Transform> transforms, Vector3 target)
        {
            return transforms.OrderBy((Transform transform) => { return (transform.position - target).sqrMagnitude; }).ToList();
        }
        public static Transform GetClosestToTarget(this IList<Transform> transforms, Vector3 target)
        {
            return transforms.SortByDistanceFromTarget(target)[0];
        }
        public static Transform GetFurthestFromTarget(this IList<Transform> transforms, Vector3 target)
        {
            var sorted = transforms.SortByDistanceFromTarget(target);
            return sorted[sorted.Count - 1];
        }
        public static IList<Transform> GetTransformsInRange(this IList<Transform> transforms, Vector3 target, float range)
        {
            List<Transform> TransformsInRange = new List<Transform>();

            foreach (Transform transform in transforms)
            {
                if ((transform.position - target).sqrMagnitude <= (range * range))
                    TransformsInRange.Add(transform);
            }

            return TransformsInRange;
        }

        /* Get Closest Vector3 from list to a certain point*/
        public static IList<Vector3> SortByDistanceFromTarget(this IList<Vector3> positions, Vector3 target)
        {
            return positions.OrderBy((Vector3 position) => { return (position - target).sqrMagnitude; }).ToList();
        }

        public static Vector3 GetClosestToTarget(this IList<Vector3> positions, Vector3 target)
        {
            return positions.SortByDistanceFromTarget(target)[0];
        }
        public static Vector3 GetFurthestFromTarget(this IList<Vector3> positions, Vector3 target)
        {
            var sorted = positions.SortByDistanceFromTarget(target);
            return sorted[sorted.Count - 1];
        }
        public static IList<Vector3> GetPositionsInRange(this IList<Vector3> positions, Vector3 target,float range)
        {
            List<Vector3> PositionsInRange = new List<Vector3>();

            foreach (Vector3 position in positions)
            {
                if((position - target).sqrMagnitude <= (range * range))
                    PositionsInRange.Add(position);
            }

            return PositionsInRange;
        }

        public static Vector3 GetAverage(this IList<Vector3> positions)
        {
            Vector3 total = Vector3.zero;
            foreach (Vector3 position in positions)
                total += position;
            
            return total / (float)positions.Count;
        }

    }
}
