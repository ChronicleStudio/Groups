﻿using Groups.API;
using ProtoBuf;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using static Groups.GUI.Network.PlayerUtilites;

namespace Groups.GUI.Network
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
			sapi?.ModLoader.GetModSystem<PlayerStandingsNetwork>().OnClientMessage(fromPlayer, new NetworkApiClientRequest() { message = Requests.RECENT_CHANGES });
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

		#endregion

		#region Client

		IClientNetworkChannel clientChannel;
		ICoreClientAPI capi;
		private Dictionary<string, sbyte?> standings;
		public Dictionary<string, sbyte?> Standings
		{
			get
			{
				if (standings == null)
				{
					capi.Network.GetChannel(channelName).SendPacket(new NetworkApiClientRequest() { message = Requests.FULL_DICTIONARY });
					return new Dictionary<string, sbyte?>();
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

			clientChannel = capi.Network.GetChannel(channelName)
				.SetMessageHandler<NetworkApiServerUpdate>(OnServerMessage);
			capi.Event.RegisterGameTickListener((args) => { capi.Network.GetChannel(channelName).SendPacket(new NetworkApiClientRequest() { message = Requests.FULL_DICTIONARY }); }, 1000 * 60 * 5);
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
				Standings = SerializerUtil.Deserialize<Dictionary<string, sbyte?>>(networkMessage.StandingsDict);
			}
			else
			{
				foreach (KeyValuePair<string, sbyte?> standing in SerializerUtil.Deserialize<Dictionary<string, sbyte?>>(networkMessage.StandingsDict))
				{
					Standings.Remove(standing.Key);
					Standings.Add(standing.Key, standing.Value);

				}
			}
		}

		public Dictionary<string, sbyte?> GetStandings() { return Standings; }

		#endregion

		#region Utilites

	}

	internal class PlayerUtilites
	{
		[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
		public class NetworkApiClientRequest
		{
			public Requests message;
		}

		[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
		public class NetworkApiServerUpdate
		{
			public string response;
			public bool isFullDictionary;
			public bool isInvalidRequest;
			public byte[] StandingsDict;
		}
	}
	public enum Requests
	{
		FULL_DICTIONARY,
		RECENT_CHANGES,

		#endregion

	}
}