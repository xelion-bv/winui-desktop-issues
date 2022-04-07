using Microsoft.UI.Input;
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
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.System;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IssuesWithWinUI3;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
    }



    private async void OnPaste(object sender, TextControlPasteEventArgs e)
    {
        e.Handled = true;

        try
        {
            var dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                var text = await dataPackageView.GetTextAsync();

                await Task.Delay(2);


                (sender as TextBox).Text = text;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        {
            if (e.OriginalSource is not TextBox textBox)
                return;

            try
            {
                if (e.Key == VirtualKey.Enter)
                {
                    if (IsShiftDown)
                    {
                        InsertNewLinePlaceholder();
                    }
                    
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
            }

            //
            // Local function for inserting a new line placholder (\r)
            // into the text.
            void InsertNewLinePlaceholder()
            {
                try
                {

                    // NOTE: A textbox does not accept \n characters.
                    //       We need to add these later on in the process!
                    var startPosition = textBox.SelectionStart;
                    var nlPlaceholder = "\r";
                    textBox.Text = textBox.Text.Insert(startPosition, nlPlaceholder);
                    textBox.SelectionStart = startPosition + 1;
                }
                catch
                {
                    // ignore edge cases with wrong selection start (should probably never occur)
                }
            }
        }
    }

    public static bool IsShiftDown
    {
        get
        {
            var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
            return (shiftState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }
    }

    private MediaPlayer _player;

    private void PlayMedia(object sender, RoutedEventArgs e)
    {

        _player = new MediaPlayer();
        
        _player.Source = MediaSource.CreateFromUri(
            new Uri("ms-appx:///Assets/testaudio.mp3"));

        _player.Play();


    }
}

