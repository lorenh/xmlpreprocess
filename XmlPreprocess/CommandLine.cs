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

using Genghis;
using clp = Genghis.CommandLineParser;
using XmlPreprocess.Util;

namespace XmlPreprocess
{
    /// <summary>
    /// A command line parser from the Genghis library.
    /// </summary>
    [clp.ParserUsage("XML File Preprocessor")]
    public class CommandLine : CommandLineParser
    {
        /// <summary>
        /// First value row default
        /// </summary>
        public const int FIRST_VALUE_ROW_DEFAULT = 7;

        /// <summary>
        /// Path to the input file.
        /// </summary>
        [clp.ValueUsage("(/i) Location of input file. You can specify multiple /i parameters (ex: /i:f1 /i:f2), or pass multiple files with semicolon delimiter (/i:f1;f2).", Optional = true, MatchPosition = false, AlternateName1="i")]
        public ArrayList input = new ArrayList();

        /// <summary>
        /// Path to output file
        /// </summary>
        [clp.ValueUsage("(/o) Location of output file. If omitted, input file will be overwritten. You can specify multiple /o parameters(ex: /o:f1 /o:f2), or pass multiple files with semicolon delimiter (/o:f1;f2). If multiple files are passed, they must correspond to the /i parameters. ", Optional = true, MatchPosition = false, AlternateName1 = "o")]
        public ArrayList output = new ArrayList();

        /// <summary>
        /// Path to settings files
        /// </summary>
        [clp.ValueUsage("(/s) Location of XML settings file with the format: <settings><property name=\"a\">1</property></settings>. You can specify multiple /s parameters (ex: /s:f1 /s:f2), or pass multiple files with semicolon delimiter (/s:f1;f2).", Optional = true, MatchPosition = false, AlternateName1 = "s")]
        public ArrayList settings = new ArrayList();

        /// <summary>
        /// Properties to define.
        /// </summary>
        [clp.ValueUsage("(/d) Properties to define, these will override properties read from the file with the same name. (example: /d:a=1 /d:b=2 /d:c=3)", Optional = true, MatchPosition = false, AlternateName1 = "d")]
        public ArrayList define = new ArrayList();

        /// <summary>
        /// Clean switch, if this is passed, preprocessor markup comments will be removed.
        /// </summary>
        [clp.FlagUsage("(/c) Clean output file of all preprocessor markup comments. Keeping these comments in allows the file to be preprocessed multiple times.", Optional = true, MatchPosition = false, AllowOnOff = false, AlternateName1 = "c")]
        public bool clean = false;

        /// <summary>
        /// Quiet switch, if this is passed, processor will not prompt for missing settings files.
        /// </summary>
        [clp.FlagUsage("(/q) Quiet mode, do not prompt for missing settings files or environments, just exit with an error if anything is missing.", Optional = true, MatchPosition = false, AllowOnOff = false, AlternateName1 = "q")]
        public bool quiet = false;

        /// <summary>
        /// Validate all, return an error and halt execution if any substitution settings are undefined
        /// </summary>
        [clp.FlagUsage("(/v) Validate all, turns on /validateSettingsExist, and /validateXmlWellFormed", Optional = true, MatchPosition = false, AllowOnOff = false, AlternateName1 = "v")]
        public bool validate = false;

        /// <summary>
        /// Validate settings, return an error and halt execution if any substitution settings are undefined
        /// </summary>
        [clp.FlagUsage("(/vs) Validate settings exist, return an error if any substitution settings are undefined.", Optional = true, MatchPosition = false, AllowOnOff = false, AlternateName1 = "vs")]
        public bool validateSettingsExist = false;

        /// <summary>
        /// Validate XML is well formed, return an error and halt execution if the output document isn't well formed
        /// </summary>
        [clp.FlagUsage("(/vx) Validate resulting XML is well-formed, return an error if the result document is not well-formed.", Optional = true, MatchPosition = false, AllowOnOff = false, AlternateName1 = "vx")]
        public bool validateXmlWellFormed = false;

        /// <summary>
        /// Do not use the value of "False" to undefine settings (instead use "#undef")
        /// </summary>
        [clp.FlagUsage("(/f) Do not use the value of \"False\" to undefine settings. (instead use \"#undef\")", Optional = true, MatchPosition = false, AllowOnOff = false, AlternateName1 = "f")]
#if NOFIXFALSE
        public bool fixFalse = false;
#else
        public bool fixFalse = true;
#endif

