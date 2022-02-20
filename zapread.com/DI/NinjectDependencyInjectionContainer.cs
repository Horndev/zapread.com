using System;
using System.Collections.Generic;
using Ninject;

namespace DI.Ninject
{
    /// <summary>
    /// 
    /// </summary>
    public class NinjectDependencyInjectionContainer
        : IDependencyInjectionContainer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public NinjectDependencyInjectionContainer(IKernel container)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            this.container = container;
        }
        private readonly IKernel container;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object GetInstance(Type type)
        {
            return container.Get(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object TryGetInstance(Type type)
        {
            return container.TryGet(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<object> GetAllInstances(Type type)
        {
            return container.GetAll(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        public void Release(object instance)
        {
            container.Release(instance);
        }
    }
}