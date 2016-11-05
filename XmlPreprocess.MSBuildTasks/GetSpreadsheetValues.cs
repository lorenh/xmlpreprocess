/*
 * Copyright (c) 2012 Loren M Halvorson
 * This source is subject to the Microsoft Public License (Ms-PL).
 * See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
 * All other rights reserved.
 */

using System;
using System.Collections.Generic;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XmlPreprocess.Tasks
{
    /// <summary>
    /// Retrieve values from an XmlPreprocess spreadsheet
    /// </summary>
    public class GetSpreadsheetValues : ToolTask
    {
        List<string> _results = new List<string>();

        [Required]
        public string Environment { get; set; }

        [Required]
        public string SettingName { get; set; }

        // One of the following is required:
        public ITaskItem SpreadsheetFile { get; set; }
        public ITaskItem Database { get; set; }
        public ITaskItem CustomDataSource { get; set; }

        public string Delimiters { get; set; }
        public string EnvironmentRow { get; set; }
        public string FirstValueRow { get; set; }
        public string SettingNameCol { get; set; }
        public string DefaultValueCol { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSpreadsheetValues"/> class.
        /// </summary>
        public GetSpreadsheetValues()
        {
            Delimiters = ";";
        }

        /// <summary>
        /// Returns the fully qualified path to the executable file.
        /// </summary>
        /// <returns>
        /// The fully qualified path to the executable file.
        /// </returns>
        protected override string GenerateFullPathToTool()
        {
            return ToolName;
        }

        /// <summary>
        /// Gets the name of the executable file to run.
        /// </summary>
        /// <returns>
        /// The name of the executable file to run.
        ///   </returns>
        protected override string ToolName
        {
            get { return "XmlPreprocess.exe"; }
        }

        /// <summary>
        /// Construct the command line from the task properties by using the CommandLineBuilder
        /// </summary>
        /// <returns></returns>
        protected override string GenerateCommandLineCommands()
        {
            XmlPreprocess.ValidatePropertyIsNullOrInteger("EnvironmentRow", EnvironmentRow);
            XmlPreprocess.ValidatePropertyIsNullOrInteger("FirstValueRow", FirstValueRow);
            XmlPreprocess.ValidatePropertyIsNullOrInteger("SettingNameCol", SettingNameCol);
            XmlPreprocess.ValidatePropertyIsNullOrInteger("DefaultValueCol", DefaultValueCol);

            int dataSourceCount = 0;

            if (SpreadsheetFile != null) dataSourceCount++;
            if (Database != null) dataSourceCount++;
            if (CustomDataSource != null) dataSourceCount++;

            if (dataSourceCount != 1)
                throw new ArgumentException("Exactly one of the following arguments must be passed: SpreadsheetFile, Database, or CustomDataSource");

            CommandLineBuilder builder = new CommandLineBuilder();

            builder.AppendSwitch("/nologo");
            builder.AppendSwitchIfNotNull("/property:", SettingName);
            builder.AppendSwitchIfNotNull("/environment:", Environment);

            builder.AppendSwitchIfNotNull("/spreadsheet:", SpreadsheetFile);
            builder.AppendSwitchIfNotNull("/database:", Database);
            builder.AppendSwitchIfNotNull("/custom:", CustomDataSource);

            if (!string.IsNullOrEmpty(Delimiters))
            {
                builder.AppendSwitchIfNotNull("/delimiters:", Delimiters);
            }

            builder.AppendSwitchIfNotNull("/environmentRow:", EnvironmentRow);
            builder.AppendSwitchIfNotNull("/firstValueRow:", FirstValueRow);
            builder.AppendSwitchIfNotNull("/settingNameCol:", SettingNameCol);
            builder.AppendSwitchIfNotNull("/defaultValueCol:", DefaultValueCol);

            // Log a High importance message stating the file that we are assembling
            Log.LogMessage(MessageImportance.Normal, "Extracting the value of {0} from {1} for the {2} environment", SettingName, SpreadsheetFile, Environment);

            // We have all of our switches added, return the commandline as a string
            return builder.ToString();
        }


        /// <summary>
        /// Parses a single line of text to identify any errors or warnings in canonical format.
        /// </summary>
        /// <param name="singleLine">A single line of text for the method to parse.</param>
        /// <param name="messageImportance">A value of <see cref="T:Microsoft.Build.Framework.MessageImportance"/> that indicates the importance level with which to log the message.</param>
        protected override void LogEventsFromTextOutput(String singleLine, MessageImportance messageImportance)
        {
            if (singleLine.StartsWith("Error XMLPP"))
            {
                Log.LogError(singleLine);
            }
            else
            {
                if (!string.IsNullOrEmpty(singleLine))
                    _results.Add(singleLine.Trim());
            }
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        [Output]
        public string[] Values
        {
            get { return _results.ToArray(); }
        }
    }
}
