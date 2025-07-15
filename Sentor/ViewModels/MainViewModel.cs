using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sentor.Models;
using Sentor.Services;

namespace Sentor.ViewModels
{
	public partial class MainViewModel : INotifyPropertyChanged
	{
		public ObservableCollection<ChatMessage> Messages { get; } = new();

		private AssistantService _assistant;

		private string _userInput = string.Empty;
		public string UserInput
		{
			get => _userInput;
			set
			{
				if (_userInput != value)
				{
					_userInput = value;
					OnPropertyChanged();
				}
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string? name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

		public MainViewModel()
		{
			var config = Config.Load("config.json");
			_assistant = new AssistantService(config.OpenAiApiKey, config.AssitantID);
		}

		[RelayCommand]
		public async Task SendAsync()
		{
			if (string.IsNullOrWhiteSpace(UserInput))
				return;

			var userMessage = new ChatMessage { Text = UserInput.Trim(), IsUser = true };
			Messages.Add(userMessage);

			var input = UserInput.Trim();
			UserInput = "";

			var config = Config.Load("config.json");
			var service = new AssistantService(config.OpenAiApiKey, config.AssitantID);

			//await service.CreateThreadAsync();
			await service.SendUserMessageAsync(input);
			var reply = await service.RunAssistantAndGetReplyAsync();

			Messages.Add(new ChatMessage
			{
				Text = reply ?? "[Asistan cevap veremedi.]",
				IsUser = false
			});
		}
	}
}