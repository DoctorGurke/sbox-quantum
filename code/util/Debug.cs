namespace Quantum;

// debug convars
public static partial class Debug {

	[ConVar.Replicated("debug")]
	public static bool Enabled { get; set; }
}

// Log.Debug
public static class LoggerExtension {
	public static void Debug(this Logger log, object obj) {
		if(!Quantum.Debug.Enabled) return;

		log.Info($"[{(Host.IsClient ? "CL" : "SV")}] {obj}");
	}
}
