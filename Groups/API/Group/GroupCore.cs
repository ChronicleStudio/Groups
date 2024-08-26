using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Groups.API.Group
{
	public class GroupCore
	{
		public string GroupName { get; set; }
		public List<IGroupProperty> Properties { get; set; }
		public string[] GroupTitles { get; set; }
		public bool[] GroupAutoProgress { get; set; }

		public Dictionary<string, sbyte> PlayerStandings { get; set; }

		public GroupCore()
		{
			GroupTitles = new string[9];
			GroupAutoProgress = new bool[9];
			PlayerStandings = new();
		}

		public string GetRankTitle(GroupRank rank) => rank switch
		{
			GroupRank.RankW => GroupTitles[0],
			GroupRank.RankE => GroupTitles[1],
			GroupRank.RankN => GroupTitles[2],
			GroupRank.RankA => GroupTitles[3],
			GroupRank.Rank1 => GroupTitles[4],
			GroupRank.Rank2 => GroupTitles[5],
			GroupRank.Rank3 => GroupTitles[6],
			GroupRank.Rank4 => GroupTitles[7],
			GroupRank.Rank5 => GroupTitles[8],
			_ => throw new InvalidEnumArgumentException()
		};

		public bool GetRankAutoProgress(GroupRank rank) => rank switch
		{
			GroupRank.RankW => GroupAutoProgress[0],
			GroupRank.RankE => GroupAutoProgress[1],
			GroupRank.RankN => GroupAutoProgress[2],
			GroupRank.RankA => GroupAutoProgress[3],
			GroupRank.Rank1 => GroupAutoProgress[4],
			GroupRank.Rank2 => GroupAutoProgress[5],
			GroupRank.Rank3 => GroupAutoProgress[6],
			GroupRank.Rank4 => GroupAutoProgress[7],
			GroupRank.Rank5 => GroupAutoProgress[8],
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

		public void SetPlayerRank(string PlayerUID, sbyte standing)
		{
			if (string.IsNullOrEmpty(PlayerUID)) throw new ArgumentNullException(nameof(PlayerUID), $"'{nameof(PlayerUID)}' cannot be null or empty.");
		}

		public string GetPlayerTitle(string PlayerUID) =>
			GetRankTitle(GetPlayerRank(PlayerUID));

		public GroupRank GetPlayerRank(string PlayerUID) =>
			GetRank(GetPlayerStanding(PlayerUID));

		public sbyte GetPlayerStanding(string PlayerUID)
		{
			if (string.IsNullOrEmpty(PlayerUID)) throw new ArgumentNullException(nameof(PlayerUID), $"'{nameof(PlayerUID)}' cannot be null or empty.");
			return PlayerStandings.GetValueOrDefault(PlayerUID, (sbyte)0);
		}

		public static GroupRank GetRank(sbyte? standing)
		{
			return standing is null ? GroupRank.RankN : GetRank((sbyte)standing);
		}
		public static GroupRank GetRank(sbyte standing)
		{
			if (standing is < -100 or > 100) throw new ArgumentOutOfRangeException();
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
			throw new ArgumentOutOfRangeException();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public GroupRank GetRankFromTitle(string title)
		{
			if (title.Equals(GroupTitles[0])) return GroupRank.RankW;
			if (title.Equals(GroupTitles[1])) return GroupRank.RankE;
			if (title.Equals(GroupTitles[2])) return GroupRank.RankN;
			if (title.Equals(GroupTitles[3])) return GroupRank.RankA;
			if (title.Equals(GroupTitles[4])) return GroupRank.Rank1;
			if (title.Equals(GroupTitles[5])) return GroupRank.Rank2;
			if (title.Equals(GroupTitles[6])) return GroupRank.Rank3;
			if (title.Equals(GroupTitles[7])) return GroupRank.Rank4;
			if (title.Equals(GroupTitles[8])) return GroupRank.Rank5;
			throw new ArgumentException();

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
