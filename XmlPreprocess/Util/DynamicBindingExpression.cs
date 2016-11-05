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
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace XmlPreprocess.Util
{
    /// <summary>
    /// Performs a replacement in the document
    /// </summary>
    public interface IDynamicResolver
    {
        /// <summary>
        /// Replaces the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="value">The value.</param>
        /// <returns>The buffer with the replacement made</returns>
        string Replace(string buffer, string value);

        /// <summary>
        /// Should process this file?
        /// </summary>
        /// <param name="fileNameBeingProcessed">The file name being processed.</param>
        /// <returns>True if this file should be processed</returns>
        bool ShouldProcess(string fileNameBeingProcessed);
    }


    /// <summary>
    /// Represents a dynamically bound property.
    /// </summary>
    public abstract class DynamicBindingExpression : IDynamicResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicBindingExpression"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="files">The files.</param>
        public DynamicBindingExpression(string path, string files)
        {
            BindingPath = path;
            FileSpec = files;
        }

        /// <summary>
        /// Gets or sets the file spec.
        /// </summary>
        /// <value>The file spec.</value>
        public string FileSpec { get; private set; }

        /// <summary>
        /// Gets or sets the binding path.
        /// </summary>
        /// <value>The binding path.</value>
        public string BindingPath { get; private set; }

        /// <summary>
        /// Shoulds the file be processed.
        /// </summary>
        /// <remarks>
        /// http://www.codeproject.com/KB/recipes/wildcardtoregex.aspx
        /// </remarks>
        /// <param name="fileNameBeingProcessed">The file.</param>
        /// <returns></returns>
        public bool ShouldProcess(string fileNameBeingProcessed)
        {
            if (string.IsNullOrEmpty(FileSpec))
                return true;

            foreach (string filePattern in FileSpec.Split(';'))
            {
                Regex r = new Regex("^" + Regex.Escape(filePattern).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase);
                if (r.IsMatch(Path.GetFileName(fileNameBeingProcessed)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Parses the specified binding value.
        /// </summary>
        /// <param name="context">The preprocessing context.</param>
        /// <param name="bindingExpression">The binding value.</param>
        /// <returns></returns>
        public static DynamicBindingExpression Parse(PreprocessingContext context, string bindingExpression)
        {
            bool isValid = true;
            string bindingType = null;
            int firstEqualPosition = -1;
            string bindingValue = bindingExpression.Trim();

            if (!(bindingValue.StartsWith(context.TokenStart) && bindingValue.EndsWith(context.TokenEnd)))
                isValid = false;

            if (isValid)
            {
                bindingValue = bindingValue.Substring(2, bindingValue.Length - 3).Trim();

                firstEqualPosition = bindingValue.IndexOf('=');

                if (firstEqualPosition < 0)
                    isValid = false;
                else
                    bindingType = bindingValue.Substring(0, firstEqualPosition);
            }

            if (isValid)
            {
                if (!bindingType.Equals("XPath", StringComparison.OrdinalIgnoreCase) &&
                    !bindingType.Equals("RegEx", StringComparison.OrdinalIgnoreCase))
                {
                    isValid = false;
                }
            }

            if (!isValid)
            {
                throw new ArgumentException(string.Format("Binding value '{0}' was not properly formed, must be ${{XPath=XpathExpression [IncludedFiles=*.config;*.xml]}} or ${{Regex=regexPattern [IncludedFiles=*.config;*.xml]}}", bindingExpression));
            }

            string files = null;
            string path = null;

            ParseFilesAndPath(bindingValue, out files, out path);

            if (bindingType.Equals("XPath", StringComparison.OrdinalIgnoreCase))
            {
                XPathBinding xpathBinding = new XPathBinding(path, files);
                return xpathBinding;
            }
            else if (bindingType.Equals("Regex", StringComparison.OrdinalIgnoreCase))
            {
                RegexBinding xpathBinding = new RegexBinding(path, files);
                return xpathBinding;
            }

            return null;
        }

        /// <summary>
        /// Parse the files and path out of the binding expression
        /// </summary>
        /// <param name="bindingValue">The binding value.</param>
        /// <param name="files">The files.</param>
        /// <param name="path">The path.</param>
        private static void ParseFilesAndPath(string bindingValue, out string files, out string path)
        {
            int firstEqualPosition = bindingValue.IndexOf('=');
            const string filePattern = @"\sIncludedFiles\s*=\s*(?'value'.*)$";

            Match filesMatch = Regex.Match(bindingValue.TrimEnd(), filePattern);
            if (filesMatch.Success)
            {
                files = filesMatch.Groups["value"].Value;
                path = bindingValue.Substring(firstEqualPosition + 1, filesMatch.Index - firstEqualPosition).Trim();
            }
            else
            {
                files = null;
                path = bindingValue.Substring(firstEqualPosition + 1).Trim();
            }
        }

        /// <summary>
        /// Performs the replacement in the specified file.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="value">The value.</param>
        /// <returns>the modified buffer</returns>
        public abstract string Replace(string buffer, string value);
    }


    /// <summary>
    /// An XPath dynamic binding
    /// </summary>
    public class XPathBinding : DynamicBindingExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XPathBinding"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="files">The files.</param>
        public XPathBinding(string path, string files)
            : base(path, files)
        {
        }

        /// <summary>
        /// Replaces the specified file name.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override string Replace(string buffer, string value)
        {
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml(buffer);

            try
            {
                XPathNavigator nav = doc.CreateNavigator();

                nav.MoveToChild(XPathNodeType.Element); // (so namespace mappings are in scope when we select)

                XPathNodeIterator nodes = null;

                // If this document has a default namespace
                if (nav.LocalName == nav.Name && !string.IsNullOrEmpty(nav.NamespaceURI))
                {
                    const string defaultNamespace = "_xmlpp";
                    string xpath = Regex.Replace(BindingPath, @"(?<=/|^)(?=\w)(?!\w+:)", defaultNamespace + ":");

                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(nav.NameTable);
                    nsmgr.AddNamespace(defaultNamespace, nav.NamespaceURI);
                    XPathExpression expr = nav.Compile(xpath);
                    expr.SetContext(nsmgr);
                    nodes = nav.Select(expr);
                }
                else
                {
                    nodes = nav.Select(BindingPath, nav);
                }

                if (null != nodes)
                {
                    List<XPathNavigator> nodesToModify = new List<XPathNavigator>();
                    foreach (XPathNavigator node in nodes)
                    {
                        nodesToModify.Add(node);
                    }

                    foreach (XPathNavigator node in nodesToModify)
                    {
                        if (value.Equals("#remove", StringComparison.OrdinalIgnoreCase))
                        {
                            node.DeleteSelf();
                        }
                        else if (value.StartsWith("<") && value.EndsWith(">"))
                        {
                            node.InnerXml = value;
                        }
                        else
                        {
                            node.SetValue(value);
                        }
                    }

                    return PrettyPrint(doc, buffer);
                }
            }
            catch (XPathException e)
            {
                // XPath didn't evaluate to anything...let the user know
                Console.WriteLine(string.Format("Warning: The XPath expression {0} did not return any nodes. {1}", BindingPath, e.Message));
            }

            return buffer;
        }


        /// <summary>
        /// Pretties the print.
        /// </summary>
        /// <param name="doc">The doc.</param>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        private string PrettyPrint(XmlDocument doc, string buffer)
        {
            StringBuilder sb = new StringBuilder();

            // special code to preserve the XML Declaration if there is one
            Match xmlDeclarationMatch = Regex.Match(buffer, @"<\?\s*xml.*\?>");
            if (null != xmlDeclarationMatch && xmlDeclarationMatch.Success)
                sb.Append(xmlDeclarationMatch.Value);

            // Settings to "pretty print" the output
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            settings.OmitXmlDeclaration = true; // wants to put UTF-16 in for me
            settings.NewLineHandling = NewLineHandling.None;
            settings.IndentChars = "  ";

            using (XmlWriter xw = XmlWriter.Create(sb, settings))
            {
                doc.WriteTo(xw);
            }

            return sb.ToString();
        }
    }



    /// <summary>
    /// A Regular Expression dynamic binding
    /// </summary>
    public class RegexBinding : DynamicBindingExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegexBinding"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="files">The files.</param>
        public RegexBinding(string path, string files)
            : base(path, files)
        {
        }

        /// <summary>
        /// Replaces the specified file name.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override string Replace(string buffer, string value)
        {
            return Regex.Replace(buffer, BindingPath, value);
        }
    }
}
