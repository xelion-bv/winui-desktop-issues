#nullable enable
using Microsoft.UI.Xaml.Controls;
using System;
using Xelion.Controls;

namespace Xelion.Features.MainWindow.Desktop
{
    public static class DesktopSurface
    {
        public static PrimaryWindow Window { get; } = new PrimaryWindow();

        public static Canvas? Canvas { get; set; } // The canvas to put the floating window in


        /// <summary>
        /// Show the control or page of the specified type in a mdi window.
        /// </summary>
        internal static void Show(Type viewType)
        {
            if (!viewType.IsSubclassOf(typeof(UserControl)))
                return;

            var content = Activator.CreateInstance(viewType) as UserControl;
            if (content != null)
                ShowControl(content);
        }

        public static void ShowControl(UserControl control)
        {
            Window.OpenNewTab(control);
        }

        /// <summary>
        /// Hide either the primary window or the phone window
        /// </summary>
        public static void HideMdiWindow(MdiWindow window)
        {
            if (Canvas == null)
                return;

            Canvas.Children.Remove(window);
        }

        internal static void CloseAllViews(bool reopenAtStartup)
        {
            Window.CleanupAndCloseAll();
        }

        /// <summary>
        /// Close the given control from either the window or from a pane
        /// </summary>
        internal static void CleanupAndClose(UserControl? control)
        {
            if (control == null)
                return;
            Window.CleanupAndClose(control);
        }
    }
}
