using Moreno.WowPatcher.UI;

namespace Moreno.WowPatcher;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		ApplicationConfiguration.Initialize();
		Application.Run(new MainForm());
	}
}
