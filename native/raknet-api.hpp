// Mirror of Pawn.RakNet/src/pawnraknet_extension_api.h.
// Keep in sync with the Pawn.RakNet fork; defines the ABI between pawnraknet.dll
// and this component.

#pragma once

#include <component.hpp>
#include <cstdint>
#include <vector>

constexpr UID kPawnRakNetComponentUID = UID(0x4a8b15c16d23e42fULL);
constexpr UID kPawnRakNetExtensionUID = UID(0x4a8b15c16d23e430ULL);

struct IPawnRakNetEventHandler
{
    virtual ~IPawnRakNetEventHandler() = default;
    virtual bool onIncomingPacket(int playerId, int packetId, int bsHandle) { return true; }
    virtual bool onIncomingRPC(int playerId, int rpcId, int bsHandle) { return true; }
    virtual bool onIncomingCustomRPC(int playerId, int rpcId, int bsHandle) { return true; }
    virtual bool onOutgoingPacket(int playerId, int packetId, int bsHandle) { return true; }
    virtual bool onOutgoingRPC(int playerId, int rpcId, int bsHandle) { return true; }
};

struct IPawnRakNetComponent : public IExtension
{
    PROVIDE_EXT_UID(kPawnRakNetExtensionUID)

    virtual int  bitStreamNew() = 0;
    virtual int  bitStreamNewCopy(int handle) = 0;
    virtual void bitStreamDelete(int handle) = 0;

    virtual void bitStreamReset(int handle) = 0;
    virtual void bitStreamResetReadPointer(int handle) = 0;
    virtual void bitStreamResetWritePointer(int handle) = 0;
    virtual void bitStreamIgnoreBits(int handle, int numberOfBits) = 0;

    virtual int  bitStreamGetReadOffset(int handle) = 0;
    virtual void bitStreamSetReadOffset(int handle, int offset) = 0;
    virtual int  bitStreamGetWriteOffset(int handle) = 0;
    virtual void bitStreamSetWriteOffset(int handle, int offset) = 0;

    virtual int  bitStreamNumberOfBitsUsed(int handle) = 0;
    virtual int  bitStreamNumberOfBytesUsed(int handle) = 0;
    virtual int  bitStreamNumberOfUnreadBits(int handle) = 0;
    virtual int  bitStreamNumberOfBitsAllocated(int handle) = 0;

    virtual void bitStreamWriteInt8(int handle, int8_t value, bool compressed) = 0;
    virtual void bitStreamWriteInt16(int handle, int16_t value, bool compressed) = 0;
    virtual void bitStreamWriteInt32(int handle, int32_t value, bool compressed) = 0;
    virtual void bitStreamWriteUint8(int handle, uint8_t value, bool compressed) = 0;
    virtual void bitStreamWriteUint16(int handle, uint16_t value, bool compressed) = 0;
    virtual void bitStreamWriteUint32(int handle, uint32_t value, bool compressed) = 0;
    virtual void bitStreamWriteFloat(int handle, float value, bool compressed) = 0;
    virtual void bitStreamWriteBool(int handle, bool value, bool compressed) = 0;

    virtual void bitStreamWriteString(int handle, const char* data, int length) = 0;
    virtual void bitStreamWriteStringCompressed(int handle, const char* data, int length) = 0;
    virtual void bitStreamWriteString8(int handle, const char* data, int length) = 0;
    virtual void bitStreamWriteString32(int handle, const char* data, int length) = 0;

    virtual void bitStreamWriteBits(int handle, const uint8_t* data, int numberOfBits, bool rightAlignedBits) = 0;

    virtual void bitStreamWriteFloat3(int handle, float x, float y, float z) = 0;
    virtual void bitStreamWriteFloat4(int handle, float x, float y, float z, float w) = 0;
    virtual void bitStreamWriteVector(int handle, float x, float y, float z) = 0;
    virtual void bitStreamWriteNormQuat(int handle, float x, float y, float z, float w) = 0;

    virtual bool bitStreamReadInt8(int handle, int8_t& outValue, bool compressed) = 0;
    virtual bool bitStreamReadInt16(int handle, int16_t& outValue, bool compressed) = 0;
    virtual bool bitStreamReadInt32(int handle, int32_t& outValue, bool compressed) = 0;
    virtual bool bitStreamReadUint8(int handle, uint8_t& outValue, bool compressed) = 0;
    virtual bool bitStreamReadUint16(int handle, uint16_t& outValue, bool compressed) = 0;
    virtual bool bitStreamReadUint32(int handle, uint32_t& outValue, bool compressed) = 0;
    virtual bool bitStreamReadFloat(int handle, float& outValue, bool compressed) = 0;
    virtual bool bitStreamReadBool(int handle, bool& outValue, bool compressed) = 0;

    virtual bool bitStreamReadString(int handle, char* outBuffer, int maxLength) = 0;
    virtual bool bitStreamReadStringCompressed(int handle, char* outBuffer, int maxLength) = 0;
    virtual int  bitStreamReadString8(int handle, char* outBuffer, int maxLength) = 0;
    virtual int  bitStreamReadString32(int handle, char* outBuffer, int maxLength) = 0;

    virtual bool bitStreamReadBits(int handle, uint8_t* outData, int numberOfBits, bool rightAlignedBits) = 0;

    virtual bool bitStreamReadFloat3(int handle, float& x, float& y, float& z) = 0;
    virtual bool bitStreamReadFloat4(int handle, float& x, float& y, float& z, float& w) = 0;
    virtual bool bitStreamReadVector(int handle, float& x, float& y, float& z) = 0;
    virtual bool bitStreamReadNormQuat(int handle, float& x, float& y, float& z, float& w) = 0;

    virtual bool sendPacket(int handle, int playerId, int priority, int reliability, uint8_t orderingChannel) = 0;
    virtual bool sendRPC(int handle, int playerId, int rpcId, int priority, int reliability, uint8_t orderingChannel) = 0;
    virtual bool emulateIncomingPacket(int handle, int playerId) = 0;
    virtual bool emulateIncomingRPC(int handle, int playerId, int rpcId) = 0;

    virtual void setCustomRPC(int rpcId) = 0;
    virtual bool isCustomRPC(int rpcId) = 0;

    virtual void addEventHandler(IPawnRakNetEventHandler* handler) = 0;
    virtual void removeEventHandler(IPawnRakNetEventHandler* handler) = 0;

    void reset() override {}
};
