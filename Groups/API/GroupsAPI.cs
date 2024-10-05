using Groups.API.Exceptions;
using Groups.API.Group;
using Groups.API.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

#pragma warning disable IDE0044 // Add readonly modifier
namespace Groups.API
{
	public class GroupsAPI : ModSystem
	{
		ICoreServerAPI sapi;
		private PlayerAPI _player;
		public PlayerAPI Player { get { return _player; } }
		private GroupAPI _group;
		public GroupAPI Group { get { return _group; } }


		public ILogger Logger;

		public override void StartServerSide(ICoreServerAPI api)
		{
			sapi = api;
			Logger = new GroupsLogger(true);
			_player = new PlayerAPI(sapi);
			_group = new GroupAPI(this, sapi);
			api.Event.SaveGameLoaded += _group.OnSaveGameLoaded;
			Events.Register(sapi, this);

		}

		public class PlayerAPI
		{
			ICoreServerAPI sapi;
			private PlayerStandingsIO _PlayerStandingsIO;
			internal PlayerAPI(ICoreServerAPI api)
			{
				this.sapi = api;
				_PlayerStandingsIO = new PlayerStandingsIO(sapi);
			}
			public void IncreaseStanding(IServerPlayer ofPlayer, IServerPlayer forPlayer, sbyte amount) { throw new NotImplementedException(); }
			public void IncreaseStanding(string ofPlayerUID, Queue<KeyValuePair<string, sbyte>> changes) { throw new NotImplementedException(); }
			public void DecreaseStanding(IServerPlayer ofPlayer, IServerPlayer forPlayer, sbyte amount) { throw new NotImplementedException(); }
			public void DecreaseStanding(string ofPlayerUID, Queue<KeyValuePair<string, sbyte>> changes) { throw new NotImplementedException(); }
			public sbyte? GetStanding(IServerPlayer ofPlayer, IServerPlayer forPlayer) =>
				GetStanding(ofPlayer, forPlayer.PlayerUID, out sbyte? standings) ? standings : null;

			public bool GetStanding(IServerPlayer ofPlayer, string forPlayerUID, out sbyte? standings) =>
				GetAllStandings(ofPlayer).TryGetValue(forPlayerUID, out standings);

			public Dictionary<string, sbyte?> GetAllStandings(IServerPlayer player) =>
				_PlayerStandingsIO.LoadDictionary(player);

			public Dictionary<string, sbyte?> GetRecentChanges(IServerPlayer player) =>
				_PlayerStandingsIO.LoadChanges(player);

			public void SetStanding(IServerPlayer ofPlayer, Dictionary<string, sbyte?> standings) =>
				_PlayerStandingsIO.SaveChanges(ofPlayer, standings);

			public void SetStanding(IServerPlayer ofPlayer, IServerPlayer forPlayer, sbyte? value)
			{
				_PlayerStandingsIO.SaveChanges(ofPlayer, new Dictionary<string, sbyte?>() { { forPlayer.PlayerUID, value } });
			}
			private void AddPlayer(IServerPlayer ofPlayer, IServerPlayer forPlayer) { if (!GetStanding(ofPlayer, forPlayer.PlayerUID, out _)) SetStanding(ofPlayer, forPlayer, 0); }
			public void AddPlayers(IServerPlayer Player1, IServerPlayer Player2) { AddPlayer(Player1, Player2); AddPlayer(Player2, Player1); }
		}

		public class GroupAPI
		{
			public bool IsLoadCompleted { get; private set; }
			private bool CreateDefaultGroup = false;

			GroupsAPI gapi;
			ICoreServerAPI sapi;
#pragma warning disable IDE1006 // Naming Styles
			//<PlayerUID, GroupUID>
			private Dictionary<string, int> _JoinedGroup;
			private List<GroupSettings> _Groups;
			private List<GroupStandings> _GroupStandings;
			private List<Type> _RegisteredProperties { get; set; }
#pragma warning restore IDE1006 // Naming Styles

			internal GroupAPI(GroupsAPI gapi, ICoreServerAPI api)
			{
				this.gapi = gapi;
				this.sapi = api;
				_GroupStandings = new();
				_RegisteredProperties = new();
				_JoinedGroup = new();
				IsLoadCompleted = false;
			}

