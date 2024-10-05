using Groups.API.Group;
using System.Collections.Generic;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Groups.API.IO
{
	internal class GroupSettingsIO
	{
		internal static List<GroupSettings> LoadGroups(ICoreServerAPI sapi)
		{
			byte[] data = CommonIO.ReadData(sapi, "Group");
			return data is null ? new() : SerializerUtil.Deserialize<List<GroupSettings>>(data);
		}
		internal static void SaveGroups(ICoreServerAPI sapi, List<GroupSettings> Groups)
		{
			Groups.ForEach(Group => Group.PackProperties());
			CommonIO.WriteModData(sapi, "Group", SerializerUtil.Serialize(Groups), jData: Groups);
		}
		/*

		internal List<GroupSettings> Groups;
		/// <summary>
		/// Dictionary &#60;string, Dictionary&#60;string, string&#62;&#62; where key for primary dictionary is the GroupID, and the second dictionary is string as {Property.ModID + Property.PropertyID} and the value is the value of the property for the specific Propery in a specific group.
		/// </summary>
		internal Dictionary<string, Dictionary<string, string>> GroupProperties;

		public static string LoadPropertyValue(ICoreServerAPI sapi, GroupSettings group, IGroupProperty newProperty)
		{
			TestData testData = new TestData() { test = "Test", test2 = 2, test3 = true };
			CommonIO.WriteModData(sapi, "Group", SerializerUtil.Serialize(testData), jData: testData);
			TestData td = SerializerUtil.Deserialize<TestData>(CommonIO.ReadData(sapi, "Group"));
			sapi.Logger.Warning($"Test: {(td?.test)}, Test2: {td?.test2}, Test3: {td?.test3}");
			return "";
		}
		public static void SavePropertyValue() { }
	}
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	internal class TestData
	{
		[JsonInclude]
		public string test;
		[JsonInclude]
		public int test2;
		[JsonInclude]
		public bool test3;

		public TestData() { }
		*/
	}
}
