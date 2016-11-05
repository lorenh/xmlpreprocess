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
using System.Text;
using System.CodeDom;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.IO;
using Microsoft.Win32;

namespace XmlPreprocess.Util
{
    /// <summary>
    /// Compiles an expression of C# code and executes it, resolving to a string. Used to
    /// enable more complex scripting expressions to be embedded in property values
    /// </summary>
    /// <example>
    /// ${Script= Property("val1")=="abc" ? "yes" : Property("val3")}
    /// </example>
    public class DynamicEvaluator
    {
        private const string STRING_CODE_TEMPLATE = @"using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
namespace XmlPreprocess {{
    public class StringEvaluator {{
        private PreprocessingContext _context = null;
        public StringEvaluator(PreprocessingContext context) {{
            _context = context;
        }}
        public string GetProperty(string key) {{
            return _context.ResolveContent(""{0}""+key+""{1}"");
        }}
        public bool defined(string key) {{
            return _context.Properties.ContainsKey(key);
        }}
        public string Evaluate() {{
            return {2};
        }}
    }}
}}";

        private const string BOOL_CODE_TEMPLATE = @"using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
namespace XmlPreprocess {{
    public class BoolEvaluator {{
        private PreprocessingContext _context = null;
        public BoolEvaluator(PreprocessingContext context) {{
            _context = context;
        }}
        public string GetProperty(string key) {{
            return _context.ResolveContent(""{0}""+key+""{1}"");
        }}
        public bool defined(string key) {{
            return _context.Properties.ContainsKey(key);
        }}
        public bool Evaluate() {{
            return {2};
        }}
    }}
}}";

        /// <summary>
        /// Evaluates the provided C# expression
        /// </summary>
        /// <param name="context">The preprocessing context.</param>
        /// <param name="input">The input.</param>
        /// <returns>The expression resolved to a single string</returns>
        public string EvaluateToString(PreprocessingContext context, string input)
        {
            // generate code
            string code = string.Format(STRING_CODE_TEMPLATE, context.TokenStart, context.TokenEnd, input);

            // compile it
            CompilerResults results = CompileAssembly(code);
            if (null != results && null != results.CompiledAssembly)
            {
                // execute it
                return RunStringEvaluation(context, results);
            }
            return "";
        }

        /// <summary>
        /// Evaluates the provided C# expression
        /// </summary>
        /// <param name="context">The preprocessing context.</param>
        /// <param name="input">The input.</param>
        /// <returns>The expression resolved to a single string</returns>
        public bool EvaluateToBool(PreprocessingContext context, string input)
        {
            // generate code
            string code = string.Format(BOOL_CODE_TEMPLATE, context.TokenStart, context.TokenEnd, input);

            // compile it
            CompilerResults results = CompileAssembly(code);
            if (null != results && null != results.CompiledAssembly)
            {
                // execute it
                return RunBoolEvaluation(context, results);
            }
            return false;
        }


        /// <summary>
        /// Compiles the c# into an assembly if there are no syntax errors
        /// </summary>
        /// <returns>The CompilerResults object for the compiled code</returns>
        private CompilerResults CompileAssembly(string source)
        {
            CodeDomProvider compiler = new CSharpCodeProvider();

            CompilerParameters compilerParams = new CompilerParameters();

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8));
            compilerParams.CompilerOptions = String.Format("/target:library /optimize /lib:\"{0}\"", path);

            compilerParams.GenerateExecutable = false;
            compilerParams.GenerateInMemory = true;
            compilerParams.IncludeDebugInformation = false;
            compilerParams.ReferencedAssemblies.Add("mscorlib.dll");
            compilerParams.ReferencedAssemblies.Add("System.dll");
            compilerParams.ReferencedAssemblies.Add("System.Xml.dll");
            compilerParams.ReferencedAssemblies.Add("XmlPreprocess.exe");

            CompilerResults results = compiler.CompileAssemblyFromSource(compilerParams, source);

            if (results.Errors.Count > 0)
            {
                StringBuilder errors = new StringBuilder();
                foreach (CompilerError error in results.Errors)
                {
                    errors.Append(error.ErrorText).Append("\r\n");
                }
                throw new InvalidOperationException(errors.ToString());
            }