			internal void OnSaveGameLoaded()
			{
				_Groups = GroupSettingsIO.LoadGroups(sapi);
				_Groups.ForEach(Group => gapi.Logger.Audit(Group.ToString()));
				_GroupStandings = GroupStandingsIO.LoadStandings(sapi);
				_Groups.ForEach(Group => Group.GetJoinedPlayers().ForEach(PlayerUID => _JoinedGroup.Add(PlayerUID, Group.GroupUID)));
				IsLoadCompleted = true;
			}
			internal void Save()
			{
				GroupSettingsIO.SaveGroups(sapi, _Groups);
			}

			#region Group Settings

			public bool CreateGroup(string GroupName, string PlayerUID, out int GroupUID)
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				GroupUID = -1;
				if (GroupName == "PropertyTest" && !CreateDefaultGroup)
				{
					gapi.LogGroupCreatation(GroupName);
					return false;
				}
				if (!PlayerUID.StartsWith("System") && sapi.World.PlayerByUid(PlayerUID) is null)
				{
					gapi.LogGroupCreatation(GroupName);
					return false;
				}
				try
				{
					foreach (GroupSettings group in _Groups)
					{
						if (group.GroupName == GroupName)
						{
							gapi.LogGroupCreatation(GroupName);
							return false;
						}
					}
					if (GetPlayerGroup(PlayerUID) != -1)
					{
						gapi.LogGroupCreatation(GroupName);
						gapi.Logger.Warning(new InvalidPlayerUID("Player already in a group, Player cannot create a new group."));
						return false;
					}
					GroupUID = _Groups.Count + 1;
					GroupSettings newGroup = new() { GroupName = GroupName, GroupUID = GroupUID };
					_Groups.Add(newGroup);
					foreach (Type T in _RegisteredProperties)
					{
						IGroupProperty property = (IGroupProperty)Activator.CreateInstance(T);
						property.UID = _RegisteredProperties.IndexOf(T);
						property.Value = property.GetDefaultValue(gapi, newGroup.GroupUID);
						newGroup.Properties.Add(property);
					}
					SetPlayerStanding(GroupUID, PlayerUID, 100);
					_JoinedGroup.Add(PlayerUID, GroupUID);
					Save();
					gapi.LogGroupCreatation(GroupName, GroupUID);
					return true;
				}
				catch
				{
					gapi.LogGroupCreatation(GroupName); return false;
				}
			}
			public bool DeleteGroup(string GroupName)
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				try
				{
					int? GroupUID = _Groups.Find(Group => Group.GroupName == GroupName)?.GroupUID;
					_JoinedGroup.Foreach(kvp => { if (kvp.Value == GroupUID) _JoinedGroup.Remove(kvp.Key); });
					bool result = _Groups.RemoveAll(Group => Group.GroupName == GroupName) > 0;
					if (result) { GroupSettingsIO.SaveGroups(sapi, _Groups); }
					return result;
				}
				catch { return false; }
			}
			public bool DeleteGroup(int GroupUID)
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				try
				{
					_JoinedGroup.Foreach(kvp => { if (kvp.Value == GroupUID) _JoinedGroup.Remove(kvp.Key); });
					bool result = _Groups.RemoveAll(Group => Group.GroupUID == GroupUID) > 0;
					if (result) { GroupSettingsIO.SaveGroups(sapi, _Groups); }
					return result;
				}
				catch { return false; }
			}

			public string[] GetGroupNames()
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				List<string> Names = new();
				foreach (GroupSettings group in _Groups)
				{
					Names.Add(group.GroupName);
				}
				return Names.ToArray();
			}
			public bool JoinGroup(int GroupUID, string PlayerUID)
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();

				throw new NotImplementedException();
			}
			public bool LeaveGroup(string PlayerUID)
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				throw new NotImplementedException();
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="GroupUID"></param>
			/// <exception cref="InvalidGroupUID"></exception>
			/// <exception cref="ArgumentNullException">Thrown if the <paramref name="GroupUID"/> paramaeter is null or empty.</exception>
			/// <exception cref="Exception"></exception>
			public GroupSettings GetGroupSettings(int GroupUID)
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				ArgumentNullException.ThrowIfNull(GroupUID, nameof(GroupUID));
				try
				{
					GroupSettings group = _Groups.ElementAt(GroupUID);
					if (group.GroupUID == GroupUID) return group;

				}
				catch { }
				try
				{
					GroupSettings group = _Groups.AsQueryable().Where(Group => Group.GroupUID == GroupUID)?.First();
					if (group is not null && group.GroupUID == GroupUID) return group;
				}
				catch { throw new InvalidGroupUID(); }
				throw new InvalidGroupUID();
			}
			/// <summary>
			/// Gets the GroupID for the given player.
			/// </summary>
			/// <param name="PlayerUID">The Player UID to check for group.</param>
			/// <exception cref="InvalidPlayerUID"></exception>
			/// <exception cref="ArgumentNullException">Thrown if the <paramref name="PlayerUID"/> paramaeter is null or empty.</exception>
			/// <exception cref="Exception"></exception>
			public int GetPlayerGroup(string PlayerUID)
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				try
				{
					ArgumentNullException.ThrowIfNull(PlayerUID, nameof(PlayerUID));
					if (!PlayerUID.StartsWith("System") && sapi.World.PlayerByUid(PlayerUID) == null) throw new InvalidPlayerUID("Invalid Player UID, Player UID not valid for any player that has joined the server.");
					return _JoinedGroup.GetValueOrDefault(PlayerUID, -1);
				}
				catch { throw; }
			}
			/// <summary>
			/// Gets the group rank of a player for the group that they are in or another group.
			/// </summary>
			/// <param name="PlayerUID">The Player's UID for use in checking their group, and the value of the player within the group specified.</param>
			/// <param name="GroupUID">If included, will check the player;'s rank in relation to the group specified, if blank or null, will check the players group standings.</param>
			/// <returns>The players Group Rank within the group specified</returns>
			/// <exception cref="InvalidGroupUID"></exception>
			/// <exception cref="ArgumentNullException">Thrown if the <paramref name="PlayerUID"/> paramaeter is null or empty.</exception>
			/// <exception cref="Exception"></exception>
			public GroupRank GetPlayerRank(string PlayerUID, int? GroupUID = null)
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				ArgumentNullException.ThrowIfNull(PlayerUID, nameof(PlayerUID));
				return GroupSettings.GetRank(GetPlayerStanding(GroupUID ?? (GetPlayerGroup(PlayerUID)), PlayerUID));
			}
			/// <summary>
			/// Get's the title of the player for the specified group, if GroupName is ommited, whill default to the player's group.
			/// </summary>
			/// <param name="PlayerUID">The Player's UID for check the standings level of the player.</param>
			/// <param name="GroupUID">The Name of the group to check, if omitted or null, will check agiainst the player's group.</param>
			/// <returns>The Title of the player as a string, releative to the sp</returns>
			/// <exception cref="InvalidGroupUID"></exception>
			/// <exception cref="ArgumentNullException">Thrown if the <paramref name="PlayerUID"/> paramaeter is null or empty.</exception>
			/// <exception cref="ArgumentException"></exception>
			/// <exception cref="Exception"></exception>
			public string GetPlayerTitle(string PlayerUID, int? GroupUID = null)
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				ArgumentNullException.ThrowIfNull(PlayerUID, nameof(PlayerUID));
				try { return GetGroupSettings(GroupUID ?? GetPlayerGroup(PlayerUID))?.GetRankTitle(GetPlayerRank(PlayerUID, GroupUID)) ?? throw new ArgumentException("Player Titles could not be gathered, Invalid PlayerUID and/or GroupUID"); } catch { throw; }
			}
			/// <summary>
			/// Gets the array of Titles for the given levels of a group, this is a list of all titles for the group. This is usefull for check if registered parameter matches a set title. 
			/// </summary>
			/// <param name="GroupUID">The name of the group to check the titles, cannot be null or empty and will throw error.</param>
			/// <returns>Returns the array of strings for a given group. You can use the following method to get what rank level the title matches: <seealso cref="GetRankFromTitle(int, string)"/></returns>
			/// <exception cref="InvalidGroupUID"></exception>
			/// <exception cref="ArgumentNullException">Thrown if the <paramref name="GroupUID"/> paramaeter is null or empty.</exception>
			/// <exception cref="ArgumentException"></exception>
			/// <exception cref="Exception">API Methods do not catch exceptions, this should be handled by the caller.</exception>
			public string[] GetGroupTitles(int GroupUID)
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				ArgumentNullException.ThrowIfNull(GroupUID, nameof(GroupUID));
				try { return GetGroupSettings(GroupUID)?.GroupTitles ?? throw new ArgumentException("Player Titles could not be gathered, Invalid GroupUID"); } catch { throw; }
			}
