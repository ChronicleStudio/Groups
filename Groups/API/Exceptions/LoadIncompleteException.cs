using System;

namespace Groups.API.Exceptions
{
	internal class LoadIncompleteException : Exception
	{
		public LoadIncompleteException() : base() { }
		public LoadIncompleteException(string message) : base(message) { }
	}
}
