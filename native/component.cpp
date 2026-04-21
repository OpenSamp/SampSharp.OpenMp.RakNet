// SampSharp.RakNet — open.mp component that bridges pawnraknet.dll (exposed as
// IPawnRakNetComponent IExtension) to managed .NET via C-exports + function
// pointer callbacks.
//
// Architecture:
//   open.mp loads 3 components independently:
//     - pawnraknet.dll         (provides IPawnRakNetComponent extension)
//     - SampSharp.dll          (hosts the .NET runtime + gamemode)
//     - SampSharp.RakNet.dll   (THIS) — pure C-API shim
//
//   At onInit this component queryComponent's pawnraknet and registers a C++
//   event handler. Managed side (SampSharp.OpenMp.RakNet.csproj) P/Invokes
//   into this DLL's exports.
//
//   No dependency on SampSharp.dll — entirely decoupled.

#include <sdk.hpp>

#include "raknet-api.hpp"

namespace
{
    // Component identity. Different from both pawnraknet.dll and SampSharp.dll UIDs.
    constexpr UID kSampSharpRakNetUID = UID(0x537353526b4e7400ULL); // "SsSRkNt\0"

    IPawnRakNetComponent* g_rn = nullptr;

    // ---- Managed callback function pointers ----
    // Each event callback takes (playerId, id, bsHandle) and returns 0/1 (veto/continue).
    using FnEvent = unsigned char (__cdecl*)(int, int, int);

    FnEvent cb_incomingPacket     = nullptr;
    FnEvent cb_incomingRpc        = nullptr;
    FnEvent cb_incomingCustomRpc  = nullptr;
    FnEvent cb_outgoingPacket     = nullptr;
    FnEvent cb_outgoingRpc        = nullptr;

    class Handler : public IPawnRakNetEventHandler
    {
    public:
        bool onIncomingPacket(int p, int id, int bs) override
        { return cb_incomingPacket ? cb_incomingPacket(p, id, bs) != 0 : true; }
        bool onIncomingRPC(int p, int id, int bs) override
        { return cb_incomingRpc ? cb_incomingRpc(p, id, bs) != 0 : true; }
        bool onIncomingCustomRPC(int p, int id, int bs) override
        { return cb_incomingCustomRpc ? cb_incomingCustomRpc(p, id, bs) != 0 : true; }
        bool onOutgoingPacket(int p, int id, int bs) override
        { return cb_outgoingPacket ? cb_outgoingPacket(p, id, bs) != 0 : true; }
        bool onOutgoingRPC(int p, int id, int bs) override
        { return cb_outgoingRpc ? cb_outgoingRpc(p, id, bs) != 0 : true; }
    };

    Handler g_handler;
    bool g_handlerRegistered = false;

    class SampSharpRakNetComponent final : public IComponent
    {
    public:
        PROVIDE_UID(kSampSharpRakNetUID)

        StringView componentName() const override { return "SampSharp.RakNet"; }
        SemanticVersion componentVersion() const override { return SemanticVersion(1, 0, 0, 0); }

        void onLoad(ICore* c) override { core_ = c; }

        void onInit(IComponentList* components) override
        {
            if (!components) return;
            IComponent* rn = components->queryComponent(kPawnRakNetComponentUID);
            if (!rn)
            {
                if (core_) core_->logLn(LogLevel::Warning,
                    "SampSharp.RakNet: pawnraknet.dll not loaded; all RakNet_* calls will be no-op");
                return;
            }
            g_rn = queryExtension<IPawnRakNetComponent>(rn);
            if (!g_rn)
            {
                if (core_) core_->logLn(LogLevel::Warning,
                    "SampSharp.RakNet: pawnraknet.dll loaded but IPawnRakNetComponent extension missing; "
                    "use the OpenSamp Pawn.RakNet.OMP fork");
                return;
            }
            g_rn->addEventHandler(&g_handler);
            g_handlerRegistered = true;
            if (core_) core_->printLn("SampSharp.RakNet: bound to pawnraknet.dll IPawnRakNetComponent extension");
        }

