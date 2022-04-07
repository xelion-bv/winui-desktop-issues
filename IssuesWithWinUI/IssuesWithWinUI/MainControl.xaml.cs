using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Xelion.Features.MainWindow.Desktop;

namespace IssuesWithWinUI
{
    public sealed partial class MainControl : UserControl
    {
        public MainControl()
        {
            this.InitializeComponent();
            DesktopSurface.Canvas = canvas;
        }

    

        private void AddCounting(object sender, RoutedEventArgs e)
        {
            DesktopSurface.Show(typeof(DatabindingIssue));
        }

        private void AddItemsRepeater(object sender, RoutedEventArgs e)
        {
            DesktopSurface.Show(typeof(ItemsRepeaterIssue));
        }

        private void AddItemsControl(object sender, RoutedEventArgs e)
        {
            DesktopSurface.Show(typeof(ItemsControlWorksAsExpected));
        }
    }
}
