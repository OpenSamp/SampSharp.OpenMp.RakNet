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
using System.Collections.Generic;
using System.Text;
using SampSharp.RakNet.Entities.Interop;
using SampSharp.RakNet.Entities.Pawn.Definitions;

namespace SampSharp.RakNet.Entities.Pawn;

// Legacy variadic BitStream API реимплементирован поверх
// SampSharp.OpenMp.RakNet.Interop.RakNetInterop (x64 open.mp world).
//
// Старая реализация опиралась на SampSharp.Core.Natives.NativeObjects + AMX-variadic
// native "BS_ReadValue" / "BS_WriteValue", которых в x64-стэке нет. Здесь мы
// проходим по tagged-аргументам и вызываем типизированные RakNet_Bs_Read{Write}Xxx.
public partial class BitStream
{
    #region BitStreamInternal

    protected static BitStreamInternal Internal { get; set; }

    static BitStream()
    {
        Internal = new BitStreamInternal();
    }

    public partial class BitStreamInternal
    {
        // ---- Lifecycle -------------------------------------------------------

        public virtual int BS_New() => RakNetInterop.RakNet_Bs_New();
        public virtual int BS_NewCopy(int bs) => RakNetInterop.RakNet_Bs_NewCopy(bs);

        public virtual int BS_Delete(ref int bs)
        {
            if (bs == 0) return 0;
            RakNetInterop.RakNet_Bs_Delete(bs);
            bs = 0;
            return 1;
        }

        public virtual int BS_RPC(int bs, int playerid, int rpcid, int priority, int reliability,
            int orderingchannel = 0)
            => RakNetInterop.RakNet_SendRPC(bs, playerid, rpcid, priority, reliability,
                (byte)orderingchannel) ? 1 : 0;

        public virtual int BS_Send(int bs, int playerid, int priority, int reliability, int orderingchannel = 0)
            => RakNetInterop.RakNet_SendPacket(bs, playerid, priority, reliability,
                (byte)orderingchannel) ? 1 : 0;

        // ---- Reset / pointer ops --------------------------------------------

        public virtual int BS_Reset(int bs)              { RakNetInterop.RakNet_Bs_Reset(bs); return 1; }
        public virtual int BS_ResetReadPointer(int bs)   { RakNetInterop.RakNet_Bs_ResetReadPointer(bs); return 1; }
        public virtual int BS_ResetWritePointer(int bs)  { RakNetInterop.RakNet_Bs_ResetWritePointer(bs); return 1; }
        public virtual int BS_IgnoreBits(int bs, int n)  { RakNetInterop.RakNet_Bs_IgnoreBits(bs, n); return 1; }

        public virtual int BS_SetWriteOffset(int bs, int offset)
        { RakNetInterop.RakNet_Bs_SetWriteOffset(bs, offset); return 1; }
        public virtual int BS_GetWriteOffset(int bs, out int offset)
        { offset = RakNetInterop.RakNet_Bs_GetWriteOffset(bs); return 1; }
        public virtual int BS_SetReadOffset(int bs, int offset)
        { RakNetInterop.RakNet_Bs_SetReadOffset(bs, offset); return 1; }
        public virtual int BS_GetReadOffset(int bs, out int offset)
        { offset = RakNetInterop.RakNet_Bs_GetReadOffset(bs); return 1; }

        public virtual int BS_GetNumberOfBitsUsed(int bs, out int number)
        { number = RakNetInterop.RakNet_Bs_NumberOfBitsUsed(bs); return 1; }
        public virtual int BS_GetNumberOfBytesUsed(int bs, out int number)
        { number = RakNetInterop.RakNet_Bs_NumberOfBytesUsed(bs); return 1; }
        public virtual int BS_GetNumberOfUnreadBits(int bs, out int number)
        { number = RakNetInterop.RakNet_Bs_NumberOfUnreadBits(bs); return 1; }
        public virtual int BS_GetNumberOfBitsAllocated(int bs, out int number)
        { number = RakNetInterop.RakNet_Bs_NumberOfBitsAllocated(bs); return 1; }

