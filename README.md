# CodeTitans Signature Project
Utility tool to help digitally sign applications (binaries) on Windows.

[![Build status](https://ci.appveyor.com/api/projects/status/64t2qdfp8q72b5yu?svg=true)](https://ci.appveyor.com/project/phofman/signature)

# Overview
As an author of any command-line tool, driver or Visual Studio plugin it might be required to confirm its origin by putting a digital signature inside. Unfortunatelly its not as easy as could be on Windows and this utility tries to fill the gap. It provides an easy-to-use UI to let specify the binary and certificate (installed or from .pfx file) to complete the process.

It has been inspired by some questions about signing projects available on [StackOverflow.com](http://stackoverflow.com/questions/1177552/code-signing-certificate-for-open-source-projects/18959881) and [Jeff Wilcox's post](http://www.jeff.wilcox.name/2010/03/vsixcodesigning/) about VSIX post-processing.

# Certificate
For open-source projects you can get a free 1-year valid code-signing certificate from [Certum.pl](http://www.certum.pl/certum/cert,oferta_Open_Source_Signing.xml).

# Usage
Picture worth more than a thousand words.

![Screenshot](https://raw.github.com/phofman/signature/master/res/v1.3/screenshot.png)

1. name the binary to sign (Ctrl+O)
2. select certificate (by using filter to find the installed one or by specifying path to .pfx file and password)
3. click **Sign** button (Ctrl+S)
4. navigate to result (Ctrl+R) 

# Requirements
Microsoft .NET Framework 4.5

# Download
Latest compiled version of this tool is available at '[Releases](https://github.com/phofman/signature/releases/latest)' section.

# Contribution
Feel free to fire a feature request using '[Issues](https://github.com/phofman/signature/issues)' panel and I will try to add it for benefit of all.