        /// <summary>
        /// Environment column to use if settings files are Spreadsheet format
        /// </summary>
        [clp.ValueUsage("(/e) Name of environment column to use if settings files are spreadsheet format. If not passed, and /quiet was not passed, the user will be prompted for which environment to use.", Optional = true, MatchPosition = false, AlternateName1 = "e", EmptyValueAllowed = true)]
        public string environment = null;

        /// <summary>
        /// Path to spreadsheet format settings files
        /// </summary>
        [clp.ValueUsage("(/x) Location of spreadsheet file, must also specify environment. Valid formats are XML Spreadsheet 2003 (*.xml), Comma Separated Values (*.csv) or Excel 2003 or older (*.xls). You can specify multiple /x parameters (ex: /x:f1 /x:f2), or pass multiple files with semicolon delimiter (/x:f1;f2).", Optional = true, MatchPosition = false, AlternateName1 = "x")]
        public ArrayList spreadsheet = new ArrayList();

        /// <summary>
        /// Connection string for database
        /// </summary>
        [clp.ValueUsage("(/db) Connection string to database, must also specify environment. You can specify multiple /db parameters (ex: /db:f1 /db:f2).", Optional = true, MatchPosition = false, AlternateName1 = "db")]
        public ArrayList database = new ArrayList();

        /// <summary>
        /// Connection string for database
        /// </summary>
        [clp.ValueUsage("(/cx) Command line for custom data source, must also specify environment. You can specify multiple /cx parameters (ex: /cx:f1 /cx:f2).", Optional = true, MatchPosition = false, AlternateName1 = "cx")]
        public ArrayList custom = new ArrayList();

        /// <summary>
        /// Environment column to use if settings files are Spreadsheet format
        /// </summary>
        [clp.FlagUsage("(/l) List environment names from a spreadsheet file to the console. Must be combined with a single /x parameter.", Optional = true, MatchPosition = false, AllowOnOff = false, AlternateName1 = "l")]
        public bool list = false;

        /// <summary>
        /// Name of property to output to console
        /// </summary>
        [clp.ValueUsage("(/p) Name of property to output to the console.", Optional = true, MatchPosition = false, AlternateName1 = "p")]
        public string property = null;

        /// <summary>
        /// Name of property to output to console
        /// </summary>
        [clp.ValueUsage("(/t) Delimiter characters to use to split a compound property value into multiple lines, used in conjunction with /p.", Optional = true, MatchPosition = false, AlternateName1 = "t")]
        public string delimiters = null;

        /// <summary>
        /// Row index of environment name in input spreadsheet
        /// </summary>
        [clp.ValueUsage("(/er) Row number (1 based) of environment name in input spreadsheet.", Optional = true, MatchPosition = false, AlternateName1 = "er")]
        public int environmentRow = 2;

        /// <summary>
        /// Row index of first value in input spreadsheet
        /// </summary>
        [clp.ValueUsage("(/vr) Row number (1 based) of first value in input spreadsheet.", Optional = true, MatchPosition = false, AlternateName1 = "vr")]
        public int firstValueRow = FIRST_VALUE_ROW_DEFAULT;

        /// <summary>
        /// Column index of setting names in input spreadsheet
        /// </summary>
        [clp.ValueUsage("(/nc) Column index (1 based) of setting names in input spreadsheet.", Optional = true, MatchPosition = false, AlternateName1 = "nc")]
        public int settingNameCol = 1;

        /// <summary>
        /// Column index of default values in input spreadsheet
        /// </summary>
        [clp.ValueUsage("(/dc) Column index of default values in input spreadsheet.", Optional = true, MatchPosition = false, AlternateName1 = "dc")]
        public int defaultValueCol = 2;

        /// <summary>
        /// Prompt for environment only
        /// </summary>
        [clp.ValueUsage("(/ef) File in which to save the name of the chosen environment. Passing a value indicates you wish to only prompt for environment and immediately exit, Must be combined with /x", Optional = true, MatchPosition = false, AlternateName1 = "ef")]
        public string environmentFile = null;

        /// <summary>
        /// No markup
        /// </summary>
        [clp.FlagUsage("(/n) No preprocessor directives needed. Just globally replace all macros.", Optional = true, MatchPosition = false, AllowOnOff = false, AlternateName1 = "n")]
        public bool noDirectives = false;

        /// <summary>
        /// Token Start
        /// </summary>
        [clp.ValueUsage("(/ts) The starting token (default is \"${\").", Optional = true, MatchPosition = false, AlternateName1 = "ts")]
        public string tokenStart = null;