        // ---- Variadic Write --------------------------------------------------
        //
        // Argument convention (from VSRP/RakNet/BitStream.accessory.cs):
        //   (ParamType, value)                        — для примитивов
        //   (ParamType.Bits, int value, int count)    — bits + bit count
        //   (ParamType.String,   string data)         — строка без префикса
        //   (ParamType.String8,  string data)         — 1-byte-length prefix
        //   (ParamType.String32, string data)         — 4-byte-length prefix
        //   (ParamType.Float3, float x, float y, float z)
        //   (ParamType.Float4, float x, float y, float z, float w)
        //   (ParamType.Vector, float x, float y, float z)
        //   (ParamType.NormQuat, float w, float x, float y, float z)
        //   (ParamType.IgnoreBits, int count)
        public virtual void BS_WriteValue(int bs, params object[] arguments)
        {
            int i = 0;
            while (i < arguments.Length)
            {
                if (arguments[i] is not ParamType pt)
                    throw new RakNetException($"BS_WriteValue: expected ParamType at index {i}");
                i++;
                switch (pt)
                {
                    case ParamType.Int8:
                        RakNetInterop.RakNet_Bs_WriteInt8(bs, (sbyte)AsInt(arguments[i++]), false); break;
                    case ParamType.Int16:
                        RakNetInterop.RakNet_Bs_WriteInt16(bs, (short)AsInt(arguments[i++]), false); break;
                    case ParamType.Int32:
                        RakNetInterop.RakNet_Bs_WriteInt32(bs, AsInt(arguments[i++]), false); break;
                    case ParamType.UInt8:
                        RakNetInterop.RakNet_Bs_WriteUint8(bs, (byte)AsInt(arguments[i++]), false); break;
                    case ParamType.UInt16:
                        RakNetInterop.RakNet_Bs_WriteUint16(bs, (ushort)AsInt(arguments[i++]), false); break;
                    case ParamType.UInt32:
                        RakNetInterop.RakNet_Bs_WriteUint32(bs, (uint)AsInt(arguments[i++]), false); break;
                    case ParamType.Float:
                        RakNetInterop.RakNet_Bs_WriteFloat(bs, AsFloat(arguments[i++]), false); break;
                    case ParamType.Bool:
                        RakNetInterop.RakNet_Bs_WriteBool(bs, AsBool(arguments[i++]), false); break;

                    case ParamType.CompressedInt8:
                        RakNetInterop.RakNet_Bs_WriteInt8(bs, (sbyte)AsInt(arguments[i++]), true); break;
                    case ParamType.CompressedInt16:
                        RakNetInterop.RakNet_Bs_WriteInt16(bs, (short)AsInt(arguments[i++]), true); break;
                    case ParamType.CompressedInt32:
                        RakNetInterop.RakNet_Bs_WriteInt32(bs, AsInt(arguments[i++]), true); break;
                    case ParamType.CompressedUInt8:
                        RakNetInterop.RakNet_Bs_WriteUint8(bs, (byte)AsInt(arguments[i++]), true); break;
                    case ParamType.CompressedUInt16:
                        RakNetInterop.RakNet_Bs_WriteUint16(bs, (ushort)AsInt(arguments[i++]), true); break;
                    case ParamType.CompressedUInt32:
                        RakNetInterop.RakNet_Bs_WriteUint32(bs, (uint)AsInt(arguments[i++]), true); break;
                    case ParamType.CompressedFloat:
                        RakNetInterop.RakNet_Bs_WriteFloat(bs, AsFloat(arguments[i++]), true); break;
                    case ParamType.CompressedBool:
                        RakNetInterop.RakNet_Bs_WriteBool(bs, AsBool(arguments[i++]), true); break;

                    case ParamType.String:
                        WriteStringUtf8(bs, (string)arguments[i++], compressed: false); break;
                    case ParamType.CompressedString:
                        WriteStringUtf8(bs, (string)arguments[i++], compressed: true); break;
                    case ParamType.String8:
                        WriteString8(bs, (string)arguments[i++]); break;
                    case ParamType.String32:
                        WriteString32(bs, (string)arguments[i++]); break;

                    case ParamType.Bits:
                    {
                        int value = AsInt(arguments[i++]);
                        int count = AsInt(arguments[i++]);
                        unsafe
                        {
                            // Little-endian encoding; RakNet's WriteBits чтит rightAligned=true как
                            // "bits low→high within each byte and skip trailing zeros of last byte".
                            byte* buf = stackalloc byte[8];
                            for (int b = 0; b < 8; b++) buf[b] = (byte)((value >> (b * 8)) & 0xff);
                            RakNetInterop.RakNet_Bs_WriteBits(bs, buf, count, true);
                        }
                        break;
                    }

                    case ParamType.Float3:
                    {
                        float x = AsFloat(arguments[i++]);
                        float y = AsFloat(arguments[i++]);
                        float z = AsFloat(arguments[i++]);
                        RakNetInterop.RakNet_Bs_WriteFloat3(bs, x, y, z);
                        break;
                    }
                    case ParamType.Float4:
                    {
                        float x = AsFloat(arguments[i++]);
                        float y = AsFloat(arguments[i++]);
                        float z = AsFloat(arguments[i++]);
                        float w = AsFloat(arguments[i++]);
                        RakNetInterop.RakNet_Bs_WriteFloat4(bs, x, y, z, w);
                        break;
                    }
                    case ParamType.Vector:
                    {
                        float x = AsFloat(arguments[i++]);
                        float y = AsFloat(arguments[i++]);
                        float z = AsFloat(arguments[i++]);
                        RakNetInterop.RakNet_Bs_WriteVector(bs, x, y, z);
                        break;
                    }
                    case ParamType.NormQuat:
                    {
                        float w = AsFloat(arguments[i++]);
                        float x = AsFloat(arguments[i++]);
                        float y = AsFloat(arguments[i++]);
                        float z = AsFloat(arguments[i++]);
                        RakNetInterop.RakNet_Bs_WriteNormQuat(bs, w, x, y, z);
                        break;
                    }

                    case ParamType.IgnoreBits:
                    {
                        int count = AsInt(arguments[i++]);
                        int off = RakNetInterop.RakNet_Bs_GetWriteOffset(bs);
                        RakNetInterop.RakNet_Bs_SetWriteOffset(bs, off + count);
                        break;
                    }

                    default:
                        throw new RakNetException($"BS_WriteValue: unsupported ParamType {pt}");
                }
            }
        }

