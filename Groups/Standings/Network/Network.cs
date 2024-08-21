using Groups.Standings.Server;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
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
                .RegisterMessageType(typeof(NetworkApiClientRequest))
            ;
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

        private void OnClientMessage(IServerPlayer fromPlayer, NetworkApiClientRequest networkMessage)
        {
            NetworkApiServerUpdate nasu;
            switch (networkMessage.message)
            {
                case Requests.FULL_DICTIONARY:
                    nasu = new NetworkApiServerUpdate() { isFullDictionary = true, StandingsDict = PlayersStandings.Serilize(PlayersStandings.LoadDictionary(fromPlayer)) };
                    break;
                case Requests.RECENT_CHANGES:
                    nasu = new NetworkApiServerUpdate() { isFullDictionary = false, StandingsDict = PlayersStandings.Serilize(PlayersStandings.LoadChanges(fromPlayer)) };
                    break;
                default:
                    nasu = new NetworkApiServerUpdate() { isInvalidRequest = true, response = "Invalid Request Type, Type " + nameof(networkMessage.message) };
                    break;
            }
            serverChannel.SendPacket(nasu, fromPlayer);
        }
        IClientNetworkChannel clientChannel;
        ICoreClientAPI capi;

        #endregion
        #region Client
        private Dictionary<string, IStandings> standings;
        public Dictionary<string, IStandings> Standings
        {
            get
            {
                if (standings == null)
                {
                    capi.Network.GetChannel(channelName).SendPacket(new NetworkApiClientRequest() { message = Requests.FULL_DICTIONARY });
                    return new Dictionary<string, IStandings>();
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
                Standings = PlayersStandings.Deserialize(networkMessage.StandingsDict);
            }
            else
            {
                foreach (KeyValuePair<string, IStandings> standing in PlayersStandings.Deserialize(networkMessage.StandingsDict))
                {
                    Standings.Remove(standing.Key);
                    Standings.Add(standing.Key, standing.Value);

                }
            }
        }

        public Dictionary<string, IStandings> GetStandings() { return Standings; }

        #endregion
    }
}
