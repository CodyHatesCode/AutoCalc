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
        OutputMode outputMode;
        DispatcherTimer infoTextTimer;

        public MainWindow()
        {
            InitializeComponent();

            outputMode = OutputMode.Decimal;

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
                case Key.LeftShift:
                case Key.RightShift:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        CycleOutputModes();
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
                    inputText = Utility.ConvertTerms(inputText, "H", 16);

                    // split by Z to get just the base2 numbers
                    inputText = Utility.ConvertTerms(inputText, "Z", 2);

                }
                catch(Exception)
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
                    switch(outputMode)
                    {
                        case OutputMode.Decimal:
                            resultLabel.Content = result;
                            break;
                        case OutputMode.Hexadecimal:
                            // convert the result to a string with the 0x identifier
                            string hexResult = string.Format("0x{0:X}", Convert.ToInt32(result));
                            resultLabel.Content = hexResult;
                            break;
                        case OutputMode.Binary:
                            string binaryResult = string.Format("0b{0}", Convert.ToString((short)result, 2));
                            resultLabel.Content = binaryResult;
                            break;
                    }

                    if(outputMode != OutputMode.Decimal)
                    {
                        // decimals are automatically rounded during conversion - notify the user
                        if (result.ToString().Contains("."))
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

        private void CycleOutputModes()
        {
            switch(outputMode)
            {
                case OutputMode.Decimal:
                    outputMode = OutputMode.Hexadecimal;
                    break;
                case OutputMode.Hexadecimal:
                    outputMode = OutputMode.Binary;
                    break;
                case OutputMode.Binary:
                    outputMode = OutputMode.Decimal;
                    break;
            }
        }
    }
}
