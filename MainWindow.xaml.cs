using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;


namespace BemisAutoTyper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : System.Windows.Window
    {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const int KEYEVENTF_KEYDOWN = 0x0000;
        private const int KEYEVENTF_KEYUP = 0x0002;
        public MainWindow()
        {
            InitializeComponent();
            KeyDown += MainWindow_KeyDown;
        }
        private void TextBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                lineNumbersScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
            }
        }


        private void IntervalTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Validate the input when focus is lost
            System.Windows.Controls.TextBox textBox = (System.Windows.Controls.TextBox)sender;
            string newText = textBox.Text;
            switch (IsValidInt(newText))
            {
                case "match":
                    break;
                case "empty":
                    textBox.Text = "10";
                    break;
                case "1000+":
                    textBox.Text = "1000";
                    break;
                case "1-":
                    textBox.Text = "1";
                    break;

            }
        }

        private string IsValidInt(string text)
        {
            // Use regex to validate the input as a float number between 0 and 1000
            Regex regex = new Regex(@"^(1000|[1-9]\d{0,2}|1)$");
            if (regex.IsMatch(text)) {
                return "match";
            }
            else if (string.IsNullOrEmpty(text)){
                return "empty";
            }else{
                if (int.Parse(text) > 1000)
                {
                    return "1000+";
                }else if (int.Parse(text) < 0)
                {
                    return "1-";
                }else
                {
                    return "empty";
                }
            };
        }

        private void TurboModeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            IntervalTextBox.IsEnabled = false;
        }
        private void TurboModeCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            IntervalTextBox.IsEnabled = true;
        }
        private async void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the F8 key is pressed
            if (e.Key == Key.F8)
            {
                // Get the text from the TextBox
                string textToType = DataTextBox.Text;
                int interval;
                if (!int.TryParse(IntervalTextBox.Text, out interval))
                {
                    // Handle invalid input from IntervalTextBox
                    MessageBox.Show("Invalid interval value.");
                    return;
                }

                // Start typing based on the specified settings
                await StartTypingAsync(textToType, interval, TurboModeCheckBox.IsChecked);
            }
        }

        // Inside your StartTyping method
        private async Task StartTypingAsync(string text, int interval, bool? turboMode)
        {
            // Check if turboMode is enabled
            if (turboMode ?? false)
            {
                // Type all characters instantly
                foreach (char c in text)
                {
                    // Convert character to upper case to ensure correct virtual key code
                    keybd_event((byte)char.ToUpper(c), 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero); // Key down
                    keybd_event((byte)char.ToUpper(c), 0, KEYEVENTF_KEYUP, UIntPtr.Zero);   // Key up
                }
            }
            else
            {
                // Type each character with the specified interval
                foreach (char c in text)
                {
                    // Convert character to upper case to ensure correct virtual key code
                    keybd_event((byte)char.ToUpper(c), 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero); // Key down
                    keybd_event((byte)char.ToUpper(c), 0, KEYEVENTF_KEYUP, UIntPtr.Zero);   // Key up
                    await Task.Delay(interval);
                }
            }
        }


    }
}
