using Microsoft.Extensions.Configuration;
using Otus.Chat.Model;
using System.IO;

namespace Otus.Chat.Server
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			IConfiguration configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddCommandLine(args)
				.Build();

			var tcpConnection = new TcpConnectionOption();
			configuration.GetSection("TcpConnection").Bind(tcpConnection);

			using var server = new Server(tcpConnection);
			server.Start();
		}
	}
}
