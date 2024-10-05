using System;

namespace Groups.API.Group
{
	public class GroupStandings
	{
		public string Group1Name { get; set; }
		public string Group2Name { get; set; }
		public sbyte? Standing { get; set; }

		public bool DoesMatch(string group1Name, string group2Name)
		{
			if (string.IsNullOrEmpty(group1Name)) throw new ArgumentNullException(group1Name, $"'{nameof(group1Name)}' cannot be null or empty.");
			if (string.IsNullOrEmpty(group2Name)) throw new ArgumentNullException(group2Name, $"'{nameof(group2Name)}' cannot be null or empty.");
			if (group1Name == group2Name) return false;
			return (Group1Name.Equals(group1Name) && Group2Name.Equals(group2Name)) || (Group1Name.Equals(group2Name) && Group2Name.Equals(group1Name));
		}

	}
}