        // ---- Variadic Read ---------------------------------------------------
        //
        // Argument convention (from VSRP/RakNet/Syncs/*.cs):
        //   (ParamType, string label)                 — читает значение, кладёт в dict[label]
        //   (ParamType.Bits, string label, int count) — bits + bit count
        //   (ParamType.String,   string label, int length) — прочитать N байт
        //   (ParamType.String8,  string label) — 1-byte length prefix + payload
        //   (ParamType.String32, string label) — 4-byte length prefix + payload
        //   (ParamType.Float3, "label") → dict["label_0"], dict["label_1"], dict["label_2"]
        //     — но consumer'ы чаще сами расписывают 3×Float, здесь "_N" — страховка.
        public virtual Dictionary<string, object> BS_ReadValue(int bs, params object[] arguments)
        {
            var result = new Dictionary<string, object>();
            int i = 0;
            while (i < arguments.Length)
            {
                if (arguments[i] is not ParamType pt)
                    throw new RakNetException($"BS_ReadValue: expected ParamType at index {i}");
                i++;
                string label = (string)arguments[i++];

                switch (pt)
                {
                    case ParamType.Int8:     result[label] = (int)ReadInt8(bs, false); break;
                    case ParamType.Int16:    result[label] = (int)ReadInt16(bs, false); break;
                    case ParamType.Int32:    result[label] = ReadInt32(bs, false); break;
                    case ParamType.UInt8:    result[label] = (int)ReadUint8(bs, false); break;
                    case ParamType.UInt16:   result[label] = (int)ReadUint16(bs, false); break;
                    case ParamType.UInt32:   result[label] = (int)ReadUint32(bs, false); break;
                    case ParamType.Float:    result[label] = ReadFloat(bs, false); break;
                    case ParamType.Bool:     result[label] = ReadBool(bs, false); break;

                    case ParamType.CompressedInt8:    result[label] = (int)ReadInt8(bs, true); break;
                    case ParamType.CompressedInt16:   result[label] = (int)ReadInt16(bs, true); break;
                    case ParamType.CompressedInt32:   result[label] = ReadInt32(bs, true); break;
                    case ParamType.CompressedUInt8:   result[label] = (int)ReadUint8(bs, true); break;
                    case ParamType.CompressedUInt16:  result[label] = (int)ReadUint16(bs, true); break;
                    case ParamType.CompressedUInt32:  result[label] = (int)ReadUint32(bs, true); break;
                    case ParamType.CompressedFloat:   result[label] = ReadFloat(bs, true); break;
                    case ParamType.CompressedBool:    result[label] = ReadBool(bs, true); break;

                    case ParamType.String:
                        result[label] = ReadStringUtf8(bs, AsInt(arguments[i++]), compressed: false); break;
                    case ParamType.CompressedString:
                        result[label] = ReadStringUtf8(bs, AsInt(arguments[i++]), compressed: true); break;
                    case ParamType.String8:
                        result[label] = ReadString8(bs); break;
                    case ParamType.String32:
                        result[label] = ReadString32(bs); break;

                    case ParamType.Bits:
                    {
                        int count = AsInt(arguments[i++]);
                        result[label] = ReadBits(bs, count);
                        break;
                    }

                    case ParamType.Float3:
                    {
                        float x = ReadFloat(bs, false), y = ReadFloat(bs, false), z = ReadFloat(bs, false);
                        result[$"{label}_0"] = x; result[$"{label}_1"] = y; result[$"{label}_2"] = z;
                        break;
                    }
                    case ParamType.Float4:
                    {
                        float x = ReadFloat(bs, false), y = ReadFloat(bs, false),
                              z = ReadFloat(bs, false), w = ReadFloat(bs, false);
                        result[$"{label}_0"] = x; result[$"{label}_1"] = y;
                        result[$"{label}_2"] = z; result[$"{label}_3"] = w;
                        break;
                    }
                    case ParamType.Vector:
                    {
                        unsafe
                        {
                            float x = 0, y = 0, z = 0;
                            RakNetInterop.RakNet_Bs_ReadVector(bs, &x, &y, &z);
                            result[$"{label}_0"] = x; result[$"{label}_1"] = y; result[$"{label}_2"] = z;
                        }
                        break;
                    }
                    case ParamType.NormQuat:
                    {
                        unsafe
                        {
                            float w = 0, x = 0, y = 0, z = 0;
                            RakNetInterop.RakNet_Bs_ReadNormQuat(bs, &w, &x, &y, &z);
                            // NormQuat order: W, X, Y, Z. VSRP sometimes reads с суффиксами
                            // _X/_Y/_Z/_W, иногда _0/_1/_2/_3 — заполняем оба варианта.
                            result[$"{label}_W"] = w; result[$"{label}_X"] = x;
                            result[$"{label}_Y"] = y; result[$"{label}_Z"] = z;
                            result[$"{label}_0"] = w; result[$"{label}_1"] = x;
                            result[$"{label}_2"] = y; result[$"{label}_3"] = z;
                        }
                        break;
                    }

                    case ParamType.IgnoreBits:
                    {
                        int count = AsInt(arguments[i++]);
                        RakNetInterop.RakNet_Bs_IgnoreBits(bs, count);
                        break;
                    }

                    default:
                        throw new RakNetException($"BS_ReadValue: unsupported ParamType {pt}");
                }
            }
            return result;
        }

