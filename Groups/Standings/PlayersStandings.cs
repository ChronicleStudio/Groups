using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Server;

namespace Groups.Standings
{
    internal class PlayersStandings : IStandings
    {
        public double Standings { get; }
        
        public PlayersStandings(double  Standings)
        {
            this.Standings = Standings;
        }

        public static List<KeyValuePair<IPlayer, IStandings>> GetAllPlayersStandings(ICoreClientAPI capi)
        {
            List<KeyValuePair<IPlayer, IStandings>> standings = new();

            List<IPlayer> players = capi.World.AllOnlinePlayers
                .OrderBy(player => player.PlayerName)
                .ToList();
            for (int i = 55; i > 0; i--)
            //for (int i = new Random().Next(5, 50); i > 0; i--)
            {
                players.Add(players.First());
            }


            foreach (IPlayer player in players) {
                IStandings playerStandings = GetPlayerStanding(capi, player);
                if (playerStandings != null) {
                    standings.Add(new KeyValuePair<IPlayer, IStandings>(player, playerStandings)); 
                }
            }

            return standings;
        }

        public static IStandings GetPlayerStanding(ICoreClientAPI capi, IPlayer player)
        {
            return new PlayersStandings(Math.Round((new Random().NextDouble() * 2 - 1) * 100) / 100.0);
        }
    }
}
