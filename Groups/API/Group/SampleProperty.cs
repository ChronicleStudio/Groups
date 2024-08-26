using System.Linq;

namespace Groups.API.Group
{
	internal class SampleProperty : IGroupProperty
	{

		public string Name { get; set; }
		public string Description { get; set; }
		public string Value { get; set; }
		public string ModID { get; set; }
		public string PropertyID { get; set; }
		public string UID { get; set; }

		public bool IsValueValid(GroupsAPI gapi, string groupName, string newValue)
		{
			return gapi.Group.GetGroupTitles(groupName).ToList().Find(title => title.Equals(newValue)) != null;
		}
	}
}
