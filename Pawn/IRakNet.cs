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
using SampSharp.RakNet.Entities.Pawn.Events;

namespace SampSharp.RakNet.Entities.Pawn;

public interface IRakNet
{
    event EventHandler<PacketRpcEventArgs> IncomingRpc;
    event EventHandler<PacketRpcEventArgs> IncomingCustomRpc;
    event EventHandler<PacketRpcEventArgs> OutgoingRpc;
    event EventHandler<PacketRpcEventArgs> OutcomingRpc;
    event EventHandler<PacketRpcEventArgs> IncomingPacket;
    event EventHandler<PacketRpcEventArgs> OutgoingPacket;
    event EventHandler<PacketRpcEventArgs> OutcomingPacket;

    void SetLogging(bool incomingRpc, bool outcomingRpc, bool incomingPacket, bool outcomingPacket, bool blockingRpc,
        bool blockingPacket);

    void BlockRpc();
    void BlockPacket();

    int SendPacket(BitStream bs, int playerId, Definitions.PacketPriority priority = Definitions.PacketPriority.HighPriority,
        Definitions.PacketReliability reliability = Definitions.PacketReliability.ReliableOrdered, int orderingChannel = 0);
    int SendRpc(BitStream bs, int playerId, int rpcId,
        Definitions.PacketPriority priority = Definitions.PacketPriority.HighPriority,
        Definitions.PacketReliability reliability = Definitions.PacketReliability.ReliableOrdered, int orderingChannel = 0);
    int EmulateIncomingPacket(BitStream bs, int playerId);
    int EmulateIncomingRpc(BitStream bs, int playerId, int rpcId);
}