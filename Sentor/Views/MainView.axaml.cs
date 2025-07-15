using Avalonia.Controls;
using Avalonia.Interactivity;
using Sentor.ViewModels;

namespace Sentor.Views
{
	public partial class MainView : UserControl
	{
		public MainView()
		{
			InitializeComponent();
		}

		private async void TextBox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
		{
			MainViewModel viewModel = new MainViewModel();
			await viewModel.SendAsync();
		}
	}
}
