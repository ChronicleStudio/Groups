using Groups.API.Exceptions;
using Groups.API.Group;
using Groups.API.IO;
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
			public void IncreaseStanding(string ofPlayerUID, Queue<KeyValuePair<string, sbyte>> changes) { throw new NotImplementedException(); }
			public void DecreaseStanding(IServerPlayer ofPlayer, IServerPlayer forPlayer, sbyte amount) { throw new NotImplementedException(); }
			public void DecreaseStanding(string ofPlayerUID, Queue<KeyValuePair<string, sbyte>> changes) { throw new NotImplementedException(); }
			public sbyte? GetStanding(IServerPlayer ofPlayer, IServerPlayer forPlayer)
			{ return GetStanding(ofPlayer, forPlayer.PlayerUID, out sbyte? standings) ? standings : null; }

			public bool GetStanding(IServerPlayer ofPlayer, string forPlayerUID, out sbyte? standings)
			{ return GetAllStandings(ofPlayer).TryGetValue(forPlayerUID, out standings); }

			public Dictionary<string, sbyte?> GetAllStandings(IServerPlayer player)
			{ return _PlayerStandingsIO.LoadDictionary(player); }

			public Dictionary<string, sbyte?> GetRecentChanges(IServerPlayer player)
			{ return _PlayerStandingsIO.LoadChanges(player); }

			public void SetStanding(IServerPlayer ofPlayer, Dictionary<string, sbyte?> standings)
			{
				_PlayerStandingsIO.SaveChanges(ofPlayer, standings);
			}
			public void SetStanding(IServerPlayer ofPlayer, IServerPlayer forPlayer, sbyte? value)
			{
				_PlayerStandingsIO.SaveChanges(ofPlayer, new Dictionary<string, sbyte?>() { { forPlayer.PlayerUID, value } });
			}
			private void AddPlayer(IServerPlayer ofPlayer, IServerPlayer forPlayer) { if (!GetStanding(ofPlayer, forPlayer.PlayerUID, out sbyte? standings)) SetStanding(ofPlayer, forPlayer, 0); }
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

			public string[] GetGroupNames() { throw new NotImplementedException(); }
			public void JoinGroup() { throw new NotImplementedException(); }
			public void LeaveGroup() { throw new NotImplementedException(); }

			public void IncreasePlayerStanding(string GroupName, string PlayerUID, sbyte amount, bool isRelevitve = true) { throw new NotImplementedException(); }
			public void DecreasePlayerStanding(string GroupName, string PlayerUID, sbyte amount, bool isRelevitve = true) { throw new NotImplementedException(); }
			public sbyte GetPlayerStanding(string GroupName, string PlayerUID) { throw new NotImplementedException(); }
			public void SetPlayerStanding(string GroupName, string PlayerUID, sbyte amount) { throw new NotImplementedException(); }

			public void IncreaseGroupStanding(string ofGroupName, string forGroupName, sbyte amount, bool isRelevitve = true) { throw new NotImplementedException(); }
			public void DecreaseGroupStanding(string ofGroupName, string forGroupName, sbyte amount, bool isRelevitve = true) { throw new NotImplementedException(); }
			public sbyte GetGroupStanding(string ofGroupName, string forGroupName) { throw new NotImplementedException(); }
			public void SetGroupStanding(string ofGroupName, string forGroupName, sbyte amount) { throw new NotImplementedException(); }

			/// <summary>
			/// Gets the value of a Registered property, this allows other mods to get the value set in the GUI.
			/// </summary>
			/// <param name="PropertyUID">The property UID that was returned when the property was registered.</param>
			/// <param name="GroupName">The name of the group that the property value needs to be checked.</param>
			/// <returns>The value of the property set for the given group.</returns>
			/// <exception cref="InvalidGroupName"></exception>
			/// <exception cref="InvalidPropertyUID"></exception>
			/// <exception cref="ArgumentNullException">Thrown if the <paramref name="GroupName"/> or <paramref name="PropertyUID"/> paramaeter is null or empty.</exception>
			public string GetPropertyValue(string PropertyUID, string GroupName)
			{
				if (string.IsNullOrEmpty(PropertyUID))
				{
					throw new ArgumentNullException(nameof(PropertyUID), $"'{nameof(PropertyUID)}' cannot be null or empty.");
				}
				if (string.IsNullOrEmpty(GroupName))
				{
					throw new ArgumentNullException(nameof(GroupName), $"'{nameof(GroupName)}' cannot be null or empty.");
				}
				try { return GetGroupProperties(GroupName).Properties.Find(property => property.UID.Equals(PropertyUID))?.Value ?? throw new InvalidPropertyUID(); } catch { throw; }
			}
			/// <summary>
			///
			/// </summary>
			/// <param name="GroupName"></param>
			/// <exception cref="InvalidGroupName"></exception>
			/// <exception cref="ArgumentNullException">Thrown if the <paramref name="GroupName"/> paramaeter is null or empty.</exception>
			/// <exception cref="Exception"></exception>
