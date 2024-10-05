using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Vintagestory.API.Util;

namespace Groups.API.Group
{
	[ProtoContract]
	public class GroupSettings
	{
		private int _GroupUID = -100;
		[ProtoMember(1)]
		public int GroupUID
		{
			get { return _GroupUID; }
			set
			{
				if (_GroupUID < 0) { _GroupUID = value; } else throw new InvalidOperationException();
			}
		}
		[ProtoMember(2)]
		public string GroupName { get; set; }
		[ProtoMember(3)]
		public string[] GroupTitles { get; set; }
		[ProtoMember(4)]
		public bool[] GroupAutoProgress { get; set; }

		public List<IGroupProperty> Properties { get; set; }
		[ProtoMember(5)]
		public Dictionary<string, sbyte> PlayerStandings { get; set; }
		[ProtoMember(6)]
		public Dictionary<string, string> PackedProperties { get; set; }

		public GroupSettings(string GroupName) : this()
		{
			this.GroupName = GroupName;
		}
		public GroupSettings()
		{
			GroupTitles = GetRandomTitles();
			GroupAutoProgress = new bool[] { false, false, true, true, false, false, true, true, false };
			Properties = new();
			PlayerStandings = new();
		}

		public void PackProperties()
		{
			PackedProperties = new();
			foreach (var property in Properties)
			{
				Type T = property.GetType();
				PackedProperties.Add((T.GetField("ModID").GetValue(null) as string) + (T.GetField("PropertyID").GetValue(null) as string), property.Value);
			}
		}
		public bool UnpackProperty(GroupsAPI gapi, Type T, int UID)
		{
			bool success = PackedProperties.TryGetValue((T.GetField("ModID").GetValue(null) as string) + (T.GetField("PropertyID").GetValue(null) as string), out string value);

			IGroupProperty property = (IGroupProperty)(Activator.CreateInstance(T) ?? throw new NullReferenceException());

			property.Value = success && property.IsValueValid(gapi, GroupUID, value) ? value : property.GetDefaultValue(gapi, GroupUID);
			property.UID = UID;
			Properties.Add(property);
			return success;

		}


		public override string ToString()
		{
			string resault = $"\nGroupUID: {GroupUID}\nGroupName: {GroupName}\nGroupTitles: [{String.Join(", ", GroupTitles)}]\nGroupAutoProgress: [{String.Join(", ", GroupAutoProgress)}]\n";
			foreach (IGroupProperty property in Properties)
			{
				Type T = property.GetType();
				resault += $"Property Class: {T.FullName}\n" +
					$"Property UID: {property.UID}\n" +
					$"Property ModID: {T.GetField("ModID").GetValue(null)}\n" +
					$"Property PropertyID: {T.GetField("PropertyID").GetValue(null)}\n" +
					$"Property Name: {T.GetField("Name").GetValue(null)}\n" +
					$"Property Description: {T.GetField("Description").GetValue(null)}\n" +
					$"Property Value: {property.Value}\n\n";
			}
			return resault;
		}

		public string GetRankTitle(GroupRank rank) => rank switch
		{
			GroupRank.Rank5 => GroupTitles[0],
			GroupRank.Rank4 => GroupTitles[1],
			GroupRank.Rank3 => GroupTitles[2],
			GroupRank.Rank2 => GroupTitles[3],
			GroupRank.Rank1 => GroupTitles[4],
			GroupRank.RankA => GroupTitles[5],
			GroupRank.RankN => GroupTitles[6],
			GroupRank.RankE => GroupTitles[7],
			GroupRank.RankW => GroupTitles[8],
			_ => throw new InvalidEnumArgumentException()
		};

		public bool GetRankAutoProgress(GroupRank rank) => rank switch
		{
			GroupRank.Rank5 => GroupAutoProgress[0],
			GroupRank.Rank4 => GroupAutoProgress[1],
			GroupRank.Rank3 => GroupAutoProgress[2],
			GroupRank.Rank2 => GroupAutoProgress[3],
			GroupRank.Rank1 => GroupAutoProgress[4],
			GroupRank.RankA => GroupAutoProgress[5],
			GroupRank.RankN => GroupAutoProgress[6],
			GroupRank.RankE => GroupAutoProgress[7],
			GroupRank.RankW => GroupAutoProgress[8],
			_ => throw new InvalidEnumArgumentException()
		};

		public static sbyte GetRankMinStanding(GroupRank rank) => rank switch
		{
			GroupRank.RankW => -100,
			GroupRank.RankE => -99,
			GroupRank.RankN => 0,
			GroupRank.RankA => 30,
			GroupRank.Rank1 => 50,
			GroupRank.Rank2 => 60,
			GroupRank.Rank3 => 70,
			GroupRank.Rank4 => 80,
			GroupRank.Rank5 => 90,
			_ => throw new InvalidEnumArgumentException()
		};

