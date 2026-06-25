using Qt.Quick;

namespace PocketknifeDesktop;

public class Program
{
	internal static void Main(string[] args)
	{
		// Use Qt's Fusion style as the base; the QML layer paints Win95-style
		// raised/sunken bevels on top of it for a classic, non-Material look.
		Environment.SetEnvironmentVariable("QT_QUICK_CONTROLS_STYLE", "Fusion");

		Qml.LoadFromRootModule("Main");
		Qml.WaitForExit();
	}
}
