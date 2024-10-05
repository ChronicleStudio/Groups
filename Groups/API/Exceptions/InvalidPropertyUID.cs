using System;

namespace Groups.API.Exceptions
{
	public class InvalidPropertyUID : Exception
	{
		public InvalidPropertyUID() : base() { }
		public InvalidPropertyUID(string message) : base(message) { }
	}
}
