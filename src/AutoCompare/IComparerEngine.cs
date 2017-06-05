﻿using AutoCompare.Configuration;
using System;
using System.Collections.Generic;

namespace AutoCompare
{
    /// <summary>
    /// AutoCompare engine. Handles type configuration, compilation and cache
    /// </summary>
    public interface IComparerEngine
    {
        /// <summary>
        /// Configures how this engine should handle comparison of the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IComparerConfiguration<T> Configure<T>() where T : class;

        /// <summary>
        /// Compares two objects of type T and returns a list
        /// of Difference for every member that has a different value
        /// </summary>
        /// <typeparam name="T">Type of object to compare</typeparam>
        /// <param name="oldObject">The object containing old values</param>
        /// <param name="newObject">The object containing updated values</param>
        /// <returns>A list of members and values that are different between
        /// the old object and the new object</returns>
        IList<Difference> Compare<T>(T oldObject, T newObject) where T : class;

        /// <summary>
        /// Returns the compiled comparer for the specified type. Should only be used when calling 
        /// Compare in a loop to avoid checking the cache on every call (for a minuscule, almost negligible performance gain)
        /// </summary>
        /// <typeparam name="T">Type of object to compare</typeparam>
        /// <returns>CompiledComparer</returns>
        CompiledComparer<T> Get<T>() where T : class;

        /// <summary>
        /// Returns if the type is already compiled by this engine
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsTypeCompiled(Type type);

        /// <summary>
        /// Returns if the type is already configured by this engine
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsTypeConfigured(Type type);
    }
}
