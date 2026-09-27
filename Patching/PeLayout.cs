using System.Buffers.Binary;

namespace Moreno.WowPatcher.Patching;

internal sealed class PeLayout
{
	private readonly Section[] _sections;
	private readonly int _fileLength;
	public ushort Machine { get; }
	public uint ImageBase { get; }

	private PeLayout(ushort machine, uint imageBase, Section[] sections, int fileLength)
	{
		Machine = machine;
		ImageBase = imageBase;
		_sections = sections;
		_fileLength = fileLength;
	}

	public static bool TryParse(ReadOnlySpan<byte> file, out PeLayout? layout)
	{
		layout = null;
		if (file.Length < 0x40 || BinaryPrimitives.ReadUInt16LittleEndian(file) != 0x5A4D) return false;
		var peOffset = BinaryPrimitives.ReadInt32LittleEndian(file[0x3C..]);
		if (peOffset < 0 ||
			(long)peOffset + 24 > file.Length ||
			BinaryPrimitives.ReadUInt32LittleEndian(file[peOffset..]) != 0x00004550) return false;
		var machine = BinaryPrimitives.ReadUInt16LittleEndian(file[(peOffset + 4)..]);
		var sectionCount = BinaryPrimitives.ReadUInt16LittleEndian(file[(peOffset + 6)..]);
		var optionalSize = BinaryPrimitives.ReadUInt16LittleEndian(file[(peOffset + 20)..]);
		var optionalOffset = peOffset + 24;
		if (sectionCount == 0 ||
			sectionCount > 96 ||
			optionalSize < 64 ||
			(long)optionalOffset + optionalSize > file.Length ||
			BinaryPrimitives.ReadUInt16LittleEndian(file[optionalOffset..]) != 0x10B) return false;
		var imageBase = BinaryPrimitives.ReadUInt32LittleEndian(file[(optionalOffset + 28)..]);
		var sectionOffset = optionalOffset + optionalSize;
		if ((long)sectionOffset + (long)sectionCount * 40 > file.Length) return false;
		var sections = new Section[sectionCount];
		for (var i = 0; i < sections.Length; i++)
		{
			var header = file.Slice(sectionOffset + i * 40, 40);
			sections[i] = new(
				BinaryPrimitives.ReadUInt32LittleEndian(header[8..]),
				BinaryPrimitives.ReadUInt32LittleEndian(header[12..]),
				BinaryPrimitives.ReadUInt32LittleEndian(header[16..]),
				BinaryPrimitives.ReadUInt32LittleEndian(header[20..]));
		}
		layout = new(machine, imageBase, sections, file.Length);
		return true;
	}

	public bool TryGetFileOffset(uint rva, int length, out int offset)
	{
		offset = 0;
		if (length <= 0) return false;
		var match = -1;
		foreach (var section in _sections)
		{
			if (rva < section.VirtualAddress) continue;
			var delta = (ulong)rva - section.VirtualAddress;
			var extent = Math.Max(section.VirtualSize, section.RawSize);
			if (delta + (uint)length > extent || delta + (uint)length > section.RawSize) continue;
			var rawOffset = (ulong)section.RawOffset + delta;
			if (rawOffset + (uint)length > (ulong)_fileLength || rawOffset > int.MaxValue || match >= 0) return false;
			match = (int)rawOffset;
		}
		if (match < 0) return false;
		offset = match;
		return true;
	}

	private sealed record Section(uint VirtualSize, uint VirtualAddress, uint RawSize, uint RawOffset);
}
