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
using System.Text;
using System.Data;
using System.IO;

namespace XmlPreprocess.DataSources
{
    /// <summary>
    /// Read settings from an Excel spreadsheet
    /// </summary>
    public class SettingsLoader
    {
        /// <summary>
        /// Preprocessing context
        /// </summary>
        public PreprocessingContext Context { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsLoader"/> class.
        /// </summary>
        public SettingsLoader(PreprocessingContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Gets the environments from data table.
        /// </summary>
        /// <param name="settingsTable">The settings table.</param>
        /// <returns>list of strings containing the environment names</returns>
        public List<string> GetEnvironmentsFromDataTable(DataTable settingsTable)
        {
            List<string> environments = new List<string>();

            for (int columnIndex = Context.DefaultValueColumnIndex;
                 columnIndex < settingsTable.Columns.Count &&
                 !settingsTable.Rows[Context.EnvironmentNameRowIndex - 1].IsNull(columnIndex);
                 columnIndex++)
            {
                string environmentColumnName = (string)settingsTable.Rows[Context.EnvironmentNameRowIndex - 1][columnIndex];

                if (!string.IsNullOrEmpty(environmentColumnName))
                {
                    environments.Add(environmentColumnName);
                }
            }

            return environments;
        }


        /// <summary>
        /// Gets the environments.
        /// </summary>
        /// <param name="source">The settings source.</param>
        /// <returns>list of strings containing the environment names</returns>
        public List<string> GetEnvironments(DataSource source)
        {
            DataTable settingsTable = LoadDataTableFromDataSource(source);
            return GetEnvironmentsFromDataTable(settingsTable);
        }


        /// <summary>
        /// Read the contents of the Settings worksheet in the specified Excel file into a DataTable.
        /// Excel files up to Excel 2000 and XML Spreadsheet 2003 files are both supported.
        /// </summary>
        /// <param name="dataSource">Path to the source (file or db connection)</param>
        public DataTable LoadDataTableFromDataSource(DataSource dataSource)
        {
            IConfigSettingsReader reader = null;

            switch (dataSource.SourceType)
            {
                case DataSourceType.Spreadsheet:

                    switch (dataSource.SpreadsheetFormat)
                    {
                        case DataSourceSpreadsheetFormat.Xls: // Excel binary format
                            reader = new BinarySpreadsheetFileReader();
                            break;

                        case DataSourceSpreadsheetFormat.Csv: // CSV format
                            reader = new CsvSpreadsheetFileReader();
                            break;

                        case DataSourceSpreadsheetFormat.Xml: // XML Spreadsheet 2003 format
                            reader = new XmlSpreadsheetFileReader();
                            break;

                        default:
                            throw new ArgumentException(string.Format("Spreadsheet file type not supported: {0}", dataSource));
                    }

                    break;

                case DataSourceType.Database:
                    reader = new SqlDatabaseReader();
                    break;

                case DataSourceType.Custom:
                    reader = new CustomReader();
                    break;
            }


            if (!dataSource.Exists)
            {
                throw new FileNotFoundException("The specified input file " + dataSource.Path + " does not exist.", dataSource.Path);
            }

            DataTable dt = reader.ReadSettings(dataSource, Context);

            // some formats such as CSV are going to come in with env names in row 0 and first values in row 1
            // adjust them to look like the excel spreadsheets by putting in empty rows
            if (dataSource.RequiresRowFixup)
            {
                // Add rows so that environment names are found in _environmentNameRowIndex
                for (int i = 1; i < Context.EnvironmentNameRowIndex; i++)
                {
                    dt.Rows.InsertAt(dt.NewRow(), 0);
                }

                // Add rows so that first values are found in _firstValueRowIndex
                for (int i = Context.EnvironmentNameRowIndex + 1; i < Context.FirstValueRowIndex; i++)
                {
                    dt.Rows.InsertAt(dt.NewRow(), Context.EnvironmentNameRowIndex);
                }
            }

            return dt;
        }


        /// <summary>
        /// Read the contents of the Settings worksheet in the specified Excel file into a DataTable.
        /// Excel files up to Excel 2000 and XML Spreadsheet 2003 files are both supported.
        /// </summary>
        /// <param name="source">Data Source</param>
        /// <param name="properties">Properties to load</param>
        /// <param name="environmentName">Name of environment to load</param>
        public void LoadSettingsFromDataSource(DataSource source, PreprocessingProperties properties, string environmentName)
        {
            DataTable dt = LoadDataTableFromDataSource(source);
            LoadSettingsFromDataTable(dt, properties, environmentName);
        }


        /// <summary>
        /// Export the settings contained in a DataTable to multiple XML files, one per declared environment.
        /// </summary>
        /// <param name="settingsTable">DataTable containing settings values</param>
        /// <param name="properties">Properties dictionary to load</param>
        /// <param name="environmentName">Environment name to load</param>
        public void LoadSettingsFromDataTable(DataTable settingsTable, PreprocessingProperties properties, string environmentName)
        {
            int columnIndex = FindEnvironmentColumnIndex(settingsTable, environmentName);

            if (columnIndex < 0)
            {
                throw new ArgumentException(string.Format("Environment {0} was not found in settings spreadsheet", environmentName));
            }

            // Loop through the rows that contain settings and export each one to the XML file.
            for (int rowIndex = Context.FirstValueRowIndex - 1; rowIndex < settingsTable.Rows.Count; rowIndex++)
            {
                // Determine the setting name, or skip the row if there is no setting name value.
                if (settingsTable.Rows[rowIndex].IsNull(Context.SettingNameColumnIndex - 1))
                {
                    continue;
                }

                string settingName = (string)settingsTable.Rows[rowIndex][Context.SettingNameColumnIndex - 1];
                if (settingName.Trim().Length == 0)
                {
                    continue;
                }

                // Determine the setting value.
                string settingValue = GetSettingValue(settingsTable, rowIndex, columnIndex);

                properties.Add(settingName, settingValue);
            }
        }


        /// <summary>
        /// Finds the index of the environment column.
        /// </summary>
        /// <param name="settingsTable">The settings table.</param>
        /// <param name="environmentName">Name of the environment.</param>
        /// <returns></returns>
        private int FindEnvironmentColumnIndex(DataTable settingsTable, string environmentName)
        {
            int environmentColumnIndex = -1;

            for (int columnIndex = Context.DefaultValueColumnIndex;
                 columnIndex < settingsTable.Columns.Count &&
                 !settingsTable.Rows[Context.EnvironmentNameRowIndex - 1].IsNull(columnIndex);
                 columnIndex++)
            {
                string environmentColumnName = (string)settingsTable.Rows[Context.EnvironmentNameRowIndex - 1][columnIndex];

                if (environmentName.Equals(environmentColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    environmentColumnIndex = columnIndex;
                }
            }

            return environmentColumnIndex;
        }


        /// <summary>
        /// Finds the index of the row containing the desired setting.
        /// </summary>
        /// <param name="settingsTable">The settings table.</param>
        /// <param name="settingNameToFind">Name of the setting to find.</param>
        /// <param name="context">Preprocessing context.</param>
        /// <returns>row index of setting, or -1 if not found</returns>
        private int FindSettingRowIndex(DataTable settingsTable, string settingNameToFind, PreprocessingContext context)
        {
            int propertyRowIndex = -1;

            // Loop through the rows that contain settings and export each one to the XML file.
            for (int rowIndex = context.FirstValueRowIndex - 1; rowIndex < settingsTable.Rows.Count; rowIndex++)
            {
                // Determine the setting name, or skip the row if there is no setting name value.
                if (settingsTable.Rows[rowIndex].IsNull(context.SettingNameColumnIndex - 1))
                {
                    continue;
                }

                string settingName = (string)settingsTable.Rows[rowIndex][context.SettingNameColumnIndex - 1];
                if (settingName.Trim().Length == 0)
                {
                    continue;
                }

                if (settingName.Equals(settingNameToFind, StringComparison.OrdinalIgnoreCase))
                {
                    propertyRowIndex = rowIndex;
                    break;
                }
            }
            return propertyRowIndex;
        }


        /// <summary>
        /// Safely get a value for an environment setting. If no value is specified, try to get the default value. Otherwise, return
        /// an empty string.
        /// </summary>
        /// <param name="settingsTable">DataTable containing the setting values</param>
        /// <param name="rowIndex">Row index of the cell</param>
        /// <param name="columnIndex">Column index of the cell</param>
        /// <returns></returns>
        private string GetSettingValue(DataTable settingsTable, int rowIndex, int columnIndex)
        {
            string value = GetCellValue(settingsTable, rowIndex, columnIndex);

            if (string.IsNullOrEmpty(value))
            {
                value = GetCellValue(settingsTable, rowIndex, Context.DefaultValueColumnIndex - 1);
            }

            return value;
        }


        /// <summary>
        /// Gets the cell value.
        /// </summary>
        /// <param name="settingsTable">The settings table.</param>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="columnIndex">Index of the column.</param>
        /// <returns></returns>
        private string GetCellValue(DataTable settingsTable, int rowIndex, int columnIndex)
        {
            if (settingsTable.Rows[rowIndex].IsNull(columnIndex))
            {
                return string.Empty;
            }
            else
            {
                return ((string)settingsTable.Rows[rowIndex][columnIndex]).Trim();
            }
        }
    }
}