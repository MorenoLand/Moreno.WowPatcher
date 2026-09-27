using Moreno.WowPatcher.Patching.Patches;

namespace Moreno.WowPatcher.Patching;

internal static class PatchCatalog
{
	private static readonly PatchDefinition[] Entries = [MouseCameraPatch.Definition];
	public static IReadOnlyList<PatchDefinition> Definitions { get; } = Array.AsReadOnly(Entries);
}
