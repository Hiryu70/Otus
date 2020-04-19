using System;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Otus.Chat.Model;

namespace Otus.Chat.Server
{
	public class ClientObject
	{
		private readonly BinaryFormatter _formatter;
		private string _userName;
		private readonly TcpClient _client;
		private readonly ServerObject _server;

		public ClientObject(TcpClient tcpClient, ServerObject serverObject)
		{
			Id = Guid.NewGuid().ToString();
			_client = tcpClient;
			_server = serverObject;
			_formatter = new BinaryFormatter();
			serverObject.AddConnection(this);
		}

		public string Id { get; }
		public NetworkStream Stream { get; private set; }

		public void Process()
		{
			try
			{
				Stream = _client.GetStream();
				var message = GetMessage();
				_userName = message;

				message = $"{_userName} вошел в чат";
				_server.BroadcastMessage(message, Id);
				Console.WriteLine(message);
				
				while (true)
				{
					message = GetMessage();
					message = $"{_userName}: {message}";
					Console.WriteLine(message);
					_server.BroadcastMessage(message, Id);
				}
			}
			catch (SerializationException e)
			{
				_server.ReturnErrorMessage(e.Message, Id);
				Console.WriteLine(e.Message);

				SendMessageAfterErrorForAll();
			}
			
			catch (Exception e)
			{
				_server.ReturnErrorMessage(e.Message, Id);
				Console.WriteLine(e.Message);

				SendMessageAfterErrorForAll();
			}
			finally
			{
				_server.RemoveConnection(Id);
				Close();
			}
		}

		/// <summary>
		/// Закрытие подключения
		/// </summary>
		public void Close()
		{
			Stream?.Close();
			_client?.Close();
		}

		private void SendMessageAfterErrorForAll()
		{
			var messageForOtherClient = $"{_userName}: покинул чат";
			Console.WriteLine(messageForOtherClient);
			_server.BroadcastMessage(messageForOtherClient, Id);
		}

		/// <summary>
		/// Чтение входящего сообщения и преобразование в строку
		/// </summary>
		/// <returns></returns>
		private string GetMessage()
		{
			try
			{
				var messageObj = (Message)_formatter.Deserialize(Stream);
				return messageObj.Text;
			}
			catch
			{
				throw new SerializationException("Ошибка десериализации сообщения");
			}
		}
	}
}