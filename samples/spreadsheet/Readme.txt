
Demonstrates using a spreadsheet saved as "XML Spreadsheet 2003" format as the source for settings. Reading these files directly is new to XmlPreprocess version 2.0.

Note: There are some hidden rows in the header of this spreadsheet. These rows are present for backward compatibility with the previous version of XmlPreprocess spreadsheets and Tom Abraham's Environment Settings Manager, but are not used in version 2.0. If you remove them, you will need to pass the /firstValueRow=3 on the command line to override the default value of 7