#nullable enable
			public GroupCore? GetGroupProperties(string GroupName)
			{
				if (string.IsNullOrEmpty(GroupName))
				{
					throw new ArgumentNullException(nameof(GroupName), $"'{nameof(GroupName)}' cannot be null or empty.");
				}
				throw new NotImplementedException();
			}

			/// <summary>
			/// Registers a property for use in the Groups GUI, this allows other mods to hook into the gui to allow groups to conrol settings relevent to their own mod.
			/// </summary>
			/// <param name="property"> A instance of IGroupPrperty, with all of the neccasary information for registering the property.</param>
			/// <returns>Returns the Property UID, this value is not static and can change on load, this UID would be used to check the value of the property.</returns>
			public string RegisterProperty(IGroupProperty property) { throw new NotImplementedException(); }

			/// <summary>
			///
			/// </summary>
			/// <param name="GroupName"></param>
			/// <exception cref="InvalidPlayerUID"></exception>
			/// <exception cref="ArgumentNullException">Thrown if the <paramref name="PlayerUID"/> paramaeter is null or empty.</exception>
			/// <exception cref="Exception"></exception>
			public string GetPlayerGroup(string PlayerUID)
			{
				if (string.IsNullOrEmpty(PlayerUID))
				{
					throw new ArgumentNullException(nameof(PlayerUID), $"'{nameof(PlayerUID)}' cannot be null or empty.");
				}
				throw new NotImplementedException();
			}
			/// <summary>
			/// Gets the group rank of a player for the group that they are in or another group.
			/// </summary>
			/// <param name="PlayerUID">The Player's UID for use in checking their group, and the value of the player within the group specified.</param>
			/// <param name="GroupName">If included, will check the player;'s rank in relation to the group specified, if blank or null, will check the players group standings.</param>
			/// <returns>The players Group Rank within the group specified</returns>
			/// <exception cref="InvalidGroupName"></exception>
			/// <exception cref="ArgumentNullException">Thrown if the <paramref name="PlayerUID"/> paramaeter is null or empty.</exception>
			/// <exception cref="Exception"></exception>
			public GroupRank GetPlayerRank(string PlayerUID, string? GroupName = null)
			{
				if (string.IsNullOrEmpty(PlayerUID))
				{
					throw new ArgumentNullException(nameof(PlayerUID), $"'{nameof(PlayerUID)}' cannot be null or empty.");
				}
				return GroupCore.GetRank(GetPlayerStanding(GroupName ?? (GetPlayerGroup(PlayerUID)), PlayerUID));
			}
			/// <summary>
			/// Get's the title of the player for the specified group, if GroupName is ommited, whill default to the player's group.
			/// </summary>
			/// <param name="PlayerUID">The Player's UID for check the standings level of the player.</param>
			/// <param name="GroupName">The Name of the group to check, if omitted or null, will check agiainst the player's group.</param>
			/// <returns>The Title of the player as a string, releative to the sp</returns>
			/// <exception cref="InvalidGroupName"></exception>
			/// <exception cref="ArgumentNullException">Thrown if the <paramref name="PlayerUID"/> paramaeter is null or empty.</exception>
			/// <exception cref="ArgumentException"></exception>
			/// <exception cref="Exception"></exception>
			public string GetPlayerTitle(string PlayerUID, string? GroupName = null)
			{
				if (string.IsNullOrEmpty(PlayerUID))
				{
					throw new ArgumentNullException(nameof(PlayerUID), $"'{nameof(PlayerUID)}' cannot be null or empty.");
				}
				try { return GetGroupProperties(GroupName ?? GetPlayerGroup(PlayerUID))?.GetRankTitle(GetPlayerRank(PlayerUID, GroupName)) ?? throw new ArgumentException(); } catch { throw; }
			}
			/// <summary>
			/// Gets the array of Titles for the given levels of a group, this is a list of all titles for the group. This is usefull for check if registered parameter matches a set title. 
			/// </summary>
			/// <param name="GroupName">The name of the group to check the titles, cannot be null or empty and will throw error.</param>
			/// <returns>Returns the array of strings for a given group. You can use the following method to get what rank level the title matches: <seealso cref="GetRankFromTitle(string, string)"/></returns>
			/// <exception cref="InvalidGroupName"></exception>
			/// <exception cref="ArgumentNullException">Thrown if the <paramref name="GroupName"/> paramaeter is null or empty.</exception>
			/// <exception cref="ArgumentException"></exception>
			/// <exception cref="Exception">API Methods do not catch exceptions, this should be handled by the caller.</exception>
			public string[] GetGroupTitles(string GroupName)
			{
				if (string.IsNullOrEmpty(GroupName))
				{
					throw new ArgumentNullException(nameof(GroupName), $"'{nameof(GroupName)}' cannot be null or empty.");
				}
				try { return GetGroupProperties(GroupName)?.GroupTitles ?? throw new ArgumentException(); } catch { throw; }
			}
			/// <summary>
			/// Gets the 
			/// </summary>
			/// <param name="GroupName"></param>
			/// <param name="Title"></param>
			/// <returns></returns>
			/// <exception cref="InvalidGroupName"></exception>
			/// <exception cref="ArgumentNullException">Thrown if the <paramref name="GroupName"/> or <paramref name="Title"/> paramaeter is null or empty.</exception>
			/// <exception cref="ArgumentException"></exception>
			/// <exception cref="Exception"></exception>
			public GroupRank GetRankFromTitle(string GroupName, string Title)
			{
				if (string.IsNullOrEmpty(GroupName))
				{
					throw new ArgumentNullException(nameof(GroupName), $"'{nameof(GroupName)}' cannot be null or empty.");
				}

				if (string.IsNullOrEmpty(Title))
				{
					throw new ArgumentNullException(nameof(Title), $"'{nameof(Title)}' cannot be null or empty.");
				}

				try { return GetGroupProperties(GroupName)?.GetRankFromTitle(Title) ?? throw new ArgumentException(); } catch { throw; }
			}

		}
	}
}
