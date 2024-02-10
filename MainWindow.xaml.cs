using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Diagnostics;


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

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public MainWindow()
        {
            InitializeComponent();
            KeyDown += MainWindow_KeyDown;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
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

                // Clear progress bar before typing
                TypeProgressbar.Value = 0;

                // Start typing based on the specified settings
                await StartTypingAsync(textToType, interval, TurboModeCheckBox.IsChecked);
            }
        }

        private async Task StartTypingAsync(string text, int interval, bool? turboMode)
        {
            int totalCharacters = text.Length;

            if (turboMode ?? false)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    // Update progress bar
                    Application.Current.Dispatcher.Invoke(() => TypeProgressbar.Value = (i + 1) * 100 / totalCharacters);

                    keybd_event((byte)char.ToUpper(text[i]), 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero); // Key down
                    keybd_event((byte)char.ToUpper(text[i]), 0, KEYEVENTF_KEYUP, UIntPtr.Zero);   // Key up
                }
            }
            else
            {
                for (int i = 0; i < text.Length; i++)
                {
                    // Update progress bar
                    Application.Current.Dispatcher.Invoke(() => TypeProgressbar.Value = (i + 1) * 100 / totalCharacters);

                    keybd_event((byte)char.ToUpper(text[i]), 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero); // Key down
                    keybd_event((byte)char.ToUpper(text[i]), 0, KEYEVENTF_KEYUP, UIntPtr.Zero);   // Key up

                    await Task.Delay(interval);
                }
            }

            // Ensure the progress bar reaches 100% after typing is completed
            Application.Current.Dispatcher.Invoke(() => TypeProgressbar.Value = 100);
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _hookID = SetHook(_proc);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                // Handle the key event here
                if (Keyboard.IsKeyDown(Key.LeftCtrl) && key == Key.F)
                {
                    MessageBox.Show("Ctrl + F pressed!");
                    return (IntPtr)1; // Prevent further processing of the key
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }




    }
}
