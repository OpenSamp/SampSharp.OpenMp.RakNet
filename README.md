# SampSharp.OpenMp.RakNet

Managed C# bindings for [Pawn.RakNet](https://github.com/katursis/Pawn.RakNet) on open.mp
x64, for gamemodes running on the SampSharp open.mp host. Gives C# code raw access to the
SA-MP network layer: intercept, rewrite, block and synthesise packets and RPCs before the
server or the client sees them.

## Architecture

open.mp loads three independent components; this repository provides the middle one plus
the C# bindings on top of it.

```
┌──────────────────────────────────────────────────────────────────────┐
│  C# gamemode                                                         │
│     IRakNetService (low level)  ·  Pawn.IRakNet (legacy facade)      │
└──────────────────────────────────────────────────────────────────────┘
                               │   P/Invoke
                               ▼
┌──────────────────────────────────────────────────────────────────────┐
│  SampSharp.RakNet.dll  (this repo, native/)                          │
│     pure C-API shim: C exports + function-pointer callbacks          │
│     queryComponent<IPawnRakNetComponent>() at onInit                 │
└──────────────────────────────────────────────────────────────────────┘
                               │   direct C++ virtual calls
                               ▼
┌──────────────────────────────────────────────────────────────────────┐
│  pawnraknet.dll  (provides IPawnRakNetComponent)                     │
└──────────────────────────────────────────────────────────────────────┘
```

The shim has **no link-time dependency on `SampSharp.dll`**. It is loaded by open.mp on its
own and only talks to `pawnraknet.dll`; the managed side reaches it by P/Invoke.

## Runtime dependencies

| Component              | Where from                                                  |
|------------------------|-------------------------------------------------------------|
| `pawnraknet.dll`/`.so` | Pawn.RakNet — provides `IPawnRakNetComponent`                |
| `SampSharp.RakNet.dll` | Built from `native/` in this repository                      |
| `SampSharp.dll`        | `SampSharp/src/sampsharp-component/`                         |
| .NET 10 runtime        | System-wide                                                  |

All three DLLs go in the server's `components/` directory. `pawnraknet.cfg` sits next to
them and controls which RPCs the plugin hooks.

If `pawnraknet.dll` is not loaded, every call becomes a no-op returning `false` and
`IsAvailable` reports `false`.

## Wiring

```csharp
// ConfigureServices — before AddSystemsInAssembly
services.AddRakNet();

// ECS builder
builder.EnableRakNetEvents();
```

`AddRakNet` registers the whole stack: `IRakNetService`, the event system that attaches the
native callbacks, and the `Pawn.IRakNet` facade.

## Two APIs

**`IRakNetService`** is the thin one. `BitStream` plus `SendPacket` / `SendRpc` /
`EmulateIncomingPacket` / `EmulateIncomingRpc`, with `playerId = -1` meaning broadcast, and
`SetCustomRpc` to route an id through `OnIncomingCustomRPC` instead of `OnIncomingRPC`.

**`Pawn.IRakNet`** is a facade shaped like the original SA-MP Pawn plugin, for porting
existing code: .NET events (`IncomingRpc`, `OutgoingPacket`, …), `BlockRpc` / `BlockPacket`,
and a variadic BitStream API. Same underlying bridge — pick whichever reads better.

## Events

Handlers are ordinary ECS `[Event]` methods:

`OnIncomingPacket` · `OnIncomingRPC` · `OnIncomingCustomRPC` · `OnOutgoingPacket` ·
`OnOutgoingRPC`

Returning `false` from a handler vetoes the packet — it is dropped before reaching its
destination. Any other return value (including no return) lets it through.

## Building

Two artifacts, built separately:

```bash
# managed bindings
dotnet build SampSharp.OpenMp.RakNet.csproj

# native shim
cmake -B build -S . -DCMAKE_BUILD_TYPE=Release
cmake --build build --config Release
```

Both need the SampSharp repository checked out alongside this one — the csproj references
`SampSharp.OpenMp.Core` and `SampSharp.OpenMp.Entities` by relative path, and CMake takes
the open.mp SDK from `SampSharp/external/sdk`. Either nesting works: SampSharp as a direct
sibling, or one level up (`src/SampSharp` with this repo under `src/submodules/`). Override
with `-DOMP_SDK_DIR=<path>` if your layout differs.

## License

Apache-2.0.

---

Powered by [vs-rp.org](https://vs-rp.org)