#if DEBUG
			public string DumpGroups()
			{
				return String.Join("\n\n", _Groups);
			}
#endif
			/// <summary>
			/// Gets the Rank enum type for a give title name in a given title group.
			/// </summary>
			/// <param name="GroupUID"></param>
			/// <param name="Title"></param>
			/// <returns></returns>
			/// <exception cref="InvalidGroupUID"></exception>
			/// <exception cref="ArgumentNullException">Thrown if the <paramref name="GroupUID"/> or <paramref name="Title"/> paramaeter is null or empty.</exception>
			/// <exception cref="ArgumentException"></exception>
			/// <exception cref="Exception"></exception>
			public GroupRank GetRankFromTitle(int GroupUID, string Title)
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				ArgumentNullException.ThrowIfNull(GroupUID, nameof(GroupUID));
				ArgumentNullException.ThrowIfNull(Title, nameof(Title));

				try { return GetGroupSettings(GroupUID)?.GetRankFromTitle(Title) ?? throw new ArgumentException("Player Titles could not be gathered, Invalid GroupUID and/or Title"); } catch { throw; }
			}

			#endregion

			#region Group Standings
			public void IncreasePlayerStanding(int GroupUID, string PlayerUID, sbyte amount, bool isRelevitve = true)
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				throw new NotImplementedException();
			}
			public void DecreasePlayerStanding(int GroupUID, string PlayerUID, sbyte amount, bool isRelevitve = true)
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				throw new NotImplementedException();
			}
			public sbyte GetPlayerStanding(int GroupUID, string PlayerUID)
			{
				ArgumentNullException.ThrowIfNull(GroupUID, nameof(GroupUID));
				ArgumentNullException.ThrowIfNull(PlayerUID, nameof(PlayerUID));
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				try
				{
					return GetGroupSettings(GroupUID).GetPlayerStanding(PlayerUID);
				}
				catch { throw; }
			}
			public void SetPlayerStanding(int GroupUID, string PlayerUID, sbyte amount)
			{
				ArgumentNullException.ThrowIfNull(GroupUID, nameof(GroupUID));
				ArgumentNullException.ThrowIfNull(PlayerUID, nameof(PlayerUID));
				ArgumentNullException.ThrowIfNull(amount, nameof(amount));
				if (-100 > amount || amount > 100) throw new ArgumentOutOfRangeException(nameof(amount), "Standings amount must be between -100 and 100, inclusive.");
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				try
				{
					GetGroupSettings(GroupUID).SetPlayerStanding(PlayerUID, amount);
					Save();
				}
				catch { throw; }

			}

			public void IncreaseGroupStanding(int ofGroupUID, int forGroupUID, sbyte amount, bool isRelevitve = true)
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				throw new NotImplementedException();
			}
			public void DecreaseGroupStanding(int ofGroupUID, int forGroupUID, sbyte amount, bool isRelevitve = true)
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				throw new NotImplementedException();
			}
			public sbyte GetGroupStanding(int ofGroupUID, int forGroupUID)
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				throw new NotImplementedException();
			}
			public void SetGroupStanding(int ofGroupUID, int forGroupUID, sbyte amount)
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();
				throw new NotImplementedException();
			}
			#endregion

			#region Group Properies

			public bool SetPropertyValue(int PropertyUID, int GroupUID, string value)
			{

				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();

				ArgumentNullException.ThrowIfNull(PropertyUID, nameof(PropertyUID));
				ArgumentNullException.ThrowIfNull(GroupUID, nameof(GroupUID));
				ArgumentNullException.ThrowIfNull(value, nameof(PropertyUID));
				try
				{
					GroupSettings group = GetGroupSettings(GroupUID);
					IGroupProperty Property = group.Properties.FirstOrDefault(property => property.UID == PropertyUID, null) ?? throw new IndexOutOfRangeException();
					if (Property.IsValueValid(gapi, GroupUID, value)) { Property.Value = value; return true; }
					return false;
				}
				catch { throw; }
			}


			/// <summary>
			/// Gets the value of a Registered property, this allows other mods to get the value set in the GUI.
			/// </summary>
			/// <param name="PropertyUID">The property UID that was returned when the property was registered.</param>
			/// <param name="GroupUID">The name of the group that the property value needs to be checked.</param>
			/// <returns>The value of the property set for the given group.</returns>
			/// <exception cref="InvalidGroupUID"></exception>
			/// <exception cref="InvalidPropertyUID"></exception>
			/// <exception cref="ArgumentNullException">Thrown if the <paramref name="GroupUID"/> or <paramref name="PropertyUID"/> paramaeter is null or empty.</exception>
			public string GetPropertyValue(int PropertyUID, int GroupUID)
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();

				ArgumentNullException.ThrowIfNull(PropertyUID, nameof(PropertyUID));
				ArgumentNullException.ThrowIfNull(GroupUID, nameof(GroupUID));
				try { return GetGroupSettings(GroupUID).Properties.Find(property => property.UID.Equals(PropertyUID))?.Value ?? throw new InvalidPropertyUID(); } catch { throw; }
			}

			/// <summary>
			/// Registers a property for use in the Groups GUI, this allows other mods to hook into the gui to allow groups to control settings relevent to their own mod.
			/// </summary>
			/// <typeparam name="T"> An implemeation of IGroupPrperty, with all of the neccasary information for registering the property.</typeparam>
			/// <returns>Returns the Property UID, this value is not static and can change on load, this UID is used to check the value of the property. Returns -1 if ModID+Property ID are not unique.</returns>
			/// <exception cref="InvalidProperty">Invalid Property is thrown if the property's default value does not</exception>
			public int RegisterProperty<T>() where T : IGroupProperty
			{
				_ = IsLoadCompleted ? "" : throw new LoadIncompleteException();

				try
				{
					if (typeof(IGroupProperty).IsAssignableFrom(typeof(T)))
					{
						foreach (Type _T in _RegisteredProperties)
						{
							if (_T.Equals(typeof(T))
								|| (typeof(T).GetField("ModID").GetValue(null).Equals(_T.GetField("ModID").GetValue(null))
								&& (typeof(T).GetField("PropertyID").GetValue(null).Equals(_T.GetField("PropertyID").GetValue(null)))))
							{
								sapi.Logger.Error($"Invalid Property: Property {typeof(T).FullName} already registered.");
								gapi.Logger.Audit($"Invalid Property: Property {typeof(T).FullName} already registered.");
								return -1;
							}
						}
						IGroupProperty testProperty = (IGroupProperty)(Activator.CreateInstance(typeof(T)) ?? throw new NullReferenceException());
						testProperty.UID = _RegisteredProperties.Count;
						CreateDefaultGroup = true;
						bool createdDefaultGroup = CreateGroup("PropertyTest", "System", out int GroupUID);
						bool resault = true;
						foreach (GroupSettings Group in _Groups)
						{
							resault = resault && testProperty.IsValueValid(gapi, Group.GroupUID, testProperty.GetDefaultValue(gapi, Group.GroupUID));
							if (!resault) { break; }
						}
						DeleteGroup(GroupUID);
						if (resault)
						{
							_RegisteredProperties.Add(typeof(T));
							int index = _RegisteredProperties.IndexOf(typeof(T));
							_Groups.ForEach(Group => Group.UnpackProperty(gapi, typeof(T), index));
							return _RegisteredProperties.IndexOf(typeof(T));
						}
						throw new InvalidProperty();
					}
					throw new InvalidCastException();
				}
				catch { throw; }
			}

			#endregion
		}

		private void LogGroupCreatation(string GroupName, int UID = -1)
		{
			if (GroupName == "PropertyTest") return;
			bool successful = UID != -1;
			sapi.Logger.Notification("Group Creatation Attempted! See Audit Log For Details!");
			string log = $"Group \"{GroupName}\" Creation {(successful ? "Successful! Group Details: " : "Unsuccessful!")}";
			log += successful ? Group.GetGroupSettings(UID).ToString() : "";
			if (!successful) sapi.Logger.Warning(log);
			this.Logger.Audit(log);
		}
	}
}
