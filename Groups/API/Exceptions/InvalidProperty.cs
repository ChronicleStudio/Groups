using System;

namespace Groups.API.Exceptions
{
	internal class InvalidProperty : Exception
	{
		public InvalidProperty() : base() { }
		public InvalidProperty(string message) : base(message) { }
	}
}
