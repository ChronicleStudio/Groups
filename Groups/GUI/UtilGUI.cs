using System;

namespace Groups.GUI
{
	internal class Utilities
	{
		public static Sort IncrementSort(Sort sort) => sort switch
		{
			Sort.Player => Sort.Standing,
			Sort.Standing => Sort.Default,
			Sort.Default => Sort.Player,
			_ => throw new ArgumentOutOfRangeException(nameof(sort)),
		};
		public static SortModifier IncrementSortModifier(SortModifier sort) => sort switch
		{
			SortModifier.Ascending => SortModifier.Descending,
			SortModifier.Descending => SortModifier.Ascending,
			_ => throw new ArgumentOutOfRangeException(nameof(sort)),
		};

	}

	static class GUIColors
	{
		public static string RED = "#FF0000";
		public static string ORANGE = "#FF8800";
		public static string YELLOW = "#FFFF00";
		public static string BLUE = "#00FFFF";
		public static string GREEN = "#00FF00";
	}
	enum SortModifier
	{
		Ascending,
		Descending,
	}
	enum Sort
	{
		Player,
		Standing,
		Default,
	}
}
