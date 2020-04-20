using Microsoft.Extensions.Configuration;
using Otus.Chat.Model;
using System.IO;

namespace Otus.Chat.Client
{
	internal static class Program
	{
		internal static void Main(string[] args)
		{
			IConfiguration configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddCommandLine(args)
				.Build();

			var tcpConnection = new TcpConnectionOption();
			configuration.GetSection("TcpConnection").Bind(tcpConnection);

			using var client = new Client(tcpConnection);
			client.Connect();
		}
	}
}
