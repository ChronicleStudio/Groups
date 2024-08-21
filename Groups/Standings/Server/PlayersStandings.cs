using System.Collections.Generic;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Groups.Standings.Server
{
    public class PlayersStandings
    {
        private const string ModDataKey = "PlayersStandingsData";
        private const string ChangesKey = "PlayersStandingsChanges";

        public static Dictionary<string, IStandings> LoadDictionary(IServerPlayer player)
        {
            Dictionary<string, IStandings> standings = GetModData(player, ModDataKey);
            if (standings == null)
            {
                standings = new Dictionary<string, IStandings>
                {
                    { player.PlayerUID, new Client.PlayersStandings(0) }
                };
                SaveChanges(player, standings);
            }
            return standings;
        }

        public static Dictionary<string, IStandings> LoadChanges(IServerPlayer player)
        {
            Dictionary<string, IStandings> standings = GetModData(player, ChangesKey);
            if (standings == null)
            {
                standings = new Dictionary<string, IStandings>();
            }
            else
            {
                player.RemoveModdata(ChangesKey);
            }
            return standings;
        }

        /// <summary>
        /// Saves the changes of standings for a give player from a list of players, adds the changes to the changes queue, and updates the dictionary.
        /// </summary>
        /// <param name="sapi"></param>
        /// <param name="player">The player that is prospective of the change.</param>
        /// <param name="standings">A dictionary of the new current standings of a player, not the change of player.</param>
        public static void SaveChanges(IServerPlayer player, Dictionary<string, IStandings> standings)
        {
            if (standings == null) return;

            // Update list of changes, used for fast lookup and retival for the GUI
            Dictionary<string, IStandings> oldStandings = GetModData(player, ChangesKey);

            if (oldStandings == null && standings != null)
            {

                SetModData(player, ChangesKey, standings);
            }
            else if (standings.Count > oldStandings.Count)
            {
                foreach (KeyValuePair<string, IStandings> standing in oldStandings)
                {
                    if (!standings.ContainsKey(standing.Key)) { standings.Add(standing.Key, standing.Value); }
                }
                SetModData(player, ChangesKey, standings);
            }
            else
            {
                foreach (KeyValuePair<string, IStandings> standing in standings)
                {
                    if (!oldStandings.ContainsKey(standing.Key)) { oldStandings.Add(standing.Key, standing.Value); }
                }
                SetModData(player, ChangesKey, oldStandings);
            }

            // Update the dictionary list needed for when player logs in. 
            Dictionary<string, IStandings> standingsDirectory = GetModData(player, ModDataKey);
            if (standingsDirectory == null)
            {
                SetModData(player, ModDataKey, standings);
            }
            else
            {
                foreach (KeyValuePair<string, IStandings> standing in standings)
                {
                    try { standingsDirectory.Remove(standing.Key); }
                    catch { }
                    standingsDirectory.Add(standing.Key, standing.Value);
                }
                SetModData(player, ModDataKey, standingsDirectory);
            }

        }
        public static void SetModData(IServerPlayer player, string key, Dictionary<string, IStandings> standings)
        {
            player.SetModdata(key, Serilize(standings));
        }
        public static Dictionary<string, IStandings> GetModData(IServerPlayer player, string key)
        {
            return Deserialize(player.GetModdata(key));
        }

        public static byte[] Serilize(Dictionary<string, IStandings> standings)
        {
            List<KeyValuePair<string, byte[]>> standingsData = new();
            foreach (KeyValuePair<string, IStandings> standing in standings)
            {
                standingsData.Add(new KeyValuePair<string, byte[]>(standing.Key, SerializerUtil.Serialize(standing.Value)));
            }
            return SerializerUtil.Serialize(standingsData);

        }
        public static Dictionary<string, IStandings> Deserialize(byte[] data)
        {
            Dictionary<string, IStandings> standings = new Dictionary<string, IStandings>();
            List<KeyValuePair<string, byte[]>> standingsData = data == null ? null : SerializerUtil.Deserialize<List<KeyValuePair<string, byte[]>>>(data);
            if (standingsData == null)
            {
                return null;
            }
            foreach (KeyValuePair<string, byte[]> kv in standingsData)
            {
                standings.Add(kv.Key, kv.Value == null ? null : (IStandings)SerializerUtil.Deserialize<Client.PlayersStandings>(kv.Value));
            }
            return standings;
        }
    }
}
