using System;

namespace Groups.API.Exceptions
{
	public class InvalidGroupUID : Exception
	{
		public InvalidGroupUID() : base() { }
		public InvalidGroupUID(string message) : base(message) { }
	}
}
