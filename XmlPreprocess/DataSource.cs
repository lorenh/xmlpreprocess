/*
 * Copyright (c) 2004-2016 Loren M Halvorson
 * This source is subject to the Microsoft Public License (Ms-PL).
 * See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
 * All other rights reserved.
 * Portions copyright 2002-2007 The Genghis Group (http://www.genghisgroup.com/)
 * Portions copyright 2007-08 Thomas F. Abraham.
 */

using System.IO;
using XmlPreprocess.Util;
namespace XmlPreprocess
{
    /// <summary>
    /// Specifies the type of data source
    /// </summary>
    public enum DataSourceType
    {
        /// <summary>
        /// Spreadsheet (XML, XSL, CSV)
        /// </summary>
        Spreadsheet,

        /// <summary>
        /// Database source
        /// </summary>
        Database,

        /// <summary>
        /// Custom source
        /// </summary>
        Custom
    }

    /// <summary>
    /// Specifies the type of data source
    /// </summary>
    public enum DataSourceSpreadsheetFormat
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Spreadsheet
        /// </summary>
        Xml,

        /// <summary>
        /// Excel
        /// </summary>
        Xls,

        /// <summary>
        /// Comma Separated Value
        /// </summary>
        Csv
    }


    /// <summary>
    /// A source of settings
    /// </summary>
    public class DataSource
    {
        /// <summary>
        /// Source information
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Type of source
        /// </summary>
        public DataSourceType SourceType { get; set; }

        /// <summary>
        /// Type of spreadsheet
        /// </summary>
        public DataSourceSpreadsheetFormat SpreadsheetFormat { get; set; }

        /// <summary>
        /// Create a data source
        /// </summary>
        /// <param name="path">path or connection string</param>
        /// <param name="sourceType">type of data source</param>
        /// <param name="spreadsheetFormat">the format if it is a spreadsheet</param>
        public DataSource(string path, DataSourceType sourceType, DataSourceSpreadsheetFormat spreadsheetFormat)
        {
            Path = path;
            SourceType = sourceType;
            SpreadsheetFormat = spreadsheetFormat;
        }

        /// <summary>
        /// Create a data source
        /// </summary>
        /// <param name="path">path or connection string</param>
        /// <param name="sourceType">type of data source</param>
        public DataSource(string path, DataSourceType sourceType) : this(path, sourceType, DataSourceSpreadsheetFormat.None)
        {
        }

        /// <summary>
        /// Does the data source exist (file types only)
        /// </summary>
        public bool Exists
        {
            get
            {
                if (SourceType == DataSourceType.Spreadsheet && !FileUtils.IsHttpUrl(Path))
                {
                    return File.Exists(Path);
                }
                return true;
            }
        }

        /// <summary>
        /// Does this data source need row fixups to be applied?
        /// </summary>
        public bool RequiresRowFixup
        {
            get
            {
                bool requiresFixup = 
                    SourceType == DataSourceType.Database ||
                    SourceType == DataSourceType.Custom ||
                    (SourceType == DataSourceType.Spreadsheet && SpreadsheetFormat == DataSourceSpreadsheetFormat.Csv);

                return requiresFixup;
            }
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns>the Source</returns>
        public override string ToString()
        {
            return Path;
        }
    }
}
