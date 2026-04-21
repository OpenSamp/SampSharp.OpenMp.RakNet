using System.Runtime.InteropServices;

namespace SampSharp.RakNet.Entities.Interop;

/// <summary>
/// P/Invoke-биндинги к C-exports в SampSharp.RakNet.dll, которые через
/// <c>IPawnRakNetComponent</c> IExtension форвардят вызовы в pawnraknet.dll.
///
/// Если pawnraknet.dll не загружен, все RakNet_* возвращают 0/false.
/// Проверить через <see cref="RakNet_IsAvailable"/>.
/// </summary>
internal static partial class RakNetInterop
{
    private const string Lib = "SampSharp.RakNet";

    [LibraryImport(Lib)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool RakNet_IsAvailable();

    // ----- BitStream lifecycle -------------------------------------------------

    [LibraryImport(Lib)] internal static partial int RakNet_Bs_New();
    [LibraryImport(Lib)] internal static partial int RakNet_Bs_NewCopy(int handle);
    [LibraryImport(Lib)] internal static partial void RakNet_Bs_Delete(int handle);

    [LibraryImport(Lib)] internal static partial void RakNet_Bs_Reset(int handle);
    [LibraryImport(Lib)] internal static partial void RakNet_Bs_ResetReadPointer(int handle);
    [LibraryImport(Lib)] internal static partial void RakNet_Bs_ResetWritePointer(int handle);
    [LibraryImport(Lib)] internal static partial void RakNet_Bs_IgnoreBits(int handle, int numberOfBits);

    [LibraryImport(Lib)] internal static partial int RakNet_Bs_GetReadOffset(int handle);
    [LibraryImport(Lib)] internal static partial void RakNet_Bs_SetReadOffset(int handle, int offset);
    [LibraryImport(Lib)] internal static partial int RakNet_Bs_GetWriteOffset(int handle);
    [LibraryImport(Lib)] internal static partial void RakNet_Bs_SetWriteOffset(int handle, int offset);

    [LibraryImport(Lib)] internal static partial int RakNet_Bs_NumberOfBitsUsed(int handle);
    [LibraryImport(Lib)] internal static partial int RakNet_Bs_NumberOfBytesUsed(int handle);
    [LibraryImport(Lib)] internal static partial int RakNet_Bs_NumberOfUnreadBits(int handle);
    [LibraryImport(Lib)] internal static partial int RakNet_Bs_NumberOfBitsAllocated(int handle);

    // ----- Write primitives ----------------------------------------------------

    [LibraryImport(Lib)] internal static partial void RakNet_Bs_WriteInt8(int h, sbyte v, [MarshalAs(UnmanagedType.I1)] bool compressed);
    [LibraryImport(Lib)] internal static partial void RakNet_Bs_WriteInt16(int h, short v, [MarshalAs(UnmanagedType.I1)] bool compressed);
    [LibraryImport(Lib)] internal static partial void RakNet_Bs_WriteInt32(int h, int v, [MarshalAs(UnmanagedType.I1)] bool compressed);
    [LibraryImport(Lib)] internal static partial void RakNet_Bs_WriteUint8(int h, byte v, [MarshalAs(UnmanagedType.I1)] bool compressed);
    [LibraryImport(Lib)] internal static partial void RakNet_Bs_WriteUint16(int h, ushort v, [MarshalAs(UnmanagedType.I1)] bool compressed);
    [LibraryImport(Lib)] internal static partial void RakNet_Bs_WriteUint32(int h, uint v, [MarshalAs(UnmanagedType.I1)] bool compressed);
    [LibraryImport(Lib)] internal static partial void RakNet_Bs_WriteFloat(int h, float v, [MarshalAs(UnmanagedType.I1)] bool compressed);
    [LibraryImport(Lib)] internal static partial void RakNet_Bs_WriteBool(int h, [MarshalAs(UnmanagedType.I1)] bool v, [MarshalAs(UnmanagedType.I1)] bool compressed);

    [LibraryImport(Lib)] internal static unsafe partial void RakNet_Bs_WriteString(int h, byte* data, int length);
    [LibraryImport(Lib)] internal static unsafe partial void RakNet_Bs_WriteStringCompressed(int h, byte* data, int length);
    [LibraryImport(Lib)] internal static unsafe partial void RakNet_Bs_WriteString8(int h, byte* data, int length);
    [LibraryImport(Lib)] internal static unsafe partial void RakNet_Bs_WriteString32(int h, byte* data, int length);

    [LibraryImport(Lib)] internal static unsafe partial void RakNet_Bs_WriteBits(int h, byte* data, int numberOfBits, [MarshalAs(UnmanagedType.I1)] bool rightAlignedBits);

    [LibraryImport(Lib)] internal static partial void RakNet_Bs_WriteFloat3(int h, float x, float y, float z);
    [LibraryImport(Lib)] internal static partial void RakNet_Bs_WriteFloat4(int h, float x, float y, float z, float w);
    [LibraryImport(Lib)] internal static partial void RakNet_Bs_WriteVector(int h, float x, float y, float z);
    [LibraryImport(Lib)] internal static partial void RakNet_Bs_WriteNormQuat(int h, float x, float y, float z, float w);

    // ----- Read primitives -----------------------------------------------------

    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static unsafe partial bool RakNet_Bs_ReadInt8(int h, sbyte* outValue, [MarshalAs(UnmanagedType.I1)] bool compressed);
    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static unsafe partial bool RakNet_Bs_ReadInt16(int h, short* outValue, [MarshalAs(UnmanagedType.I1)] bool compressed);
    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static unsafe partial bool RakNet_Bs_ReadInt32(int h, int* outValue, [MarshalAs(UnmanagedType.I1)] bool compressed);
    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static unsafe partial bool RakNet_Bs_ReadUint8(int h, byte* outValue, [MarshalAs(UnmanagedType.I1)] bool compressed);
    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static unsafe partial bool RakNet_Bs_ReadUint16(int h, ushort* outValue, [MarshalAs(UnmanagedType.I1)] bool compressed);
    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static unsafe partial bool RakNet_Bs_ReadUint32(int h, uint* outValue, [MarshalAs(UnmanagedType.I1)] bool compressed);
    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static unsafe partial bool RakNet_Bs_ReadFloat(int h, float* outValue, [MarshalAs(UnmanagedType.I1)] bool compressed);
    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static unsafe partial bool RakNet_Bs_ReadBool(int h, byte* outValue, [MarshalAs(UnmanagedType.I1)] bool compressed);

    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static unsafe partial bool RakNet_Bs_ReadString(int h, byte* outBuffer, int maxLength);
    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static unsafe partial bool RakNet_Bs_ReadStringCompressed(int h, byte* outBuffer, int maxLength);
    [LibraryImport(Lib)] internal static unsafe partial int RakNet_Bs_ReadString8(int h, byte* outBuffer, int maxLength);
    [LibraryImport(Lib)] internal static unsafe partial int RakNet_Bs_ReadString32(int h, byte* outBuffer, int maxLength);

    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static unsafe partial bool RakNet_Bs_ReadBits(int h, byte* outData, int numberOfBits, [MarshalAs(UnmanagedType.I1)] bool rightAlignedBits);

    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static unsafe partial bool RakNet_Bs_ReadFloat3(int h, float* x, float* y, float* z);
    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static unsafe partial bool RakNet_Bs_ReadFloat4(int h, float* x, float* y, float* z, float* w);
    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static unsafe partial bool RakNet_Bs_ReadVector(int h, float* x, float* y, float* z);
    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static unsafe partial bool RakNet_Bs_ReadNormQuat(int h, float* x, float* y, float* z, float* w);

    // ----- Send / Emulate ------------------------------------------------------

    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool RakNet_SendPacket(int h, int playerId, int priority, int reliability, byte orderingChannel);

    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool RakNet_SendRPC(int h, int playerId, int rpcId, int priority, int reliability, byte orderingChannel);

    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool RakNet_EmulateIncomingPacket(int h, int playerId);

    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool RakNet_EmulateIncomingRPC(int h, int playerId, int rpcId);

    // ----- Custom RPC ----------------------------------------------------------

    [LibraryImport(Lib)] internal static partial void RakNet_SetCustomRPC(int rpcId);
    [LibraryImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool RakNet_IsCustomRPC(int rpcId);

    // ----- Event callback registration -----------------------------------------

    [LibraryImport(Lib)] internal static unsafe partial void RakNet_SetCallback_IncomingPacket(
        delegate* unmanaged[Cdecl]<int, int, int, byte> fn);
    [LibraryImport(Lib)] internal static unsafe partial void RakNet_SetCallback_IncomingRPC(
        delegate* unmanaged[Cdecl]<int, int, int, byte> fn);
    [LibraryImport(Lib)] internal static unsafe partial void RakNet_SetCallback_IncomingCustomRPC(
        delegate* unmanaged[Cdecl]<int, int, int, byte> fn);
    [LibraryImport(Lib)] internal static unsafe partial void RakNet_SetCallback_OutgoingPacket(
        delegate* unmanaged[Cdecl]<int, int, int, byte> fn);
    [LibraryImport(Lib)] internal static unsafe partial void RakNet_SetCallback_OutgoingRPC(
        delegate* unmanaged[Cdecl]<int, int, int, byte> fn);
}
