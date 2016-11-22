using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using Microsoft.Practices.Unity;

namespace Quantumart.QP8.WebMvc.Extensions
{
    public class SignalRUnityDependencyResolver : DefaultDependencyResolver
    {
        private readonly IUnityContainer _container;

        public SignalRUnityDependencyResolver(IUnityContainer container)
        {
            _container = container;
        }

        public override object GetService(Type serviceType)
        {
            return _container.IsRegistered(serviceType) ? _container.Resolve(serviceType) : base.GetService(serviceType);
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            return _container.IsRegistered(serviceType) ? _container.ResolveAll(serviceType) : base.GetServices(serviceType);
        }
    }
}
