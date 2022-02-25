namespace Quantum;

public static partial class Frustum {

	private static QuantumPlayer Pawn => Local.Pawn as QuantumPlayer;
	private static QuantumCamera Camera => Pawn.Camera;

	/// <summary>
	/// Method to check whether or not a point is inside the current view frustum. Accounts for Farz.
	/// </summary>
	/// <param name="point">The point to check.</param>
	/// <returns>True if the point is within the view frustum, false otherwise.</returns>
	public static bool IsInsideFrustum(this Vector3 point) {
		Host.AssertClient();
		// beyond far z
		if(point.Distance(Camera.Position) > Camera.farZ) {
			return false;
		}

		// projected point not on screen
		var screen = point.ToScreen();
		if(screen.x < 0.0f || screen.y < 0.0f || screen.x > 1.0f || screen.y > 1.0f) {
			return false;
		}

		return true;
	}
}
