namespace Quantum;

partial class QuantumPlayer : Sandbox.Player {
	public QuantumCamera Camera => CameraMode as QuantumCamera;

	public override void Respawn() {
		SetModel("models/citizen/citizen.vmdl");

		Controller = new WalkController();
		Animator = new StandardPlayerAnimator();
		CameraMode = new QuantumCamera();

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		base.Respawn();
	}
}
