namespace Quantum;

public partial class QuantumHud : HudEntity<RootPanel> {
	public QuantumHud() {
		if(!IsClient) return;
		RootPanel.SetTemplate("/ui/Hud.html");
	}
}
