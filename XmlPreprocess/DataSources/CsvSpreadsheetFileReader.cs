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

using XmlPreprocess.Util;

namespace XmlPreprocess.DataSources
{
    /// <summary>
    /// Reads a CSV file
    /// </summary>
    public class CsvSpreadsheetFileReader : IConfigSettingsReader
    {
        /// <summary>
        /// Read the settings from a CSV file into a DataTable.
        /// </summary>
        /// <param name="dataSource">The settings data source.</param>
        /// <param name="context">The preprocessing context.</param>
        /// <returns></returns>
        public DataTable ReadSettings(DataSource dataSource, PreprocessingContext context)
        {
            DataTable dt = new DataTable("Settings");

            TextReader textReader = null;
            FileStream fileStream = null;
            try
            {
                if (FileUtils.IsHttpUrl(dataSource.Path))
                {
                    string contents = FileUtils.DownloadFile(dataSource.Path);
                    textReader = new StringReader(contents);
                }
                else
                {
                    fileStream = File.Open(dataSource.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    textReader = new StreamReader(fileStream);
                }

                CsvFileReader csvReader = new CsvFileReader(textReader, "#");

                bool hasHeaderRowBeenEncountered = false;
                List<string> row = csvReader.ReadRow();
                while (null != row)
                {
                    if (!hasHeaderRowBeenEncountered)
                    {
                        // skip blank rows
                        if (row.Count > 0)
                        {
                            // Add a column for each cell in header row
                            for (int index = 0; index < row.Count; index++)
                            {
                                dt.Columns.Add(index.ToString(), typeof(string));
                            }
                            hasHeaderRowBeenEncountered = true;
                        }
                    }

                    if (hasHeaderRowBeenEncountered)
                    {
                        DataRow newRow = dt.NewRow();
                        for (int columnIndex = 0; columnIndex < row.Count; columnIndex++)
                        {
                            // this will throw out values in columns beyond the length of the header row
                            if (columnIndex < dt.Columns.Count)
                                newRow[columnIndex] = row[columnIndex];
                        }
                        dt.Rows.Add(newRow);
                    }

                    row = csvReader.ReadRow();
                }
            }
            finally
            {
                if (null != textReader)
                    textReader.Close();

                if (null != fileStream)
                    fileStream.Close();
            }

            return dt;
        }
    }
}
