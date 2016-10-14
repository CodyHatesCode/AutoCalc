# AutoCalc

A simple calculator that accepts formulas of varying complexity and can produce results on-the-fly.

Uses WPF, implements [ncalc](https://www.nuget.org/packages/ncalc/) for expression parsing. Supports logical tests and hexadecimal, binary, and octal input/output.

* Numbers prefixed with **H** are interpreted as hexadecimal.
* Numbers prefixed with **Z** are interpreted as binary.
* Numbers prefixed with **O** are interpreted as octal.

* Press **Enter** to copy the result to the clipboard, and **Escape** to reset the calculator.
* Press **Ctrl-Shift** to toggle between decimal, hexadecimal, binary, and octal output.

![Screenshots](screenshots.png)