        /// <summary>
        /// Token End
        /// </summary>
        [clp.ValueUsage("(/te) The ending token (default is \"}\").", Optional = true, MatchPosition = false, AlternateName1 = "te")]
        public string tokenEnd = null;

        /// <summary>
        /// Report property usage counts
        /// </summary>
        [clp.ValueUsage("(/cr) File in which to write property usage counts, will be written in CSV format.", Optional = true, MatchPosition = false, AlternateName1 = "cr")]
        public string countReportFile = null;

        /// <summary>
        /// Nologo switch, if this is passed, the logo will not be output to the console.
        /// </summary>
        [clp.FlagUsage("Do not output logo.", Optional = true, MatchPosition = false, AllowOnOff = false)]
        public bool noLogo = false;

        internal string GetCommandLineLogo()
        {
            return base.GetLogo();
        }


        /// <summary>
        /// Creates the preprocessing context.
        /// </summary>
        /// <returns>A new preprocessing context</returns>
        public PreprocessingContext CreatePreprocessingContext()
        {
            // Create the preprocessing context
            PreprocessingContext context = new PreprocessingContext(fixFalse);

            context.SettingsFiles = settings;
            context.PreserveMarkup = !clean;
            context.QuietMode = quiet;
            if (validate)
            {
                context.ValidateSettingsExist = true;
                context.ValidateXmlWellFormed = true;
            }
            else
            {
                context.ValidateSettingsExist = validateSettingsExist;
                context.ValidateXmlWellFormed = validateXmlWellFormed;
            }

            // create data sources collection
            List<DataSource> dataSources = new List<DataSource>();
            foreach (string spreadsheetFile in spreadsheet)
            {
                DataSourceSpreadsheetFormat spreadsheetFormat = DetermineSpreadsheetFormat(spreadsheetFile);
                dataSources.Add(new DataSource(spreadsheetFile, DataSourceType.Spreadsheet, spreadsheetFormat));
            }

            foreach (string databaseConnectionString in database)
            {
                dataSources.Add(new DataSource(databaseConnectionString, DataSourceType.Database));
            }

            foreach (string customCommandLineString in custom)
            {
                dataSources.Add(new DataSource(customCommandLineString, DataSourceType.Custom));
            }

            context.DataSources = dataSources;

            context.EnvironmentName = environment;
            context.FirstValueRowIndex = firstValueRow;
            context.EnvironmentNameRowIndex = environmentRow;
            context.DefaultValueColumnIndex = defaultValueCol;
            context.SettingNameColumnIndex = settingNameCol;
            context.List = list;
            context.PropertyToExtract = property;
            context.Delimiters = delimiters;
            context.NoDirectives = noDirectives;
            context.CountUsage = !string.IsNullOrEmpty(countReportFile);

            if (!string.IsNullOrEmpty(tokenStart))
            {
                context.TokenStart = tokenStart;

                if (string.IsNullOrEmpty(tokenEnd))
                    context.TokenEnd = tokenStart;
            }

            if (!string.IsNullOrEmpty(tokenEnd))
                context.TokenEnd = tokenEnd;

            return context;
        }


        /// <summary>
        /// Try to determine the format of the spreadsheet
        /// </summary>
        /// <param name="spreadsheetFile">The path to the spreadsheet</param>
        /// <returns>The type</returns>
        private DataSourceSpreadsheetFormat DetermineSpreadsheetFormat(string spreadsheetFile)
        {
            DataSourceSpreadsheetFormat spreadsheetFormat = DataSourceSpreadsheetFormat.Unknown;
            string extension = Path.GetExtension(spreadsheetFile);
            if (!string.IsNullOrEmpty(extension))
            {
                switch (extension.ToLower())
                {
                    case ".xls": // Excel binary format
                        spreadsheetFormat = DataSourceSpreadsheetFormat.Xls;
                        break;

                    case ".csv": // CSV format
                        spreadsheetFormat = DataSourceSpreadsheetFormat.Csv;
                        break;

                    case ".xml": // XML Spreadsheet 2003 format
                        spreadsheetFormat = DataSourceSpreadsheetFormat.Xml;
                        break;
                }
            }

            if (spreadsheetFormat == DataSourceSpreadsheetFormat.Unknown)
            {
                if (FileUtils.IsHttpUrl(spreadsheetFile))
                {
                    // Try to determine spreadsheet type from URL
                    // This is admittedly crude as it just looks for .xls/=xls, .csv/=csv, or .xml/=xml in the url
                    // Other possibilities:
                    // * use Uri class to break into more granular components
                    //     Uri uri = new Uri(spreadsheetFile);
                    //     string cleanURL = uri.Scheme + "://" + uri.GetComponents(UriComponents.HostAndPort, UriFormat.UriEscaped);
                    // * perhaps defer to mime type (would require refactoring of download mechanism)
                    string spreadsheetFileLowered = spreadsheetFile.ToLower();
                    if (spreadsheetFileLowered.Contains(".xls") || spreadsheetFileLowered.Contains("=xls"))
                        spreadsheetFormat = DataSourceSpreadsheetFormat.Xls;
                    else if (spreadsheetFileLowered.Contains(".csv") || spreadsheetFileLowered.Contains("=csv"))
                        spreadsheetFormat = DataSourceSpreadsheetFormat.Csv;
                    else if (spreadsheetFileLowered.Contains(".xml") || spreadsheetFileLowered.Contains("=xml"))
                        spreadsheetFormat = DataSourceSpreadsheetFormat.Xml;
                }
            }

            return spreadsheetFormat;
        }

