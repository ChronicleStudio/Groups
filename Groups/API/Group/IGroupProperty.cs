using ProtoBuf;

namespace Groups.API.Group
{
	[ProtoContract()]
	public interface IGroupProperty
	{
		/// <summary>
		/// The modid for the mod registering the property.
		/// </summary>
		public static readonly string ModID;
		/// <summary>
		/// The Property ID provided by the mod adding the property, this should be a unique value within the mod itself.
		/// </summary>
		public static readonly string PropertyID;
		/// <summary>
		/// A generated Unique ID for the property provided once the property is registerd. This is only used to lookup the value using the API.
		/// </summary>
		public int UID { get; set; }
		/// <summary>
		/// The Name that should be displayed on the GUI.
		/// </summary>
		public static readonly string Name;
		/// <summary>
		/// The discription that should be displayed on the tool tip when the player hovers over the property.
		/// </summary>
		public static readonly string Description;
		/// <summary>
		/// The Value of this property that would be set once the player configures their group, this value is returned through the API.
		/// </summary>
		public string Value { get; set; }
		/// <summary>
		/// This method allows for checking if the property is valid within te gui before the GUI Saves the value. The GUI will call this function and expects a true if the value is valid and false if it not.
		/// </summary>
		/// <param name="gapi">The isntance of GroupsAPI for use in looking up up Group Titles ect.</param>
		/// <param name="groupName">The name of the group that is changing the value.</param>
		/// <param name="newValue">The proposed new value for the property.</param>
		/// <returns>The true or false value for weither or not the imput is valid.</returns>
		public bool IsValueValid(GroupsAPI gapi, int groupUID, string newValue);

		public string GetDefaultValue(GroupsAPI gapi, int groupUID);
	}

}