        // ---- Helpers ---------------------------------------------------------

        private static int AsInt(object o) => Convert.ToInt32(o);
        private static float AsFloat(object o) => Convert.ToSingle(o);
        private static bool AsBool(object o) => o is bool b ? b : Convert.ToInt32(o) != 0;

        private static unsafe sbyte ReadInt8(int bs, bool compressed)
        { sbyte v = 0; RakNetInterop.RakNet_Bs_ReadInt8(bs, &v, compressed); return v; }
        private static unsafe short ReadInt16(int bs, bool compressed)
        { short v = 0; RakNetInterop.RakNet_Bs_ReadInt16(bs, &v, compressed); return v; }
        private static unsafe int ReadInt32(int bs, bool compressed)
        { int v = 0; RakNetInterop.RakNet_Bs_ReadInt32(bs, &v, compressed); return v; }
        private static unsafe byte ReadUint8(int bs, bool compressed)
        { byte v = 0; RakNetInterop.RakNet_Bs_ReadUint8(bs, &v, compressed); return v; }
        private static unsafe ushort ReadUint16(int bs, bool compressed)
        { ushort v = 0; RakNetInterop.RakNet_Bs_ReadUint16(bs, &v, compressed); return v; }
        private static unsafe uint ReadUint32(int bs, bool compressed)
        { uint v = 0; RakNetInterop.RakNet_Bs_ReadUint32(bs, &v, compressed); return v; }
        private static unsafe float ReadFloat(int bs, bool compressed)
        { float v = 0; RakNetInterop.RakNet_Bs_ReadFloat(bs, &v, compressed); return v; }
        private static unsafe bool ReadBool(int bs, bool compressed)
        { byte v = 0; RakNetInterop.RakNet_Bs_ReadBool(bs, &v, compressed); return v != 0; }

