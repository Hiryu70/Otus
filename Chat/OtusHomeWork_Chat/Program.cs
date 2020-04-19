using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Otus.Chat.Model;

namespace Otus.Chat.Server
{
	public static class Program
	{
		private static ServerObject _server;
		private static Thread _listenThread;
		public static void Main(string[] args)
		{
			IConfiguration configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddCommandLine(args)
				.Build();

			var tcpConnection = new TcpConnectionOption();
			configuration.GetSection("TcpConnection").Bind(tcpConnection);

			try
			{
				_server = new ServerObject(tcpConnection);
				_listenThread = new Thread(_server.Listen);
				_listenThread.Start();
			}
			catch (Exception ex)
			{
				_server.Disconnect();
				Console.WriteLine(ex.Message);
			}
		}
	}
}
