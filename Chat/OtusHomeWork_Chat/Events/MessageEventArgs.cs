using System;
using Otus.Chat.Model;

namespace Otus.Chat.Server.Events
{
	public class MessageEventArgs : EventArgs
	{
		public Message Message { get; set; }
	}
}