        private static unsafe int ReadBits(int bs, int count)
        {
            byte* buf = stackalloc byte[8];
            RakNetInterop.RakNet_Bs_ReadBits(bs, buf, count, true);
            int value = 0;
            int bytes = (count + 7) / 8;
            for (int b = 0; b < bytes && b < 8; b++) value |= buf[b] << (b * 8);
            return value;
        }

        private static unsafe string ReadStringUtf8(int bs, int length, bool compressed)
        {
            if (length <= 0) return string.Empty;
            Span<byte> buf = length <= 1024 ? stackalloc byte[length] : new byte[length];
            bool ok;
            fixed (byte* p = buf)
                ok = compressed
                    ? RakNetInterop.RakNet_Bs_ReadStringCompressed(bs, p, length)
                    : RakNetInterop.RakNet_Bs_ReadString(bs, p, length);
            if (!ok) return string.Empty;
            int trim = buf.IndexOf((byte)0);
            return Encoding.UTF8.GetString(trim >= 0 ? buf[..trim] : buf);
        }

        private static unsafe string ReadString8(int bs, int max = 256)
        {
            Span<byte> buf = stackalloc byte[max];
            int actual;
            fixed (byte* p = buf) actual = RakNetInterop.RakNet_Bs_ReadString8(bs, p, max);
            return actual <= 0 ? string.Empty : Encoding.UTF8.GetString(buf[..actual]);
        }

        private static unsafe string ReadString32(int bs, int max = 4096)
        {
            Span<byte> buf = max <= 1024 ? stackalloc byte[max] : new byte[max];
            int actual;
            fixed (byte* p = buf) actual = RakNetInterop.RakNet_Bs_ReadString32(bs, p, max);
            return actual <= 0 ? string.Empty : Encoding.UTF8.GetString(buf[..actual]);
        }

        private static unsafe void WriteStringUtf8(int bs, string s, bool compressed)
        {
            if (string.IsNullOrEmpty(s)) return;
            int n = Encoding.UTF8.GetByteCount(s);
            Span<byte> buf = n <= 1024 ? stackalloc byte[n] : new byte[n];
            Encoding.UTF8.GetBytes(s, buf);
            fixed (byte* p = buf)
            {
                if (compressed) RakNetInterop.RakNet_Bs_WriteStringCompressed(bs, p, n);
                else            RakNetInterop.RakNet_Bs_WriteString(bs, p, n);
            }
        }

        private static unsafe void WriteString8(int bs, string s)
        {
            s ??= string.Empty;
            int n = Encoding.UTF8.GetByteCount(s);
            if (n > byte.MaxValue) throw new RakNetException($"String8 length must fit in byte, got {n}");
            Span<byte> buf = stackalloc byte[n];
            Encoding.UTF8.GetBytes(s, buf);
            fixed (byte* p = buf) RakNetInterop.RakNet_Bs_WriteString8(bs, p, n);
        }

        private static unsafe void WriteString32(int bs, string s)
        {
            s ??= string.Empty;
            int n = Encoding.UTF8.GetByteCount(s);
            Span<byte> buf = n <= 1024 ? stackalloc byte[n] : new byte[n];
            Encoding.UTF8.GetBytes(s, buf);
            fixed (byte* p = buf) RakNetInterop.RakNet_Bs_WriteString32(bs, p, n);
        }
    }

    #endregion
}
