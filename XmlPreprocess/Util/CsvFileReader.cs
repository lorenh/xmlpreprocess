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
using System.IO;
using System.Text;

namespace XmlPreprocess.Util
{
    /// <summary>
    /// Class to read data from a CSV file
    /// </summary>
    public class CsvFileReader
    {
        private TextReader _reader = null;
        private string _commentPrefix = null;

        /// <summary>
        /// Construct a CSV reader 
        /// </summary>
        /// <param name="reader">TextReader containing CSV</param>
        public CsvFileReader(TextReader reader)
        {
            _reader = reader;
        }

        /// <summary>
        /// Construct a CSV reader 
        /// </summary>
        /// <param name="reader">TextReader containing CSV</param>
        /// <param name="commentPrefix">lines prefixed with this string will be ignored</param>
        public CsvFileReader(TextReader reader, string commentPrefix)
            : this(reader)
        {
            _commentPrefix = commentPrefix;
        }

        /// <summary>
        /// Reads a row of data from a CSV file
        /// </summary>
        /// <returns></returns>
        public List<string> ReadRow()
        {
            List<string> row = null;

            string line = _reader.ReadLine();
            if (line != null) // at end of file
            {
                row = new List<string>();

                // comment line: ignore lines starting with comment symbol
                if (string.IsNullOrEmpty(_commentPrefix) || !line.StartsWith(_commentPrefix)) 
                {
                    int index = 0;
                    while (index < line.Length)
                    {
                        row.Add(GetValue(line, ref index));

                        // Advance to the next comma, or to the end, whichever comes first
                        while (index < line.Length && line[index] != ',')
                        {
                            index++;
                        }

                        index++;
                    }
                }
            }
            return row;
        }


        /// <summary>
        /// Get a single value at the specified offset index, includes
        /// special processing for quoted values according to the CSV
        /// convention
        /// </summary>
        /// <param name="line"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private string GetValue(string line, ref int index)
        {
            string value = null;

            if (line[index] != '"')
            {
                int start = index;
                while (index < line.Length && line[index] != ',')
                {
                    index++;
                }
                value = line.Substring(start, index - start);
            }
            else // if value is quoted
            {
                int start = ++index; // advance past quote
                while (index < line.Length)
                {
                    if (line[index] == '"')
                    {
                        // look ahead to next character
                        // if we are at the end of the line,
                        // or the next character isn't another quote
                        // then we are at the end of the quoted field
                        int nextChar = index + 1;
                        if (nextChar >= line.Length
                            || line[nextChar] != '"')
                        {
                            break;
                        }
                        else
                        {
                            index++;
                        }
                    }
                    index++;
                }
                value = line.Substring(start, index - start);

                // double quotes are replaced with single quote
                // as per the CSV convention
                value = value.Replace("\"\"", "\"");
            }

            return value;
        }
    }
}
