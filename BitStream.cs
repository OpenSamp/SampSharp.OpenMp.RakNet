using System;
using System.Text;
using SampSharp.RakNet.Entities.Interop;

namespace SampSharp.RakNet.Entities;

/// <summary>
/// Обёртка BitStream-handle из pawnraknet.dll. Managed представление —
/// <see cref="Id"/> (int32), под которым живёт native BitStream в pool'е.
///
/// Жизненный цикл:
/// <list type="bullet">
/// <item><b>Owned</b> (создан через <see cref="New"/>): disposing освобождает ресурс.</item>
/// <item><b>Borrowed</b> (получен через event-callback): dispose — no-op, handle
///   инвалидируется native-стороной сразу после возврата из callback'а.</item>
/// </list>
/// </summary>
public struct BitStream : IDisposable
{
    /// <summary>Native-handle из <c>BitStreamHandleTable</c>. 0 — invalid.</summary>
    public int Id { get; private set; }

    /// <summary>Owns the native allocation (<see cref="Dispose"/> will call <see cref="RakNetInterop.RakNet_Bs_Delete"/>).</summary>
    public bool Owned { get; }

    private BitStream(int id, bool owned)
    {
        Id = id;
        Owned = owned;
    }

    /// <summary>Alloc a fresh BitStream from the plugin pool.</summary>
    public static BitStream New() => new(RakNetInterop.RakNet_Bs_New(), owned: true);

    /// <summary>Copy contents of an existing BitStream into a new allocation.</summary>
    public static BitStream NewCopy(BitStream source) => new(RakNetInterop.RakNet_Bs_NewCopy(source.Id), owned: true);

    /// <summary>
    /// Wrap a handle received from an event callback. The returned <see cref="BitStream"/>
    /// does NOT own the allocation; after the callback returns the handle is invalid.
    /// </summary>
    public static BitStream Borrow(int handle) => new(handle, owned: false);

    /// <summary>True if the handle is non-zero. Does not check native liveness.</summary>
    public readonly bool IsValid => Id != 0;

    public void Dispose()
    {
        if (Owned && Id != 0)
        {
            RakNetInterop.RakNet_Bs_Delete(Id);
            Id = 0;
        }
    }

    // ----- Pointer / size queries --------------------------------------------

    public readonly void Reset() => RakNetInterop.RakNet_Bs_Reset(Id);
    public readonly void ResetReadPointer() => RakNetInterop.RakNet_Bs_ResetReadPointer(Id);
    public readonly void ResetWritePointer() => RakNetInterop.RakNet_Bs_ResetWritePointer(Id);
    public readonly void IgnoreBits(int bits) => RakNetInterop.RakNet_Bs_IgnoreBits(Id, bits);

    public int ReadOffset
    {
        readonly get => RakNetInterop.RakNet_Bs_GetReadOffset(Id);
        set => RakNetInterop.RakNet_Bs_SetReadOffset(Id, value);
    }
    public int WriteOffset
    {
        readonly get => RakNetInterop.RakNet_Bs_GetWriteOffset(Id);
        set => RakNetInterop.RakNet_Bs_SetWriteOffset(Id, value);
    }

    public readonly int NumberOfBitsUsed => RakNetInterop.RakNet_Bs_NumberOfBitsUsed(Id);
    public readonly int NumberOfBytesUsed => RakNetInterop.RakNet_Bs_NumberOfBytesUsed(Id);
    public readonly int NumberOfUnreadBits => RakNetInterop.RakNet_Bs_NumberOfUnreadBits(Id);
    public readonly int NumberOfBitsAllocated => RakNetInterop.RakNet_Bs_NumberOfBitsAllocated(Id);

    // ----- Write primitives --------------------------------------------------
    //
    // <paramref name="compressed"/> enables RakNet's built-in delta compression.
    // Corresponds to PR_Cxxx tags in Pawn.RakNet.inc.

