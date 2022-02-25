namespace Quantum;

[Library("point_quantum")]
[EditorModel("models/quantum_test.vmdl")]
public partial class QuantumPoint : Entity {
	public override void Spawn() {
		base.Spawn();
		// should probably just be serverside and network the transform, but this makes networking easier, since quantum swaps are technically client authorative
		Transmit = TransmitType.Always;
	}

	/// <summary> Name of the corresponding Quantum entity. </summary>
	[Property] public string QuantumEntity { get; set; }
}
