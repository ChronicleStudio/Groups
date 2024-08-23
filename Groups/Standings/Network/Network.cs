using Groups.API;
using Groups.Standings.Client;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using static Groups.Standings.Network.Utilites;

namespace Groups.Standings.Network
{


	/// <summary>
	/// A basic example of client<->server networking using a custom network communication
	/// </summary>
	public class PlayerStandingsNetwork : ModSystem
	{
		private const string channelName = "playerstandingsnetwork";
		public override void Start(ICoreAPI api)
		{
			api.Network
				.RegisterChannel(channelName)
				.RegisterMessageType(typeof(NetworkApiServerUpdate))
				.RegisterMessageType(typeof(NetworkApiClientRequest));
		}

		#region Server
		IServerNetworkChannel serverChannel;
		ICoreServerAPI sapi;

		public override void StartServerSide(ICoreServerAPI api)
		{
			sapi = api;

			serverChannel = api.Network.GetChannel(channelName)
				.SetMessageHandler<NetworkApiClientRequest>(OnClientMessage);
		}

		public static void SendClinetUpdate(ICoreServerAPI sapi, IServerPlayer fromPlayer)
		{
			if (sapi != null) { sapi.ModLoader.GetModSystem<PlayerStandingsNetwork>().OnClientMessage(fromPlayer, new NetworkApiClientRequest() { message = Requests.RECENT_CHANGES }); }
		}

		private void OnClientMessage(IServerPlayer fromPlayer, NetworkApiClientRequest networkMessage)
		{
			NetworkApiServerUpdate nasu = networkMessage.message switch
			{
				Requests.FULL_DICTIONARY => new NetworkApiServerUpdate() { isFullDictionary = true, StandingsDict = SerializerUtil.Serialize(sapi.ModLoader.GetModSystem<GroupsAPI>().Player.GetAllStandings(fromPlayer)) },
				Requests.RECENT_CHANGES => new NetworkApiServerUpdate() { isFullDictionary = false, StandingsDict = SerializerUtil.Serialize(sapi.ModLoader.GetModSystem<GroupsAPI>().Player.GetRecentChanges(fromPlayer)) },
				_ => new NetworkApiServerUpdate() { isInvalidRequest = true, response = "Invalid Request Type, Type " + nameof(networkMessage.message) },
			};
			serverChannel.SendPacket(nasu, fromPlayer);
		}
		IClientNetworkChannel clientChannel;
		ICoreClientAPI capi;

		#endregion
		#region Client
		private Dictionary<string, PlayerStandings> standings;
		public Dictionary<string, PlayerStandings> Standings
		{
			get
			{
				if (standings == null)
				{
					capi.Network.GetChannel(channelName).SendPacket(new NetworkApiClientRequest() { message = Requests.FULL_DICTIONARY });
					return new Dictionary<string, PlayerStandings>();
				}

				return standings;
			}
			set
			{
				standings = value;
			}
		}

		public override void StartClientSide(ICoreClientAPI api)
		{
			capi = api;

			clientChannel = api.Network.GetChannel(channelName)
				.SetMessageHandler<NetworkApiServerUpdate>(OnServerMessage);
			api.Event.RegisterGameTickListener((args) => { capi.Network.GetChannel(channelName).SendPacket(new NetworkApiClientRequest() { message = Requests.FULL_DICTIONARY }); }, 1000 * 60 * 5);
		}

		private void OnServerMessage(NetworkApiServerUpdate networkMessage)
		{
			if (networkMessage.isInvalidRequest)
			{
				throw new InvalidOperationException(networkMessage.response);
			}
			if (networkMessage.StandingsDict == null) return;
			if (networkMessage.isFullDictionary)
			{
				Standings = SerializerUtil.Deserialize<Dictionary<string, PlayerStandings>>(networkMessage.StandingsDict);
			}
			else
			{
				foreach (KeyValuePair<string, PlayerStandings> standing in SerializerUtil.Deserialize<Dictionary<string, PlayerStandings>>(networkMessage.StandingsDict))
				{
					Standings.Remove(standing.Key);
					Standings.Add(standing.Key, standing.Value);

				}
			}
		}

		public Dictionary<string, PlayerStandings> GetStandings() { return Standings; }

		#endregion
	}
}
