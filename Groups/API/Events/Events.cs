using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Server;

namespace Groups.API
{
	internal class Events
	{
		public static void Register(ICoreServerAPI sapi, GroupsAPI groupsAPI)
		{
			sapi.Event.PlayerJoin += (player) =>
			{
				sapi.Event.RegisterGameTickListener((args) =>
				{
					Dictionary<string, sbyte?>.KeyCollection MeetPlayers = groupsAPI.Player.GetAllStandings(player).Keys;

					foreach (IServerPlayer newPlayer in sapi.World.GetPlayersAround(player.Entity.Pos.XYZ, 32f, 32f, _player => !MeetPlayers.Contains(_player.PlayerUID)))
					{
						groupsAPI.Player.AddPlayers(player, newPlayer);
					}
				}, 30000);
			};
		}
	}
}
