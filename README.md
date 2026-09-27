# World of Warcraft 3.3.5 Patcher

A Windows utility for applying targeted patches to compatible 32-bit World of Warcraft 3.3.5a client executables.

Choose `wow.exe` and a patch. The patcher checks the executable layout and every patch site before enabling changes, saves a full backup beside the executable, and verifies the resulting bytes.

The first available patch fixes jumpy mouse camera movement. Patch definitions are kept separate from the shared patch engine so additional patches can be registered in the catalog.

## Requirements

- Windows
- .NET 9 SDK

## Run

```powershell
dotnet run --project .\Moreno.WowPatcher.csproj
```

## Compatibility

The current patch definition matches a specific 32-bit WoW 3.3.5a executable layout. Other executables are refused unless their PE layout and patch-site bytes match that definition.
