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
                // Ctrl+Shift - Toggle hex output
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        hexOutput = !hexOutput;
                        UpdateSolution(this, null);
                    }
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
                    // split by H to get just the base16 number

                    char[] operators = new char[] { ' ', '+', '-', '*', '/', '(', ')', 'x', 'X' };
                    string[] delimiters = new string[operators.Length];

                    for(int i = 0; i < operators.Length; i++) { delimiters[i] = operators[i] + "H"; }

                    if(inputText.ToUpper().Contains("H"))
                    {
                        string[] hex = inputText.ToUpper().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                        for(int i = 0; i < hex.Length; i++)
                        {
                            if(i == 0 && !inputText.ToUpper().StartsWith("H"))
                            {
                                continue;
                            }
                            else if(i == 0 && inputText.ToUpper().StartsWith("H"))
                            {
                                hex[i] = hex[i].Replace("H", string.Empty);
                            }

                            // strip off everything past the next space/operator
                            string chunk = hex[i].Split(operators)[0];

                            // replace the hex numbers with base-10 numbers
                            int actualNum = Convert.ToInt32(chunk, 16);

                            hex[i] = hex[i].Replace(chunk, actualNum.ToString());
                        }

                        // reassemble the input string
                        inputText = string.Concat(hex);
                    }

                    // split by Z to get just base2 numbers

                    delimiters = new string[operators.Length];
                    for(int i = 0; i < operators.Length; i++) { delimiters[i] = operators[i] + "Z"; }

                    if(inputText.ToUpper().Contains("Z"))
                    {
                        string[] binary = inputText.ToUpper().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                        for(int i = 0; i < binary.Length; i++)
                        {
                            if(i == 0 && !inputText.ToUpper().StartsWith("Z"))
                            {
                                continue;
                            }
                            else if(i == 0 && inputText.ToUpper().StartsWith("Z"))
                            {
                                binary[i] = binary[i].Replace("Z", string.Empty);
                            }

                            // strip off everything past the next space/operator
                            string chunk = binary[i].Split(operators)[0];

                            // replace binary numbers with base10 numbers
                            int actualNum = Convert.ToInt32(chunk, 2);

                            binary[i] = binary[i].Replace(chunk, actualNum.ToString());
                        }

                        // reassemble
                        inputText = string.Concat(binary);
                    }

                    /*
                    if(inputText.StartsWith("0x") || delimiters.Any(d => inputText.Contains(d)))
                    {
                        string[] hex = inputText.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < hex.Length; i++)
                        {
                            if(i == 0 && !inputText.StartsWith("0x"))
                            {
                                continue;
                            }
                            else if(i == 0 && inputText.StartsWith("0x"))
                            {
                                hex[0] = hex[0].Replace("0x", "");
                            }

                            // strip off everything past the next space/operator
                            string chunk = hex[i].Split(operators)[0];
                            /*

                            {
                                char[] operatorsWithoutX = operators.Take(7).ToArray();
                                chunk = hex[i].Split(operatorsWithoutX)[0];
                            }

                            


                            hex[i] = hex[i].Replace(chunk, actualNum.ToString());
                        }

                        // reassemble the input string
                        inputText = string.Concat(hex);
                    }*/
                }
                catch(Exception ex)
                {
                    error = true;
                }

                // allow usage of "x" or "X" in place of "*" for multiplication
                // todo: hex recognition interferes with lowercase x
                inputText = inputText.Replace('x', '*').Replace('X', '*');

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
