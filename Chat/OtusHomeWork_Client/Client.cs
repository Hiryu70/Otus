using System;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Otus.Chat.Model;

namespace Otus.Chat.Client
{
	/// <summary>
	/// Клиент.
	/// </summary>
	public class Client : IDisposable
	{
		private readonly TcpConnectionOption _tcpConnection;
		private string _userName;
		private TcpClient _client;
		private NetworkStream _stream;

		public Client(TcpConnectionOption tcpConnection)
		{
			_tcpConnection = tcpConnection;
			ReadUserName();
		}


		/// <summary>
		/// Подключение к серверу
		/// </summary>
		public void Connect()
		{
			try
			{
				_client = new TcpClient();
				_client.Connect(_tcpConnection.Host, _tcpConnection.Port);
				_stream = _client.GetStream();

				var formatter = new BinaryFormatter();
				SendSingleMessage(formatter, _userName);

				var receiveThread = new Thread(() => ReceiveMessage(formatter));
				receiveThread.Start();

				SendMessages(formatter);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			_stream?.Close();
			_client?.Close();
			Environment.Exit(0);
		}

		/// <summary>
		/// Отправка сообщений
		/// </summary>
		private void SendMessages(IFormatter formatter)
		{
			Console.WriteLine($"Добро пожаловать, {_userName}");
			Console.WriteLine("Введите сообщение: ");

			while (true)
			{
				SendSingleMessage(formatter, Console.ReadLine());
			}
		}

		/// <summary>
		/// Получение сообщений
		/// </summary>
		private void ReceiveMessage(IFormatter formatter)
		{
			while (true)
			{
				try
				{
					var message = (Message)formatter.Deserialize(_stream);

					switch (message.Status)
					{
						case Status.Ok:
							Console.WriteLine(message.Text);
							break;
						case Status.Error:
							Console.WriteLine(message.Text);
							throw new SocketException();
					}
				}
				catch
				{
					Console.WriteLine("Подключение прервано!");
					Console.ReadLine();
					Dispose();
				}
			}
		}

		/// <summary>
		/// Получение имени пользователя
		/// </summary>
		private void ReadUserName()
		{
			Console.Write("Введите свое имя: ");
			_userName = Console.ReadLine();

			while (string.IsNullOrEmpty(_userName))
			{
				Console.Write("Имя не может быть пустым. Введите другое имя:");
				_userName = Console.ReadLine();
			}
		}

		/// <summary>
		/// Отправить сообщение на сервер
		/// </summary>
		private void SendSingleMessage(IFormatter formatter, string text)
		{
			var message = new Message
			{
				Text = text,
				Status = Status.Ok
			};

			formatter.Serialize(_stream, message);
		}
	}
}
