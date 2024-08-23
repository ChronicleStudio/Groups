using Groups.Standings.Network;
using ProtoBuf;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace Groups.Standings.Client
{
	[ProtoContract]
	public class PlayerStandings
	{
		[ProtoMember(1)]

		public sbyte Standings { get; set; }

		public PlayerStandings(sbyte Standings)
		{
			this.Standings = Standings;
		}
		public PlayerStandings()
		{
		}

		public static List<KeyValuePair<IPlayer, PlayerStandings>> GetAllPlayersStandings(ICoreClientAPI capi)
		{
			List<KeyValuePair<IPlayer, PlayerStandings>> standings = new();

			List<IPlayer> players = capi.World.AllOnlinePlayers.ToList();
			Dictionary<string, PlayerStandings> standingsDictionary = capi.ModLoader.GetModSystem<PlayerStandingsNetwork>().GetStandings();

			foreach (IPlayer player in players)
			{


				PlayerStandings playerStandings = standingsDictionary.Get(player.PlayerUID);
				if (playerStandings != null)
				{
					standings.Add(new KeyValuePair<IPlayer, PlayerStandings>(player, playerStandings));
				}
			}

			return standings;
		}
	}
}