            return results;
        }


        /// <summary>
        /// Runs the code.
        /// </summary>
        /// <param name="context">The preprocessing context.</param>
        /// <param name="results">The results.</param>
        /// <returns>The expression resolved to a single string.</returns>
        private string RunStringEvaluation(PreprocessingContext context, CompilerResults results)
        {
            string result = "";

            Assembly executingAssembly = results.CompiledAssembly;
            if (executingAssembly != null)
            {
                object assemblyInstance = executingAssembly.CreateInstance(
                    "XmlPreprocess.StringEvaluator", false, BindingFlags.CreateInstance, null,
                    new object[] { context }, null, null);

                Module[] modules = executingAssembly.GetModules(false);
                Type[] types = modules[0].GetTypes();
                Type type = types[0];
                MethodInfo mi = type.GetMethod("Evaluate");

                result = (string)mi.Invoke(assemblyInstance, null);
            }

            return result;
        }

        /// <summary>
        /// Runs the code.
        /// </summary>
        /// <param name="context">The preprocessing context.</param>
        /// <param name="results">The results.</param>
        /// <returns>The expression resolved to a single string.</returns>
        private bool RunBoolEvaluation(PreprocessingContext context, CompilerResults results)
        {
            bool result = false;

            Assembly executingAssembly = results.CompiledAssembly;
            if (executingAssembly != null)
            {
                object assemblyInstance = executingAssembly.CreateInstance(
                    "XmlPreprocess.BoolEvaluator", false, BindingFlags.CreateInstance, null,
                    new object[] { context }, null, null);

                Module[] modules = executingAssembly.GetModules(false);
                Type[] types = modules[0].GetTypes();
                Type type = types[0];
                MethodInfo mi = type.GetMethod("Evaluate");

                result = (bool)mi.Invoke(assemblyInstance, null);
            }

            return result;
        }

    }


    /// <summary>
    /// Retrieves a value from the registry
    /// </summary>
    public class RegistryEvaluator
    {
        /// <summary>
        /// Retrieves a value from the registry
        /// </summary>
        /// <param name="registryPath"></param>
        /// <returns>the value at the requested path, or null if it didn't exist</returns>
        public static string GetValue(string registryPath)
        {
            string val = null;

            // example:
            // HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework\InstallRoot,Default Value (optional)}

            int hiveDelimPos = registryPath.IndexOf('\\');
            string hiveName = registryPath.Substring(0, hiveDelimPos);

            int valueDelimPos = registryPath.LastIndexOf('\\');
            string valueName = registryPath.Substring(valueDelimPos + 1);

            string defaultValue = null;
            int defaultValueDelimPos = valueName.IndexOf(',');
            if (defaultValueDelimPos > -1)
            {
                defaultValue = valueName.Substring(defaultValueDelimPos + 1).Trim();
                valueName = valueName.Substring(0, defaultValueDelimPos).Trim();
            }

            string subkeyPath = registryPath.Substring(hiveDelimPos + 1, valueDelimPos - hiveDelimPos - 1);

            RegistryKey hive = null;
            switch (hiveName.Trim().ToUpper())
            {
                case "HKEY_CURRENT_USER":
                    hive = Registry.CurrentUser;
                    break;
                case "HKEY_LOCAL_MACHINE":
                    hive = Registry.LocalMachine;
                    break;
                case "HKEY_CLASSES_ROOT":
                    hive = Registry.ClassesRoot;
                    break;
                case "HKEY_USERS":
                    hive = Registry.Users;
                    break;
                case "HKEY_PERFORMANCE_DATA":
                    hive = Registry.PerformanceData;
                    break;
                case "HKEY_CURRENT_CONFIG":
                    hive = Registry.CurrentConfig;
                    break;
            }

            using (RegistryKey regkey = hive.OpenSubKey(subkeyPath, false))
            {
                if (null != regkey)
                {
                    object objValue = regkey.GetValue(valueName);
                    if (null != objValue)
                        val = objValue.ToString();
                    else
                        val = defaultValue;
                }
                else
                {
                    val = defaultValue;
                }
            }
            return val;
        }
    }
}