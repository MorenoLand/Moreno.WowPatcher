namespace Moreno.WowPatcher.Patching;

internal sealed record PatchDefinition(string Id, string Name, uint ImageBase, PatchSpan[] Spans);
