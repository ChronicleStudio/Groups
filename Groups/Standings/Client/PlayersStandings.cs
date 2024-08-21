﻿using Groups.Standings.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace Groups.Standings.Client
{
    internal class PlayersStandings : IStandings
    {
        public double Standings { get; }

        public PlayersStandings(double Standings)
        {
            this.Standings = Standings;
        }
        public PlayersStandings()
        {
        }

        public static List<KeyValuePair<IPlayer, IStandings>> GetAllPlayersStandings(ICoreClientAPI capi)
        {
            List<KeyValuePair<IPlayer, IStandings>> standings = new();

            List<IPlayer> players = capi.World.AllOnlinePlayers.ToList();
            Dictionary<string, IStandings> standingsDictionary = capi.ModLoader.GetModSystem<PlayerStandingsNetwork>().GetStandings();

            foreach (IPlayer player in players)
            {


                IStandings playerStandings = standingsDictionary.Get(player.PlayerUID);
                if (playerStandings != null)
                {
                    standings.Add(new KeyValuePair<IPlayer, IStandings>(player, playerStandings));
                }
            }

            return standings;
        }

        public static IStandings GetPlayerStanding(ICoreClientAPI capi, IPlayer player)
        {/*
            HeyServer get palyer standings for me of palayer

                has anything changed?
                No? okay

                Or Yes?

                Server here you go, here the dictiononar 

                okay thanks, return Dictionary
            */
            return new PlayersStandings(Math.Round((new Random().NextDouble() * 2 - 1) * 100) / 100.0);
        }
    }
}
