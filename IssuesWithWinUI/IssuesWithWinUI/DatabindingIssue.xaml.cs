using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace IssuesWithWinUI
{
    public class ShowNumberViewModel :ObservableObject
    {
        private int _number;
        public int Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }
    }

    public sealed partial class DatabindingIssue : UserControl
    {
        private ObservableCollection<ShowNumberViewModel> _numberVms = new ObservableCollection<ShowNumberViewModel>();
        private DispatcherTimer _timer;


        public DatabindingIssue()
        {
            this.InitializeComponent();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        private void OnTimerTick(object sender, object e)
        {
            foreach(var numberVm in _numberVms)
            {
                UiUtil.RunOnUiThread(() =>
                {
                    numberVm.Number++;
                });
               
            }
        }

        
        private void AddCountingControl(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                UiUtil.RunOnUiThread(() =>
                {
                    _numberVms.Add(new ShowNumberViewModel());
                });
            });
        }

        private void RemoveCountingControl(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                UiUtil.RunOnUiThread(() =>
                {
                    if (_numberVms.Count > 0)
                    _numberVms.RemoveAt(_numberVms.Count-1);
                });
            });
        }
    }
}
