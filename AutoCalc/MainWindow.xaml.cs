using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AutoCalc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool hexOutput;
        DispatcherTimer infoTextTimer;

        public MainWindow()
        {
            InitializeComponent();

            hexOutput = false;

            // initialize the text boxes
            formulaBox.Text = string.Empty;
            infoLabel.Content = string.Empty;
            resultLabel.Content = string.Empty;

            // timer to clear away the info text
            infoTextTimer = new DispatcherTimer();
            infoTextTimer.Interval = new TimeSpan(0, 0, 5);

            // events
            formulaBox.TextChanged += UpdateSolution;
            formulaBox.KeyUp += KeyCheck;

            infoTextTimer.Tick += ResetInfoText;
        }

        private void KeyCheck(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                // Enter - copy the result to the clipboard
                case Key.Enter:
                    Clipboard.SetText(resultLabel.Content.ToString());
                    infoLabel.Content = "Result copied to clipboard.";
                    infoTextTimer.Start();
                    break;
                // Escape - Reset the calculator
                case Key.Escape:
                    formulaBox.Text = string.Empty;
                    UpdateSolution(this, null);
                    ResetInfoText(this, null);
                    break;
                // Ctrl - Toggle hex output
                case Key.LeftShift:
                case Key.RightShift:
                    hexOutput = !hexOutput;
                    UpdateSolution(this, null);
                    break;
            }
        }

        private void UpdateSolution(object sender, TextChangedEventArgs e)
        {
            string inputText = formulaBox.Text.Trim();

            // Check if the box is empty before indicating the error
            if (inputText == string.Empty)
            {
                resultLabel.Content = string.Empty;
            }
            else
            {
                bool error = false;

                NCalc.Expression expr = null;
                decimal result = 0;

                // attempt to parse hexadecimal numbers
                try
                {
                    // split by 0x to get just the base16 number
                    string[] hex = inputText.Split(new string[] { "0x" }, StringSplitOptions.RemoveEmptyEntries);
                    if (inputText.Contains("0x"))
                    {
                        for (int i = 0; i < hex.Length; i++)
                        {
                            // strip off everything past the next space/operator
                            string chunk = hex[i].Split(new char[] { ' ', '+', '*', '/', '(', ')' })[0];

                            // replace the hex numbers with base-10 numbers
                            int actualNum = Convert.ToInt32(chunk, 16);

                            hex[i] = hex[i].Replace(chunk, actualNum.ToString());
                        }

                        // reassemble the input string
                        inputText = string.Concat(hex);
                    }
                }
                catch(Exception)
                {
                    error = true;
                }

                try
                {
                    // attempt to evaluate the expression in the box
                    expr = new NCalc.Expression(inputText);
                    result = Convert.ToDecimal(expr.Evaluate());
                }
                catch (Exception)
                {
                    error = true;
                }
                

                if (error)
                {
                    resultLabel.Foreground = new SolidColorBrush(Color.FromRgb(187, 82, 82));
                    resultLabel.Content = "Error";
                }
                else
                {
                    resultLabel.Foreground = new SolidColorBrush(Color.FromRgb(90, 158, 34));

                    // defaults to non-hex output
                    if(!hexOutput)
                    {
                        resultLabel.Content = result;
                    }
                    else
                    {
                        // convert the result to a string with the 0x identifier
                        string hexResult = string.Format("0x{0:X}", Convert.ToInt32(result));
                        resultLabel.Content = hexResult;

                        // decimals are automatically rounded during conversion - notify the user
                        if(result.ToString().Contains("."))
                        {
                            infoLabel.Content = "Decimals have been rounded off.";
                            infoTextTimer.Start();
                        }
                    }


                    // attempt to read/display the result as a bool (logical operators)
                    bool boolResult;

                    if (bool.TryParse(expr.Evaluate().ToString(), out boolResult))
                    {
                        resultLabel.Content = boolResult;
                    }

                }
            }
        }

        private void ResetInfoText(object sender, EventArgs e)
        {
            infoTextTimer.Stop();
            infoLabel.Content = string.Empty;
        }
    }
}