        void free() override
        {
            if (g_rn && g_handlerRegistered)
            {
                g_rn->removeEventHandler(&g_handler);
                g_handlerRegistered = false;
            }
            g_rn = nullptr;
            delete this;
        }

        void reset() override {}

    private:
        ICore* core_ = nullptr;
    };

    SampSharpRakNetComponent* g_componentInstance = nullptr;
}

COMPONENT_ENTRY_POINT()
{
    if (!g_componentInstance) g_componentInstance = new SampSharpRakNetComponent();
    return g_componentInstance;
}

// ============================================================================
// C-exports. The RakNet_ prefix keeps managed P/Invoke EntryPoint names clean.
// ============================================================================

extern "C" SDK_EXPORT bool __CDECL RakNet_IsAvailable() { return g_rn != nullptr; }

// -------- BitStream lifecycle -------------------------------------------------

extern "C" SDK_EXPORT int __CDECL RakNet_Bs_New()
{ return g_rn ? g_rn->bitStreamNew() : 0; }
extern "C" SDK_EXPORT int __CDECL RakNet_Bs_NewCopy(int h)
{ return g_rn ? g_rn->bitStreamNewCopy(h) : 0; }
extern "C" SDK_EXPORT void __CDECL RakNet_Bs_Delete(int h)
{ if (g_rn) g_rn->bitStreamDelete(h); }

extern "C" SDK_EXPORT void __CDECL RakNet_Bs_Reset(int h)
{ if (g_rn) g_rn->bitStreamReset(h); }
extern "C" SDK_EXPORT void __CDECL RakNet_Bs_ResetReadPointer(int h)
{ if (g_rn) g_rn->bitStreamResetReadPointer(h); }
extern "C" SDK_EXPORT void __CDECL RakNet_Bs_ResetWritePointer(int h)
{ if (g_rn) g_rn->bitStreamResetWritePointer(h); }
extern "C" SDK_EXPORT void __CDECL RakNet_Bs_IgnoreBits(int h, int bits)
{ if (g_rn) g_rn->bitStreamIgnoreBits(h, bits); }

extern "C" SDK_EXPORT int __CDECL RakNet_Bs_GetReadOffset(int h)
{ return g_rn ? g_rn->bitStreamGetReadOffset(h) : 0; }
extern "C" SDK_EXPORT void __CDECL RakNet_Bs_SetReadOffset(int h, int off)
{ if (g_rn) g_rn->bitStreamSetReadOffset(h, off); }
extern "C" SDK_EXPORT int __CDECL RakNet_Bs_GetWriteOffset(int h)
{ return g_rn ? g_rn->bitStreamGetWriteOffset(h) : 0; }
extern "C" SDK_EXPORT void __CDECL RakNet_Bs_SetWriteOffset(int h, int off)
{ if (g_rn) g_rn->bitStreamSetWriteOffset(h, off); }

extern "C" SDK_EXPORT int __CDECL RakNet_Bs_NumberOfBitsUsed(int h)
{ return g_rn ? g_rn->bitStreamNumberOfBitsUsed(h) : 0; }
extern "C" SDK_EXPORT int __CDECL RakNet_Bs_NumberOfBytesUsed(int h)
{ return g_rn ? g_rn->bitStreamNumberOfBytesUsed(h) : 0; }
extern "C" SDK_EXPORT int __CDECL RakNet_Bs_NumberOfUnreadBits(int h)
{ return g_rn ? g_rn->bitStreamNumberOfUnreadBits(h) : 0; }
extern "C" SDK_EXPORT int __CDECL RakNet_Bs_NumberOfBitsAllocated(int h)
{ return g_rn ? g_rn->bitStreamNumberOfBitsAllocated(h) : 0; }

// -------- Write primitives ----------------------------------------------------

