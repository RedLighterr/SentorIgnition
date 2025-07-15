using System.IO;
using System.Text.Json;

namespace Sentor
{
	public class Config
	{
		public string OpenAiApiKey { get; set; } = "";
		public string AssitantID { get; set; } = "";
		public string ThreadID { get; set; } = "";

		public static Config Load(string filePath)
		{
			var json = File.ReadAllText(filePath);
			return JsonSerializer.Deserialize<Config>(json) ?? new Config();
		}
	}
}
