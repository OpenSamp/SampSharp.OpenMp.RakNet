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
using SampSharp.Entities.SAMP;
using SampSharp.RakNet.Entities.Pawn.Events;

namespace SampSharp.RakNet.Entities.Pawn;

public partial class RakNet
{
    public event EventHandler<PacketRpcEventArgs>? IncomingRpc;
    public event EventHandler<PacketRpcEventArgs>? IncomingCustomRpc;
    public event EventHandler<PacketRpcEventArgs>? OutgoingRpc;
    public event EventHandler<PacketRpcEventArgs>? OutcomingRpc;
    public event EventHandler<PacketRpcEventArgs>? IncomingPacket;
    public event EventHandler<PacketRpcEventArgs>? OutgoingPacket;
    public event EventHandler<PacketRpcEventArgs>? OutcomingPacket;

    // SampSharp.OpenMp.RakNet dispatches these as (EntityId, int id, int bsHandle).
    // Declaring `Player player` lets the middleware resolve the Player component
    // from the EntityId; native playerid is read off `player.Entity.Handle`.
    //
    // Returning false → veto the packet/RPC at the networking layer. Used by
    // BlockRpc/BlockPacket (see RakNet.cs).

    [Event]
    public bool OnIncomingRPC(Player player, int rpcid, int bs)
    {
        int pid = player?.Entity.Handle ?? -1;
        IncomingRpc?.Invoke(this, new PacketRpcEventArgs(rpcid, pid, bs));
        if (LoggingIncomingRpc)
            Console.WriteLine($"[VSRP.RakNet] Hooking Incoming RPC {pid}, {rpcid}, {bs}");
        return true;
    }

    [Event]
    public bool OnIncomingCustomRPC(Player player, int rpcid, int bs)
    {
        int pid = player?.Entity.Handle ?? -1;
        IncomingCustomRpc?.Invoke(this, new PacketRpcEventArgs(rpcid, pid, bs));
        if (LoggingIncomingRpc)
            Console.WriteLine($"[VSRP.RakNet] Hooking Incoming Custom RPC {pid}, {rpcid}, {bs}");
        return true;
    }

    [Event]
    public bool OnOutgoingRPC(Player player, int rpcid, int bs)
    {
        int pid = player?.Entity.Handle ?? -1;
        var args = new PacketRpcEventArgs(rpcid, pid, bs);
        OutgoingRpc?.Invoke(this, args);
        OutcomingRpc?.Invoke(this, args);
        if (LoggingOutcomingRpc)
            Console.WriteLine($"[VSRP.RakNet] Hooking Outgoing RPC {pid}, {rpcid}, {bs}");

        return !ConsumeRpcBlock();
    }

    [Event]
    public bool OnIncomingPacket(Player player, int packetid, int bs)
    {
        int pid = player?.Entity.Handle ?? -1;
        IncomingPacket?.Invoke(this, new PacketRpcEventArgs(packetid, pid, bs));
        if (LoggingIncomingPacket)
            Console.WriteLine($"[VSRP.RakNet] Hooking Incoming Packet {pid}, {packetid}, {bs}");
        return true;
    }

    [Event]
    public bool OnOutgoingPacket(Player player, int packetid, int bs)
    {
        int pid = player?.Entity.Handle ?? -1;
        var args = new PacketRpcEventArgs(packetid, pid, bs);
        OutgoingPacket?.Invoke(this, args);
        OutcomingPacket?.Invoke(this, args);
        if (LoggingOutcomingPacket)
            Console.WriteLine($"[VSRP.RakNet] Hooking Outgoing Packet {pid}, {packetid}, {bs}");

        return !ConsumePacketBlock();
    }
}
