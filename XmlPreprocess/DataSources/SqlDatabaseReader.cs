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
using System.Data.SqlClient;

namespace XmlPreprocess.DataSources
{
    /// <summary>
    /// Reads settings from database
    /// </summary>
    public class SqlDatabaseReader : IConfigSettingsReader
    {
        /// <summary>
        /// Used to temporarily store values while reading from db
        /// </summary>
        private class SqlSettings
        {
            public string SettingName { get; set; }
            public Dictionary<string, string> Values { get; set; }

            public SqlSettings(string settingName)
            {
                SettingName = settingName;
                Values = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Read the settings from a database into a DataTable.
        /// </summary>
        /// <param name="dataSource">The settings data source.</param>
        /// <param name="context">The preprocessing context.</param>
        /// <returns></returns>
        public DataTable ReadSettings(DataSource dataSource, PreprocessingContext context)
        {
            var environments = new List<string>();
            var rows = new List<SqlSettings>();

            string sql = @"select e.EnvironmentName, e.IsDefault, n.SettingName, n.SettingType, v.SettingValue from ConfigSettingNames n
                    left outer join ConfigSettingValues v on n.SettingID = v.SettingID
                    left join ConfigEnvironments e on e.EnvironmentID = v.EnvironmentID
                order by n.ViewOrder, e.ViewOrder";

            using (SqlConnection connection = new SqlConnection(dataSource.Path))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string envName = (!reader.IsDBNull(0) ? reader.GetString(0) : null);
                            bool isDefault = (!reader.IsDBNull(1) ? reader.GetBoolean(1) : false);
                            string settingName = (!reader.IsDBNull(2) ? reader.GetString(2) : null);
                            int settingType = (!reader.IsDBNull(3) ? reader.GetInt32(3) : 0);
                            string settingValue = (!reader.IsDBNull(4) ? reader.GetString(4) : null);

                            if (!string.IsNullOrEmpty(envName) && !environments.Contains(envName))
                            {
                                if (isDefault)
                                    environments.Insert(0, envName);
                                else
                                    environments.Add(envName);
                            }

                            SqlSettings row = rows.Find(delegate(SqlSettings s) { return s.SettingName.Equals(settingName, StringComparison.OrdinalIgnoreCase); });
                            if (null == row)
                            {
                                row = new SqlSettings(settingName);
                                rows.Add(row);
                            }

                            if (!string.IsNullOrEmpty(envName))
                                row.Values[envName] = settingValue;
                        }
                    }
                }
            }


            DataTable dt = new DataTable("Settings");

            // Add columns
            dt.Columns.Add("0", typeof(string)); // for setting name
            for (int i = 0; i < environments.Count; i++) // one for each environment
            {
                dt.Columns.Add((i+1).ToString(), typeof(string));
            }

            // Add first row
            DataRow newRow = dt.NewRow();
            newRow[0] = "";
            for (int i=0; i<environments.Count; i++)
            {
                newRow[i+1] = environments[i];
            }
            dt.Rows.Add(newRow);

            // Add values
            foreach (SqlSettings row in rows)
            {
                newRow = dt.NewRow();
                newRow[0] = row.SettingName;
                for (int i=0; i<environments.Count; i++)
                {
                    string envName = environments[i];
                    if (row.Values.ContainsKey(envName))
                    {
                        newRow[i + 1] = row.Values[envName];
                    }
                }
                dt.Rows.Add(newRow);
            }

            return dt;
        }
    }
}
