using Groups.Standings.Client;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Groups.API
{
	public class GroupsAPI : ModSystem
	{
		ICoreServerAPI sapi;
		private PlayerAPI _player;
		public PlayerAPI Player { get { return _player; } }
		private GroupAPI _group;
		public GroupAPI Group { get { return _group; } }

		private PlayerStandingsIO _PlayerStandingsIO;
		public override void StartServerSide(ICoreServerAPI api)
		{
			sapi = api;
			_PlayerStandingsIO = new PlayerStandingsIO(sapi);
			_player = new PlayerAPI(sapi, _PlayerStandingsIO);
			_group = new GroupAPI(sapi, _PlayerStandingsIO);
			Events.Register(sapi, this);
		}

		public class PlayerAPI
		{
			ICoreServerAPI sapi;
			private PlayerStandingsIO _PlayerStandingsIO;
			internal PlayerAPI(ICoreServerAPI api, PlayerStandingsIO playerStandingsIO)
			{
				this.sapi = api;
				this._PlayerStandingsIO = playerStandingsIO;
			}
			public void IncreaseStanding(IServerPlayer ofPlayer, IServerPlayer forPlayer, sbyte amount) { throw new NotImplementedException(); }
			public void DecreaseStanding(IServerPlayer ofPlayer, IServerPlayer forPlayer, sbyte amount) { throw new NotImplementedException(); }
			public PlayerStandings GetStanding(IServerPlayer ofPlayer, IServerPlayer forPlayer)
			{ return GetStanding(ofPlayer, forPlayer.PlayerUID, out PlayerStandings standings) ? standings : null; }

			public bool GetStanding(IServerPlayer ofPlayer, string forPlayerUID, out PlayerStandings standings)
			{ return GetAllStandings(ofPlayer).TryGetValue(forPlayerUID, out standings); }

			public Dictionary<string, PlayerStandings> GetAllStandings(IServerPlayer player)
			{ return _PlayerStandingsIO.LoadDictionary(player); }

			public Dictionary<string, PlayerStandings> GetRecentChanges(IServerPlayer player)
			{ return _PlayerStandingsIO.LoadChanges(player); }

			public void SetStanding(IServerPlayer ofPlayer, Dictionary<string, sbyte> standings)
			{
				Dictionary<string, PlayerStandings> newStandings = new();
				foreach (KeyValuePair<string, sbyte> kvp in standings)
				{
					PlayerStandings currentStanding = GetStanding(ofPlayer, sapi.World.PlayerByUid(kvp.Key) as IServerPlayer) ?? new();
					currentStanding.Standings = kvp.Value;
					newStandings.Add(kvp.Key, currentStanding);
				}
				_PlayerStandingsIO.SaveChanges(ofPlayer, newStandings);
			}
			public void SetStanding(IServerPlayer ofPlayer, IServerPlayer forPlayer, sbyte value)
			{
				PlayerStandings standings = GetStanding(ofPlayer, forPlayer) ?? new();
				standings.Standings = value;
				_PlayerStandingsIO.SaveChanges(ofPlayer, new Dictionary<string, PlayerStandings>() { { forPlayer.PlayerUID, standings } });
			}
			private void AddPlayer(IServerPlayer ofPlayer, IServerPlayer forPlayer) { if (!GetStanding(ofPlayer, forPlayer.PlayerUID, out PlayerStandings standings)) SetStanding(ofPlayer, forPlayer, 0); }
			public void AddPlayers(IServerPlayer Player1, IServerPlayer Player2) { AddPlayer(Player1, Player2); AddPlayer(Player2, Player1); }
		}

		public class GroupAPI
		{
			ICoreServerAPI sapi;
			private PlayerStandingsIO _PlayerStandingsIO;
			internal GroupAPI(ICoreServerAPI api, PlayerStandingsIO playerStandingsIO)
			{
				this.sapi = api;
				this._PlayerStandingsIO = playerStandingsIO;
			}
			public void CreateGroup() { throw new NotImplementedException(); }
			public void DeleteGroup() { throw new NotImplementedException(); }

			public void LeaveGroup() { throw new NotImplementedException(); }

			public void IncreasePlayerStanding(bool isRelevitve) { throw new NotImplementedException(); }
			public void DecreasePlayerStanding(bool isRelevitve) { throw new NotImplementedException(); }
			public void GetPlayerStanding() { throw new NotImplementedException(); }
			public void SetPlayerStanding() { throw new NotImplementedException(); }

			public void IncreaseGroupStanding(bool isRelevitve) { throw new NotImplementedException(); }
			public void DecreaseGroupStanding(bool isRelevitve) { throw new NotImplementedException(); }
			public void GetGroupStanding() { throw new NotImplementedException(); }
			public void SetGroupStanding() { throw new NotImplementedException(); }

			public void GetPlayerPermisions() { throw new NotImplementedException(); }
			private void SetPlayerPermisions() { throw new NotImplementedException(); }

		}
	}
}
