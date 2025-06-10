using System;
using System.Collections.Generic;
using Core.Api;

namespace Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, IService> Services = new();

        public static void Bind<T>(T service) where T : class, IService
        {
            if (Services.ContainsKey(typeof(T)))
                return;

            Services[typeof(T)] = service;
        }

        public static T Get<T>() where T : class, IService => 
            Services.ContainsKey(typeof(T)) ? (T)Services[typeof(T)] : null;
    }
}