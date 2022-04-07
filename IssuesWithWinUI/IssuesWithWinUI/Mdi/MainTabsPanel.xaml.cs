using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Xelion.Features.MainWindow.Desktop
{
    public sealed partial class MainTabsPanel : UserControl
    {
        public MainTabsPanel()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            DesktopSurface.Window.MainTabs.CollectionChanged += OnTabCollectionChanged;
        }

        private async void OnTabCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateScrollButtons();

            await Task.Delay(200); // wait for UI to settle

            if (scrollViewer.ScrollableWidth > 0)
            {
                scrollViewer.ChangeView(scrollViewer.ScrollableWidth, null, null);
            }

        }

        private void ViewInfoClick(object sender, RoutedEventArgs e)
        {

            if (sender is HyperlinkButton button)
            {
                if (button.Tag is MainTab tab)
                {
                    DesktopSurface.Window.SelectTab(tab);
                }
            }
        }

        private void CloseTabClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag is MainTab tab)
                {
                    DesktopSurface.Window.CleanupAndCloseTab(tab);
                }
            }

        }

        private void SelectTabNumber(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            int tabToSelect = 0;

            switch (sender.Key)
            {
                case VirtualKey.Number1:
                    tabToSelect = 0;
                    break;
                case VirtualKey.Number2:
                    tabToSelect = 1;
                    break;
                case VirtualKey.Number3:
                    tabToSelect = 2;
                    break;
                case VirtualKey.Number4:
                    tabToSelect = 3;
                    break;
                case VirtualKey.Number5:
                    tabToSelect = 4;
                    break;
                case VirtualKey.Number6:
                    tabToSelect = 5;
                    break;
                case VirtualKey.Number7:
                    tabToSelect = 6;
                    break;
                case VirtualKey.Number8:
                    tabToSelect = 7;
                    break;
                case VirtualKey.Number9:
                    // Select the last tab
                    tabToSelect = DesktopSurface.Window.MainTabs.Count - 1;
                    break;
            }

            // Only select the tab if it is in the list
            if (tabToSelect < DesktopSurface.Window.MainTabs.Count)
            {
                DesktopSurface.Window.SelectTab(DesktopSurface.Window.MainTabs[tabToSelect]);
            }
            args.Handled = true;
        }

        private void CloseSelectedTab(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            var selectedTab = DesktopSurface.Window.MainTabs.FirstOrDefault(t => t.Selected);
            if (selectedTab != null)
            {
                DesktopSurface.Window.CleanupAndCloseTab(selectedTab);
            }
        }

        private void ScrollLeft(object sender, RoutedEventArgs e)
        {
            var changed = scrollViewer.ChangeView(scrollViewer.HorizontalOffset - 160, null, null);

        }

        private void ScrollRight(object sender, RoutedEventArgs e)
        {
            var changed = scrollViewer.ChangeView(scrollViewer.HorizontalOffset + 160, null, null);

        }


        
        private void UpdateScrollButtons()
        {   
            var scrollableWidth = scrollViewer.ScrollableWidth; // distance that can be scrolled

            if (scrollableWidth > 0)
            {
                // we can scroll to the left
                leftButton.Visibility = Visibility.Visible;
                rightButton.Visibility = Visibility.Visible;
            }
            else
            {
                leftButton.Visibility = Visibility.Collapsed;
                rightButton.Visibility = Visibility.Collapsed;
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateScrollButtons();
        }
    }
}
