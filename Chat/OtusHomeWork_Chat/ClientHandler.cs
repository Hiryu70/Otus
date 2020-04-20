using Otus.Chat.Model;
using Otus.Chat.Server.Events;
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace Otus.Chat.Server
{
	/// <summary>
	/// Обработчик соединения с клиентом.
	/// </summary>
	public class ClientHandler : IDisposable
	{
		private readonly BinaryFormatter _formatter = new BinaryFormatter();
		private readonly TcpClient _client;
		private NetworkStream _stream;

		public ClientHandler(TcpClient tcpClient)
		{
			_client = tcpClient;
		}

		/// <summary>
		/// Идентификатор клиента.
		/// </summary>
		public string Id { get; } = Guid.NewGuid().ToString();

		/// <summary>
		/// Имя пользователя.
		/// </summary>
		public string UserName { get; private set; }

		/// <summary>
		/// Подключен новый пользователь.
		/// </summary>
		public event MessageEventHandler NewUser;

		/// <summary>
		/// Получения новое сообщение.
		/// </summary>
		public event MessageEventHandler NewMessage;

		/// <summary>
		/// Подключение разорвано.
		/// </summary>
		public event ErrorEventHandler ConnectionLost;

		/// <summary>
		/// Начать обработку входящих сообщений клиента.
		/// </summary>
		public void Process()
		{
			try
			{
				_stream = _client.GetStream();

				var initialMessage = (Message)_formatter.Deserialize(_stream);
				UserName = initialMessage.Text;

				var eventArgs = new MessageEventArgs
				{
					Message = initialMessage
				};
				NewUser?.Invoke(this, eventArgs);

				while (true)
				{
					var message = (Message)_formatter.Deserialize(_stream);
					eventArgs = new MessageEventArgs
					{
						Message = message
					};
					NewMessage?.Invoke(this, eventArgs);
				}
			}
			catch (Exception e)
			{
				var eventArgs = new ErrorEventArgs(e);
				ConnectionLost?.Invoke(this, eventArgs);
			}
			finally
			{
				Dispose();
			}
		}

		/// <summary>
		/// Отправить сообщение.
		/// </summary>
		/// <param name="message">Сообщение.</param>
		public void SendMessage(Message message)
		{
			_formatter.Serialize(_stream, message);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_stream?.Close();
			_client?.Close();
		}
	}
}
