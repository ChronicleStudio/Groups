using ProtoBuf;

namespace Groups.GUI.Network
{
	internal class GroupNetwork
	{
	}

	internal class GroupUtilites
	{
		[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
		public class NetworkApiClientRequestData
		{
			public Requests message;
		}
		[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
		public class NetorkAPIClientRequestGroupChange
		{
			public ChangeType request;
			public int GroupUID { get; set; }
			public string GroupName { get; set; }
			public string[] GroupTitles { get; set; }
			public bool[] GroupAutoProgress { get; set; }

		}
		[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
		public class NetorkAPIClientRequestPropertyChange
		{

			public int GroupUID { get; set; }
			public int PropertyUID { get; set; }
			public int NewValue { get; set; }
		}

	}
	public class NetworkAPIServerResponse
	{

	}
	public enum ChangeType
	{
		NewGroup,
		GroupTitles,
		GroupAutoProgress,
	}
}
