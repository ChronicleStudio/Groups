using Groups.API.Group;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Groups.API
{
	internal class Events
	{
		public static void Register(ICoreServerAPI sapi, GroupsAPI groupsAPI)
		{
			sapi.Event.PlayerJoin += (player) =>
			{
#if DEBUG
				int UID;
				groupsAPI.Group.RegisterProperty<SampleProperty>();
				groupsAPI.Group.RegisterProperty<SampleProperty>();
				groupsAPI.Group.RegisterProperty<SampleProperty2>();
				groupsAPI.Group.RegisterProperty<SampleProperty2>();
				groupsAPI.Group.RegisterProperty<SampleProperty3>();
				groupsAPI.Group.RegisterProperty<SampleProperty3>();
				groupsAPI.Group.CreateGroup("DebugTestGroup0", "System0", out UID);
				groupsAPI.Group.CreateGroup("DebugTestGroup0", "System0", out UID);
				groupsAPI.Group.CreateGroup("DebugTestGroup1", "System1", out UID);
				groupsAPI.Group.RegisterProperty<SampleProperty4>();
				groupsAPI.Group.RegisterProperty<SampleProperty4>();
				groupsAPI.Group.RegisterProperty<SampleProperty5>();
				/*
				var propertyUID = groupsAPI.Group.RegisterProperty<SampleProperty5>();

				var gapi = sapi.ModLoader.GetModSystem<GroupsAPI>();
				int GroupUID = gapi.Group.GetPlayerGroup(player.PlayerUID);

				GroupRank? rank = null; //get rank from sancuary gui

				rank ??= gapi.Group.GetRankFromTitle(GroupUID, gapi.Group.GetPropertyValue(propertyUID, GroupUID));
				bool HasPermission = gapi.Group.GetPlayerRank(player.PlayerUID, GroupUID) >= rank;
				if (!HasPermission) { gapi.Group.DecreasePlayerStanding(GroupUID, player.PlayerUID, 5, true); }
				*/

#endif


				sapi.Event.RegisterGameTickListener((args) =>
				{
					Dictionary<string, sbyte?>.KeyCollection MeetPlayers = groupsAPI.Player.GetAllStandings(player).Keys;
					if (sapi.World.AllPlayers.Length <= MeetPlayers.Count) return;
					sapi.World.GetPlayersAround(player.Entity.Pos.XYZ, 32f, 32f, _player => !MeetPlayers.Contains(_player.PlayerUID))
						.Foreach(newPlayer => groupsAPI.Player.AddPlayers(player, (IServerPlayer)newPlayer));
				}, 30000);
			};
		}
	}
}
