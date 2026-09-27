# Contributing

Contributions for the WoW 3.3.5a patcher are welcome.

Keep each patch's compatibility data and byte spans in its own definition under `Patching/Patches`; register it in `Patching/PatchCatalog.cs`. Include the supported executable layout and complete original and replacement bytes for every site. Replacement spans must preserve the original lengths. Unknown or partially matching executables must remain unsupported.

Use the .NET 9 SDK on Windows to run the application:

```powershell
dotnet run --project .\Moreno.WowPatcher.csproj
```

Do not commit game executables, generated `bin`/`obj` output, or patch backups.
