namespace Moreno.WowPatcher.Patching;

internal sealed record PatchSpan(uint Rva, byte[] Original, byte[] Patched);
