using Groups.Standings.Client;
using Groups.Standings.Network;
using System.Collections.Generic;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Groups.API
{
	internal class PlayerStandingsIO
	{
		public ICoreServerAPI sapi;
		private const string ModDataKey = "PlayersStandingsData";
		private Dictionary<IServerPlayer, Dictionary<string, PlayerStandings>> Changes;
		private Dictionary<IServerPlayer, Dictionary<string, PlayerStandings>> Standings;

		public PlayerStandingsIO(ICoreServerAPI sapi)
		{
			this.sapi = sapi;
			Changes = new();
			Standings = new();
		}
		public Dictionary<string, PlayerStandings> LoadDictionary(IServerPlayer player) { return LoadDictionary(player, out bool output); }

		public Dictionary<string, PlayerStandings> LoadDictionary(IServerPlayer player, out bool doesExist)
		{
			sapi.Logger.Audit($"{player.PlayerName} / {player.PlayerUID} has requested their player standings.");

			if (!Standings.TryGetValue(player, out Dictionary<string, PlayerStandings> standing))
			{
				byte[] data = player.GetModdata(ModDataKey);
				standing = data == null ? null : SerializerUtil.Deserialize<Dictionary<string, PlayerStandings>>(data);
				doesExist = data != null;
				standing ??= new Dictionary<string, PlayerStandings>()
				{
					{ player.PlayerUID, new PlayerStandings(0) }
				};
				if (!doesExist) SaveChanges(player, standing, true);
			}
			else { doesExist = true; }

			return standing;
		}

		public Dictionary<string, PlayerStandings> LoadChanges(IServerPlayer player)
		{
			Changes.Remove(player, out Dictionary<string, PlayerStandings> standings);
			return standings ?? new();
		}

		/// <summary>
		/// Saves the changes of standings for a give player from a list of players, adds the changes to the changes queue, and updates the dictionary.
		/// </summary>
		/// <param name="sapi"></param>
		/// <param name="player">The player that is prospective of the change.</param>
		/// <param name="standings">A dictionary of the new current standings of a player, not the change of player.</param>
		public void SaveChanges(IServerPlayer player, Dictionary<string, PlayerStandings> standings, bool blankData = false)
		{
			// Update the dictionary list needed for when player logs in. 
			if (blankData)
			{
				player.SetModdata(ModDataKey, SerializerUtil.Serialize(standings));
			}
			if (standings == null) return;
			Dictionary<string, PlayerStandings> standingsDirectory = LoadDictionary(player, out bool doesExist);
			if (!doesExist)
			{
				player.SetModdata(ModDataKey, SerializerUtil.Serialize(standings));
				Standings.Add(player, standingsDirectory);
				return;
			}

			foreach (KeyValuePair<string, PlayerStandings> standing in standings)
			{
				try { standingsDirectory.Remove(standing.Key); }
				catch { }
				standingsDirectory.Add(standing.Key, standing.Value);
			}
			player.SetModdata(ModDataKey, SerializerUtil.Serialize(standingsDirectory));
			Standings.Remove(player);
			Standings.Add(player, standingsDirectory);


			// Update list of changes, used for fast lookup and retival for the GUI

			bool playerChangesExisted = Changes.TryGetValue(player, out Dictionary<string, PlayerStandings> oldStandings);

			if (!playerChangesExisted || oldStandings == null)
			{
				Changes.Add(player, standings);
			}
			else if (standings.Count > oldStandings.Count)
			{
				foreach (KeyValuePair<string, PlayerStandings> standing in oldStandings)
				{
					if (!standings.ContainsKey(standing.Key)) { standings.Add(standing.Key, standing.Value); }
				}
				Changes.Add(player, standings);
			}
			else
			{
				foreach (KeyValuePair<string, PlayerStandings> standing in standings)
				{
					if (!oldStandings.ContainsKey(standing.Key)) { oldStandings.Add(standing.Key, standing.Value); }
				}
				Changes.Add(player, standings);
			}

			PlayerStandingsNetwork.SendClinetUpdate(sapi, player);

		}
	}
}
