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
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using XmlPreprocess.Util;

namespace XmlPreprocess
{
    /// <summary>
    /// Simple XML file preprocesser.
    /// </summary>
    ///
    /// <remarks>
    /// <para>
    /// <b>Built-In properties</b>
    /// </para>
    /// <para>
    /// These properties can be referred to within the XML file.
    /// </para>
    /// <list type="table">
    /// <listheader>
    ///   <term>Property Name</term>
    ///   <description>Description</description>
    /// </listheader>
    /// <item>
    ///   <term>_xml_preprocess</term>
    ///   <description>implicit property automatically defined whenever running under the xml preprocessor</description>
    /// </item>
    /// <item>
    ///   <term>_dest_dir</term>
    ///   <description>Directory portion of destination file. Example: d:\inetpub\wwwroot\myweb</description>
    /// </item>
    /// <item>
    ///   <term>_machine_name</term>
    ///   <description>Name of machine. Example: vmspap02caa</description>
    /// </item>
    /// <item>
    ///   <term>_machine_id</term>
    ///   <description>Numeric portion of machine name. Example: 02</description>
    /// </item>
    /// <item>
    ///   <term>_os_platform</term>
    ///   <description>Machine OS platform. Example: Win32NT</description>
    /// </item>
    /// <item>
    ///   <term>_os_version</term>
    ///   <description>Machine OS version. Example: 5.1.2600.0</description>
    /// </item>
    /// <item>
    ///   <term>_system_dir</term>
    ///   <description>Machine's system directory. Example: C:\WINNT\System32</description>
    /// </item>
    /// <item>
    ///   <term>_current_dir</term>
    ///   <description>Machine's current directory. Example: "C:\Installs</description>
    /// </item>
    /// <item>
    ///   <term>_clr_version</term>
    ///   <description>Gets the major, minor, build, and revision numbers of the common language runtime. Example: 1.0.3705.288</description>
    /// </item>
    /// <item>
    ///   <term>_user_name</term>
    ///   <description>Gets the user name of the person who started the current thread. Example: EFudd</description>
    /// </item>
    /// <item>
    ///   <term>_user_domain_name</term>
    ///   <description>Gets the network domain name associated with the current user. Example: MyDomain</description>
    /// </item>
    /// <item>
    ///   <term>_user_interactive</term>
    ///   <description>Gets a value indicating whether the current process is running in user interactive mode. Example: True</description>
    /// </item>
    /// <item>
    ///   <term>_system_date</term>
    ///   <description>System date. Example: 8/5/2003</description>
    /// </item>
    /// <item>
    ///   <term>_system_time</term>
    ///   <description>System time. Example: 9:50 AM</description>
    /// </item>
    /// <item>
    ///   <term>_framework_dir</term>
    ///   <description>.NET Framework directory. Example: C:\WINNT\Microsoft.NET\Framework\v1.0.3705</description>
    /// </item>
    /// <item>
    ///   <term>_env_*</term>
    ///   <description>Environment variables. Example: _ENV_PATH contains system's path environment variable.</description>
    /// </item>
    /// </list>
    ///
    /// <b>Version History</b>
    /// <list type="table">
    /// <listheader>
    ///   <term>Date</term>
    ///   <description>Changes</description>
    /// </listheader>
    /// <item>
    ///   <term>2003-07-30</term>
    ///   <description>LH: Initial release</description>
    /// </item>
    /// <item>
    ///   <term>2003-08-01</term>
    ///   <description>
    ///      LH: Enhanced match patterns so whitespace around macro comments does
    ///      not get rendered. Made search algoritm more robust with some limited
    ///      error checking for mismatched ifdef/endif). Fixed issue that else's
    ///      and endif's were being searched for globally instead of only within
    ///      the body of a single ifdef/endif pair.
    ///   </description>
    /// </item>
    /// <item>
    ///   <term>2003-08-05</term>
    ///   <description>
    ///      LH: Added several useful built-in properties.
    ///   </description>
    /// </item>
    /// <item>
    ///   <term>2003-09-12</term>
    ///   <description>
    ///      LH: Removed case sensitivity. Made macro resolution
    ///      recursive (one macro can contain another...as long
    ///      as it is not circular).
    ///   </description>
    /// </item>
    /// </list>
    /// </remarks>
    public class XmlPreprocessor
    {
        private static string patternDefine = @"<!--\s*#\s*define\s+(?<expression>[^\r\n]+)\s*=\s*(?<value>.*?)-->";
        private static string patternIfdef = @"(?:\r\n|\n)[\f\t\v\x85\p{Z}]*<!--\s*(?<keyword>#*\s*if|#*\s*ifdef)\s+(?<condition>.*)-->";
        private static string patternElse = @"(?:\r\n|\n)[\f\t\v\x85\p{Z}]*<!--\s*#*\s*else\s*-->";
        //private static string patternElif = @"(?:\r\n|\n)[\f\t\v\x85\p{Z}]*<!--\s*#*\s*elif\s*(?<condition>\S*)\s*-->";
        private static string patternEndif = @"(?:\r\n|\n)[\f\t\v\x85\p{Z}]*<!--\s*#*\s*endif\s*-->";
        private static string patternInclude = @"<!--\s*#\s*include\s+[""|'](?<file>[^""|']*)[""|']\s*(?:xpath\s*=\s*[""|'](?<xpath>.*)[""|']\s*)*-->";
        private static string patternForEach = @"^(?<whitespace>[\r\n\s]*)(?<foreachstart>\#\s*foreach\s*\(\s*)(?<name>.+)(?<foreachend>\s*\)\s*)";

