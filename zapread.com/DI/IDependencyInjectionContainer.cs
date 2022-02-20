using System;
using System.Collections.Generic;

namespace DI
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDependencyInjectionContainer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        object GetInstance(Type type);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        object TryGetInstance(Type type);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IEnumerable<object> GetAllInstances(Type type);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        void Release(object instance);
    }

    /// <summary>
    /// 
    /// </summary>
    public static class DependencyInjectionContainerExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        public static T GetInstance<T>(this IDependencyInjectionContainer container)
        {
            return (T)container.GetInstance(typeof(T));
        }
    }
}
