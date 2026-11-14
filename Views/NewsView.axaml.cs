using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using AvaloniaAppCrypto.ViewModels;

namespace AvaloniaAppCrypto.Views
{
    public partial class NewsView : UserControl
    {
        private MainWindowViewModel? _viewModel;

        public NewsView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object? sender, System.EventArgs e)
        {
            if (_viewModel is not null)
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

            _viewModel = DataContext as MainWindowViewModel;

            if (_viewModel is not null)
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Detaydan listeye dönünce başlık ve arama kutusu görünsün diye
            // listeyi başa sar.
            if (e.PropertyName == nameof(MainWindowViewModel.IsNewsDetailOpen) &&
                _viewModel?.IsNewsDetailOpen == false)
            {
                NewsListScroll.Offset = new Vector(0, 0);
            }
        }
    }
}
