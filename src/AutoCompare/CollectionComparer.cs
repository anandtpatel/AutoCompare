﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoCompare.Extensions;
using AutoCompare.Compilation;

namespace AutoCompare
{
    /// <summary>
    /// Compares collections using the Comparer
    /// </summary>
    public static class CollectionComparer
    {
        private static readonly MethodInfo _compareIEnumerableMethodInfo;
        private static readonly MethodInfo _compareIEnumerableWithKeyMethodInfo;
        private static readonly MethodInfo _compareIEnumerableWithKeyAndDefaultMethodInfo;
        private static readonly MethodInfo _compareIDictionaryMethodInfo;
        private static readonly MethodInfo _deepCompareIDictionaryMethodInfo;

        static CollectionComparer()
        {
            var selfType = typeof(CollectionComparer);
            _compareIEnumerableMethodInfo = selfType.GetTypeInfo().GetMethod("CompareIEnumerable", BindingFlags.Public | BindingFlags.Static);
            _compareIEnumerableWithKeyMethodInfo = selfType.GetTypeInfo().GetMethod("CompareIEnumerableWithKey", BindingFlags.Public | BindingFlags.Static);
            _compareIEnumerableWithKeyAndDefaultMethodInfo = selfType.GetTypeInfo().GetMethod("CompareIEnumerableWithKeyAndDefault", BindingFlags.Public | BindingFlags.Static);
            _compareIDictionaryMethodInfo = selfType.GetTypeInfo().GetMethod("CompareIDictionary", BindingFlags.Public | BindingFlags.Static);
            _deepCompareIDictionaryMethodInfo = selfType.GetTypeInfo().GetMethod("DeepCompareIDictionary", BindingFlags.Public | BindingFlags.Static);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="engine"></param>
        /// <param name="name"></param>
        /// <param name="oldModel"></param>
        /// <param name="newModel"></param>
        /// <param name="selector"></param>
        /// <param name="defaultKey"></param>
        /// <returns></returns>
        public static IEnumerable<Difference> CompareIEnumerableWithKeyAndDefault<T, TKey>(IComparerEngine engine, string name, IEnumerable<T> oldModel, IEnumerable<T> newModel, Func<T, TKey> selector, TKey defaultKey) where T : class
        {
            var changes = new List<Difference>();
            var comparer = engine.Get<T>();

            var oldList = oldModel.EmptyIfNull().ToList();
            var newList = newModel.EmptyIfNull().ToList();

            var oldModelWithDefaultKey = oldList.Where(x => selector(x).Equals(defaultKey)).ToList();
            var newModelWithDefaultKey = newList.Where(x => selector(x).Equals(defaultKey)).ToList();
            var oldDict = oldList.Except(oldModelWithDefaultKey).ToDictionary(selector);
            var newDict = newList.Except(newModelWithDefaultKey).ToDictionary(selector);

            changes.AddRange(DeepCompareIDictionary(engine, name, oldDict, newDict));
            var counter = 1;
            Func<Difference, int, Difference> updateName = (x, y) =>
            {
                var id = $"{{New {y}}}";
                x.Name = $"{name}.{id}.{x.Name}";
                return x;
            };
            foreach (var model in newModelWithDefaultKey)
            {
                changes.AddRange(comparer(null, model).Select(y => updateName(y, counter)));
                counter++;
            }
            return changes;
        }

        /// <summary>
        /// Compares two IEnumerable and returns the list of added/removed items
        /// </summary>
        /// <typeparam name="T">Type of IEnumerables</typeparam>
        /// <param name="name">Name of the member</param>
        /// <param name="oldModel">The list of values from the old model</param>
        /// <param name="newModel">The list of updated values from the new model</param>
        /// <returns>A list of values that have been added or removed from the list</returns>
        public static IEnumerable<Difference> CompareIEnumerable<T>(string name, IEnumerable<T> oldModel, IEnumerable<T> newModel)
        {
            var oldHash = new HashSet<T>(oldModel.EmptyIfNull());
            var newHash = new HashSet<T>(newModel.EmptyIfNull());

            var changed = newHash.Except(oldHash).Select(added => new Difference
            {
                Name = name,
                OldValue = null,
                NewValue = added,
            }).ToList();

            changed.AddRange(oldHash.Except(newHash).Select(removed => new Difference
            {
                Name = name,
                OldValue = removed,
                NewValue = null,
            }));
            return changed;
        }


        /// <summary>
        /// Compares two IDictionary and returns the list of 
        /// updated/added/removed values from the dictionary
        /// </summary>
        /// <typeparam name="TKey">Type of the IDictionary key</typeparam>
        /// <typeparam name="TValue">Type of the IDictionary value</typeparam>
        /// <param name="name">Name of the member</param>
        /// <param name="oldModel">The dictionary of values from the old model</param>
        /// <param name="newModel">The dictionary of updated values from the new model</param>
        /// <returns>A list of values that have been updated, added or removed from the dictionary</returns>
        public static IEnumerable<Difference> CompareIDictionary<TKey, TValue>(string name, IDictionary<TKey, TValue> oldModel, IDictionary<TKey, TValue> newModel)
        {
            var oldHash = new HashSet<TKey>(oldModel.EmptyIfNull().Keys);
            var newHash = new HashSet<TKey>(newModel.EmptyIfNull().Keys);

            var changed = (from key in oldHash.Intersect(newHash)
                           where !Equals(oldModel[key], newModel[key])
                           select new Difference
                           {
                               Name = $"{name}.{key}",
                               OldValue = oldModel[key],
                               NewValue = newModel[key],
                           }).ToList();

            changed.AddRange(newHash.Except(oldHash).Select(added => new Difference
            {
                Name = $"{name}.{added}",
                OldValue = null,
                NewValue = newModel[added],
            }));

            changed.AddRange(oldHash.Except(newHash).Select(removed => new Difference
            {
                Name = $"{name}.{removed}",
                OldValue = oldModel[removed],
                NewValue = null,
            }));
            return changed;
        }

        /// <summary>
        /// Compares two IDictionary by doing a deep object comparison and returns the list of 
        /// updated/added/removed values from the dictionary
        /// </summary>
        /// <typeparam name="TKey">Type of the IDictionary key</typeparam>
        /// <typeparam name="TValue">Type of the IDictionary value</typeparam>
        /// <param name="engine"></param>
        /// <param name="name">Name of the member</param>
        /// <param name="oldModel">The dictionary of values from the old model</param>
        /// <param name="newModel">The dictionary of updated values from the new model</param>
        /// <returns>A list of values that have been updated, added or removed from the dictionary</returns>
        public static IEnumerable<Difference> DeepCompareIDictionary<TKey, TValue>(IComparerEngine engine, string name, IDictionary<TKey, TValue> oldModel, IDictionary<TKey, TValue> newModel) where TValue : class
        {
            var comparer = engine.Get<TValue>();

            var oldHash = new HashSet<TKey>(oldModel.EmptyIfNull().Keys);
            var newHash = new HashSet<TKey>(newModel.EmptyIfNull().Keys);

            foreach (var key in oldHash.Intersect(newHash))
            {
                foreach (var update in comparer(oldModel[key], newModel[key]))
                {
                    update.Name = $"{name}.{key}.{update.Name}";
                    yield return update;
                }
            }

            foreach (var key in newHash.Except(oldHash))
            {
                foreach (var update in comparer(null, newModel[key]))
                {
                    update.Name = $"{name}.{key}.{update.Name}";
                    update.OldValue = null;
                    yield return update;
                }
            }

            foreach (var key in oldHash.Except(newHash))
            {
                foreach (var update in comparer(oldModel[key], null))
                {
                    update.Name = $"{name}.{key}.{update.Name}";
                    update.NewValue = null;
                    yield return update;
                }
            }
        }

        /// <summary>
        /// Returns a generic MethodInfo for the CompareIEnumerable method of the right type
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        internal static MethodInfo GetCompareIEnumerableMethodInfo(params Type[] types)
        {
            if (types.Length != 1) throw new ArgumentOutOfRangeException("Must have exactly one type");
            var type = types[0];

            return _compareIEnumerableMethodInfo.MakeGenericMethod(types);
        }

        /// <summary>
        /// Returns a generic MethodInfo for the CompareIEnumerableWithKeyAndDefault method of the right type
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        internal static MethodInfo GetCompareIEnumerableWithKeyAndDefaultMethodInfo(params Type[] types)
        {
            if (types.Length != 2) throw new ArgumentOutOfRangeException("Must have exactly two types");

            var method = _compareIEnumerableWithKeyAndDefaultMethodInfo.MakeGenericMethod(types);
            return method;
        }

        /// <summary>
        /// Returns a generic MethodInfo for the CompareIDictionary method of the right types
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        internal static MethodInfo GetCompareIDictionaryMethodInfo(params Type[] types)
        {
            if (types.Length != 2) throw new ArgumentOutOfRangeException("Must have exactly 2 types");
            var tKey = types[0];
            var tValue = types[1];

            if (Builder.IsSimpleType(tValue))
            {
                return _compareIDictionaryMethodInfo.MakeGenericMethod(types);
            }
            return _deepCompareIDictionaryMethodInfo.MakeGenericMethod(types);
        }
    }
}
