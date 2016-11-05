// (c) Copyright 2007-08 Thomas F. Abraham.
// This source is subject to the Microsoft Public License (Ms-PL).
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;

namespace XmlPreprocess.DataSources
{
    /// <summary>
    /// Reads a binary Excel workbook file (Office 2003 format or older).
    /// </summary>
    public class BinarySpreadsheetFileReader : IConfigSettingsReader
    {
        /// <summary>
        /// Read the settings from a binary Excel XLS file into a DataTable.
        /// </summary>
        /// <param name="dataSource">The settings data source.</param>
        /// <param name="context">The preprocessing context.</param>
        /// <returns></returns>
        public DataTable ReadSettings(DataSource dataSource, PreprocessingContext context)
        {
            string connectionString =
                string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=\"Excel 8.0;HDR=NO;MAXSCANROWS=1\"", dataSource.Path);

            DataSet ds = new DataSet();

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                OleDbDataAdapter da = new OleDbDataAdapter("SELECT * FROM [Settings$]", conn);
                da.Fill(ds);
            }

            DataTable dt = ds.Tables[0];

            dt.Columns.Add("Comment", typeof(string));

            return dt;
        }
    }
}

