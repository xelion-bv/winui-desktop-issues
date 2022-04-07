using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Text;

namespace Xelion.Features.MainWindow.Desktop
{
    /// <summary>
    /// A tab with header content that can be displayed in a tab header
    /// and with content that can be placed in a floating window 
    /// </summary>
    public class MainTab : ObservableObject
    {
        private bool _selected;
        private UserControl _view;
        public UserControl View
        {
            get => _view;
            set
            {
                SetProperty(ref _view, value);
                OnPropertyChanged(nameof(Label));
            }
        }

        public bool Selected
        {
            get => _selected;
            set
            {
                SetProperty(ref _selected, value);
                OnPropertyChanged(nameof(FontWeight));
            }
        }

        public string Label => _view.GetType().Name;

        public FontWeight FontWeight => Selected ? FontWeights.Bold : FontWeights.Normal;
    }
}