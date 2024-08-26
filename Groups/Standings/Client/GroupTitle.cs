using ProtoBuf;

namespace Groups.Standings.Client
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class GroupTitle
	{
		public string name;
		public sbyte min;
		public sbyte maz;
	}
}
