using System;

namespace Otus.Chat.Model
{
	/// <summary>
	/// Сообщение.
	/// </summary>
	[Serializable]
	public class Message
	{
		/// <summary>
		/// Статус сообщения
		/// </summary>
		public Status Status { get; set; }

		/// <summary>
		/// Текст сообщения
		/// </summary>
		public string Text { get; set; }
	}
}
