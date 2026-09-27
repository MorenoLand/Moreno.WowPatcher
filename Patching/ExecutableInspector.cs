namespace Moreno.WowPatcher.Patching;

internal static class ExecutableInspector
{
	public static ExecutableInspection Inspect(string path)
	{
		try
		{
			using var stream = File.OpenRead(path);
			using var reader = new BinaryReader(stream);
			if (stream.Length < 0x40 || reader.ReadUInt16() != 0x5A4D) return new(false, false, stream.Length, "Not a Windows executable.");
			stream.Position = 0x3C;
			var peOffset = reader.ReadInt32();
			if (peOffset < 0 || peOffset > stream.Length - 24) return new(false, false, stream.Length, "Invalid PE header.");
			stream.Position = peOffset;
			if (reader.ReadUInt32() != 0x00004550) return new(false, false, stream.Length, "Invalid PE signature.");
			var machine = reader.ReadUInt16();
			return new(true, machine == 0x014C, stream.Length, machine == 0x014C ? "32-bit PE loaded." : "PE loaded, but it is not 32-bit.");
		}
		catch (IOException ex)
		{
			return new(false, false, 0, ex.Message);
		}
		catch (UnauthorizedAccessException ex)
		{
			return new(false, false, 0, ex.Message);
		}
	}
}
