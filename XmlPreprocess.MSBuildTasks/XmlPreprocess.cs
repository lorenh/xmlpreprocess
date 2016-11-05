/*
 * Copyright (c) 2012 Loren M Halvorson
 * This source is subject to the Microsoft Public License (Ms-PL).
 * See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
 * All other rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;


namespace XmlPreprocess.Tasks
{
    /// <summary>
    /// Preprocess files
    /// </summary>
    public class XmlPreprocess : ToolTask
    {
        [Required]
        public ITaskItem[] InputFiles { get; set; }
        public ITaskItem[] OutputFiles { get; set; }
        public string Environment { get; set; }
        public ITaskItem[] SpreadsheetFiles { get; set; }
        public ITaskItem[] SettingsFiles { get; set; }
        // Note: will need to escape the semicolons in a connection string with %3B
        public ITaskItem[] Databases { get; set; }
        public ITaskItem[] CustomDataSources { get; set; }
        public bool FixFalse { get; set; }
        public bool Clean { get; set; }
        public bool NoDirectives { get; set; }
        public bool Validate { get; set; }
        public bool ValidateSettingsExist { get; set; }
        public bool ValidateXmlWellFormed { get; set; }
        public ITaskItem[] Properties { get; set; }
        public string EnvironmentRow { get; set; }
        public string FirstValueRow { get; set; }
        public string SettingNameCol { get; set; }
        public string DefaultValueCol { get; set; }
        public string TokenStart { get; set; }
        public string TokenEnd { get; set; }
        public ITaskItem CountReportFile { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlPreprocess"/> class.
        /// </summary>
        public XmlPreprocess()
        {
            FixFalse = true;
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
            ValidatePropertyIsNullOrInteger("EnvironmentRow", EnvironmentRow);
            ValidatePropertyIsNullOrInteger("FirstValueRow", FirstValueRow);
            ValidatePropertyIsNullOrInteger("SettingNameCol", SettingNameCol);
            ValidatePropertyIsNullOrInteger("DefaultValueCol", DefaultValueCol);

            CommandLineBuilder builder = new CommandLineBuilder();

            builder.AppendSwitch("/nologo");
            builder.AppendSwitch("/quiet");

            if (FixFalse) builder.AppendSwitch("/fixFalse");
            if (Clean) builder.AppendSwitch("/clean");
            if (NoDirectives) builder.AppendSwitch("/noDirectives");
            if (Validate) builder.AppendSwitch("/validate");
            if (ValidateSettingsExist) builder.AppendSwitch("/validateSettingsExist");
            if (ValidateXmlWellFormed) builder.AppendSwitch("/validateXmlWellFormed");

            builder.AppendSwitchIfNotNull("/input:", InputFiles, ";");
            builder.AppendSwitchIfNotNull("/output:", OutputFiles, ";");
            builder.AppendSwitchIfNotNull("/spreadsheet:", SpreadsheetFiles, ";");
            builder.AppendSwitchIfNotNull("/settings:", SettingsFiles, ";");
            
            if (null != Databases)
            {
                foreach (ITaskItem db in Databases)
                {
                    builder.AppendSwitchIfNotNull("/database:", db);
                }
            }

            if (null != CustomDataSources)
            {
                foreach (ITaskItem custom in CustomDataSources)
                {
                    builder.AppendSwitchIfNotNull("/custom:", custom);
                }
            }

            if (null != Properties)
            {
                foreach (ITaskItem property in Properties)
                {
                    builder.AppendSwitchIfNotNull("/define:", property);
                }
            }

            builder.AppendSwitchIfNotNull("/environment:", Environment);

            builder.AppendSwitchIfNotNull("/environmentRow:", EnvironmentRow);
            builder.AppendSwitchIfNotNull("/firstValueRow:", FirstValueRow);
            builder.AppendSwitchIfNotNull("/settingNameCol:", SettingNameCol);
            builder.AppendSwitchIfNotNull("/defaultValueCol:", DefaultValueCol);

            builder.AppendSwitchIfNotNull("/tokenStart:", TokenStart);
            builder.AppendSwitchIfNotNull("/tokenEnd:", TokenEnd);
            builder.AppendSwitchIfNotNull("/countReportFile:", CountReportFile);

            // We have all of our switches added, return the commandline as a string
            return builder.ToString();
        }


        /// <summary>
        /// Validates the property is null or integer.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        internal static void ValidatePropertyIsNullOrInteger(string propertyName, string value)
        {
            int outVal = 0;
            if (value != null && !Int32.TryParse(value, out outVal))
            {
                throw new ArgumentException(string.Format("{0} must be an integer.", propertyName), propertyName);
            }
        }


        /// <summary>
        /// Parses a single line of text to identify any errors or warnings in canonical format.
        /// </summary>
        /// <param name="singleLine">A single line of text for the method to parse.</param>
        /// <param name="messageImportance">A value of <see cref="T:Microsoft.Build.Framework.MessageImportance"/> that indicates the importance level with which to log the message.</param>
        protected override void LogEventsFromTextOutput(String singleLine, MessageImportance messageImportance)
        {
            Log.LogMessage(MessageImportance.Normal, singleLine);

            base.LogEventsFromTextOutput(singleLine, messageImportance);
        }
    }
}
