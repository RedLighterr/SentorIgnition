using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sentor.Services
{
	public class AssistantService
	{
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;
		private readonly string _assistantId;
		private string _threadId = "";

		public AssistantService(string apiKey, string assistantId)
		{
			_apiKey = apiKey;
			_assistantId = assistantId;
			_httpClient = new HttpClient();
			
			var config = Config.Load("config.json");
			_threadId = config.ThreadID;

			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
			_httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
		}

		public async Task<bool> CreateThreadAsync()
		{
			try
			{
				var response = await _httpClient.PostAsync("https://api.openai.com/v1/threads", null);
				var responseBody = await response.Content.ReadAsStringAsync();

				if (!response.IsSuccessStatusCode)
				{
					System.Diagnostics.Debug.WriteLine($"[CreateThread Error] {response.StatusCode} - {responseBody}");
					return false;
				}

				using var doc = JsonDocument.Parse(responseBody);
				_threadId = doc.RootElement.GetProperty("id").GetString() ?? "";
				return true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("[CreateThread Exception] " + ex.Message);
				return false;
			}
		}

		public async Task<bool> SendUserMessageAsync(string userMessage)
		{
			try
			{
				var messageContent = new
				{
					role = "user",
					content = userMessage
				};

				var content = new StringContent(JsonSerializer.Serialize(messageContent), Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync(
					$"https://api.openai.com/v1/threads/{_threadId}/messages", content);

				if (!response.IsSuccessStatusCode)
				{
					var error = await response.Content.ReadAsStringAsync();
					System.Diagnostics.Debug.WriteLine($"[Message Error] {response.StatusCode} - {error}");
					return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("[SendUserMessage Exception] " + ex.Message);
				return false;
			}
		}

		public async Task<string?> RunAssistantAndGetReplyAsync()
		{
			try
			{
				// Run başlat
				var runBody = new { assistant_id = _assistantId };
				var runContent = new StringContent(JsonSerializer.Serialize(runBody), Encoding.UTF8, "application/json");

				var runResponse = await _httpClient.PostAsync(
					$"https://api.openai.com/v1/threads/{_threadId}/runs", runContent);

				var runJson = await runResponse.Content.ReadAsStringAsync();

				if (!runResponse.IsSuccessStatusCode)
				{
					System.Diagnostics.Debug.WriteLine($"[Run Start Error] {runResponse.StatusCode} - {runJson}");
					return null;
				}

				var runId = JsonDocument.Parse(runJson).RootElement.GetProperty("id").GetString();

				// Durum bekle
				string status = "queued";
				int retries = 0;

				while (status != "completed" && retries++ < 30)
				{
					await Task.Delay(1000);
					var checkResponse = await _httpClient.GetAsync(
						$"https://api.openai.com/v1/threads/{_threadId}/runs/{runId}");

					var checkJson = await checkResponse.Content.ReadAsStringAsync();
					using var doc = JsonDocument.Parse(checkJson);
					status = doc.RootElement.GetProperty("status").GetString() ?? "unknown";

					if (status == "failed")
					{
						System.Diagnostics.Debug.WriteLine("[Run Failed]");
						return null;
					}
				}

				if (status != "completed")
				{
					System.Diagnostics.Debug.WriteLine("[Run Timeout]");
					return null;
				}

				// Mesajları getir
				var msgRes = await _httpClient.GetAsync($"https://api.openai.com/v1/threads/{_threadId}/messages");
				var msgJson = await msgRes.Content.ReadAsStringAsync();
				using var msgDoc = JsonDocument.Parse(msgJson);
				var data = msgDoc.RootElement.GetProperty("data");

				foreach (var message in data.EnumerateArray())
				{
					if (message.GetProperty("role").GetString() == "assistant")
					{
						return message
							.GetProperty("content")[0]
							.GetProperty("text")
							.GetProperty("value")
							.GetString();
					}
				}

				return "[Asistan cevap veremedi.]";
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("[Run Exception] " + ex.Message);
				return null;
			}
		}

		public string GetThreadId() => _threadId;
	}
}
