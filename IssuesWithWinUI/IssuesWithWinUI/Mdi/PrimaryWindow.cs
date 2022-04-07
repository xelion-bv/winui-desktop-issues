#nullable enable
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.System;
using Xelion.Controls;

namespace Xelion.Features.MainWindow.Desktop
{
    /// <summary>
    /// Manages the primary content window and the associated tabs (breadcrumbs)
    /// </summary>
    public class PrimaryWindow
    {
        private MdiWindow? _window = null;

        public ObservableCollection<MainTab> MainTabs { get; } = new ObservableCollection<MainTab>();


        /// <summary>
        /// Check if the element is in a window and cleanup and close the window and its contents.
        /// </summary>
        public void CleanupAndClose(UserControl? control)
        {
            if (control == null)
                return;

            // Get the tab index if this element is in a tab
            var tabIndex = -1;
            for (int i = 0; i < MainTabs.Count; i++)
            {
                if (MainTabs[i].View == control)
                {
                    tabIndex = i;
                    break;
                }
            }

            //
            // Remove the tab and (if it is displayed) the window
            if (tabIndex >= 0)
            {
                CleanupAndCloseTab(MainTabs[tabIndex]);
            }
        }

        public void RemoveTabItem(UserControl? control)
        {
            if (control == null)
                return;

            var tab = MainTabs.FirstOrDefault(t => t.View == control);

            if (tab != null)
            {
                MainTabs.Remove(tab);
            }
        }


        /// <summary>
        /// Adds a new tab to the list of tabs, and select it.
        /// Display the content in the primary mdi window.
        /// </summary>
        public void OpenNewTab(UserControl control)
        {
            // Show the tab contents
            var window = UseWindow();
            if (window == null)
                return;

            window.Show(control, null);

            RemoveEarlierNonFormTab();

            // Unselect other tabs
            foreach (var tab in MainTabs)
                tab.Selected = false;

            var newTab = new MainTab { View = control, Selected = true };
            // Add a new tab and select it.
            MainTabs.Add(newTab);
        }

        private void RemoveEarlierNonFormTab()
        {
            int maxNonFormFloaties = 50;

            var limit = MemoryManager.AppMemoryUsageLimit;
            var usage = MemoryManager.AppMemoryUsage / (double)limit;
            if (usage > 0.8)
            {
                maxNonFormFloaties = 1;
            }

            var nonFormTabs = MainTabs
                .Where(t => t is MainTab tab)
                .ToList();

            if (nonFormTabs.Count >= maxNonFormFloaties)
            {
                var tabToClose = nonFormTabs[0];
               
                MainTabs.Remove(tabToClose);
            }
        }


        public bool UnselectTabItem(UserControl? control)
        {
            if (control == null)
                return false;

            var tab = MainTabs.FirstOrDefault(t => t.View == control);

            if (tab != null)
            {
                tab.Selected = false;
                return true;
            }

            return false;
        }


        

        /// <summary>
        /// Close the tab, and if the tab is selected, also close the associated mdi window
        /// </summary>
        /// <param name="tab"></param>
        public void CleanupAndCloseTab(MainTab tab)
        {
            //
            // If tab is opened in the window, close the window.
            // This will also close the tab item.
            if (_window != null && _window.Content == tab.View)
            {
                _window.CleanupAndClose();
                return;
            }

            //
            // Cleanup the view and remove the tab.
           
            MainTabs.Remove(tab);
        }

        /// <summary>
        /// Select a tab and show its contents 
        /// </summary>
        public void SelectTab(MainTab tab)
        {
            if (!MainTabs.Contains(tab))
                return; // we can't select a tab that does not exist

            if (!(tab.View is UserControl element))
                return; // we can't select a tab that has no valid view

            var selectedTab = MainTabs.FirstOrDefault(tab => tab.Selected);
            if (selectedTab == tab)
                return; // we can't select a tab that is already selected

            // make sure the content is not attached to the current tab float
            if (_window != null)
                _window.Content = null;

            // Unselect the currently selected tab
            if (selectedTab != null)
            {
                selectedTab.Selected = false;
            }

            tab.Selected = true;

            var window = UseWindow();
            if (window == null)
                return;

            window.Show(element);
        }

        /// <summary>
        /// Use a floating window that is used to put the tab content in
        /// </summary>
        private MdiWindow? UseWindow()
        {
            var canvas = DesktopSurface.Canvas;

            //
            // Do not return a view if the canvas has not a valid width/height
            if (canvas == null || canvas.ActualWidth < 1 || canvas.ActualHeight < 1)
            {
                return null;
            }

            //
            // Put the tab float in the canvas if not already there
            if (_window == null)
            {
                _window = new MdiWindow { Content = null };
                _window.DefaultPositionAndDimensions(canvas.ActualWidth, canvas.ActualHeight, 700, 700);
            }

            if (!canvas.Children.Contains(_window))
                canvas.Children.Add(_window);

            return _window;
        }

        internal void CleanupAndCloseAll()
        {
     

            MainTabs.Clear();

            // Close the MDI window
            if (_window != null)
                _window.CleanupAndClose();
        }
    }

}
