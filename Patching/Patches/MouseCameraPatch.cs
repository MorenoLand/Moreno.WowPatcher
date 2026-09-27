using Moreno.WowPatcher.Patching;

namespace Moreno.WowPatcher.Patching.Patches;

internal static class MouseCameraPatch
{
	public static PatchDefinition Definition { get; } = new(
		"mouse-camera-fix",
		"Mouse camera fix",
		0x400000,
		[
			new(
				0x469DB1,
				ParseHex("""
					8B EC 83 EC 10 8D 45 F0 50 6A 00 E8 DF 28 00 00
					83 C4 04 50 FF 15 0C F6 9D 00 8B 45 F8 99 2B C2
					8B C8 8B 45 FC 99 2B C2 D1 F8 D1 F9 50 51 89 0D
					EC 13 D4 00 A3 F0 13 D4 00 E8 81 EE FF FF 83 C4 08
					8B E5 5D C3 CC CC CC CC CC CC CC CC CC CC
					"""),
				ParseHex("""
					89 E5 8B 05 FC 13 D4 00 8B 0D F8 13 D4 00 EB C2
					7D 03 83 C1 01 83 C0 32 83 C1 32 3B 0D EC BC CA 00
					7E 03 83 E9 01 3B 05 F0 BC CA 00 7E 03 83 E8 01
					83 E9 32 83 E8 32 89 0D F8 13 D4 00 89 05 FC 13 D4 00
					89 EC 5D E9 B4 F7 FF FF EC 5D C3 C3
					""")),
			new(
				0x469D83,
				ParseHex("CC CC CC CC CC CC CC CC CC CC CC CC CC"),
				ParseHex("83 F8 32 7D 03 83 C0 01 83 F9 32 EB 31")),
			new(
				0x46A62C,
				ParseHex("8B 45 F0 8B 15 EC 13 D4 00 8B 1D F0"),
				ParseHex("E9 71 F0 0B 00 F8 13 D4 00 8B 1D FC")),
			new(
				0x5296A2,
				ParseHex("CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC"),
				ParseHex("""
					8D 4D F0 51 57 FF 15 DC F5 9D 00 8B 45 F0 8B 15
					F8 13 D4 00 E9 7A 0F F4 FF
					""")),
		]);

	private static byte[] ParseHex(string value) => Convert.FromHexString(string.Concat(value.Where(Uri.IsHexDigit)));
}
