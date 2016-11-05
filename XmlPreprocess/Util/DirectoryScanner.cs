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
using System.Collections.Generic;

namespace XmlPreprocess.Util
{
    /// <summary>
    /// This class enables the use of wildcards to specify files to preprocess.
    /// </summary>
    /// 
    /// <example>
    ///  DirectoryScanner scanner = new DirectoryScanner();
    ///  scanner.Includes.Add("*.config");
    ///  scanner.Scan();
    ///  foreach (string filename in scanner.FileNames)
    ///  {
    ///      Console.WriteLine(filename);
    ///  }
    /// </example>
    /// 
    /// <remarks>
    /// It would be ideal to plug in the DirectoryScanner class from NAnt
    /// https://github.com/nant/nant/tree/master/src/NAnt.Core, which would also
    /// enable recursive patters such as "**\*.config" but as I read it,
    /// the GPL library that NAnt is released under is incompatible with MS-PL
    /// license that XmlPreprocess is released under.  So for now, in the interest
    /// of keeping things simple, this class simply scans a single directory.
    /// </remarks>
    public class DirectoryScanner
    {
        private List<string> _fileNames = null;

        /// <summary>
        /// The file specifications for which to scan (ex: "temp\*.config")
        /// </summary>
        public List<string> Includes { get; private set; }

        /// <summary>
        /// Construct an instance of a DirectoryScanner
        /// </summary>
        public DirectoryScanner()
        {
            Includes = new List<string>();
        }

        /// <summary>
        /// Scan the includes for all matching files
        /// </summary>
        public void Scan()
        {
            _fileNames = new List<string>();
            foreach (string include in Includes)
            {
                string path = include;

                if (!Path.IsPathRooted(path))
                    path = Path.Combine(Environment.CurrentDirectory, path);

                string searchPattern = null;

                int lastSlashPos = path.LastIndexOfAny(new char[] { '\\', '/' });
                if (lastSlashPos > -1)
                {
                    searchPattern = path.Substring(lastSlashPos + 1);
                    path = path.Substring(0, lastSlashPos);
                }

                string[] files = Directory.GetFiles(path, searchPattern);
                foreach (string file in files)
                {
                    _fileNames.Add(file);
                }
            }
        }

        /// <summary>
        /// The resulting file names after the scan is completed (calls Scan() if it wasn't called yet)
        /// </summary>
        public List<string> FileNames
        {
            get
            {
                if (_fileNames == null)
                {
                    Scan();
                }
                return _fileNames;
            }
        }
    }
}