#define WRITE_EXPORT(name, CType)                                              \
    extern "C" SDK_EXPORT void __CDECL RakNet_Bs_Write##name(int h, CType v,   \
        bool c) { if (g_rn) g_rn->bitStreamWrite##name(h, v, c); }

WRITE_EXPORT(Int8,   int8_t)
WRITE_EXPORT(Int16,  int16_t)
WRITE_EXPORT(Int32,  int32_t)
WRITE_EXPORT(Uint8,  uint8_t)
WRITE_EXPORT(Uint16, uint16_t)
WRITE_EXPORT(Uint32, uint32_t)
WRITE_EXPORT(Float,  float)
WRITE_EXPORT(Bool,   bool)

#undef WRITE_EXPORT

extern "C" SDK_EXPORT void __CDECL RakNet_Bs_WriteString(int h, const char* s, int len)
{ if (g_rn) g_rn->bitStreamWriteString(h, s, len); }
extern "C" SDK_EXPORT void __CDECL RakNet_Bs_WriteStringCompressed(int h, const char* s, int len)
{ if (g_rn) g_rn->bitStreamWriteStringCompressed(h, s, len); }
extern "C" SDK_EXPORT void __CDECL RakNet_Bs_WriteString8(int h, const char* s, int len)
{ if (g_rn) g_rn->bitStreamWriteString8(h, s, len); }
extern "C" SDK_EXPORT void __CDECL RakNet_Bs_WriteString32(int h, const char* s, int len)
{ if (g_rn) g_rn->bitStreamWriteString32(h, s, len); }

extern "C" SDK_EXPORT void __CDECL RakNet_Bs_WriteBits(int h, const uint8_t* data, int nbits, bool rightAligned)
{ if (g_rn) g_rn->bitStreamWriteBits(h, data, nbits, rightAligned); }

extern "C" SDK_EXPORT void __CDECL RakNet_Bs_WriteFloat3(int h, float x, float y, float z)
{ if (g_rn) g_rn->bitStreamWriteFloat3(h, x, y, z); }
extern "C" SDK_EXPORT void __CDECL RakNet_Bs_WriteFloat4(int h, float x, float y, float z, float w)
{ if (g_rn) g_rn->bitStreamWriteFloat4(h, x, y, z, w); }
extern "C" SDK_EXPORT void __CDECL RakNet_Bs_WriteVector(int h, float x, float y, float z)
{ if (g_rn) g_rn->bitStreamWriteVector(h, x, y, z); }
extern "C" SDK_EXPORT void __CDECL RakNet_Bs_WriteNormQuat(int h, float x, float y, float z, float w)
{ if (g_rn) g_rn->bitStreamWriteNormQuat(h, x, y, z, w); }

// -------- Read primitives -----------------------------------------------------

#define READ_EXPORT(name, CType)                                               \
    extern "C" SDK_EXPORT bool __CDECL RakNet_Bs_Read##name(int h, CType* out, \
        bool c) {                                                              \
        if (!g_rn || !out) return false;                                       \
        return g_rn->bitStreamRead##name(h, *out, c);                          \
    }

READ_EXPORT(Int8,   int8_t)
READ_EXPORT(Int16,  int16_t)
READ_EXPORT(Int32,  int32_t)
READ_EXPORT(Uint8,  uint8_t)
READ_EXPORT(Uint16, uint16_t)
READ_EXPORT(Uint32, uint32_t)
READ_EXPORT(Float,  float)
READ_EXPORT(Bool,   bool)

#undef READ_EXPORT

extern "C" SDK_EXPORT bool __CDECL RakNet_Bs_ReadString(int h, char* buf, int maxLen)
{ return g_rn && g_rn->bitStreamReadString(h, buf, maxLen); }
extern "C" SDK_EXPORT bool __CDECL RakNet_Bs_ReadStringCompressed(int h, char* buf, int maxLen)
{ return g_rn && g_rn->bitStreamReadStringCompressed(h, buf, maxLen); }
extern "C" SDK_EXPORT int __CDECL RakNet_Bs_ReadString8(int h, char* buf, int maxLen)
{ return g_rn ? g_rn->bitStreamReadString8(h, buf, maxLen) : 0; }
extern "C" SDK_EXPORT int __CDECL RakNet_Bs_ReadString32(int h, char* buf, int maxLen)
{ return g_rn ? g_rn->bitStreamReadString32(h, buf, maxLen) : 0; }