        private Regex regexDefine = new Regex(patternDefine, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private Regex regexIfdef = new Regex(patternIfdef, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private Regex regexElse = new Regex(patternElse, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        //private Regex regexElif = new Regex(patternElif, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private Regex regexEndif = new Regex(patternEndif, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private Regex regexInclude = new Regex(patternInclude, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private Regex regexForEach = new Regex(patternForEach, RegexOptions.Singleline |  RegexOptions.IgnoreCase);

        /// <summary>
        /// Construct an instance of the XmlPreprocessor class
        /// </summary>
        public XmlPreprocessor()
        {
        }

        /// <summary>
        /// Deploy an XML file using preprocessor.
        /// </summary>
        /// <param name="context">Preprocessing Context</param>
        public int Preprocess(PreprocessingContext context)
        {
            // load the file into a string
            string source = FileUtils.LoadFile(context.SourceFile);

            // add all of the built-in properties like _current_dir
            context.AddBuiltInProperties();

            // pre-load embedded defines in case there are any defines used in includes
            ExtractDefines(context, source);

            // pull-in any externally included files
            string dest = ProcessIncludes(context, source);

            // again scan for embedded defines in case the includes contained them
            ExtractDefines(context, dest);

            // process the entire file
            dest = Process(context, dest);

            if (context.ValidateSettingsExist && context.Errors.Count > 0)
            {
                // don't write file, just return error code
                return 2;
            }
            else
            {
                if (context.ValidateXmlWellFormed)
                {
                    try
                    {
                        XmlDocument wellFormednessCheckingDocument = new XmlDocument();
                        wellFormednessCheckingDocument.LoadXml(dest);
                    }
                    catch (XmlException e)
                    {
                        string errorFile = context.DestinationFile + ".error";
                        FileUtils.WriteFile(errorFile, dest);

                        ErrorInfo errorInfo = new ErrorInfo(ErrorCode.ErrorNotWellFormed, context.SourceFile);
                        errorInfo.Message = string.Format("Output was not well-formed: {0}, A copy of the file that was not well-formed was saved to {1}.", e.Message, errorFile);
                        context.Errors.Add(errorInfo);

                        return 1;
                    }
                }


                // Only write out the file if the destination is different
                // than the source or if changes were made
                if (!context.DestinationFile.Equals(context.SourceFile) || !source.Equals(dest))
                {
                    FileUtils.WriteFile(context.DestinationFile, dest);
                }
            }
            return 0;
        }


        /// <summary>
        /// Extracts the #defines from the document
        /// </summary>
        /// <param name="context">The Preprocessing context.</param>
        /// <param name="content">The content.</param>
        private void ExtractDefines(PreprocessingContext context, string content)
        {
            for (Match matchDefine = regexDefine.Match(content);
                matchDefine.Success;
                matchDefine = matchDefine.NextMatch())
            {
                Group expressionGroup = matchDefine.Groups["expression"];
                Group valueGroup = matchDefine.Groups["value"];
                if (null != expressionGroup && null != valueGroup &&
                    !string.IsNullOrEmpty(expressionGroup.Value) &&
                    !string.IsNullOrEmpty(valueGroup.Value))
                {
                    string expression = expressionGroup.Value.Trim();
                    string value = valueGroup.Value.Trim();
                    context.Properties.Add(expression, value);
                }
            }
        }


        /// <summary>
        /// Processes the #includes
        /// </summary>
        /// <param name="context">The Preprocessing context.</param>
        /// <param name="content">The content.</param>
        private string ProcessIncludes(PreprocessingContext context, string content)
        {
            while (true)
            {
                Match matchInclude = regexInclude.Match(content);
                if (!matchInclude.Success)
                {
                    break;
                }

                Group fileGroup = matchInclude.Groups["file"];
                Group xpathGroup = matchInclude.Groups["xpath"];

                if (null != fileGroup && !string.IsNullOrEmpty(fileGroup.Value))
                {
                    string file = fileGroup.Value.Trim();
                    // replace any macros in file name
                    string resolvedFile = ResolveProperties(file, context, null);

                    if (context.Errors.Count > 0)
                    {
                        return content;
                    }

                    // turn relative paths into fully qualified relative to the location of source file
                    if (!Path.IsPathRooted(resolvedFile))
                    {
                        resolvedFile = Path.Combine(Path.GetDirectoryName(context.SourceFile), resolvedFile);
                    }

                    if (!File.Exists(resolvedFile))
                    {
                        throw new FileNotFoundException(string.Format("Could not find file {0}", resolvedFile), resolvedFile);
                    }

                    string source = "";

                    if (null != xpathGroup && !string.IsNullOrEmpty(xpathGroup.Value))
                    {
                        string xpathUnresolved = xpathGroup.Value.Trim();
                        // replace any macros in xpath
                        string xpath = ResolveProperties(xpathUnresolved, context, null);

                        if (context.Errors.Count > 0)
                        {
                            return content;
                        }

                        source = GetXmlContentFromIncludeFile(resolvedFile, xpath);
                    }
                    else
                    {
                        // Load file
                        source = FileUtils.LoadFile(resolvedFile);
                    }

                    // Insert contents
                    content = content.Substring(0, matchInclude.Index) +
                                source +
                                content.Substring(matchInclude.Index + matchInclude.Length);
                }
            }

            return content;
        }


        /// <summary>
        /// Gets the XML content from include file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="bindingPath">The binding path.</param>
        /// <returns></returns>
        public string GetXmlContentFromIncludeFile(string fileName, string bindingPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.Load(fileName);

            XPathNavigator nav = doc.CreateNavigator();

            nav.MoveToChild(XPathNodeType.Element); // (so namespace mappings are in scope when we select)

            XPathNavigator node = null;

            // If this document has a default namespace
            if (nav.LocalName == nav.Name && !string.IsNullOrEmpty(nav.NamespaceURI))
            {
                const string defaultNamespace = "_xmlpp";
                string xpath = Regex.Replace(bindingPath, @"(?<=/|^)(?=\w)(?!\w+:)", defaultNamespace + ":");

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(nav.NameTable);
                nsmgr.AddNamespace(defaultNamespace, nav.NamespaceURI);
                XPathExpression expr = nav.Compile(xpath);
                expr.SetContext(nsmgr);
                node = nav.SelectSingleNode(expr);
            }
            else
            {
                node = nav.SelectSingleNode(bindingPath, nav);
            }

            if (null != node)
            {
                return node.InnerXml;
            }

            return "";
        }


        /// <summary>
        /// Process the file.
        /// </summary>
        /// <param name="context">The preprocessing context.</param>
        /// <param name="content">Contents of a file as a string.</param>
        /// <returns>Returns the file contents with preprocessing applied.</returns>
        private string Process(PreprocessingContext context, string content)
        {
            StringBuilder result = new StringBuilder();
            int offset = 0;

            if (context.NoDirectives)
            {
                result.Append(ResolveProperties(content, context));
            }
            else
            {
                for (Match matchIfdef = regexIfdef.Match(content);
                    matchIfdef.Success;
                    matchIfdef = matchIfdef.NextMatch())
                {
                    if (context.PreserveMarkup)
                        result.Append(content.Substring(offset, (matchIfdef.Index + matchIfdef.Length) - offset));
                    else
                        result.Append(content.Substring(offset, matchIfdef.Index - offset));

                    offset = matchIfdef.Index + matchIfdef.Length;

                    Match matchEndif = regexEndif.Match(content, offset);
                    if (matchEndif.Success)
                    {
                        string condition = RemoveMacro(context, matchIfdef.Groups["condition"].Value);
                        string keyword = RemoveMacro(context, matchIfdef.Groups["keyword"].Value);
                        string body = content.Substring(offset, matchEndif.Index - offset);

                        // Check to see if there is another ifdef in the body,
                        // if so, it is an error
                        Match matchErroneousIfdef = regexIfdef.Match(body);
                        if (matchErroneousIfdef.Success)
                        {
                            throw new Exception("Comments are malformed, endif missing.");
                        }

                        offset = matchEndif.Index + matchEndif.Length;

                        result.Append(ProcessBody(context, keyword, condition, body));

                        if (context.PreserveMarkup)
                        {
                            result.Append(content.Substring(matchEndif.Index, matchEndif.Length));
                        }
                    }
                    else
                    {
                        throw new Exception("Comments are malformed, no endif found.");
                    }
                }

                result.Append(content.Substring(offset));
            }

            return ProcessDynamicallyBoundProperties(context, result.ToString());
        }


        /// <summary>
        /// Processes the dynamically bound properties.
        /// </summary>
        /// <remarks>
        /// This is a fairly inefficient implementation for the first cut, it would be
        /// nicer if a buffer could be reused to avoid reallocating huge strings for
        /// every single property, especially in the case of XML.
        /// </remarks>
        /// <param name="context">The Preprocessing context.</param>
        /// <param name="content">The content.</param>
        /// <returns>The buffer with any dynamically bound properties replaced</returns>
        private string ProcessDynamicallyBoundProperties(PreprocessingContext context, string content)
        {
            string resultBuffer = content;

            foreach (PreprocessingProperty property in context.Properties)
            {
                if (context.IsDynamicProperty(property.Key))
                {
                    IDynamicResolver resolver = property.GetDynamicResolver(context);
                    if (resolver.ShouldProcess(context.SourceFile))
                    {
                        string replacementValue = ResolveProperties(property.Value, context);
                        resultBuffer = resolver.Replace(resultBuffer, replacementValue);
                    }
                }
            }

            return resultBuffer;
        }


        /// <summary>
        /// Process the body of a macro expression.
        /// </summary>
        /// <param name="context">The preprocessing context.</param>
        /// <param name="keyword">Keyword (#if or #ifdef).</param>
        /// <param name="condition">Condition to test.</param>
        /// <param name="body">Body of macro expression.</param>
        /// <returns>Returns the preprocessed result of the macro expression.</returns>
        private string ProcessBody(PreprocessingContext context, string keyword, string condition, string body)
        {
            string ifbody = null;
            string elsebody = null;
            bool hasElse = false;
            bool conditionTrue = false;

            ////////////
            //for (Match matchElif = regexElif.Match(body);
            //    matchElif.Success;
            //    matchElif = matchElif.NextMatch())
            //{
            //}
            ////////////

            Match matchElse = regexElse.Match(body);
            if (matchElse.Success)
            {
                hasElse = true;

                ifbody = body.Substring(0, matchElse.Index);
                elsebody = body.Substring(matchElse.Index + matchElse.Length);
            }
            else
            {
                ifbody = body;
            }

            string result = "";

            conditionTrue = false;
            if (keyword.EndsWith("ifdef"))
            {
                conditionTrue = (!string.IsNullOrEmpty(condition) && context.Properties.ContainsKey(condition));
            }
            else
            {
                DynamicEvaluator evaluator = new DynamicEvaluator();
                conditionTrue = evaluator.EvaluateToBool(context, condition);
            }

            if (context.PreserveMarkup)
            {
                string whitespacePrefix = "";
                for (int i=0; i<ifbody.Length; i++)
                {
                    if (!Char.IsWhiteSpace(ifbody[i]))
                    {
                        whitespacePrefix = ifbody.Substring(0, i);
                        break;
                    }
                }
                string commentedOutIfBody = RemoveComment(ifbody, false).Trim();
                if (null != commentedOutIfBody && commentedOutIfBody.Length > 0)
                {
                    if (commentedOutIfBody.IndexOf('\n') > -1) // multi-line
                        result += whitespacePrefix + "<!--" + whitespacePrefix + commentedOutIfBody + whitespacePrefix + "-->";
                    else // single line
                        result += whitespacePrefix + "<!-- " + commentedOutIfBody + " -->";
                }

                if (!hasElse) // always add an else
                {
                    if (keyword.StartsWith("#"))
                        result += whitespacePrefix + "<!-- #else -->";
                    else
                        result += whitespacePrefix + "<!-- else -->";
                }
                else
                {
                    result += body.Substring(matchElse.Index, matchElse.Length);
                }
            }

            if (conditionTrue)
            {
                if (context.PreserveMarkup && !hasElse)
                    result += ResolveProperties(RemoveComment(ifbody, true), context);

                if (!(context.PreserveMarkup && !hasElse))
                    result += ResolveProperties(RemoveComment(ifbody, true), context);

                if (context.PreserveMarkup && hasElse && 0 == ifbody.Trim().Length)
                    result += "<!-- " + RemoveComment(elsebody, false) + " -->";
            }
            else
            {
                if (hasElse)
                    result += elsebody;
            }

            return result;
        }


        /// <summary>
        /// Remove XML comments from a section of content.
        /// </summary>
        /// <param name="content">Content from which to remove comments.</param>
        /// <param name="replaceNestedComments">Should nested comments be replaced.</param>
        /// <returns>Content without comments.</returns>
        private string RemoveComment(string content, bool replaceNestedComments)
        {
            string result = null;
            if (null != content)
            {
                result = Regex.Replace(content, @"<!--\s*", "");
                result = Regex.Replace(result, @"\s*-->", "");

                if (replaceNestedComments)
                {
                    // replace spaced-out comments with real ones
                    result = result.Replace(@"< ! - -", "<!--");
                    result = result.Replace(@"- - >", "-->");
                }
            }
            return result;
        }


        /// <summary>
        /// Remove macro braces (example: ${...}) from a section of content.
        /// </summary>
        /// <param name="context">The preprocessing context.</param>
        /// <param name="content">The content from which to remove the macro braces.</param>
        /// <returns>Content without macro braces.</returns>
        private string RemoveMacro(PreprocessingContext context, string content)
        {
            string result = null;
            if (null != content)
            {
                result = content.Replace(context.TokenStart, "");
                result = result.Replace(context.TokenEnd, "");
                result = result.Trim();
            }
            return result;
        }


        /// <summary>
        /// Replace any macros with their property value.
        /// </summary>
        /// <param name="content">Content in which to replace all macros.</param>
        /// <param name="context">The preprocessing context.</param>
        /// <returns>The content with macros replaced with corresponding property values.</returns>
        private string ResolveProperties(string content, PreprocessingContext context)
        {
            // search for the #foreach(property1,property2) construct which 
            // expands the content multiple times
            Match match = regexForEach.Match(content);
            if (match.Success)
            {
                Group nameGroup = match.Groups["name"];
                if (null != nameGroup && !string.IsNullOrEmpty(nameGroup.Value))
                {
                    // Remove the #foreach(...) construct
                    Group foreachEndGroup = match.Groups["foreachend"];
                    content = content.Substring(0, match.Groups["foreachstart"].Index) +
                        content.Substring(foreachEndGroup.Index + foreachEndGroup.Length);

                    // Get the values to expand
                    int maxCount = 0;
                    Dictionary<string, string[]> compoundValues =
                        ExtractForEachProperties(nameGroup.Value, context, out maxCount);

                    // Loop through the largest collection
                    string combinedContent = "";
                    for (int i = 0; i < maxCount; i++)
                    {
                        // values to override when resolving properties within repeated body
                        Dictionary<string, string> overriddenValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        // override all looped properties
                        foreach (string key in compoundValues.Keys)
                        {
                            string overriddenValue = "";
                            string[] values = compoundValues[key];
                            if (values.Length > i)
                                overriddenValue = values[i];

                            overriddenValues.Add(key, overriddenValue.Trim());
                        }

                        combinedContent += ResolveProperties(content, context, overriddenValues);
                    }

                    return combinedContent;
                }
            }

            return ResolveProperties(content, context, null);
        }


        /// <summary>
        /// Extracts for each properties.
        /// </summary>
        /// <param name="compoundValueNames">The compound value names.</param>
        /// <param name="context">The context.</param>
        /// <param name="maxCount">The max count.</param>
        /// <returns></returns>
        private Dictionary<string, string[]> ExtractForEachProperties(string compoundValueNames, PreprocessingContext context, out int maxCount)
        {
            maxCount = 0;
            Dictionary<string, string[]> compoundValues = new Dictionary<string, string[]>();

            string[] compoundValueNamesArray = compoundValueNames.Split(',');
            foreach (string compoundValueName in compoundValueNamesArray)
            {
                string compoundValueNameTrimmed = compoundValueName.Trim();
                if (!string.IsNullOrEmpty(compoundValueNameTrimmed))
                {
                    string compoundValue = "";
                    PreprocessingProperty compoundProperty = context.Properties[compoundValueNameTrimmed];
                    if (null != compoundProperty)
                    {
                        compoundValue = ResolveProperties(compoundProperty.Value, context, null);
                    }

                    string[] compoundValuesArray = null;
                    if (!string.IsNullOrEmpty(compoundValue))
                    {
                        compoundValuesArray = compoundValue.Split(';');
                        if (compoundValuesArray.Length > maxCount)
                            maxCount = compoundValuesArray.Length;
                    }

                    compoundValues.Add(compoundValueNameTrimmed, compoundValuesArray);
                }
            }

            return compoundValues;
        }

        /// <summary>
        /// Replace any macros with their property value.
        /// </summary>
        /// <param name="content">Content in which to replace all macros.</param>
        /// <param name="context">The preprocessing context.</param>
        /// <param name="overriddenValues">The value to insert.</param>
        /// <returns>The content with macros replaced with corresponding property values.</returns>
        private string ResolveProperties(string content, PreprocessingContext context, IDictionary<string, string> overriddenValues)
        {
            bool containedEscapedMacros = false;
            const string startEscapedMacro = "{6496D0A7-21B9-4603-A2E5-43C64FCD435E{";
            const string endEscapedMacro = "}6496D0A7-21B9-4603-A2E5-43C64FCD435E}";
            bool isSameTokenUsedForStartAndEnd = context.TokenStart.Equals(context.TokenEnd, StringComparison.OrdinalIgnoreCase);

            if (null != content && null != context.Properties)
            {
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
                    string macro = content.Substring(macroPosition, endMacroPosition - macroPosition+1);
                    string key = macro.Substring(context.TokenStart.Length, macro.Length-(context.TokenStart.Length + context.TokenEnd.Length)).Trim();

                    string val = null;

                    // if the key starts out with "script=" treat it as an expression
                    if (key.Length > 6 &&
                        key.StartsWith("script", StringComparison.OrdinalIgnoreCase) &&
                        key.Substring(6).Trim().StartsWith("="))
                    {
                        key = key.Substring(6).Trim().Substring(1).Trim();
                        if (!string.IsNullOrEmpty(key))
                            val = ResolveExpression(key, context);
                    }
                    else if (key.Length > 8 &&
                        key.StartsWith("registry", StringComparison.OrdinalIgnoreCase) &&
                        key.Substring(8).Trim().StartsWith("="))
                    {
                        key = key.Substring(8).Trim().Substring(1).Trim();
                        if (!string.IsNullOrEmpty(key))
                        {
                            val = RegistryEvaluator.GetValue(key);
                        }
                    }
                    else
                    {
                        if (null != overriddenValues && overriddenValues.ContainsKey(key))
                        {
                            val = overriddenValues[key];
                        }
                        else
                        {
                            PreprocessingProperty property = null;
                            
                            // Implementation of fallback properties.
                            // Go through the semicolon delimited list 
                            // of properties looking for the first one that exists
                            // ex: ${PROPERTY_ABC;PROPERTY_DEF;PROPERTY}
                            // useful for machine-specific configuration
                            // when coupled with nested properties like this:
                            // ${PROPERTY_${_machine_name};PROPERTY}

                            string[] keys = key.Split(';');
                            foreach (string keyPart in keys)
                            {
                                string keyPartTrimmed = keyPart.Trim();
                                if (!string.IsNullOrEmpty(keyPartTrimmed))
                                {
                                    property = context.Properties[keyPartTrimmed];
                                    if (null != property)
                                        break;
                                }
                            }

                            if (null != property)
                            {
                                val = property.Value;

                                if (context.CountUsage)
                                {
                                    int useCount = CountOccurrences(content, macro);
                                    property.UseCount = property.UseCount + useCount;
                                }
                            }
                            else
                            {
                                val = "<!-- " + key + " not defined -->";

                                if (context.ValidateSettingsExist)
                                {
                                    ErrorInfo errorInfo = new ErrorInfo(ErrorCode.ErrorMissingToken, context.SourceFile);
                                    errorInfo.Message = string.Format("The setting named '{0}' was not defined.", key);
                                    context.Errors.Add(errorInfo);
                                }
                            }
                        }
                    }

                    // Enable Escaping
                    if (!string.IsNullOrEmpty(val))
                    {
                        int escapedMacroStartPos = val.IndexOf(context.TokenStartEscaped);
                        int escapedMacroEndPos = val.IndexOf(context.TokenEndEscaped);
                        if (escapedMacroStartPos > -1 && escapedMacroEndPos > escapedMacroStartPos)
                        {
                            val = val.Replace(context.TokenStartEscaped, startEscapedMacro).Replace(context.TokenEndEscaped, endEscapedMacro);
                            containedEscapedMacros = true;
                        }
                    }

                    content = content.Replace(macro, val);

                    if (!isSameTokenUsedForStartAndEnd)
                        macroPosition = content.LastIndexOf(context.TokenStart);
                    else
                        macroPosition = content.IndexOf(context.TokenStart);
                }
            }

            if (containedEscapedMacros)
            {
                content = content.Replace(startEscapedMacro, context.TokenStart).Replace(endEscapedMacro, context.TokenEnd);
            }

            return content;
        }


        /// <summary>
        /// Counts the occurrences.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <param name="checkString">The check string.</param>
        /// <returns></returns>
        private int CountOccurrences(string inputString, string checkString)
        {
            if (checkString.Length > inputString.Length || string.IsNullOrEmpty(checkString))
                return 0;

            int lengthDifference = inputString.Length - checkString.Length;
            int occurrences = 0;
            for (int i = 0; i < lengthDifference; i++)
            {
                if (inputString.Substring(i, checkString.Length).Equals(checkString))
                {
                    occurrences++;
                    i += checkString.Length - 1;
                }
            }

            return occurrences;
        }


        /// <summary>
        /// Resolves an expression
        /// </summary>
        /// <param name="expression">The function.</param>
        /// <param name="context">The preprocessing context.</param>
        /// <returns>The resulting value</returns>
        private string ResolveExpression(string expression, PreprocessingContext context)
        {
            string val = "";
            DynamicEvaluator evaluator = new DynamicEvaluator();

            try
            {
                val = evaluator.EvaluateToString(context, expression);
            }
            catch (Exception ex)
            {
                val = "<!-- " + expression + " not properly formed -->";

                ErrorInfo errorInfo = new ErrorInfo(ErrorCode.ErrorNotWellFormed, context.SourceFile);
                errorInfo.Message = string.Format("The expression '{0}' was not properly formed. {1}", expression, ex.ToString());
                context.Errors.Add(errorInfo);
            }

            return val;
        }
    }
}
