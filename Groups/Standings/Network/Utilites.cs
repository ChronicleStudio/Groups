using ProtoBuf;

namespace Groups.Standings.Network
{
	internal class Utilites
	{
		[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
		public class NetworkApiClientRequest
		{
			public Requests message;
		}

		[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
		public class NetworkApiServerUpdate
		{
			public string response;
			public bool isFullDictionary;
			public bool isInvalidRequest;
			public byte[] StandingsDict;
		}
	}
	public enum Requests
	{
		FULL_DICTIONARY,
		RECENT_CHANGES,
	}
}
