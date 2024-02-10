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

namespace BemisAutoTyper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : System.Windows.Window
    {

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
                // Get the text from the TextBox
                string textToType = DataTextBox.Text;

                // Simulate typing the text
                
            }
        }
    }
}
