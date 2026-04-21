using Microsoft.Extensions.DependencyInjection;
using SampSharp.Entities;
using SampSharp.RakNet.Entities.Pawn;

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
    /// Регистрирует в DI полный RakNet-стек:
    /// <list type="bullet">
    /// <item><see cref="IRakNetService"/> — низкоуровневый API (BitStream + Send/Emulate/SetCustomRpc).</item>
    /// <item><see cref="RakNetEventSystem"/> — мост нативных событий в ECS.</item>
    /// <item><see cref="Pawn.RakNet"/> / <see cref="Pawn.IRakNet"/> — legacy SAMP-Pawn-style
    ///   facade с <c>.NET event</c>-ами (<c>IncomingRpc</c>, <c>OutgoingPacket</c>, …),
    ///   <c>BlockRpc/BlockPacket</c>, SendPacket/SendRpc/Emulate* и вариадик BitStream-API.</item>
    /// </list>
    /// </summary>
    public static IServiceCollection AddRakNet(this IServiceCollection services)
    {
        services.AddSingleton<IRakNetService, RakNetService>();
        services.AddSystem<RakNetEventSystem>();

        services.AddSystem<Pawn.RakNet>();
        services.AddSingleton<Pawn.IRakNet>(sp => sp.GetRequiredService<Pawn.RakNet>());
        return services;
    }
}
