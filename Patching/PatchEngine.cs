namespace Moreno.WowPatcher.Patching;

internal sealed class PatchEngine
{
	public PatchResult Inspect(string executablePath, PatchDefinition? patchDefinition)
	{
		if (patchDefinition is null) return new(PatchState.Unsupported, "Select a patch.");
		if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath)) return new(PatchState.Unsupported, "wow.exe not found.");
		try
		{
			return Analyze(File.ReadAllBytes(executablePath), patchDefinition, out _);
		}
		catch (UnauthorizedAccessException)
		{
			return new(PatchState.Unsupported, "Cannot read wow.exe; access denied.");
		}
		catch (IOException)
		{
			return new(PatchState.Unsupported, "Cannot read wow.exe.");
		}
	}

	public PatchResult Apply(string executablePath, PatchDefinition? patchDefinition) => Change(executablePath, patchDefinition, true);
	public PatchResult Restore(string executablePath, PatchDefinition? patchDefinition) => Change(executablePath, patchDefinition, false);

	private static PatchResult Change(string executablePath, PatchDefinition? patchDefinition, bool apply)
	{
		if (patchDefinition is null) return new(PatchState.Unsupported, "Select a patch.");
		if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath)) return new(PatchState.Unsupported, "wow.exe not found.");
		try
		{
			using var stream = new FileStream(executablePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			if (stream.Length > int.MaxValue) return new(PatchState.Unsupported, "Unsupported wow.exe layout.");
			var originalBytes = ReadStreamBytes(stream);
			var before = Analyze(originalBytes, patchDefinition, out var fileOffsets);
			var expected = apply ? PatchState.Ready : PatchState.Applied;
			var target = apply ? PatchState.Applied : PatchState.Ready;
			if (before.State != expected) return before;
			try
			{
				CreateBackup(executablePath, originalBytes, patchDefinition.Id);
			}
			catch (UnauthorizedAccessException)
			{
				return new(before.State, "Cannot create backup; access denied.");
			}
			catch (IOException)
			{
				return new(before.State, "Cannot create backup; wow.exe unchanged.");
			}
			try
			{
				WriteSpans(stream, fileOffsets, patchDefinition, apply);
				stream.Flush(true);
				var after = Analyze(ReadStreamBytes(stream), patchDefinition, out _);
				if (after.State == target)
					return new(target, apply ? "Patch applied; backup saved beside wow.exe." : "Patch restored; backup saved beside wow.exe.");
				WriteSpans(stream, fileOffsets, patchDefinition, !apply);
				stream.Flush(true);
				return new(before.State, "Patch verification failed; original bytes restored.");
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				var failureMessage = ex is UnauthorizedAccessException ? "Update denied." : "Update failed.";
				try
				{
					WriteSpans(stream, fileOffsets, patchDefinition, !apply);
					stream.Flush(true);
					return new(before.State, failureMessage + " Original bytes restored.");
				}
				catch (Exception rollbackException) when (rollbackException is IOException or UnauthorizedAccessException)
				{
					return new(PatchState.Partial, failureMessage + " Rollback failed; backup saved beside wow.exe.");
				}
			}
		}
		catch (UnauthorizedAccessException)
		{
			return new(PatchState.Unsupported, "Cannot update wow.exe; access denied.");
		}
		catch (IOException)
		{
			return new(PatchState.Unsupported, "Cannot update wow.exe; close the game and retry.");
		}
	}

	private static PatchResult Analyze(byte[] executableBytes, PatchDefinition patchDefinition, out int[] fileOffsets)
	{
		fileOffsets = [];
		if (patchDefinition.Spans.Length == 0 ||
			!PeLayout.TryParse(executableBytes, out var pe) ||
			pe is null ||
			pe.Machine != 0x014C ||
			pe.ImageBase != patchDefinition.ImageBase) return new(PatchState.Unsupported, "Unsupported wow.exe layout.");
		fileOffsets = new int[patchDefinition.Spans.Length];
		var originalCount = 0;
		var patchedCount = 0;
		for (var i = 0; i < patchDefinition.Spans.Length; i++)
		{
			var span = patchDefinition.Spans[i];
			if (span.Original.Length != span.Patched.Length) return new(PatchState.Unsupported, "Unsupported wow.exe layout.");
			if (!pe.TryGetFileOffset(span.Rva, span.Original.Length, out fileOffsets[i]))
				return new(PatchState.Unsupported, "Unsupported wow.exe layout.");
			var bytes = executableBytes.AsSpan(fileOffsets[i], span.Original.Length);
			if (bytes.SequenceEqual(span.Original)) originalCount++;
			else if (bytes.SequenceEqual(span.Patched)) patchedCount++;
			else return new(PatchState.Unsupported, "wow.exe does not match the selected patch.");
		}
		if (originalCount == patchDefinition.Spans.Length) return new(PatchState.Ready, "Selected patch is ready to apply.");
		if (patchedCount == patchDefinition.Spans.Length) return new(PatchState.Applied, "Patch is applied; restore is available.");
		return new(PatchState.Partial, "Partial patch detected; no changes made.");
	}

	private static byte[] ReadStreamBytes(FileStream stream)
	{
		stream.Position = 0;
		var bytes = new byte[(int)stream.Length];
		stream.ReadExactly(bytes);
		stream.Position = 0;
		return bytes;
	}

	private static void WriteSpans(FileStream stream, int[] fileOffsets, PatchDefinition patchDefinition, bool patched)
	{
		for (var i = 0; i < patchDefinition.Spans.Length; i++)
		{
			stream.Position = fileOffsets[i];
			stream.Write(patched ? patchDefinition.Spans[i].Patched : patchDefinition.Spans[i].Original);
		}
	}

	private static void CreateBackup(string executablePath, byte[] executableBytes, string patchId)
	{
		var safePatchId = new string(patchId.Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '_').ToArray());
		var backupPath = Path.GetFullPath(executablePath) + ".wowpatch-" + safePatchId + "-" + Guid.NewGuid().ToString("N") + ".bak";
		var created = false;
		try
		{
			using var backup = new FileStream(backupPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
			created = true;
			backup.Write(executableBytes);
			backup.Flush(true);
		}
		catch
		{
			if (created) File.Delete(backupPath);
			throw;
		}
	}
}
