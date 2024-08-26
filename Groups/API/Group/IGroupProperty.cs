namespace Groups.API.Group
{
	public interface IGroupProperty
	{
		/// <summary>
		/// The modid for the mod registering the property.
		/// </summary>
		public string ModID { get; set; }
		/// <summary>
		/// The Property ID provided by the mod adding the property, this should be a unique value within the mod itself.
		/// </summary>
		public string PropertyID { get; set; }
		/// <summary>
		/// A generated Unique ID for the property provided once the property is registerd. This is only used to lookup the value using the API.
		/// </summary>
		public string UID { get; set; }
		/// <summary>
		/// The Name that should be displayed on the GUI.
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// The discription that should be displayed on the tool tip when the player hovers over the property.
		/// </summary>
		public string Description { get; set; }
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
		public bool IsValueValid(GroupsAPI gapi, string groupName, string newValue);
	}
}
