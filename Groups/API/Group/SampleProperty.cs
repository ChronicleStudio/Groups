using System.Linq;

namespace Groups.API.Group
{
	internal class SampleProperty : IGroupProperty
	{

		public static readonly string Name = "Test Property";
		public static readonly string Description = "A Dummy Property";
		public string Value { get; set; }
		public static readonly string ModID = "GroupsModApi";
		public static readonly string PropertyID = "TestProperty";
		public int UID { get; set; }

		public bool IsValueValid(GroupsAPI gapi, int groupUID, string newValue)
		{
			return gapi.Group.GetGroupTitles(groupUID).ToList().Find(title => title.Equals(newValue)) != null;
		}

		public string GetDefaultValue(GroupsAPI gapi, int groupUID)
		{
			return gapi.Group.GetGroupTitles(groupUID)[2];
		}
	}
	internal class SampleProperty2 : IGroupProperty
	{

		public static readonly string Name = "Test Property";
		public static readonly string Description = "A Dummy Property";
		public string Value { get; set; }
		public static readonly string ModID = "GroupsModApi";
		public static readonly string PropertyID = "TestProperty";
		public int UID { get; set; }

		public bool IsValueValid(GroupsAPI gapi, int groupUID, string newValue)
		{
			return gapi.Group.GetGroupTitles(groupUID).ToList().Find(title => title.Equals(newValue)) != null;
		}

		public string GetDefaultValue(GroupsAPI gapi, int groupUID)
		{
			return gapi.Group.GetGroupTitles(groupUID)[2];
		}
	}
	internal class SampleProperty3 : IGroupProperty
	{

		public static readonly string Name = "Test Property 3";
		public static readonly string Description = "A Dummy Property";
		public string Value { get; set; }
		public static readonly string ModID = "GroupsModApi";
		public static readonly string PropertyID = "TestProperty3";
		public int UID { get; set; }

		public bool IsValueValid(GroupsAPI gapi, int groupUID, string newValue)
		{
			return gapi.Group.GetGroupTitles(groupUID).ToList().Find(title => title.Equals(newValue)) != null;
		}

		public string GetDefaultValue(GroupsAPI gapi, int groupUID)
		{
			return gapi.Group.GetGroupTitles(groupUID)[2];
		}
	}
	internal class SampleProperty4 : IGroupProperty
	{

		public static readonly string Name = "Test Property";
		public static readonly string Description = "A Dummy Property";
		public string Value { get; set; }
		public static readonly string ModID = "GroupsModApi4";
		public static readonly string PropertyID = "TestProperty";
		public int UID { get; set; }

		public bool IsValueValid(GroupsAPI gapi, int groupUID, string newValue)
		{
			return gapi.Group.GetGroupTitles(groupUID).ToList().Find(title => title.Equals(newValue)) != null;
		}

		public string GetDefaultValue(GroupsAPI gapi, int groupUID)
		{
			return gapi.Group.GetGroupTitles(groupUID)[2];
		}
	}
	internal class SampleProperty5 : IGroupProperty
	{

		public static readonly string Name = "Test Property";
		public static readonly string Description = "A Dummy Property";
		public string Value { get; set; }
		public static readonly string ModID = "GroupsModApi5";
		public static readonly string PropertyID = "TestProperty5";
		public int UID { get; set; }

		public bool IsValueValid(GroupsAPI gapi, int groupUID, string newValue)
		{
			return gapi.Group.GetGroupTitles(groupUID).ToList().Find(title => title.Equals(newValue)) != null;
		}

		public string GetDefaultValue(GroupsAPI gapi, int groupUID)
		{
			return gapi.Group.GetGroupTitles(groupUID)[2];
		}
	}
}
