using Otus.Chat.Model;
using Otus.Chat.Server.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Otus.Chat.Server
{
	public class Server : IDisposable
	{
		private readonly TcpConnectionOption _tcpConnectionOption;
		private static TcpListener _tcpListener;
		private readonly List<ClientHandler> _clients = new List<ClientHandler>();

		public Server(TcpConnectionOption tcpConnectionOption)
		{
			_tcpConnectionOption = tcpConnectionOption;
		}

		/// <summary>
		/// Запуск сервера. Прослушивание входящих подключений
		/// </summary>
		public void Start()
		{
			try
			{
				_tcpListener = new TcpListener(IPAddress.Any, _tcpConnectionOption.Port);
				_tcpListener.Start();
				Console.WriteLine("Сервер запущен. Ожидание подключений...");

				while (true)
				{
					TcpClient tcpClient = _tcpListener.AcceptTcpClient();

					var client = new ClientHandler(tcpClient);
					client.NewUser += Client_NewUser;
					client.NewMessage += Client_NewMessage;
					client.ConnectionLost += Client_ConnectionLost;
					_clients.Add(client);

					var clientThread = new Thread(client.Process);
					clientThread.Start();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Dispose();
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_tcpListener.Stop();
			foreach (ClientHandler client in _clients)
			{
				client.Dispose();
			}
			Environment.Exit(0);
		}

		/// <summary>
		/// Трансляция сообщения подключенным клиентам
		/// </summary>
		/// <param name="text">Текст сообщения</param>
		/// <param name="id">Идентификатор отправителя</param>
		private void BroadcastMessage(string text, string id)
		{
			Console.WriteLine(text);

			var message = new Message
			{
				Text = text,
				Status = Status.Ok
			};

			foreach (ClientHandler client in _clients)
			{
				if (client.Id == id)
					continue;

				client.SendMessage(message);
			}
		}

		/// <summary>
		/// Обработка события подключения нового клиента
		/// </summary>
		/// <param name="sender">Клиент</param>
		/// <param name="e">Аргументы</param>
		private void Client_NewUser(object sender, MessageEventArgs e)
		{
			var client = (ClientHandler) sender;
			var message = $"{client.UserName} вошел в чат";

			BroadcastMessage(message, client.Id);
		}

		/// <summary>
		/// Обработка события получения нового сообщения от клиента
		/// </summary>
		/// <param name="sender">Клиент</param>
		/// <param name="e">Аргументы</param>
		private void Client_NewMessage(object sender, MessageEventArgs e)
		{
			var client = (ClientHandler)sender;
			var message = $"{client.UserName}: {e.Message.Text}";

			BroadcastMessage(message, client.Id);
		}

		/// <summary>
		/// Обработка события отключения клиента
		/// </summary>
		/// <param name="sender">Клиент</param>
		/// <param name="e">Аргументы</param>
		private void Client_ConnectionLost(object sender, ErrorEventArgs e)
		{
			var client = (ClientHandler)sender;
			var message = $"{client.UserName} покинул чат";

			_clients.Remove(client);

			BroadcastMessage(message, client.Id);
		}
	}
}