extern "C" SDK_EXPORT bool __CDECL RakNet_Bs_ReadBits(int h, uint8_t* out, int nbits, bool rightAligned)
{ return g_rn && g_rn->bitStreamReadBits(h, out, nbits, rightAligned); }

extern "C" SDK_EXPORT bool __CDECL RakNet_Bs_ReadFloat3(int h, float* x, float* y, float* z)
{ return g_rn && x && y && z && g_rn->bitStreamReadFloat3(h, *x, *y, *z); }
extern "C" SDK_EXPORT bool __CDECL RakNet_Bs_ReadFloat4(int h, float* x, float* y, float* z, float* w)
{ return g_rn && x && y && z && w && g_rn->bitStreamReadFloat4(h, *x, *y, *z, *w); }
extern "C" SDK_EXPORT bool __CDECL RakNet_Bs_ReadVector(int h, float* x, float* y, float* z)
{ return g_rn && x && y && z && g_rn->bitStreamReadVector(h, *x, *y, *z); }
extern "C" SDK_EXPORT bool __CDECL RakNet_Bs_ReadNormQuat(int h, float* x, float* y, float* z, float* w)
{ return g_rn && x && y && z && w && g_rn->bitStreamReadNormQuat(h, *x, *y, *z, *w); }

// -------- Send / Emulate ------------------------------------------------------

extern "C" SDK_EXPORT bool __CDECL RakNet_SendPacket(int h, int pid, int prio, int reliab, uint8_t channel)
{ return g_rn && g_rn->sendPacket(h, pid, prio, reliab, channel); }
extern "C" SDK_EXPORT bool __CDECL RakNet_SendRPC(int h, int pid, int rpcId, int prio, int reliab, uint8_t channel)
{ return g_rn && g_rn->sendRPC(h, pid, rpcId, prio, reliab, channel); }
extern "C" SDK_EXPORT bool __CDECL RakNet_EmulateIncomingPacket(int h, int pid)
{ return g_rn && g_rn->emulateIncomingPacket(h, pid); }
extern "C" SDK_EXPORT bool __CDECL RakNet_EmulateIncomingRPC(int h, int pid, int rpcId)
{ return g_rn && g_rn->emulateIncomingRPC(h, pid, rpcId); }

// -------- Custom RPC ----------------------------------------------------------

extern "C" SDK_EXPORT void __CDECL RakNet_SetCustomRPC(int rpcId)
{ if (g_rn) g_rn->setCustomRPC(rpcId); }
extern "C" SDK_EXPORT bool __CDECL RakNet_IsCustomRPC(int rpcId)
{ return g_rn && g_rn->isCustomRPC(rpcId); }

// -------- Callback registration -----------------------------------------------

extern "C" SDK_EXPORT void __CDECL RakNet_SetCallback_IncomingPacket(FnEvent fn)    { cb_incomingPacket    = fn; }
extern "C" SDK_EXPORT void __CDECL RakNet_SetCallback_IncomingRPC(FnEvent fn)       { cb_incomingRpc       = fn; }
extern "C" SDK_EXPORT void __CDECL RakNet_SetCallback_IncomingCustomRPC(FnEvent fn) { cb_incomingCustomRpc = fn; }
extern "C" SDK_EXPORT void __CDECL RakNet_SetCallback_OutgoingPacket(FnEvent fn)    { cb_outgoingPacket    = fn; }
extern "C" SDK_EXPORT void __CDECL RakNet_SetCallback_OutgoingRPC(FnEvent fn)       { cb_outgoingRpc       = fn; }