		public static sbyte GetRankMaxStanding(GroupRank rank) => rank switch
		{
			GroupRank.RankW => -100,
			GroupRank.RankE => -1,
			GroupRank.RankN => 29,
			GroupRank.RankA => 49,
			GroupRank.Rank1 => 59,
			GroupRank.Rank2 => 69,
			GroupRank.Rank3 => 79,
			GroupRank.Rank4 => 89,
			GroupRank.Rank5 => 100,
			_ => throw new InvalidEnumArgumentException()
		};


		public string GetPlayerTitle(string PlayerUID) =>
			GetRankTitle(GetPlayerRank(PlayerUID));

		public GroupRank GetPlayerRank(string PlayerUID) =>
			GetRank(GetPlayerStanding(PlayerUID));

		public sbyte GetPlayerStanding(string PlayerUID)
		{
			ArgumentNullException.ThrowIfNullOrEmpty(PlayerUID, nameof(PlayerUID));
			return PlayerStandings.GetValueOrDefault(PlayerUID, (sbyte)0);
		}

		public void SetPlayerStanding(string PlayerUID, sbyte standing)
		{
			ArgumentNullException.ThrowIfNullOrEmpty(PlayerUID, nameof(PlayerUID));
			if (standing is < -100 or > 100) throw new ArgumentOutOfRangeException(nameof(standing), "Standing must be between -100 and 100, inclusive.");
			try
			{
				PlayerStandings.Remove(PlayerUID);
				PlayerStandings.Add(PlayerUID, standing);
			}
			catch { throw; }
		}

		public List<string> GetJoinedPlayers()
		{
			List<string> Players = new();
			PlayerStandings.Foreach(Standings => { if (Standings.Value >= GetRankMinStanding(GroupRank.Rank1)) { Players.Add(Standings.Key); } });
			return Players;
		}

		public static GroupRank GetRank(sbyte? standing)
		{
			return standing is null ? GroupRank.RankN : GetRank((sbyte)standing);
		}
		public static GroupRank GetRank(sbyte standing)
		{
			if (standing is < -100 or > 100) throw new ArgumentOutOfRangeException(nameof(standing), "Standing must be between -100 and 100, inclusive.");
			GroupRank[] ranks = {
			GroupRank.RankW,
			GroupRank.RankE,
			GroupRank.RankN,
			GroupRank.RankA,
			GroupRank.Rank1,
			GroupRank.Rank2,
			GroupRank.Rank3,
			GroupRank.Rank4,
			GroupRank.Rank5,
			};
			foreach (GroupRank rank in ranks)
			{
				if (GetRankMinStanding(rank) <= standing && standing <= GetRankMaxStanding(rank)) return rank;
			}
			throw new ArgumentOutOfRangeException(nameof(standing), "Standing must be between -100 and 100, inclusive.");
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public GroupRank GetRankFromTitle(string title)
		{
			if (title.Equals(GroupTitles[0])) return GroupRank.Rank5;
			if (title.Equals(GroupTitles[1])) return GroupRank.Rank4;
			if (title.Equals(GroupTitles[2])) return GroupRank.Rank3;
			if (title.Equals(GroupTitles[3])) return GroupRank.Rank2;
			if (title.Equals(GroupTitles[4])) return GroupRank.Rank1;
			if (title.Equals(GroupTitles[5])) return GroupRank.RankA;
			if (title.Equals(GroupTitles[6])) return GroupRank.RankN;
			if (title.Equals(GroupTitles[7])) return GroupRank.RankE;
			if (title.Equals(GroupTitles[8])) return GroupRank.RankW;
			throw new ArgumentException("Player Titles could not be gathered, Invalid ~~Title");

		}


		private static string[] GetRandomTitles()
		{
			string[][] Titles = {
				new string[]{ "Monarch", "Right Hand", "Noble", "Kight", "Pessent", "Sworn Alegant", "Foreigner", "Bandant", "Adversary" },
				new string[] { "Jarl", "Jomsviking", "Thagn", "Vikingr", "Drengr", "Friomenn", "Kylfing", "Argr", "Ovinur" },
				new string[] { "Firstman", "Councillor", "Reeve", "Elder", "Townsman", "United", "Stranger", "Outcast", "Invader" }
			};
			return Titles[new Random().Next(0, Titles.Length)];


		}
	}

	public enum GroupRank
	{
		RankW = -10,
		RankE = -2,
		RankN = -1,
		RankA = 0,
		Rank1 = 1,
		Rank2 = 2,
		Rank3 = 3,
		Rank4 = 4,
		Rank5 = 5,
	}


}
