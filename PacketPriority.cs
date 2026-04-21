namespace SampSharp.RakNet.Entities;

/// <summary>Приоритет отправки пакета (соответствует <c>PR_PacketPriority</c>).</summary>
public enum PacketPriority
{
    SystemPriority = 0,
    HighPriority = 1,
    MediumPriority = 2,
    LowPriority = 3,
}

/// <summary>Надёжность пакета (соответствует <c>PR_PacketReliability</c>).</summary>
public enum PacketReliability
{
    Unreliable = 6,
    UnreliableSequenced = 7,
    Reliable = 8,
    ReliableOrdered = 9,
    ReliableSequenced = 10,
}

/// <summary>Тип RakNet-события (соответствует <c>PR_EventType</c>).</summary>
public enum RakNetEventType
{
    IncomingPacket = 0,
    IncomingRpc = 1,
    OutgoingPacket = 2,
    OutgoingRpc = 3,
    IncomingCustomRpc = 4,
}
