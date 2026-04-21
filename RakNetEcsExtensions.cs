using Microsoft.Extensions.DependencyInjection;
using SampSharp.Entities;

namespace SampSharp.RakNet.Entities;

public static class RakNetEcsExtensions
{
    /// <summary>
    /// Активирует RakNet-мост в ECS. Регистрирует <see cref="RakNetEventSystem"/>,
    /// который при старте attach'ит <c>UnmanagedCallersOnly</c>-callback'и в
    /// SampSharp.RakNet.dll → pawnraknet.dll.
    /// </summary>
    public static IEcsBuilder EnableRakNetEvents(this IEcsBuilder builder) => builder;

    /// <summary>
    /// Регистрирует в DI <see cref="IRakNetService"/> и event-систему.
    /// </summary>
    public static IServiceCollection AddRakNet(this IServiceCollection services)
    {
        services.AddSingleton<IRakNetService, RakNetService>();
        services.AddSystem<RakNetEventSystem>();
        return services;
    }
}
