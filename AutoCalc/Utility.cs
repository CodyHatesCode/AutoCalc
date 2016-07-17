using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCalc
{
    public static class Utility
    {
        public static string ConvertTerms(string inputText, string delimiter, int fromBase)
        {
            delimiter = delimiter.ToUpper();

            char[] operators = new char[] { ' ', '+', '-', '*', '/', '(', ')', 'x', 'X' };
            string[] delimiters = new string[operators.Length];

            for (int i = 0; i < operators.Length; i++) { delimiters[i] = operators[i] + delimiter; }

            if (inputText.ToUpper().Contains(delimiter))
            {
                string[] terms = inputText.ToUpper().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < terms.Length; i++)
                {
                    if (i == 0 && !inputText.ToUpper().StartsWith(delimiter))
                    {
                        continue;
                    }
                    else if (i == 0 && inputText.ToUpper().StartsWith(delimiter))
                    {
                        terms[i] = terms[i].Replace(delimiter, string.Empty);
                    }

                    // strip off everything past the next space/operator
                    string chunk = terms[i].Split(operators)[0];

                    // replace the hex numbers with base-10 numbers
                    int actualNum = Convert.ToInt32(chunk, fromBase);

                    terms[i] = terms[i].Replace(chunk, actualNum.ToString());
                }

                // reassemble the input string
                inputText = string.Concat(terms);
            }

            return inputText;
        }
    }
}
