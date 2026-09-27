namespace Moreno.WowPatcher.Patching;

internal sealed record ExecutableInspection(bool IsPe, bool IsX86, long Length, string Message);
