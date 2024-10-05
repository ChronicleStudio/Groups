using System;

namespace Groups.API.Exceptions
{
	public class InvalidPlayerUID : Exception
	{
		public InvalidPlayerUID() : base() { }
		public InvalidPlayerUID(string message) : base(message) { }
	}
}
