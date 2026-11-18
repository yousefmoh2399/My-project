using System;
using System.Collections.Generic;

namespace KingWorldWar.App
{
    /// <summary>
    /// Minimalist, lightweight service container acting as the composition root registry.
    /// Replaces monolithic static singletons with explicit dependency management.
    /// </summary>
    public class ServiceContainer
    {
        private static ServiceContainer _instance;
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static ServiceContainer Instance => _instance ??= new ServiceContainer();

        public void Register<T>(T service) where T : class
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            _services[typeof(T)] = service;
        }

        public T Resolve<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out object service))
            {
                return (T)service;
            }
            throw new KeyNotFoundException($"Service of type {typeof(T).Name} is not registered in ServiceContainer.");
        }

        public bool TryResolve<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out object obj))
            {
                service = (T)obj;
                return true;
            }
            service = null;
            return false;
        }

        public void Reset()
        {
            _services.Clear();
        }
    }
}
