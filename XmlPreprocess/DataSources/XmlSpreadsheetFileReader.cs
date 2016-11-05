/*
 * Special thanks to Thomas F. Abraham for the code this was based on
 *
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
using System.Xml.XPath;

using XmlPreprocess.Util;

namespace XmlPreprocess.DataSources
{
    /// <summary>
    /// Reads a SpreadsheetML XML file (Office 2003 format or older).
    /// </summary>
    public class XmlSpreadsheetFileReader : IConfigSettingsReader
    {
        /// <summary>
        /// Read the settings from Office 2003 SpreadsheetML XML file into a DataTable.
        /// </summary>
        /// <param name="dataSource">The settings data source.</param>
        /// <param name="context">The preprocessing context.</param>
        /// <returns></returns>
        public DataTable ReadSettings(DataSource dataSource, PreprocessingContext context)
        {
            // Create a new, empty DataTable to hold the data. The resulting structure of the DataTable will
            // be identical to that produced with a binary XLS file.
            DataTable dt = new DataTable("Settings");

            FileStream fileStream = null;
            try
            {
                // Read the SpreadsheetML XML file into a new XPathDocument.
                XPathDocument doc = null;
                if (FileUtils.IsHttpUrl(dataSource.Path))
                {
                    doc = new XPathDocument(dataSource.Path);
                }
                else
                {
                    fileStream = File.Open(dataSource.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    doc = new XPathDocument(fileStream);
                }

                XPathNavigator nav = doc.CreateNavigator();

                // Check the validity of the XML
                if (!IsValidSpreadsheetMl(nav))
                {
                    throw new ArgumentException("The input file is not a valid SpreadsheetML file or it is an unsupported version.");
                }

                // Create a namespace manager and register the SpreadsheetML namespace and prefix
                XmlNamespaceManager nm = new XmlNamespaceManager(nav.NameTable);
                nm.AddNamespace("ss", "urn:schemas-microsoft-com:office:spreadsheet");
                nm.AddNamespace("x", "urn:schemas-microsoft-com:office:excel");

                // Locate the Settings worksheet
                XPathNavigator worksheetNav = nav.SelectSingleNode("//ss:Worksheet[1]/ss:Table", nm);

                if (worksheetNav == null)
                {
                    throw new ArgumentException("The input file does not contain a valid worksheet.");
                }

                // If the first value row hasn't been overridden from command line,
                // then look in the XML Spreadsheet for a frozen row, indicating the
                // end of the header, and the beginning of the data
                if (CommandLine.FIRST_VALUE_ROW_DEFAULT == context.FirstValueRowIndex)
                {
                    XPathNavigator worksheetOptionsNav = nav.SelectSingleNode("//ss:Worksheet[1]/x:WorksheetOptions", nm);
                    if (worksheetOptionsNav != null)
                    {
                        XPathNavigator splitterNode = worksheetOptionsNav.SelectSingleNode("x:SplitHorizontal", nm);
                        if (splitterNode != null)
                        {
                            int splitHorizontal = 0;
                            if (Int32.TryParse(splitterNode.Value, out splitHorizontal))
                                context.FirstValueRowIndex = splitHorizontal + 1;
                        }
                    }
                }

                int expandedColumnCount = 0;
                string expandedColumnCountString = worksheetNav.GetAttribute("ExpandedColumnCount", "urn:schemas-microsoft-com:office:spreadsheet");
                int.TryParse(expandedColumnCountString, out expandedColumnCount);

                // Add a column for each cell
                for (int index = 0; index < expandedColumnCount; index++)
                {
                    dt.Columns.Add(index.ToString(), typeof(string));
                }

                // Select all of the rows in the worksheet
                XPathNodeIterator rowsIterator = worksheetNav.Select(".//ss:Row", nm);

                // Loop through the rows
                while (rowsIterator.MoveNext())
                {
                    // This is a workaround for a characteristic of the XML Spreadsheet 2003 format
                    // where rows can be sparsely populated, and an ss:Index attribute is used (ex: ss:Index="7")
                    // to indicate how many intervening empty rows were omitted
                    string rowIndexString = rowsIterator.Current.GetAttribute("Index", nm.LookupNamespace("ss"));
                    if (!string.IsNullOrEmpty(rowIndexString))
                    {
                        int rowIndex = -1;
                        if (Int32.TryParse(rowIndexString, out rowIndex))
                        {
                            int currentRowCount = dt.Rows.Count;
                            for (int i = currentRowCount; i < rowIndex - 1; i++)
                            {
                                dt.Rows.Add(dt.NewRow());
                            }
                        }
                    }

                    // Select all of the cells in the row
                    XPathNodeIterator cellsIterator = rowsIterator.Current.Select("ss:Cell", nm);

                    // Create a new DataRow to hold the incoming values
                    DataRow newRow = dt.NewRow();

                    // Loop through the cells
                    int columnIndex = 0;
                    while (cellsIterator.MoveNext())
                    {
                        // This is a workaround for a characteristic of the XML Spreadsheet 2003 format
                        // where cells can be sparsely populated. In the following example the cell at
                        // index 2 is skipped, and the cell at index 3 is in it's place with an Index
                        // attribute telling where it should go.
                        //
                        //   <Row ss:Height="25.5">
                        //    <Cell ss:StyleID="s73"><Data ss:Type="String">SomeSetting</Data></Cell>
                        //    <Cell ss:Index="3" ss:StyleID="s75"/>
                        //    <Cell ss:StyleID="s75"/>
                        //    <Cell ss:StyleID="s75"><Data ss:Type="String">data1</Data></Cell>
                        //    <Cell ss:StyleID="s74"><Data ss:Type="String">data2</Data></Cell>
                        //   </Row>

                        string indexString = cellsIterator.Current.GetAttribute("Index", nm.LookupNamespace("ss"));
                        if (!string.IsNullOrEmpty(indexString))
                        {
                            int index = -1;
                            if (Int32.TryParse(indexString, out index))
                            {
                                columnIndex = index - 1;
                            }
                        }


                        // Select the data value in the cell, if present
                        XPathNavigator dataNav = cellsIterator.Current.SelectSingleNode("ss:Data", nm);

                        if (null != dataNav)
                        {
                            string dataType = dataNav.GetAttribute("Type", nm.LookupNamespace("ss"));
                            if ("Boolean".Equals(dataType, StringComparison.OrdinalIgnoreCase))
                            {
                                if ("0".Equals(dataNav.Value))
                                    newRow[columnIndex] = "False";
                                else if ("1".Equals(dataNav.Value))
                                    newRow[columnIndex] = "True";
                                else
                                    newRow[columnIndex] = dataNav.Value;
                            }
                            else
                            {
                                newRow[columnIndex] = dataNav.Value;
                            }
                        }

                        columnIndex++;
                    }

                    // Add the newly populated DataRow to the DataTable
                    dt.Rows.Add(newRow);
                }
            }
            finally
            {
                if (null != fileStream)
                    fileStream.Close();
            }

            return dt;
        }

        /// <summary>
        /// Check for a valid Excel mso-application processing instruction.
        /// </summary>
        /// <param name="nav"></param>
        /// <returns></returns>
        private bool IsValidSpreadsheetMl(XPathNavigator nav)
        {
            bool isValid = false;

            XPathNodeIterator piIterator = nav.SelectChildren(XPathNodeType.ProcessingInstruction);

            while (piIterator.MoveNext())
            {
                if (string.Compare(piIterator.Current.LocalName, "mso-application", true) == 0)
                {
                    if (string.Compare(piIterator.Current.Value, "progid=\"Excel.Sheet\"", true) == 0)
                    {
                        isValid = true;
                    }
                }
            }

            return isValid;
        }
    }
}