    public readonly void WriteInt8(sbyte v, bool compressed = false) => RakNetInterop.RakNet_Bs_WriteInt8(Id, v, compressed);
    public readonly void WriteInt16(short v, bool compressed = false) => RakNetInterop.RakNet_Bs_WriteInt16(Id, v, compressed);
    public readonly void WriteInt32(int v, bool compressed = false) => RakNetInterop.RakNet_Bs_WriteInt32(Id, v, compressed);
    public readonly void WriteUint8(byte v, bool compressed = false) => RakNetInterop.RakNet_Bs_WriteUint8(Id, v, compressed);
    public readonly void WriteUint16(ushort v, bool compressed = false) => RakNetInterop.RakNet_Bs_WriteUint16(Id, v, compressed);
    public readonly void WriteUint32(uint v, bool compressed = false) => RakNetInterop.RakNet_Bs_WriteUint32(Id, v, compressed);
    public readonly void WriteFloat(float v, bool compressed = false) => RakNetInterop.RakNet_Bs_WriteFloat(Id, v, compressed);
    public readonly void WriteBool(bool v, bool compressed = false) => RakNetInterop.RakNet_Bs_WriteBool(Id, v, compressed);

    public readonly unsafe void WriteString(string value)
    {
        if (string.IsNullOrEmpty(value)) return;
        int n = Encoding.UTF8.GetByteCount(value);
        Span<byte> buf = n <= 1024 ? stackalloc byte[n] : new byte[n];
        Encoding.UTF8.GetBytes(value, buf);
        fixed (byte* p = buf) RakNetInterop.RakNet_Bs_WriteString(Id, p, n);
    }

    public readonly unsafe void WriteStringCompressed(string value)
    {
        if (string.IsNullOrEmpty(value)) return;
        int n = Encoding.UTF8.GetByteCount(value);
        Span<byte> buf = n <= 1024 ? stackalloc byte[n] : new byte[n];
        Encoding.UTF8.GetBytes(value, buf);
        fixed (byte* p = buf) RakNetInterop.RakNet_Bs_WriteStringCompressed(Id, p, n);
    }

    public readonly unsafe void WriteString8(string value)
    {
        if (value is null) value = string.Empty;
        int n = Encoding.UTF8.GetByteCount(value);
        if (n > byte.MaxValue) throw new ArgumentException($"String8 length must fit in byte, got {n}");
        Span<byte> buf = n <= 1024 ? stackalloc byte[n] : new byte[n];
        Encoding.UTF8.GetBytes(value, buf);
        fixed (byte* p = buf) RakNetInterop.RakNet_Bs_WriteString8(Id, p, n);
    }

    public readonly unsafe void WriteString32(string value)
    {
        if (value is null) value = string.Empty;
        int n = Encoding.UTF8.GetByteCount(value);
        Span<byte> buf = n <= 1024 ? stackalloc byte[n] : new byte[n];
        Encoding.UTF8.GetBytes(value, buf);
        fixed (byte* p = buf) RakNetInterop.RakNet_Bs_WriteString32(Id, p, n);
    }

    public readonly unsafe void WriteBits(ReadOnlySpan<byte> data, int numberOfBits, bool rightAligned = true)
    {
        fixed (byte* p = data) RakNetInterop.RakNet_Bs_WriteBits(Id, p, numberOfBits, rightAligned);
    }

    public readonly void WriteFloat3(float x, float y, float z) => RakNetInterop.RakNet_Bs_WriteFloat3(Id, x, y, z);
    public readonly void WriteFloat4(float x, float y, float z, float w) => RakNetInterop.RakNet_Bs_WriteFloat4(Id, x, y, z, w);
    public readonly void WriteVector(float x, float y, float z) => RakNetInterop.RakNet_Bs_WriteVector(Id, x, y, z);
    public readonly void WriteNormQuat(float w, float x, float y, float z) => RakNetInterop.RakNet_Bs_WriteNormQuat(Id, w, x, y, z);

    // ----- Read primitives ---------------------------------------------------

