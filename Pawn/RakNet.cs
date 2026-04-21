// VSRP.RakNet
// Copyright 2018 Danil Zelyutin
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using SampSharp.Entities;
using SampSharp.RakNet.Entities.Pawn.Definitions;
using BridgeBitStream = SampSharp.RakNet.Entities.BitStream;
using BridgeService = SampSharp.RakNet.Entities.IRakNetService;
// PacketPriority/PacketReliability существуют в двух namespace'ах
// (SampSharp.RakNet.Entities.Pawn.Definitions — legacy enum с Definitions.*; и
//  SampSharp.RakNet.Entities.* — bridge-level enum в корне submodule'а).
// Для сигнатур IRakNet используем Pawn.Definitions-версию, для вызовов в bridge
// приводим к int и кастуем.
using BridgePacketPriority = SampSharp.RakNet.Entities.PacketPriority;
using BridgePacketReliability = SampSharp.RakNet.Entities.PacketReliability;

namespace SampSharp.RakNet.Entities.Pawn;

public partial class RakNet(BridgeService rakNet) : ISystem, IRakNet
{
    public bool LoggingIncomingRpc { get; set; }
    public bool LoggingOutcomingRpc { get; set; }
    public bool LoggingIncomingPacket { get; set; }
    public bool LoggingOutcomingPacket { get; set; }
    public bool LoggingBlockingRpc { get; set; }
    public bool LoggingBlockingPacket { get; set; }

    internal BridgeService Bridge => rakNet;

    public void SetLogging(bool incomingRpc, bool outcomingRpc, bool incomingPacket, bool outcomingPacket,
        bool blockingRpc, bool blockingPacket)
    {
        LoggingIncomingRpc = incomingRpc;
        LoggingOutcomingRpc = outcomingRpc;
        LoggingIncomingPacket = incomingPacket;
        LoggingOutcomingPacket = outcomingPacket;
        LoggingBlockingRpc = blockingRpc;
        LoggingBlockingPacket = blockingPacket;
    }

    // BlockRpc/BlockPacket historically sent a CallRemoteFunction("BlockNextRpc")
    // to a Pawn script. In the x64 world we just set a flag that the next
    // OnOutgoing* handler consumes and returns false from (veto at the
    // networking layer).
    private bool _blockNextRpc;
    private bool _blockNextPacket;

    public void BlockRpc()
    {
        if (LoggingBlockingRpc) Console.WriteLine("[S#] Blocking next Rpc");
        _blockNextRpc = true;
    }

    public void BlockPacket()
    {
        if (LoggingBlockingPacket) Console.WriteLine("[S#] Blocking next Packet");
        _blockNextPacket = true;
    }

    internal bool ConsumeRpcBlock()
    {
        if (!_blockNextRpc) return false;
        _blockNextRpc = false;
        return true;
    }

    internal bool ConsumePacketBlock()
    {
        if (!_blockNextPacket) return false;
        _blockNextPacket = false;
        return true;
    }

    public int SendPacket(BitStream bs, int playerId,
        Definitions.PacketPriority priority = Definitions.PacketPriority.HighPriority,
        Definitions.PacketReliability reliability = Definitions.PacketReliability.ReliableOrdered,
        int orderingChannel = 0)
    {
        return rakNet.SendPacket(BridgeBitStream.Borrow(bs.Id), playerId,
            (BridgePacketPriority)(int)priority, (BridgePacketReliability)(int)reliability,
            (byte)orderingChannel) ? 1 : 0;
    }

    public int SendRpc(BitStream bs, int playerId, int rpcId,
        Definitions.PacketPriority priority = Definitions.PacketPriority.HighPriority,
        Definitions.PacketReliability reliability = Definitions.PacketReliability.ReliableOrdered,
        int orderingChannel = 0)
    {
        return rakNet.SendRpc(BridgeBitStream.Borrow(bs.Id), playerId, rpcId,
            (BridgePacketPriority)(int)priority, (BridgePacketReliability)(int)reliability,
            (byte)orderingChannel) ? 1 : 0;
    }

    public int EmulateIncomingPacket(BitStream bs, int playerId)
    {
        return rakNet.EmulateIncomingPacket(BridgeBitStream.Borrow(bs.Id), playerId) ? 1 : 0;
    }

    public int EmulateIncomingRpc(BitStream bs, int playerId, int rpcId)
    {
        return rakNet.EmulateIncomingRpc(BridgeBitStream.Borrow(bs.Id), playerId, rpcId) ? 1 : 0;
    }
}
