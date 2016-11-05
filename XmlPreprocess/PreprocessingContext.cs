/*
 * Copyright (c) 2004-2016 Loren M Halvorson
 * This source is subject to the Microsoft Public License (Ms-PL).
 * See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
 * All other rights reserved.
 * Portions copyright 2002-2007 The Genghis Group (http://www.genghisgroup.com/)
 * Portions copyright 2007-08 Thomas F. Abraham.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace XmlPreprocess
{
    /// <summary>
    /// Class that handles all of the state necessary for a preprocessing session
    /// </summary>
    public class PreprocessingContext
    {
        private PreprocessingProperties _properties = null;
        private string _tokenStart = null;
        private string _tokenEnd = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreprocessingContext"/> class.
        /// </summary>
        /// <param name="fixFalse">if set to <c>true</c> fix false.</param>
        public PreprocessingContext(bool fixFalse)
        {
            FixFalse = fixFalse;
            SettingsFiles = new ArrayList();
            Errors = new List<ErrorInfo>();
            DataSources = new List<DataSource>();
        }

        /// <summary>
        /// Sequence of characters that indicate the start of a token
        /// </summary>
        public string TokenStart
        {
            get
            {
                if (string.IsNullOrEmpty(_tokenStart))
                    TokenStart = "${";
                return _tokenStart;
            }
            set
            {
                _tokenStart = value;
                TokenStartEscaped = EscapeToken(value);
            }
        }

        /// <summary>
        /// Sequence of characters that indicate the end of a token
        /// </summary>
        public string TokenEnd
        {
            get
            {
                if (string.IsNullOrEmpty(_tokenEnd))
                    TokenEnd = "}";
                return _tokenEnd;
            }
            set
            {
                _tokenEnd = value;
                TokenEndEscaped = EscapeToken(value);
            }
        }

        /// <summary>
        /// Escape a token
        /// </summary>
        /// <param name="token">the non-escaped token</param>
        /// <returns>the escaped token</returns>
        private string EscapeToken(string token)
        {
            string escapedToken = "";
            for (int i = 0; i < token.Length; i++)
            {
                escapedToken = escapedToken + token[i] + token[i];
            }
            return escapedToken;
        }

        /// <summary>
        /// Sequence of characters that indicate the start of an escaped token
        /// </summary>
        public string TokenStartEscaped { get; set; }

        /// <summary>
        /// Sequence of characters that indicate the end of an escaped token
        /// </summary>
        public string TokenEndEscaped { get; set; }

        /// <summary>
        /// Gets or sets the source file.
        /// </summary>
        /// <value>The source file.</value>
        public string SourceFile { get; set; }

        /// <summary>
        /// Gets or sets the destination file.
        /// </summary>
        /// <value>The destination file.</value>
        public string DestinationFile { get; set; }

        /// <summary>
        /// Gets or sets the settings files.
        /// </summary>
        /// <value>The settings files.</value>
        public ArrayList SettingsFiles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [preserve markup].
        /// </summary>
        /// <value><c>true</c> if [preserve markup]; otherwise, <c>false</c>.</value>
        public bool PreserveMarkup { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [quiet mode].
        /// </summary>
        /// <value><c>true</c> if [quiet mode]; otherwise, <c>false</c>.</value>
        public bool QuietMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to validate settings exist
        /// </summary>
        /// <value>
        /// 	<c>true</c> if validate settings exist; otherwise, <c>false</c>.
        /// </value>
        public bool ValidateSettingsExist { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to validate XML well formed
        /// </summary>
        /// <value>
        /// 	<c>true</c> if validate XML well formed; otherwise, <c>false</c>.
        /// </value>
        public bool ValidateXmlWellFormed { get; set; }

        /// <summary>
        /// Gets the errors.
        /// </summary>
        /// <value>The errors.</value>
        public List<ErrorInfo> Errors { get; private set; }

        /// <summary>
        /// Returns whether this property is a dynamic property or not
        /// </summary>
        /// <param name="propertyName">the property to test</param>
        /// <returns>True if it is a dynamic property</returns>
        public bool IsDynamicProperty(string propertyName)
        {
            return propertyName.StartsWith(TokenStart);
        }

        /// <summary>
        /// Resolve content
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public string ResolveContent(string content)
        {
            return Properties.ResolveContent(this, content);
        }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>The properties.</value>
        public PreprocessingProperties Properties
        {
            get
            {
                if (null == _properties)
                {
                    _properties = new PreprocessingProperties(FixFalse);
                }
                return _properties;
            }
        }

        /// <summary>
        /// Gets or sets the data sources.
        /// </summary>
        /// <value>The data sources.</value>
        public List<DataSource> DataSources { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [quiet mode].
        /// </summary>
        /// <value><c>true</c> if [quiet mode]; otherwise, <c>false</c>.</value>
        public bool List { get; set; }

        /// <summary>
        /// Gets or sets the name of the property to output to the console
        /// </summary>
        /// <value>The name of the property to output to the console</value>
        public string PropertyToExtract { get; set; }

        /// <summary>
        /// Gets or sets the delimiter characters to use to split a compound property value into multiple lines,
        /// used in conjunction with PropertyToExtract
        /// </summary>
        /// <value>The delimiter characters</value>
        public string Delimiters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [fix false].
        /// </summary>
        /// <value><c>true</c> if [fix false]; otherwise, <c>false</c>.</value>
        public bool FixFalse { get; private set; }

        /// <summary>
        /// Gets or sets the name of the environment.
        /// </summary>
        /// <value>The name of the environment.</value>
        public string EnvironmentName { get; set; }

        /// <summary>
        /// Gets or sets the index of the environment name row.
        /// </summary>
        /// <value>The index of the environment name row.</value>
        public int EnvironmentNameRowIndex { get; set; }

        /// <summary>
        /// Gets or sets the first index of the value row.
        /// </summary>
        /// <value>The first index of the value row.</value>
        public int FirstValueRowIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the setting name column.
        /// </summary>
        /// <value>The index of the setting name column.</value>
        public int SettingNameColumnIndex { get; set; }

        /// <summary>
        /// Gets or sets the default index of the value column.
        /// </summary>
        /// <value>The default index of the value column.</value>
        public int DefaultValueColumnIndex { get; set; }

        /// <summary>
        /// Gets or sets whether to use directives or not
        /// </summary>
        /// <value>False if directives are required, True if they are not</value>
        public bool NoDirectives { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether to count usage.
        /// </summary>
        /// <value>
        ///   <c>true</c> if count usage; otherwise, <c>false</c>.
        /// </value>
        public bool CountUsage { get; set; }

        /// <summary>
        /// Add some built-in properties
        /// </summary>
        public void AddBuiltInProperties()
        {
            // implicit property automatically defined whenever running
            // under the xml preprocessor
            Properties.Add("_xml_preprocess", "");

            // Built in property for environment name passed with /e argument
            if (!string.IsNullOrEmpty(EnvironmentName))
                Properties.Add("_environment_name", EnvironmentName);

            string destDir = "";
            string destRoot = "";
            try
            {
                string destPath = Path.GetFullPath(DestinationFile);
                destDir = Path.GetDirectoryName(destPath);
                destRoot = Path.GetPathRoot(destPath);
            }
            catch (Exception)
            {
                // not fatal
            }
            Properties.Add("_dest_dir", destDir);
            Properties.Add("_dest_root", destRoot);

            string machineName = "";
            try
            {
                machineName = Environment.MachineName;
            }
            catch (Exception)
            {
                // not fatal
            }
            Properties.Add("_machine_name", machineName);

            string machineId = machineName;
            Match match = Regex.Match(machineName, @"\d+");
            if (match.Success)
                machineId = match.Value;
            Properties.Add("_machine_id", machineId);

            OperatingSystem os = null;
            try
            {
                os = Environment.OSVersion;
            }
            catch (Exception)
            {
                // not fatal
            }

            string platform = "";
            if (null != os)
                platform = Environment.OSVersion.Platform.ToString();
            Properties.Add("_os_platform", platform);

            string version = "";
            if (null != os)
                version = os.Version.ToString();
            Properties.Add("_os_version", version);

            string systemDir = "";
            string systemRoot = "";
            try
            {
                systemDir = Environment.SystemDirectory;
                systemRoot = Path.GetPathRoot(systemDir);
            }
            catch (Exception)
            {
                // not fatal
            }
            Properties.Add("_system_dir", systemDir);
            Properties.Add("_system_root", systemRoot);

            string currentDir = "";
            string currentRoot = "";
            try
            {
                currentDir = Environment.CurrentDirectory;
                currentRoot = Path.GetPathRoot(currentDir);
            }
            catch (Exception)
            {
                // not fatal
            }
            Properties.Add("_current_dir", currentDir);
            Properties.Add("_current_root", currentRoot);

            string clrVersion = Environment.Version.ToString();
            Properties.Add("_clr_version", clrVersion);

            string userName = Environment.UserName;
            Properties.Add("_user_name", userName);

            string userDomainName = "";
            try
            {
                userDomainName = Environment.UserDomainName;
            }
            catch (Exception)
            {
                // not fatal
            }
            Properties.Add("_user_domain_name", userDomainName);

            string userInteractive = Environment.UserInteractive.ToString();
            Properties.Add("_user_interactive", userInteractive);

            DateTime now = DateTime.Now;
            Properties.Add("_system_date", now.ToShortDateString());
            Properties.Add("_system_time", now.ToShortTimeString());


            IDictionary envVars = null;
            try
            {
                envVars = Environment.GetEnvironmentVariables();
            }
            catch (Exception)
            {
                // not fatal
            }

            if (null != envVars)
            {
                foreach (string key in envVars.Keys)
                {
                    string lowerCaseKey = "_env_" + key.ToLower();
                    Properties.Add(lowerCaseKey, envVars[key] as string);
                }
            }

            string frameworkDir = "";
            try
            {
                frameworkDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
            }
            catch (Exception)
            {
                // not fatal
            }
            Properties.Add("_framework_dir", frameworkDir);
        }
    }
}
