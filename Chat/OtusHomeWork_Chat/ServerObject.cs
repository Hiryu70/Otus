using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Otus.Chat.Model;

namespace Otus.Chat.Server
{
	public class ServerObject
	{
		private readonly TcpConnectionOption _tcpConnectionOption;
		private static TcpListener _tcpListener;
		private readonly List<ClientObject> _clients = new List<ClientObject>();
		private readonly BinaryFormatter _formatter = new BinaryFormatter();

		public ServerObject(TcpConnectionOption tcpConnectionOption)
		{
			_tcpConnectionOption = tcpConnectionOption;
		}

		protected internal void AddConnection(ClientObject clientObject)
		{
			_clients.Add(clientObject);
		}

		protected internal void RemoveConnection(string id)
		{
			ClientObject client = _clients.FirstOrDefault(c => c.Id == id);
			if (client != null)
			{
				_clients.Remove(client);
			}
		}

		/// <summary>
		/// Прослушивание входящих подключений
		/// </summary>
		protected internal void Listen()
		{
			try
			{
				_tcpListener = new TcpListener(IPAddress.Any, _tcpConnectionOption.Port);
				_tcpListener.Start();
				Console.WriteLine("Сервер запущен. Ожидание подключений...");

				while (true)
				{
					TcpClient tcpClient = _tcpListener.AcceptTcpClient();

					var clientObject = new ClientObject(tcpClient, this);
					var clientThread = new Thread(clientObject.Process);
					clientThread.Start();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Disconnect();
			}
		}

		/// <summary>
		/// Трансляция сообщения подключенным клиентам
		/// </summary>
		/// <param name="message"></param>
		/// <param name="id"></param>
		protected internal void BroadcastMessage(string message, string id)
		{
			var messageObj = new Message
			{
				Text = message,
				Status = Status.Ok
			};

			foreach (ClientObject client in _clients)
			{
				if (client.Id != id)
				{
					_formatter.Serialize(client.Stream, messageObj);
				}
			}
		}

		/// <summary>
		/// Возврат ошибки клиенту
		/// </summary>
		/// <param name="errorText"></param>
		/// <param name="id"></param>
		protected internal void ReturnErrorMessage(string errorText, string id)
		{
			Message messageObj = new Message();
			messageObj.Text = errorText;
			messageObj.Status = Status.Error;

			_formatter.Serialize(_clients.First(c => c.Id == id).Stream, messageObj);
		}

		/// <summary>
		/// Отключение всех клиентов
		/// </summary>
		protected internal void Disconnect()
		{
			_tcpListener.Stop();
			foreach (ClientObject client in _clients)
			{
				client.Close();
			}
			Environment.Exit(0);
		}
	}
}
