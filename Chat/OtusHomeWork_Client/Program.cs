using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Otus.Chat.Model;

namespace Otus.Chat.Client
{
	internal static class Program
	{
		private static string _userName;
		private static TcpClient _client;
		private static NetworkStream _stream;
		private static BinaryFormatter _formatter;

		internal static void Main(string[] args)
		{
			IConfiguration configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddCommandLine(args)
				.Build();

			var tcpConnection = new TcpConnectionOption();
			configuration.GetSection("TcpConnection").Bind(tcpConnection);

			Console.Write("Введите свое имя: ");
			_userName = Console.ReadLine();
			_client = new TcpClient();
			_formatter = new BinaryFormatter();

			try
			{
				_client.Connect(tcpConnection.Host, tcpConnection.Port);
				_stream = _client.GetStream();

				var message = new Message
				{
					Text = _userName
				};
				_formatter.Serialize(_stream, message);


				var receiveThread = new Thread(ReceiveMessage);
				receiveThread.Start();

				Console.WriteLine($"Добро пожаловать, {_userName}");
				SendMessage();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			finally
			{
				Disconnect();
			}
		}

		/// <summary>
		/// Отправка сообщений
		/// </summary>
		private static void SendMessage()
		{
			Console.WriteLine("Введите сообщение: ");

			while (true)
			{
				var message = new Message
				{
					Text = Console.ReadLine(),
					Status = Status.Ok
				};

				_formatter.Serialize(_stream, message);
			}
		}

		/// <summary>
		/// Получение сообщений
		/// </summary>
		private static void ReceiveMessage()
		{
			while (true)
			{
				try
				{
					var message = (Message)_formatter.Deserialize(_stream);

					switch (message.Status)
					{
						case Status.Ok:
							Console.WriteLine(message.Text);
							break;
						case Status.Error:
							Console.WriteLine(message.Text);
							Console.ReadLine();
							Disconnect();
							break;
					}
				}
				catch
				{
					Console.WriteLine("Подключение прервано!");
					Console.ReadLine();
					Disconnect();
				}
			}
		}

		private static void Disconnect()
		{
			_stream?.Close();
			_client?.Close();
			Environment.Exit(0);
		}
	}
}
