using Microsoft.Toolkit.Uwp.Extensions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IssuesWithWinUI
{
    public static class UiUtil
    {
        public static Task RunOnUiThread(Action action)
        {
            try
            {
                var dispatcherQueue = MainWindow.MainDispatcherQueue;
                if (dispatcherQueue == null)
                {
                    action();
                    return Task.CompletedTask;
                }

                if (dispatcherQueue.HasThreadAccess)
                {
                    action();
                    return Task.CompletedTask;
                }
                else
                {
                    return dispatcherQueue.EnqueueAsync(action
                               , Microsoft.System.DispatcherQueuePriority.Normal);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return Task.CompletedTask;
            }
        }
    }
}