    public readonly unsafe sbyte ReadInt8(bool compressed = false)
    { sbyte v = 0; RakNetInterop.RakNet_Bs_ReadInt8(Id, &v, compressed); return v; }
    public readonly unsafe short ReadInt16(bool compressed = false)
    { short v = 0; RakNetInterop.RakNet_Bs_ReadInt16(Id, &v, compressed); return v; }
    public readonly unsafe int ReadInt32(bool compressed = false)
    { int v = 0; RakNetInterop.RakNet_Bs_ReadInt32(Id, &v, compressed); return v; }
    public readonly unsafe byte ReadUint8(bool compressed = false)
    { byte v = 0; RakNetInterop.RakNet_Bs_ReadUint8(Id, &v, compressed); return v; }
    public readonly unsafe ushort ReadUint16(bool compressed = false)
    { ushort v = 0; RakNetInterop.RakNet_Bs_ReadUint16(Id, &v, compressed); return v; }
    public readonly unsafe uint ReadUint32(bool compressed = false)
    { uint v = 0; RakNetInterop.RakNet_Bs_ReadUint32(Id, &v, compressed); return v; }
    public readonly unsafe float ReadFloat(bool compressed = false)
    { float v = 0; RakNetInterop.RakNet_Bs_ReadFloat(Id, &v, compressed); return v; }
    public readonly unsafe bool ReadBool(bool compressed = false)
    { byte v = 0; RakNetInterop.RakNet_Bs_ReadBool(Id, &v, compressed); return v != 0; }

    public readonly unsafe string ReadString(int length)
    {
        if (length <= 0) return string.Empty;
        Span<byte> buf = length <= 1024 ? stackalloc byte[length] : new byte[length];
        fixed (byte* p = buf)
        {
            if (!RakNetInterop.RakNet_Bs_ReadString(Id, p, length)) return string.Empty;
        }
        int trim = buf.IndexOf((byte)0);
        return Encoding.UTF8.GetString(trim >= 0 ? buf[..trim] : buf);
    }

    public readonly unsafe string ReadStringCompressed(int maxLength)
    {
        if (maxLength <= 0) return string.Empty;
        Span<byte> buf = maxLength <= 1024 ? stackalloc byte[maxLength] : new byte[maxLength];
        fixed (byte* p = buf)
        {
            if (!RakNetInterop.RakNet_Bs_ReadStringCompressed(Id, p, maxLength)) return string.Empty;
        }
        int trim = buf.IndexOf((byte)0);
        return Encoding.UTF8.GetString(trim >= 0 ? buf[..trim] : buf);
    }

    public readonly unsafe string ReadString8(int maxLength = 256)
    {
        Span<byte> buf = maxLength <= 1024 ? stackalloc byte[maxLength] : new byte[maxLength];
        int actual;
        fixed (byte* p = buf) actual = RakNetInterop.RakNet_Bs_ReadString8(Id, p, maxLength);
        return actual <= 0 ? string.Empty : Encoding.UTF8.GetString(buf[..actual]);
    }

    public readonly unsafe string ReadString32(int maxLength = 4096)
    {
        Span<byte> buf = maxLength <= 1024 ? stackalloc byte[maxLength] : new byte[maxLength];
        int actual;
        fixed (byte* p = buf) actual = RakNetInterop.RakNet_Bs_ReadString32(Id, p, maxLength);
        return actual <= 0 ? string.Empty : Encoding.UTF8.GetString(buf[..actual]);
    }

    public readonly unsafe bool ReadBits(Span<byte> outData, int numberOfBits, bool rightAligned = true)
    {
        fixed (byte* p = outData) return RakNetInterop.RakNet_Bs_ReadBits(Id, p, numberOfBits, rightAligned);
    }

    public readonly unsafe void ReadFloat3(out float x, out float y, out float z)
    {
        float ax, ay, az; ax = ay = az = 0;
        RakNetInterop.RakNet_Bs_ReadFloat3(Id, &ax, &ay, &az);
        x = ax; y = ay; z = az;
    }

    public readonly unsafe void ReadFloat4(out float x, out float y, out float z, out float w)
    {
        float ax, ay, az, aw; ax = ay = az = aw = 0;
        RakNetInterop.RakNet_Bs_ReadFloat4(Id, &ax, &ay, &az, &aw);
        x = ax; y = ay; z = az; w = aw;
    }

    public readonly unsafe void ReadVector(out float x, out float y, out float z)
    {
        float ax, ay, az; ax = ay = az = 0;
        RakNetInterop.RakNet_Bs_ReadVector(Id, &ax, &ay, &az);
        x = ax; y = ay; z = az;
    }

    public readonly unsafe void ReadNormQuat(out float w, out float x, out float y, out float z)
    {
        float aw, ax, ay, az; aw = ax = ay = az = 0;
        RakNetInterop.RakNet_Bs_ReadNormQuat(Id, &aw, &ax, &ay, &az);
        w = aw; x = ax; y = ay; z = az;
    }
}
