/*
 * Copyright (c) 2004-2016 Loren M Halvorson
 * This source is subject to the Microsoft Public License (Ms-PL).
 * See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
 * All other rights reserved.
 * Portions copyright 2002-2007 The Genghis Group (http://www.genghisgroup.com/)
 * Portions copyright 2007-08 Thomas F. Abraham.
 */

using System;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace XmlPreprocess.DataSources
{
    /// <summary>
    /// Dynamically invoke a custom settings reader, passing the name of a temp file
    /// the process simply needs to write CSV values into the temp file supplied
    /// XML Preprocessor will read the values from the CSV file and delete
    /// the temp file.
    ///
    /// The placeholder in command line to use is "${tempFile}"
    /// Example:
    ///  xmlpreprocess /i app.config /cx "powershell.exe -command CustomScript.ps1 -tempFile ${tempFile}"
    /// </summary>
    public class CustomReader : IConfigSettingsReader
    {
        /// <summary>
        /// Read the settings from a custom source
        /// </summary>
        /// <param name="dataSource">The settings data source.</param>
        /// <param name="context">The preprocessing context.</param>
        /// <returns></returns>
        public DataTable ReadSettings(DataSource dataSource, PreprocessingContext context)
        {
            string fileName = null;
            string arguments = null;

            ParseArguments(dataSource.Path, out fileName, out arguments);

            DataTable dt = null;
            string tempFile = Path.GetTempFileName();
            try
            {
                arguments = arguments.Replace("@tempFile@", tempFile);

                using (Process customProcess = new Process())
                {
                    customProcess.StartInfo.FileName = fileName;
                    customProcess.StartInfo.Arguments = arguments;
                    customProcess.StartInfo.UseShellExecute = false;
                    customProcess.StartInfo.RedirectStandardOutput = true;
                    customProcess.StartInfo.RedirectStandardError = false;
                    customProcess.StartInfo.CreateNoWindow = true;
                    customProcess.Start();

                    Console.WriteLine(customProcess.StandardOutput.ReadToEnd());

                    customProcess.WaitForExit();
                }

                IConfigSettingsReader reader = new CsvSpreadsheetFileReader();
                DataSource ds = new DataSource(tempFile, DataSourceType.Spreadsheet, DataSourceSpreadsheetFormat.Csv);
                dt = reader.ReadSettings(ds, context);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }

            return dt;
        }


        /// <summary>
        /// Separate executable from arguments
        /// </summary>
        /// <param name="fullCommandLine">combined command line</param>
        /// <param name="fileName">name of executable</param>
        /// <param name="arguments">arguments</param>
        /// <returns></returns>
        private bool ParseArguments(string fullCommandLine, out string fileName, out string arguments)
        {
            bool success = false;

            fileName = null;
            arguments = null;

            if (!string.IsNullOrEmpty(fullCommandLine))
            {
                fullCommandLine = fullCommandLine.Trim();

                if (fullCommandLine[0] == '\"')
                {
                    int closingQuotePos = fullCommandLine.Substring(1).IndexOf('\"');
                    if (closingQuotePos > -1)
                    {
                        fileName = fullCommandLine.Substring(1, closingQuotePos - 1);
                        arguments = fullCommandLine.Substring(closingQuotePos + 1);
                    }
                    else
                    {
                        fileName = fullCommandLine.Substring(1);
                    }
                }
                else
                {
                    int endOfFileNamePos = fullCommandLine.IndexOf(' ');
                    if (endOfFileNamePos > -1)
                    {
                        fileName = fullCommandLine.Substring(0, endOfFileNamePos);
                        arguments = fullCommandLine.Substring(endOfFileNamePos + 1);
                    }
                    else
                    {
                        fileName = fullCommandLine;
                    }
                }

                success = true;
            }

            return success;
        }
    }
}
