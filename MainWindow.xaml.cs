using Microsoft.Win32;
using System.IO;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BemisAutoTyper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : System.Windows.Window
    {
        // Import the SendInput function
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        // Input structure for SendInput function
        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint Type;
            public INPUTUNION Data;
        }

        // Input types
        const int INPUT_KEYBOARD = 1;

        // Keyboard input structure
        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort Vk;
            public ushort Scan;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        // Union for keyboard input structure
        [StructLayout(LayoutKind.Explicit)]
        struct INPUTUNION
        {
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        // Keyboard event flags
        const uint KEYEVENTF_KEYDOWN = 0x0000;
        const uint KEYEVENTF_KEYUP = 0x0002;
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
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the F8 key is pressed
            if (e.Key == Key.F8)
            {
                INPUT input = new INPUT();
                input.Type = INPUT_KEYBOARD;
                input.Data.ki.Vk = 0x41; // Virtual key code for "A"
                input.Data.ki.Flags = KEYEVENTF_KEYDOWN;                
                uint result1 = SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
                if (result1 == 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    MessageBox.Show($"SendInput failed with error code: {error}");
                }

                // Simulate key release
                input.Data.ki.Flags = KEYEVENTF_KEYUP;
                uint result2 = SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
                if (result2 == 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    MessageBox.Show($"SendInput failed with error code: {error}");
                }

                // Get the text from the TextBox
                //string textToType = DataTextBox.Text;

                // Simulate typing the text
                //SimulateTyping(textToType);

            }
        }
        private void SimulateTyping(string text)
        {
            INPUT[] inputs = new INPUT[text.Length * 2];

            for (int i = 0; i < text.Length; i++)
            {
                // Get the virtual key code for the character
                ushort vk = (ushort)MapVirtualKey(text[i], MAPVK_VK_TO_VSC);

                // Keydown event
                inputs[2 * i].Type = INPUT_KEYBOARD;
                inputs[2 * i].Data.ki.Vk = vk;
                inputs[2 * i].Data.ki.Scan = 0;
                inputs[2 * i].Data.ki.Flags = KEYEVENTF_KEYDOWN;
                inputs[2 * i].Data.ki.Time = 0;
                inputs[2 * i].Data.ki.ExtraInfo = IntPtr.Zero;

                // Keyup event
                inputs[2 * i + 1].Type = INPUT_KEYBOARD;
                inputs[2 * i + 1].Data.ki.Vk = vk;
                inputs[2 * i + 1].Data.ki.Scan = 0;
                inputs[2 * i + 1].Data.ki.Flags = KEYEVENTF_KEYUP;
                inputs[2 * i + 1].Data.ki.Time = 0;
                inputs[2 * i + 1].Data.ki.ExtraInfo = IntPtr.Zero;
            }

            uint result = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            if (result == 0)
            {
                int error = Marshal.GetLastWin32Error();
                MessageBox.Show($"SendInput failed with error code: {error}");
            }
        }
        // P/Invoke declaration for MapVirtualKey function
        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);

        // Constants for the MapVirtualKey function
        private const uint MAPVK_VK_TO_VSC = 0x00;


    }
}
