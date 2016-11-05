/*
 * Copyright (c) 2004-2016 Loren M Halvorson
 * This source is subject to the Microsoft Public License (Ms-PL).
 * See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
 * All other rights reserved.
 * Portions copyright 2002-2007 The Genghis Group (http://www.genghisgroup.com/)
 * Portions copyright 2007-08 Thomas F. Abraham.
 */

using System;
using System.Data;

namespace XmlPreprocess.DataSources
{
    /// <summary>
    /// Defines a Configuration settings reader
    /// </summary>
    public interface IConfigSettingsReader
    {
        /// <summary>
        /// Reads the settings into a DataTable
        /// </summary>
        /// <param name="dataSource">The settings data source.</param>
        /// <param name="context">The preprocessing context.</param>
        /// <returns>The settings in DataTable form</returns>
        DataTable ReadSettings(DataSource dataSource, PreprocessingContext context);
    }
}
