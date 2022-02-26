global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using Hammer;

global using System;
global using System.Collections.Generic;
global using System.Linq;

namespace Quantum;

public partial class QuantumGame : Game {

	public QuantumGame() {
		if(IsServer)
			_ = new QuantumHud();
	}

	public override void ClientJoined(Client client) {
		base.ClientJoined(client);

		var player = new QuantumPlayer();
		client.Pawn = player;

		player.Respawn();
	}
}
