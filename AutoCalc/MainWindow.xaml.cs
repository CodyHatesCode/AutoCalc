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
        DispatcherTimer infoTextTimer;

        public MainWindow()
        {
            InitializeComponent();

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
            }
        }

        private void UpdateSolution(object sender, TextChangedEventArgs e)
        {
            // Check if the box is empty before indicating the error
            if (formulaBox.Text.Trim() == string.Empty)
            {
                resultLabel.Content = string.Empty;
            }
            else
            {
                bool error = false;

                NCalc.Expression expr = null;
                decimal result = 0;

                try
                {
                    // attempt to evaluate the expression in the box
                    expr = new NCalc.Expression(formulaBox.Text);
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
                    resultLabel.Content = result;


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
