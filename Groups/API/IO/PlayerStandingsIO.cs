using Groups.GUI.Network;
using System.Collections.Generic;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Groups.API.IO
{
	internal class PlayerStandingsIO
	{
		public ICoreServerAPI sapi;
#pragma warning disable IDE0044 // Add readonly modifier
		private Dictionary<IServerPlayer, Dictionary<string, sbyte?>> Changes;
		private Dictionary<IServerPlayer, Dictionary<string, sbyte?>> Standings;
#pragma warning restore IDE0044 // Add readonly modifier

		public PlayerStandingsIO(ICoreServerAPI sapi)
		{
			this.sapi = sapi;
			Changes = new();
			Standings = new();
		}
		public Dictionary<string, sbyte?> LoadDictionary(IServerPlayer player) { return LoadDictionary(player, out _); }

		public Dictionary<string, sbyte?> LoadDictionary(IServerPlayer player, out bool doesExist)
		{
			sapi.ModLoader.GetModSystem<GroupsAPI>().Logger.Audit($"{player.PlayerName} / {player.PlayerUID} has requested their player standings.");

			if (!Standings.TryGetValue(player, out Dictionary<string, sbyte?> standing))
			{
				byte[] data = CommonIO.ReadPlayerData(sapi, player, "Player");
				standing = data == null ? null : SerializerUtil.Deserialize<Dictionary<string, sbyte?>>(data);
				doesExist = data != null;
				standing ??= new Dictionary<string, sbyte?>()
				{
					{ player.PlayerUID, new sbyte?(0) }
				};
				if (!doesExist) SaveChanges(player, standing, true);
			}
			else { doesExist = true; }

			return standing;
		}

		public Dictionary<string, sbyte?> LoadChanges(IServerPlayer player)
		{
			Changes.Remove(player, out Dictionary<string, sbyte?> standings);
			return standings ?? new();
		}

		/// <summary>
		/// Saves the changes of standings for a give player from a list of players, adds the changes to the changes queue, and updates the dictionary.
		/// </summary>
		/// <param name="sapi"></param>
		/// <param name="player">The player that is prospective of the change.</param>
		/// <param name="standings">A dictionary of the new current standings of a player, not the change of player.</param>
		public void SaveChanges(IServerPlayer player, Dictionary<string, sbyte?> standings, bool blankData = false)
		{
			// Update the dictionary list needed for when player logs in. 
			if (blankData)
			{
				CommonIO.WritePlayerData(sapi, player, "Player", SerializerUtil.Serialize(standings), jData: standings);
			}
			if (standings == null) return;
			Dictionary<string, sbyte?> standingsDirectory = LoadDictionary(player, out bool doesExist);
			if (!doesExist)
			{
				CommonIO.WritePlayerData(sapi, player, "Player", SerializerUtil.Serialize(standings), jData: standings);
				Standings.Add(player, standingsDirectory);
				return;
			}

			foreach (KeyValuePair<string, sbyte?> standing in standings)
			{
				try { standingsDirectory.Remove(standing.Key); }
				catch { }
				standingsDirectory.Add(standing.Key, standing.Value);
			}
			CommonIO.WritePlayerData(sapi, player, "Player", SerializerUtil.Serialize(standings), jData: standings);
			Standings.Remove(player);
			Standings.Add(player, standingsDirectory);


			// Update list of changes, used for fast lookup and retival for the GUI

			bool playerChangesExisted = Changes.TryGetValue(player, out Dictionary<string, sbyte?> oldStandings);

			if (!playerChangesExisted || oldStandings == null)
			{
				Changes.Add(player, standings);
			}
			else if (standings.Count > oldStandings.Count)
			{
				foreach (KeyValuePair<string, sbyte?> standing in oldStandings)
				{
					if (!standings.ContainsKey(standing.Key)) { standings.Add(standing.Key, standing.Value); }
				}
				Changes.Add(player, standings);
			}
			else
			{
				foreach (KeyValuePair<string, sbyte?> standing in standings)
				{
					if (!oldStandings.ContainsKey(standing.Key)) { oldStandings.Add(standing.Key, standing.Value); }
				}
				Changes.Add(player, standings);
			}

			PlayerStandingsNetwork.SendClinetUpdate(sapi, player);

		}
	}
}
