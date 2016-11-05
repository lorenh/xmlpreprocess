/*
 * Copyright (c) 2004-2016 Loren M Halvorson
 * This source is subject to the Microsoft Public License (Ms-PL).
 * See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
 * All other rights reserved.
 * Portions copyright 2002-2007 The Genghis Group (http://www.genghisgroup.com/)
 * Portions copyright 2007-08 Thomas F. Abraham.
 */

using System;
using System.IO;
using System.Net;

namespace XmlPreprocess.Util
{
    /// <summary>
    /// Collection of file utilities
    /// </summary>
    public class FileUtils
    {
        /// <summary>
        /// Load a file into a string.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <returns>Contents of file as a string.</returns>
        public static string LoadFile(string path)
        {
            string fileContents;

            using (StreamReader reader = File.OpenText(path))
            {
                fileContents = reader.ReadToEnd();
            }

            return fileContents;
        }

        /// <summary>
        /// Write a string to a file.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="content">Contents of file.</param>
        public static void WriteFile(string path, string content)
        {
            if (!string.IsNullOrEmpty(path) && null != content)
            {
                using (StreamWriter stream = File.CreateText(path))
                {
                    stream.Write(content);
                }
            }
        }


        /// <summary>
        /// Check to see if a path is an HTTP URL
        /// </summary>
        /// <param name="path">path to examine</param>
        /// <returns>True if path starts with "http:"</returns>
        public static bool IsHttpUrl(string path)
        {
            return path.StartsWith("http:", StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Download a file given a URL
        /// </summary>
        /// <param name="url">url to the file</param>
        /// <returns>The contents of the file</returns>
        public static string DownloadFile(string url)
        {
            string contents = null;

            // Open a connection
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            httpWebRequest.UseDefaultCredentials = true;

            // set timeout for 30 seconds
            httpWebRequest.Timeout = 30000;

            // Request response:
            using (HttpWebResponse webResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                // Open data stream:
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    // wrap stream with reader
                    using (StreamReader webContents = new StreamReader(webStream))
                    {
                        contents = webContents.ReadToEnd();
                    }
                }
            }

            return contents;
        }
    }
}