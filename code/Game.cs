global using Sandbox;
global using Hammer;

global using System;
global using System.Collections.Generic;
global using System.Linq;

namespace Quantum;

public partial class QuantumGame : Game {
	public override void ClientJoined(Client client) {
		base.ClientJoined(client);

		var player = new QuantumPlayer();
		client.Pawn = player;

		player.Respawn();
	}
}