        /// <summary>
        /// Splits files that may be semicolon delimited, make sure
        /// to call this before using the input or output arrays
        /// </summary>
        /// <remarks>
        /// This just allows a little more flexibility in how the 
        /// inputs and outputs are passed, you can pass them 
        /// with multiple /i f1 /i f2 /i f3 .. parameters or
        /// semicolon delimit (a la MSBuild) them with one
        /// parameter like this /i f1;f2;f3, or a combination
        /// /i f1 /i f2;f3 etc.
        /// </remarks>
        public void NormalizeAllFileArrays()
        {
            input = NormalizeFileArray(input);
            input = ExpandFileWildcards(input);
            output = NormalizeFileArray(output);
            settings = NormalizeFileArray(settings);
            spreadsheet = NormalizeFileArray(spreadsheet);
        }

        /// <summary>
        /// Splits files that may be semicolon delimited
        /// </summary>
        /// <param name="inList">The in list.</param>
        /// <returns>The normalized list</returns>
        private ArrayList NormalizeFileArray(ArrayList inList)
        {
            ArrayList outList = new ArrayList();

            foreach (string fileParam in inList)
            {
                if (!string.IsNullOrEmpty(fileParam))
                {
                    foreach (string file in fileParam.Split(';'))
                    {
                        outList.Add(file);
                    }
                }
                else
                {
                    outList.Add(fileParam);
                }
            }
            return outList;
        }


        /// <summary>
        /// Expands wildcards
        /// </summary>
        /// <param name="inList">The in list.</param>
        /// <returns>The expand list</returns>
        private ArrayList ExpandFileWildcards(ArrayList inList)
        {
            ArrayList outList = new ArrayList();

            foreach (string filespec in inList)
            {
                if (!string.IsNullOrEmpty(filespec))
                {
                    if (filespec.IndexOfAny(new char[] { '*', '?' }) > -1)
                    {
                        DirectoryScanner scanner = new DirectoryScanner();
                        scanner.Includes.Add(filespec);
                        // if GPL licensing permitted the NAnt DirectoryScanner to be used, there could
                        // be more bells and whistles available like recursive wildcards and excludes
                        //scanner.Excludes.Add("modules\\*\\**");
                        //scanner.BaseDirectory = new DirectoryInfo("test");
                        foreach (string filename in scanner.FileNames)
                        {
                            outList.Add(filename);
                        }
                    }
                    else
                    {
                        outList.Add(filespec);
                    }
                }
                else
                {
                    outList.Add(filespec);
                }
            }

            return outList;
        }


        /// <summary>
        /// Validates the arguments.
        /// </summary>
        /// <returns>true if everything is OK, false if there was an error</returns>
        public bool ValidateArguments()
        {
            bool isExtractingProperty = !string.IsNullOrEmpty(property);
            bool hasNoDataSource = ((null == spreadsheet || 0 == spreadsheet.Count)
                                        && (null == database || 0 == database.Count));
            bool hasMoreThanOneDataSource = (spreadsheet.Count + database.Count) > 1;

            // if they aren't listing the environments and they aren't extracting a property, 
            // they didn't pass any input files then show usage and quit
            if (!list && string.IsNullOrEmpty(environmentFile) && !isExtractingProperty && input.Count == 0)
            {
                Console.Write(GetUsage("input: Required argument not found"));
                return false;
            }

            if (list)
            {
                if (hasNoDataSource || hasMoreThanOneDataSource)
                {
                    Console.Write(GetUsage("Invalid arguments: If the /l argument is passed, you must have one and only one /x or /db argument."));
                    return false;
                }
            }

            return true;
        }

    }
}