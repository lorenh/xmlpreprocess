/*
 * Copyright (c) 2004-2016 Loren M Halvorson
 * This source is subject to the Microsoft Public License (Ms-PL).
 * See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
 * All other rights reserved.
 * Portions copyright 2002-2007 The Genghis Group (http://www.genghisgroup.com/)
 * Portions copyright 2007-08 Thomas F. Abraham.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;

using XmlPreprocess.DataSources;
using XmlPreprocess.Util;


namespace XmlPreprocess
{
    /// <summary>
    /// Command line class for the XML file preprocessor.
    /// </summary>
    public class XmlPreprocessorMain
    {
        /// <summary>
        /// Run the XML file preprocessor from the command line
        /// </summary>
        [STAThread]
        public static int Main(string[] args)
        {
            int exitCode = 0;

            // be nice & set console color back in the case of a Ctrl-C
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.CancelKeyPress += delegate { Console.ForegroundColor = originalColor; };

            // Parse the command line and show help or version or error
            CommandLine cl = new CommandLine();
            if (!cl.ParseAndContinue(args))
            {
                exitCode = 1;
                return exitCode;
            }

            // Make sure argument combinations are valid
            if (!cl.ValidateArguments())
            {
                exitCode = 1;
                return exitCode;
            }

            // Output logo if they didn't ask to turn it off, and they aren't extracting something
            bool isExtractingProperty = !string.IsNullOrEmpty(cl.property);
            if (!cl.noLogo && !cl.list && !isExtractingProperty)
            {
                Console.WriteLine(cl.GetCommandLineLogo());
            }

            cl.NormalizeAllFileArrays();

            PreprocessingContext context = cl.CreatePreprocessingContext();

            if (cl.list)
            {
                return DumpEnvironments(context);
            }

            // Do not prompt for input if extracting a property
            if (isExtractingProperty)
            {
                context.QuietMode = true;
            }

            // read settings files
            exitCode = ReadSettings(context);

            if (0 == exitCode && !string.IsNullOrEmpty(cl.environmentFile))
            {
                Console.WriteLine("Writing selected environment \"{0}\" to {1}", context.EnvironmentName, cl.environmentFile);
                FileUtils.WriteFile(cl.environmentFile, context.EnvironmentName);
                return exitCode;
            }

            if (0 == exitCode)
            {
                // Add properties from command line last so they override everything else
                context.Properties.AddPropertiesFromArrayList(context, cl.define);

                if (isExtractingProperty)
                {
                    return DumpProperty(context);
                }
                else
                {
                    XmlPreprocessor preprocessor = new XmlPreprocessor();

                    // loop through input files
                    for (int i = 0; i < cl.input.Count; i++)
                    {
                        context.SourceFile = cl.input[i] as string;

                        if (null != context.SourceFile)
                            context.SourceFile = context.SourceFile.Trim();

                        if (!string.IsNullOrEmpty(context.SourceFile))
                        {
                            if (!File.Exists(context.SourceFile))
                            {
                                ErrorInfo errorInfo = new ErrorInfo(ErrorCode.ErrorFileNotFound, context.SourceFile);
                                errorInfo.Message = string.Format("Input file was not found: \"{0}\"", context.SourceFile);
                                context.Errors.Add(errorInfo);
                                exitCode = 1;
                                break;
                            }

                            context.DestinationFile = null;
                            if (i < cl.output.Count)
                            {
                                context.DestinationFile = cl.output[i] as string;
                            }

                            // If destination file was not specified, use input file
                            if (string.IsNullOrEmpty(context.DestinationFile))
                            {
                                context.DestinationFile = context.SourceFile;
                                ConsoleUtils.WriteLine(ConsoleColor.Cyan, string.Format("Preprocessing \"{0}\"...", context.SourceFile));
                            }
                            else
                            {
                                ConsoleUtils.WriteLine(ConsoleColor.Cyan, string.Format("Preprocessing \"{0}\" to \"{1}\"...", context.SourceFile, context.DestinationFile));
                            }

                            try
                            {
                                exitCode = preprocessor.Preprocess(context);
                                if (0 != exitCode)
                                    break;
                            }
                            catch (Exception e)
                            {
                                ErrorInfo errorInfo = new ErrorInfo(ErrorCode.ErrorException, context.SourceFile);
                                errorInfo.Message = e.Message;
                                context.Errors.Add(errorInfo);
                                exitCode = 1;
                                break;
                            }
                        }
                    }
                }
            }

            ReportUseCount(cl, context);

            ReportErrorsAndWarnings(context.Errors);

            return exitCode;
        }


        /// <summary>
        /// Reports the use count.
        /// </summary>
        /// <param name="cl">The cl.</param>
        /// <param name="context">The context.</param>
        private static void ReportUseCount(CommandLine cl, PreprocessingContext context)
        {
            if (!string.IsNullOrEmpty(cl.countReportFile))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Property, Count\r\n");
                foreach (var property in context.Properties)
                {
                    if (!property.Key.StartsWith("_"))
                    {
                        sb.Append(string.Format("{0}, {1}\r\n", property.Key, property.UseCount));
                    }
                }
                FileUtils.WriteFile(cl.countReportFile, sb.ToString());
            }
        }


        /// <summary>
        /// Reports errors.
        /// </summary>
        /// <param name="errors">The errors collection.</param>
        private static void ReportErrorsAndWarnings(List<ErrorInfo> errors)
        {
            if (null != errors && errors.Count > 0)
            {
                foreach (ErrorInfo info in errors)
                {
                    ConsoleUtils.WriteLine(ConsoleColor.Red, info.ToString());
                }
            }
        }


        /// <summary>
        /// Dumps the environment names from the data source to the console.
        /// </summary>
        /// <param name="context">The preprocessing context.</param>
        /// <returns></returns>
        private static int DumpEnvironments(PreprocessingContext context)
        {
            int exitCode = 0;

            try
            {
                DataSource firstDataSource = context.DataSources[0];

                SettingsLoader loader = new SettingsLoader(context);

                DataTable settingsTable = loader.LoadDataTableFromDataSource(firstDataSource);

                List<string> environments = loader.GetEnvironmentsFromDataTable(settingsTable);
                foreach (string environment in environments)
                {
                    if (!string.IsNullOrEmpty(context.PropertyToExtract))
                    {
                        PreprocessingProperties properties = new PreprocessingProperties(context.FixFalse);
                        loader.LoadSettingsFromDataTable(settingsTable, context.Properties, environment);

                        PreprocessingProperty property = context.Properties[context.PropertyToExtract];
                        if (null != property)
                        {
                            string resolvedPropertyValue = context.ResolveContent(property.Value);

                            if (!string.IsNullOrEmpty(resolvedPropertyValue))
                                Console.WriteLine(resolvedPropertyValue);
                        }
                    }
                    else
                    {
                        Console.WriteLine(environment);
                    }
            }
            }
            catch (Exception e)
            {
                ConsoleUtils.WriteLine(ConsoleColor.Red, e.Message);
                exitCode = 1;
            }

            return exitCode;
        }


        /// <summary>
        /// Dumps an individual property from the data source to the console.
        /// </summary>
        /// <param name="context">The preprocessing context.</param>
        /// <returns></returns>
        private static int DumpProperty(PreprocessingContext context)
        {
            int exitCode = 0;

            try
            {
                PreprocessingProperty property = context.Properties[context.PropertyToExtract];

                if (null != property)
                {
                    string resolvedValue = context.ResolveContent(property.Value);

                    if (!string.IsNullOrEmpty(resolvedValue))
                    {
                        if (!string.IsNullOrEmpty(context.Delimiters))
                        {
                            foreach (string s in resolvedValue.Split(context.Delimiters.ToCharArray()))
                            {
                                Console.WriteLine(s.Trim());
                            }
                        }
                        else
                        {
                            Console.WriteLine(resolvedValue);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ConsoleUtils.WriteLine(ConsoleColor.Red, e.Message);
                exitCode = 1;
            }

            return exitCode;
        }


        /// <summary>
        /// Reads the settings from files.
        /// </summary>
        /// <param name="context">The preprocessing context.</param>
        /// <returns>Error code</returns>
        private static int ReadSettings(PreprocessingContext context)
        {
            // read data source settings files
            int exitCode = ReadSettingsFromDataSources(context);

            if (0 != exitCode)
                return exitCode;

            // read loose settings files (moved after data sources to allow overriding)
            exitCode = ReadEnvironmentSettingsFile(context);

            if (0 != exitCode)
                return exitCode;

            return exitCode;
        }


        /// <summary>
        /// Loads settings from the environment settings files.
        /// </summary>
        /// <param name="context">The preprocessing context.</param>
        /// <returns>Error code</returns>
        private static int ReadEnvironmentSettingsFile(PreprocessingContext context)
        {
            int exitCode = 0;

            bool isExtractingProperty = !string.IsNullOrEmpty(context.PropertyToExtract);

            // read settings file
            // if settings file was specified, but is empty or not found, prompt for it
            if (null != context.SettingsFiles && context.SettingsFiles.Count > 0)
            {
                for (int i = 0; i < context.SettingsFiles.Count; i++)
                {
                    string settingsFile = context.SettingsFiles[i] as string;
                    if (null != settingsFile)
                    {
                        settingsFile = settingsFile.Trim();

                        if (!FileUtils.IsHttpUrl(settingsFile) && !File.Exists(settingsFile))
                        {
                            Console.WriteLine(string.Format("\nSettings XML file not found: \"{0}\"", settingsFile));
                            if (!context.QuietMode)
                            {
                                while (true)
                                {
                                    Console.WriteLine("Enter path to settings XML file, leave blank if no settings are required.");
                                    Console.Write("Settings XML file: ");
                                    settingsFile = Console.ReadLine().Trim();

                                    if (settingsFile.Length == 0)
                                        break;

                                    if (File.Exists(settingsFile))
                                        break;

                                    Console.WriteLine(string.Format("Settings XML file not found: \"{0}\"", settingsFile));
                                }
                            }
                        }
                        else
                        {
                            if (!isExtractingProperty)
                                Console.WriteLine(string.Format("Settings XML file: \"{0}\"", settingsFile));
                        }

                        if (null != settingsFile && settingsFile.Length > 0)
                        {
                            try
                            {
                                context.Properties.ReadFromFile(settingsFile, context.EnvironmentName);
                            }
                            catch (XmlException xe)
                            {
                                ErrorInfo errorInfo = new ErrorInfo(ErrorCode.ErrorException, context.SourceFile);
                                errorInfo.Message = string.Format("Error parsing {0}: {1} at line {2}", settingsFile, xe.Message, xe.LineNumber);
                                context.Errors.Add(errorInfo);
                                exitCode = 1;
                                break;
                            }
                            catch (Exception e)
                            {
                                ErrorInfo errorInfo = new ErrorInfo(ErrorCode.ErrorException, context.SourceFile);
                                errorInfo.Message = string.Format("Error parsing {0}: {1}", settingsFile, e.Message);
                                context.Errors.Add(errorInfo);
                                exitCode = 1;
                                break;
                            }
                        }
                    }
                }
            }

            return exitCode;
        }


        /// <summary>
        /// Reads the settings from data sources.
        /// </summary>
        /// <param name="context">The preprocessing context.</param>
        /// <returns>Error code</returns>
        private static int ReadSettingsFromDataSources(PreprocessingContext context)
        {
            int exitCode = 0;

            bool isExtractingProperty = !string.IsNullOrEmpty(context.PropertyToExtract);

            // read settings file
            // if data sources were specified, but environment was not, prompt for it
            if (null != context.DataSources && context.DataSources.Count > 0)
            {
                foreach (DataSource dataSource in context.DataSources)
                {
                    if (!dataSource.Exists)
                    {
                        ErrorInfo errorInfo = new ErrorInfo(ErrorCode.ErrorFileNotFound, context.SourceFile);
                        errorInfo.Message = string.Format("Settings data source not found: \"{0}\"", dataSource);
                        context.Errors.Add(errorInfo);
                        exitCode = 1;
                        break;
                    }
                    else
                    {
                        if (!isExtractingProperty)
                            Console.WriteLine(string.Format("Settings data source: \"{0}\"", dataSource));
                    }

                    if (!string.IsNullOrEmpty(dataSource.Path))
                    {
                        if (!context.QuietMode && string.IsNullOrEmpty(context.EnvironmentName))
                        {
                            context.EnvironmentName = PromptForEnvironment(dataSource, context);
                        }

                        if (string.IsNullOrEmpty(context.EnvironmentName))
                        {
                            ErrorInfo errorInfo = new ErrorInfo(ErrorCode.ErrorException, context.SourceFile);
                            errorInfo.Message = string.Format("Error loading settings from {0}, environment name was not supplied.", dataSource);
                            context.Errors.Add(errorInfo);
                            exitCode = 1;
                            break;
                        }
                        else
                        {
                            try
                            {
                                context.Properties.LoadFromDataSource(dataSource, context);
                            }
                            catch (XmlException xe)
                            {
                                ErrorInfo errorInfo = new ErrorInfo(ErrorCode.ErrorException, context.SourceFile);
                                errorInfo.Message = string.Format("Error loading settings from {0}, {1} at line {2}", dataSource, xe.Message, xe.LineNumber);
                                context.Errors.Add(errorInfo);
                                exitCode = 1;
                                break;
                            }
                            catch (Exception e)
                            {
                                ErrorInfo errorInfo = new ErrorInfo(ErrorCode.ErrorException, context.SourceFile);
                                errorInfo.Message = string.Format("Error loading settings from {0}, {1}", dataSource, e.Message);
                                context.Errors.Add(errorInfo);
                                exitCode = 1;
                                break;
                            }
                        }
                    }
                }
            }

            return exitCode;
        }


        /// <summary>
        /// Prompts for environment by looking into spreadsheet and listing them out
        /// to the console.
        /// </summary>
        /// <param name="source">The data source.</param>
        /// <param name="context">The preprocessing context.</param>
        /// <returns></returns>
        private static string PromptForEnvironment(DataSource source, PreprocessingContext context)
        {
            SettingsLoader loader = new SettingsLoader(context);

            Console.WriteLine("Environment name was not passed.");
            Console.WriteLine("");

            ConsoleUtils.WriteLine(ConsoleColor.Cyan, "These are the environment columns found in the spreadsheet:");

            List<string> environments = loader.GetEnvironments(source);
            
            for (int j = 0; j < environments.Count; j++)
            {
                ConsoleUtils.WriteLine(ConsoleColor.Cyan, string.Format(" {0} - {1}", j, environments[j]));
            }

            ConsoleUtils.Write(ConsoleColor.Cyan, "Type the environment # to use and press Enter: ");

            string environmentIndexString = Console.ReadLine();
            int environmentIndex = -1;
            if (int.TryParse(environmentIndexString, out environmentIndex))
            {
                return environments[environmentIndex];
            }

            return null;
        }
    }
}
