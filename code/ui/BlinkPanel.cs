namespace Quantum;

[UseTemplate]
public partial class BlinkPanel : Panel {
	private Panel BlinkLayer { get; set; }

	public static BlinkPanel Current;

	public static bool EyesClosed => IsOpaque();
	private TimeSince TimeSinceBlink;

	private static bool IsOpaque() {
		if(Current is null || Current.BlinkLayer is null || Current.BlinkLayer.ComputedStyle is null || Current.BlinkLayer.ComputedStyle.Opacity is null) return false;
		return Current.BlinkLayer.ComputedStyle.Opacity.Value.AlmostEqual(1, 0.05f);
	}

	public BlinkPanel() {
		Current = this;
	}

	public override void Tick() {
		BlinkLayer.SetClass("active", TimeSinceBlink < 0.2f);
	}

	public static void Blink() {
		Current.TimeSinceBlink = 0;
	}

	[Event.BuildInput]
	public void BuildInput(InputBuilder input) {
		if(input.Down(InputButton.Walk)) {
			TimeSinceBlink = 0;
		}
	}
}
