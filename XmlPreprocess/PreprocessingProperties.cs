/*
 * Copyright (c) 2004-2016 Loren M Halvorson
 * This source is subject to the Microsoft Public License (Ms-PL).
 * See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
 * All other rights reserved.
 * Portions copyright 2002-2007 The Genghis Group (http://www.genghisgroup.com/)
 * Portions copyright 2007-08 Thomas F. Abraham.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Data;

using XmlPreprocess.DataSources;
using XmlPreprocess.Util;

namespace XmlPreprocess
{
    /// <summary>
    /// Represents a property
    /// </summary>
    public class PreprocessingProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PreprocessingProperty"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public PreprocessingProperty(string key, string value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>The key.</value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the use count.
        /// </summary>
        /// <value>
        /// The use count.
        /// </value>
        public int UseCount { get; set; }

        /// <summary>
        /// Gets the dynamic resolver.
        /// </summary>
        /// <param name="context">The preeprocessing context.</param> 
        /// <returns></returns>
        public IDynamicResolver GetDynamicResolver(PreprocessingContext context)
        {
            return DynamicBindingExpression.Parse(context, Key) as IDynamicResolver;
        }
    }


    /// <summary>
    /// Properties container
    /// </summary>
    public class PreprocessingProperties
    {
        private List<PreprocessingProperty> _properties = new List<PreprocessingProperty>();
        private const string NEW_UNDEF_TOKEN = "#UNDEF";
        private string _undefToken = NEW_UNDEF_TOKEN;


        /// <summary>
        /// Initializes a new instance of the <see cref="PreprocessingProperties"/> class.
        /// </summary>
        /// <param name="fixFalse">if set to <c>true</c> [fix false].</param>
        public PreprocessingProperties(bool fixFalse)
        {
            if (!fixFalse)
                _undefToken = "FALSE";
        }


        /// <summary>
        /// Read an XML properties file
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        /// Property file is in the following format (the element names don't matter, just the "name" attributes)
        /// </para>
        /// <code>
        /// <![CDATA[
        /// <settings>
        ///  <property name="INSTALLER">TRUE</property>
        /// </settings>
        /// ]]>
        /// </code>
        /// Or optionally, you can include environments
        /// <code>
        /// <![CDATA[
        /// <settings>
        ///   <property name="Setting4">
        ///     <environment name="Default">setting4default</environment>
        ///     <environment name="Local">localhost</environment>
        ///     <environment name="Test">testserver</environment>
        ///     <environment name="Integration">intgserver</environment>
        ///     <environment name="Production">prodserver</environment>
        ///   </property>
        /// </settings>
        /// ]]>
        /// </code>
        /// </remarks>
        ///
        /// <param name="file">name of property file</param>
        /// <param name="environmentName">name of environment (optional)</param>
        public void ReadFromFile(string file, string environmentName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);

            foreach (XmlNode propertyNode in doc.DocumentElement.ChildNodes)
            {
                XmlElement propertyElement = propertyNode as XmlElement;
                if (null != propertyElement)
                {
                    string propertyName = propertyElement.GetAttribute("name");
                    string propertyValue = null;

                    bool hasChildElements = false;

                    XmlNodeList environmentElements = propertyElement.ChildNodes;
                    if (environmentElements != null && environmentElements.Count > 0)
                    {
                        // Test to see if this element has any child _Elements_
                        foreach (XmlNode node in environmentElements)
                        {
                            if (node.NodeType == XmlNodeType.Element)
                            {
                                hasChildElements = true;
                                break;
                            }
                        }
                    }

                    if (!hasChildElements)
                    {
                        propertyValue = propertyElement.InnerText;
                    }
                    else
                    {
                        foreach (XmlNode environmentNode in environmentElements)
                        {
                            XmlElement environmentElement = environmentNode as XmlElement;
                            if (null != environmentElement)
                            {
                                string env = environmentElement.GetAttribute("name");
                                if (env.Equals(environmentName, StringComparison.OrdinalIgnoreCase))
                                {
                                    propertyValue = environmentElement.InnerText;
                                    break;
                                }

                                if (env.StartsWith("default", StringComparison.OrdinalIgnoreCase))
                                    propertyValue = environmentElement.InnerText;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(propertyName))
                        Add(propertyName, propertyValue);
                }
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public List<PreprocessingProperty>.Enumerator GetEnumerator()
        {
            return _properties.GetEnumerator();
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// 	<c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(string key)
        {
            return (null != this[key]);
        }


        /// <summary>
        /// Gets the <see cref="XmlPreprocess.PreprocessingProperty"/> with the specified key. Case insensitive.
        /// </summary>
        /// <value></value>
        public PreprocessingProperty this[string key]
        {
            get
            {
                return _properties.Find(delegate(PreprocessingProperty item) { return item.Key.Equals(key, StringComparison.OrdinalIgnoreCase); });
            }
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            PreprocessingProperty property = this[key];
            if (null != property)
            {
                return _properties.Remove(property);
            }
            return false;
        }


        /// <summary>
        /// Adds the property with protection from already exists exception.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            if (ContainsKey(key))
                Remove(key);

            if (null != value && !value.Equals(_undefToken, StringComparison.OrdinalIgnoreCase)
                && !value.Equals(NEW_UNDEF_TOKEN, StringComparison.OrdinalIgnoreCase))
            {
                _properties.Add(new PreprocessingProperty(key, RemoveQuotes(value)));
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                return _properties.Count;
            }
        }

        /// <summary>
        /// Parse any command line properties, allowing them
        /// to override the file base ones
        /// </summary>
        /// <param name="context">Preprocessing context</param>
        /// <param name="array">ArrayList read from command line</param>
        public void AddPropertiesFromArrayList(PreprocessingContext context, ArrayList array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                string property = (string)array[i];
                if (null != property && property.Length > 0)
                {
                    string propertyName = null;
                    string propertyValue = "";

                    int equalPos = -1;
                    if (context.IsDynamicProperty(property))
                        equalPos = property.LastIndexOf('=');
                    else
                        equalPos = property.IndexOf('=');

                    if (-1 == equalPos)
                    {
                        propertyName = property;
                    }
                    else
                    {
                        propertyName = property.Substring(0, equalPos);
                        propertyValue = property.Substring(equalPos + 1);
                    }

                    Add(propertyName, propertyValue);
                }
            }
        }



        /// <summary>
        /// Remove quotes from a string
        /// </summary>
        /// <param name="propertyValue">string to remove quotes from.</param>
        /// <returns>string value with quotes removed</returns>
        private string RemoveQuotes(string propertyValue)
        {
            if (propertyValue.Length > 1 &&
                (propertyValue.StartsWith("\"") && propertyValue.EndsWith("\"")) ||
                (propertyValue.StartsWith("\'") && propertyValue.EndsWith("\'")))
            {
                propertyValue = propertyValue.Substring(1, propertyValue.Length - 2);
            }
            return propertyValue;
        }


        /// <summary>
        /// Reads from spreadsheet.
        /// </summary>
        /// <param name="source">The settings source.</param>
        /// <param name="context">the context.</param>
        public void LoadFromDataSource(DataSource source, PreprocessingContext context)
        {
            SettingsLoader loader = new SettingsLoader(context);
            loader.LoadSettingsFromDataSource(source, this, context.EnvironmentName);
        }


        /// <summary>
        /// Resolves content that could possibly contain macros
        /// </summary>
        /// <param name="context">The preprocessing context.</param>
        /// <param name="content">The content that could contain macros.</param>
        /// <exception cref="UndefinedSettingException">Throws this exception if it encounters an undefined setting</exception>
        /// <returns>the resolved content</returns>
        public string ResolveContent(PreprocessingContext context, string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                bool isSameTokenUsedForStartAndEnd = context.TokenStart.Equals(context.TokenEnd, StringComparison.OrdinalIgnoreCase);

                // Look for start of tokens, order depends on type of token used
                int macroPosition = -1;

                // Evaluate from back to front if tokens are not equal 
                // this enables nested tokens such as this: ${PROPERTY_${MACHINE}}
                // Evaluate from front to back if start and end tokens are equal...nested properties with
                // custom tokens that match is not supported. You can't do this: #PROPERTY_#MACHINE##
                if (!isSameTokenUsedForStartAndEnd)
                    macroPosition = content.LastIndexOf(context.TokenStart);
                else
                    macroPosition = content.IndexOf(context.TokenStart);

                while (macroPosition > -1)
                {
                    int endMacroPosition = content.IndexOf(context.TokenEnd, macroPosition + context.TokenStart.Length);
                    string macro = content.Substring(macroPosition, endMacroPosition - macroPosition + 1);
                    string key = macro.Substring(context.TokenStart.Length, macro.Length - (context.TokenStart.Length + context.TokenEnd.Length)).Trim();

                    PreprocessingProperty property = this[key];
                    if (null == property)
                    {
                        throw new UndefinedSettingException(string.Format("{0} was not defined", key), key);
                    }

                    string val = property.Value;
                    content = content.Replace(macro, val);

                    if (!isSameTokenUsedForStartAndEnd)
                        macroPosition = content.LastIndexOf(context.TokenStart);
                    else
                        macroPosition = content.IndexOf(context.TokenStart);
                }
            }

            return content;
        }

    }

    /// <summary>
    /// Exception thrown in the case of referencing an undefined setting
    /// </summary>
    public class UndefinedSettingException : Exception
    {
        private string _settingName;

        /// <summary>
        /// Initializes a new instance of the <see cref="UndefinedSettingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="settingName">The setting name.</param>
        public UndefinedSettingException(string message, string settingName) : base(message)
        {
            _settingName = settingName;
        }

        /// <summary>
        /// Gets or sets the undefined setting name
        /// </summary>
        /// <value>The setting name.</value>
        public string SettingName
        {
            get { return _settingName; }
        }
    }
}
