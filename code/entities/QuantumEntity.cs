namespace Quantum;

[Library("ent_quantum")]
[EditorModel("models/quantum_test.vmdl")]
public partial class QuantumEntity : ModelEntity {
	public override void Spawn() {
		base.Spawn();

		SetModel("models/quantum_test.vmdl");
		SetupPhysicsFromModel(PhysicsMotionType.Keyframed);
		RenderColor = Color.Blue;

		EnableShadowCasting = false;

		InitQuantumPoints();
	}

	[Net] private IList<QuantumPoint> QuantumPoints { get; set; }

	private void InitQuantumPoints() {
		Host.AssertServer();
		foreach(var point in All.OfType<QuantumPoint>().Where(x => x.QuantumEntity == Name)) {
			QuantumPoints.Add(point);
		}
	}

	/// <summary> Last toscreen result of the center of this object. </summary>
	public Vector3 LastScreenPosition { get; private set; }
	/// <summary> Velocity on screen, based on the difference between the last screen pos and the current. </summary>
	public float ScreenVelocity { get; private set; }

	private static QuantumPlayer Pawn => Local.Pawn as QuantumPlayer;
	private static QuantumCamera Camera => Pawn.Camera;

	private IEnumerable<Vector3> Corners => Model.RenderBounds.Corners;
	private Vector3 Center => Position + Model.RenderBounds.Center * Rotation;

	private bool _lastVisible;
	public bool LastVisible { 
		get { return _lastVisible; }
		private set {
			if(value != _lastVisible) {
				OnVisibilityChanged(value);
			}
			_lastVisible = value;
		}
	}

	/// <summary> Called when LastVisible changed. </summary>
	private void OnVisibilityChanged(bool visible) {
		Host.AssertClient();
		if(!visible) {
			foreach(var qpoint in QuantumPoints.OrderBy(x => Guid.NewGuid())) {
				var qtransform = qpoint.Transform;
				// spot is safe to swap to vis vise
				if(!CheckVisibilityAtTransform(qtransform)) {
					OnClientUnseen(NetworkIdent, qpoint.NetworkIdent);
				}
			}
		}
	}

	/// <summary> Called when we want to initiate a quantum swap. </summary>
	[ServerCmd]
	public static void OnClientUnseen(int id, int pointid) {
		Host.AssertServer();
		var ent = All.OfType<QuantumEntity>().First(x => x.NetworkIdent == id);
		var point = All.OfType<QuantumPoint>().First(x => x.NetworkIdent == pointid);
		if(!ent.IsValid() || !point.IsValid()) return;
		ent.SwapQuantumTransform(point.Transform);
	}

	/// <summary> Apply quantum transforms from a quantum swap. </summary>
	private void SwapQuantumTransform(Transform qtransform) {
		Host.AssertServer();
		Position = qtransform.Position;
		Rotation = qtransform.Rotation;
		ResetInterpolation();
	}

	// Used for noticing moving occluders
	private TimeSince _checked;
	private TimeSince TimeSinceFocus;
	private int _lastOccludedPoints;

	[Event.Frame]
	public void OnRender() {
		if(!SceneObject.IsValid()) return;
		ResetInterpolation();

		UpdateScreenVelocity();

		// keep an eye on it in case it gets occluded without us moving
		if(_checked > 0.1f) {
			_checked = 0;

			var occluded = 0;

			foreach(var corner in Corners) {
				var ctrace = Trace.Ray(Camera.Position, Position + corner * Rotation)
				.WorldAndEntities()
				.Ignore(this)
				.Ignore(Pawn)
				.Run();

				if(!ctrace.EndPosition.IsNearlyEqual(Center)) {
					occluded++;
				}
			}

			var trace = Trace.Ray(Camera.Position, Center)
				.WorldAndEntities()
				.Ignore(this)
				.Ignore(Pawn)
				.Run();

			if(!trace.EndPosition.IsNearlyEqual(Center)) {
				occluded++;
			}

			if(_lastOccludedPoints != occluded) {
				TimeSinceFocus = 0;
				_lastOccludedPoints = occluded;
			}
		}

		// only if we want to observe this entity changing
		if(ObserveChange()) {
			LastVisible = CheckVisibilityAtTransform(Transform);
		}
	}

	/// <summary> True if we care about a change in this object's visibility. </summary>
	private bool ObserveChange() {
		return TimeSinceFocus < 5 || !ScreenVelocity.AlmostEqual(0, 0.001f);
	}

	/// <summary> Check if this object would be visible at a given transform. Used for regular active checks and prospective checks for quantum swaps. </summary>
	private bool CheckVisibilityAtTransform(Transform transform) {
		// can't see if we have our eyes closed
		if(BlinkPanel.EyesClosed) {
			return false;
		}

		// simple bounds check first
		if(BoundsInsideFrustum(transform)) {
			if(!VertexVisible(transform)) {
				return false;
			} else {
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Check if any vertices of the render mesh are visible. This tries to account for solid occlusion.
	/// </summary>
	/// <returns>True if any Vertex is visible by the client, false otherwise.</returns>
	private bool VertexVisible(Transform transform) {
		Host.AssertClient();

		foreach(var vert in Model.GetVertices()) {
			// no need to trace for verts facing away from us
			if(vert.Normal.Dot(Camera.Rotation.Forward) > 0)
				continue;

			var vertpos = transform.Position + vert.Position * transform.Rotation;

			var trace = Trace.Ray(Camera.Position, vertpos)
				.WorldAndEntities()
				.Ignore(Pawn)
				.Ignore(this)
				.Run();

			if(trace.EndPosition.IsNearlyEqual(vertpos, 0.01f)) {
				if(Debug.Enabled) 
					DebugOverlay.Sphere(vertpos, 1, Color.Green, false, Time.Delta);
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Check if any corner of the RenderBounds are in the client's view frustum.
	/// </summary>
	/// <returns>True if any corner of the RenderBounds are currently in the view frustum of the client, false otherwise.</returns>
	private bool BoundsInsideFrustum(Transform transform) {
		Host.AssertClient();
		foreach(var corner in Corners) {
			var point = transform.Position + corner * transform.Rotation;
			if(point.IsInsideFrustum()) {
				return true;
			}
		}
		return false;
	}

	/// <summary> Update ScreenVelocity and LastScreenPosition. </summary>
	private void UpdateScreenVelocity() {
		Host.AssertClient();
		var screenpos = Center.ToScreen();
		ScreenVelocity = (LastScreenPosition - screenpos).Length * 100;
		LastScreenPosition = screenpos;
	}

	[Event.Frame]
	public void DebugRender() {
		//DebugOverlay.Text(Center, $"{LastVisible}");
		if(Debug.Enabled) {
			// render bounds
			foreach(var corner in Corners) {
				foreach(var corner2 in Corners) {
					DebugOverlay.Line(Position + corner * Rotation, Position + corner2 * Rotation, Color.Green, Time.Delta, false);
				}
			}
		}
	}
}
