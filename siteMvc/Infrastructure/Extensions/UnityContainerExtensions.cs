using Microsoft.Practices.Unity;

namespace Quantumart.QP8.WebMvc.Infrastructure.Extensions
{
    public static class UnityContainerExtensions
    {
        public static IUnityContainer RegisterSingletonType<TFrom, TTo>(this IUnityContainer unityContainer)
            where TTo : TFrom => unityContainer.RegisterType<TFrom, TTo>(new ContainerControlledLifetimeManager());

        public static IUnityContainer RegisterSingletonType<TFrom, TTo>(this IUnityContainer unityContainer, params InjectionMember[] injectionMembers)
            where TTo : TFrom => unityContainer.RegisterType<TFrom, TTo>(new ContainerControlledLifetimeManager(), injectionMembers);
    }
}
