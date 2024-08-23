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

	class GUIColors
	{
		public const string RED = "#FF0000";
		public const string GREEN = "#00FF00";
		public const string YELLOW = "#FFFF00";
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
