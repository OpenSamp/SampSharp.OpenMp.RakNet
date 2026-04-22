using SampSharp.Entities.SAMP;

namespace SampSharp.RakNet.Entities;

/// <summary>
/// Клиентский API к pawnraknet.dll. Реализация — <see cref="RakNetService"/>,
/// пробрасывает вызовы через C-exports в SampSharp.RakNet.dll → IPawnRakNetComponent
/// extension в pawnraknet.dll.
///
/// Если pawnraknet.dll не загружен, все методы — no-op / возвращают false.
/// Проверить через <see cref="IsAvailable"/>.
/// </summary>
public interface IRakNetService
{
    /// <summary>Доступна ли pawnraknet.dll через IExtension.</summary>
    bool IsAvailable { get; }

    // ----- Send / Emulate -------------------------------------------------

    /// <summary>Отправляет пакет игроку. <paramref name="playerId"/> = -1 → broadcast.</summary>
    bool SendPacket(BitStream bs, int playerId,
        PacketPriority priority = PacketPriority.HighPriority,
        PacketReliability reliability = PacketReliability.ReliableOrdered,
        byte orderingChannel = 0);

    bool SendPacket(BitStream bs, Player player,
        PacketPriority priority = PacketPriority.HighPriority,
        PacketReliability reliability = PacketReliability.ReliableOrdered,
        byte orderingChannel = 0);

    bool SendRpc(BitStream bs, int playerId, int rpcId,
        PacketPriority priority = PacketPriority.HighPriority,
        PacketReliability reliability = PacketReliability.ReliableOrdered,
        byte orderingChannel = 0);

    bool SendRpc(BitStream bs, Player player, int rpcId,
        PacketPriority priority = PacketPriority.HighPriority,
        PacketReliability reliability = PacketReliability.ReliableOrdered,
        byte orderingChannel = 0);

    bool EmulateIncomingPacket(BitStream bs, int playerId);
    bool EmulateIncomingPacket(BitStream bs, Player player);
    bool EmulateIncomingRpc(BitStream bs, int playerId, int rpcId);
    bool EmulateIncomingRpc(BitStream bs, Player player, int rpcId);

    // ----- Custom RPC routing --------------------------------------------

    /// <summary>
    /// Помечает RPC-id как "custom" — incoming-RPC с таким id будет ходить через
    /// OnIncomingCustomRpc вместо OnIncomingRpc.
    /// </summary>
    void SetCustomRpc(int rpcId);

    bool IsCustomRpc(int rpcId);
}
