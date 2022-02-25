namespace Quantum;

/// <summary>
/// Most of this stuff is leftover from an old approach for the on-screen detection
/// </summary>
public partial class QuantumCamera : CameraMode {

	private Entity Pawn => Local.Pawn;

	/// <summary> Horizontal FOV in radians. </summary>
	public float hFovRad { get; private set; }
	/// <summary> Vertical FOV in radians. </summary>
	public float vFovRad { get; private set; }

	/// <summary> Horizontal FOV in degrees. </summary>
	public float hFov => hFovRad.RadianToDegree();
	/// <summary> Vertical FOV in degrees. </summary>
	public float vFov => vFovRad.RadianToDegree();

	// setting these since the default properties will just be zero

	/// <summary> NearZ clipping plane distance. </summary>
	public float nearZ { get; private set; }
	/// <summary> NearZ clipping plane distance. </summary>
	public float farZ { get; private set; }

	public override void Update() {
		Viewer = Pawn;
		Position = Pawn.EyePosition;
		Rotation = Pawn.EyeRotation;
	}

	// we need to set these from the cam setup
	public override void Build(ref CameraSetup camSetup) {
		base.Build(ref camSetup);
		vFovRad = camSetup.FieldOfView.DegreeToRadian();
		hFovRad = VerticalToHorizontalFieldOfView(vFovRad);

		nearZ = camSetup.ZNear;
		farZ = camSetup.ZFar;
	}

	public static float VerticalToHorizontalFieldOfView(float vFovRad) {
		return (float)(2.0f * Math.Atan(Math.Tan(vFovRad / 2.0f) / Screen.Aspect));
	}

	public static float HorizontalToVerticalFieldOfView(float hFovRad) {
		return (float)(2.0f * Math.Atan(Math.Tan(hFovRad / 2.0f) * Screen.Aspect));
	}
}
