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
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace XmlPreprocess
{
    [TestFixture]
    public class XmlPreprocessUnitTests
    {
        public static string _baseDir = Path.GetDirectoryName(typeof(XmlPreprocessUnitTests).Assembly.CodeBase).Substring(6);

        #region SimpleTest

        // ############################################################

        [Test]
        public void SimpleTest()
        {
            Console.WriteLine("** SimpleTest **");

            string input =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""abc""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""newvalue""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PRODUCTION", "/d", "PROPERTY=newvalue" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion SimpleTest

        #region OldFormatSimpleTest

        // ############################################################

        [Test]
        public void OldFormatSimpleTest()
        {
            Console.WriteLine("** OldFormatSimpleTest **");

            string input =
@"<xml>

   <!-- ifdef ${PRODUCTION} -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- else -->
   <entry foo=""abc""/>
   <!-- endif -->

</xml>";

            string expected =
@"<xml>

   <!-- ifdef ${PRODUCTION} -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- else -->
   <entry foo=""newvalue""/>
   <!-- endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PRODUCTION", "/d", "PROPERTY=newvalue" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion OldFormatSimpleTest

        #region AlternateTokenParenTest

        // ############################################################

        [Test]
        public void AlternateTokenParenTest()
        {
            Console.WriteLine("** AlternateTokenParenTest **");

            string input =
@"<xml>

   <!-- ifdef PRODUCTION -->
   <!-- <entry foo=""$(PROPERTY)""/> -->
   <!-- else -->
   <entry foo=""abc""/>
   <!-- endif -->

</xml>";

            string expected =
@"<xml>

   <!-- ifdef PRODUCTION -->
   <!-- <entry foo=""$(PROPERTY)""/> -->
   <!-- else -->
   <entry foo=""newvalue""/>
   <!-- endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PRODUCTION", "/d", "PROPERTY=newvalue", "/ts", "$(", "/te", ")" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion AlternateTokenParenTest

        #region AlternateTokenHashTest

        // ############################################################

        [Test]
        public void AlternateTokenHashTest()
        {
            Console.WriteLine("** AlternateTokenHashTest **");

            string input =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""#PROPERTY#""/> -->
   <!-- #else -->
   <entry foo=""abc""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""#PROPERTY#""/> -->
   <!-- #else -->
   <entry foo=""newvalue""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PRODUCTION", "/d", "PROPERTY=newvalue", "/ts", "#"});

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion AlternateTokenHashTest

        #region AlternateTokenResolveContentTest

        // ############################################################

        [Test]
        public void AlternateTokenResolveContentTest()
        {
            Console.WriteLine("** AlternateTokenResolveContentTest **");

            string input =
@"<xml>
   <entry foo=""#Script=#"" />
   <entry foo=""#Script= GetProperty(""val1"")==""abc"" ? ""yes"" : GetProperty(""val3"") #"" />
   <entry foo=""# script= GetProperty(""val1"")==""xyz"" ? ""yes"" : GetProperty(""val3"") #"" />
   <entry foo=""#script = GetProperty(""val1"").IndexOf(""a"") > -1 ? ""yes"" : GetProperty(""val3"") #"" />
   <entry foo=""#script=GetProperty(""val1"") + GetProperty(""val3"") #"" />
</xml>";

            string expected1 =
@"<xml>
   <entry foo="""" />
   <entry foo=""yes"" />
   <entry foo=""no"" />
   <entry foo=""yes"" />
   <entry foo=""abcno"" />
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", @"val1=abc", "/d", @"val3=no", "/n", "/ts", "#" });

            Assert.AreEqual(expected1, ReadFile(inputFile));
        }

        #endregion AlternateTokenResolveContentTest

        #region ImplicitSettingTest

        // ############################################################

        [Test]
        public void ImplicitSettingTest()
        {
            Console.WriteLine("** ImplicitSettingTest **");

            string input =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""abc""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""newvalue""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PROPERTY=newvalue" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion ImplicitSettingTest

        #region BlankPropertyTest

        // ############################################################

        [Test]
        public void BlankPropertyTest()
        {
            Console.WriteLine("** BlankPropertyTest **");

            string input =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""abc""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PROPERTY" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion BlankPropertyTest

        #region QuotedBooleanTest

        // ############################################################

        [Test]
        public void QuotedBooleanTest()
        {
            Console.WriteLine("** QuotedBooleanTest **");

            string input =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""True""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""False""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PROPERTY=\'False\'" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion QuotedBooleanTest

        #region NonQuotedBooleanTest

        // ############################################################

        [Test]
        public void NonQuotedBooleanTest()
        {
            Console.WriteLine("** NonQuotedBooleanTest **");

            string input =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""True""/>
   <!-- #endif -->

</xml>";

#if NOFIXFALSE
            string expected =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""<!-- PROPERTY not defined -->""/>
   <!-- #endif -->

</xml>";
#else
            string expected =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""False""/>
   <!-- #endif -->

</xml>";
#endif

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PROPERTY=False" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion NonQuotedBooleanTest

        #region ReprocessTest

        // ############################################################

        [Test]
        public void ReprocessTest()
        {
            Console.WriteLine("** ReprocessTest **");

            string input =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""newvalue""/>
   <!-- #endif -->

</xml>";

            string expected2 =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""newvalue2""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PRODUCTION", "/d", "PROPERTY=newvalue" });

            Assert.AreEqual(expected, ReadFile(inputFile));

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PRODUCTION", "/d", "PROPERTY=newvalue2" });

            Assert.AreEqual(expected2, ReadFile(inputFile));

        }

        #endregion ReprocessTest

        #region SimpleSettingsFileTest

        // ############################################################

        [Test]
        public void SimpleSettingsFileTest()
        {
            Console.WriteLine("** SimpleSettingsFileTest **");

            string settings =
@"<settings>
  <property name=""PRODUCTION"">TRUE</property>
  <property name=""PROPERTY"">newvalue</property>
</settings>";

            string input =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""abc""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""newvalue""/>
   <!-- #endif -->

</xml>";

            RunTest(input, settings, expected);
        }

        #endregion SimpleSettingsFileTest

        #region MultipleSettingsFileTest

        // ############################################################

        [Test]
        public void MultipleSettingsFileTest()
        {
            Console.WriteLine("** MultipleSettingsFileTest **");

            string settings1 =
@"<settings>
  <property name=""PRODUCTION"">TRUE</property>
</settings>";

            string settings2 =
@"<settings>
  <property name=""PROPERTY"">newvalue</property>
</settings>";

            string input =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""abc""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""newvalue""/>
   <!-- #endif -->

</xml>";

            int exitCode = 0;

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            WriteFile(settings1, Path.Combine(_baseDir, "settings1.xml"));
            WriteFile(settings2, Path.Combine(_baseDir, "settings2.xml"));

            exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/s", _baseDir + "\\settings1.xml", "/s", _baseDir + "\\settings2.xml" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion MultipleSettingsFileTest

        #region NoElseSettingsFileTest

        // ############################################################

        [Test]
        public void NoElseSettingsFileTest()
        {
            Console.WriteLine("** NoElseSettingsFileTest **");

            string settings =
@"<settings>
  <property name=""PRODUCTION"">TRUE</property>
  <property name=""PROPERTY"">newvalue</property>
</settings>";

            string input =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""newvalue""/>
   <!-- #endif -->

</xml>";

            RunTest(input, settings, expected);
        }

        #endregion NoElseSettingsFileTest

        #region UncommentedBodyTest

        // ############################################################

        [Test]
        public void UncommentedBodyTest()
        {
            Console.WriteLine("** UncommentedBodyTest **");

            string input =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <entry foo=""${PROPERTY}""/>
   <!-- #endif -->

</xml>";

            string expected1 =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <!-- #endif -->

</xml>";

            string expected2 =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""newvalue""/>
   <!-- #endif -->

</xml>";


            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PROPERTY=newvalue" });

            string actual = ReadFile(inputFile);
            Assert.AreEqual(expected1, actual);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PRODUCTION", "/d", "PROPERTY=newvalue" });

            actual = ReadFile(inputFile);
            Assert.AreEqual(expected2, actual);
        }

        #endregion UncommentedBodyTest

        #region CommentedBodyTest

        // ############################################################

        [Test]
        public void CommentedBodyTest()
        {
            Console.WriteLine("** CommentedBodyTest **");

            string input =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #endif -->

</xml>";

            string expected1 =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <!-- #endif -->

</xml>";

            string expected2 =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""newvalue""/>
   <!-- #endif -->

</xml>";


            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PROPERTY=newvalue" });

            string actual = ReadFile(inputFile);
            Assert.AreEqual(expected1, actual);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PRODUCTION", "/d", "PROPERTY=newvalue" });

            actual = ReadFile(inputFile);
            Assert.AreEqual(expected2, actual);
        }

        #endregion CommentedBodyTest

        #region BuiltInTest

        // ############################################################

        [Test]
        public void BuiltInTest()
        {
            Console.WriteLine("** BuiltInTest **");

            string settings =
@"<settings>
  <property name=""PRODUCTION"">TRUE</property>
</settings>";

            string input =
@"<xml>
   <!-- #ifdef PRODUCTION -->
   <!-- <entry
          dest_dir=""${_dest_dir}""
          machine_name=""${_machine_name}""
          machine_id=""${_machine_id}""
          os_platform=""${_os_platform}""
          os_version=""${_os_version}""
          system_dir=""${_system_dir}""
          current_dir=""${_current_dir}""
          clr_version=""${_clr_version}""
          user_name=""${_user_name}""
          user_domain_name=""${_user_domain_name}""
          user_interactive=""${_user_interactive}""
          system_date=""${_system_date}""
          system_time=""${_system_time}""
          framework_dir=""${_framework_dir}""
          env_number_of_processors=""${_env_number_of_processors}"" />
   -->
   <!-- #endif -->
</xml>";

            string machineId = Environment.MachineName;
            Match match = Regex.Match(machineId,@"\d+");
            if (match.Success)
                machineId = match.Value;

            string frameworkDir = "";
            try
            {
                frameworkDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
            }
            catch (Exception)
            {
                // not fatal
            }

            string numProcessors = "2";
            IDictionary envVars = null;
            try
            {
                envVars = Environment.GetEnvironmentVariables();
            }
            catch (Exception)
            {
                // not fatal
            }

            if (null != envVars)
            {
                numProcessors = envVars["number_of_processors"] as string;
                if (null == numProcessors || 0 == numProcessors.Length)
                    numProcessors = "4";
            }


            string expected =
@"<xml>
   <!-- #ifdef PRODUCTION -->
   <!--
   <entry
          dest_dir=""${_dest_dir}""
          machine_name=""${_machine_name}""
          machine_id=""${_machine_id}""
          os_platform=""${_os_platform}""
          os_version=""${_os_version}""
          system_dir=""${_system_dir}""
          current_dir=""${_current_dir}""
          clr_version=""${_clr_version}""
          user_name=""${_user_name}""
          user_domain_name=""${_user_domain_name}""
          user_interactive=""${_user_interactive}""
          system_date=""${_system_date}""
          system_time=""${_system_time}""
          framework_dir=""${_framework_dir}""
          env_number_of_processors=""${_env_number_of_processors}"" />
   -->
   <!-- #else -->
   <entry
          dest_dir=""" + _baseDir + @"""
          machine_name=""" + Environment.MachineName + @"""
          machine_id=""" + machineId + @"""
          os_platform=""" + Environment.OSVersion.Platform + @"""
          os_version=""" + Environment.OSVersion.Version + @"""
          system_dir=""" + Environment.SystemDirectory + @"""
          current_dir=""" + Environment.CurrentDirectory + @"""
          clr_version=""" + Environment.Version.ToString() + @"""
          user_name=""" + Environment.UserName + @"""
          user_domain_name=""" + Environment.UserDomainName + @"""
          user_interactive=""" + Environment.UserInteractive + @"""
          system_date=""" + DateTime.Now.ToShortDateString() + @"""
          system_time=""" + DateTime.Now.ToShortTimeString() + @"""
          framework_dir=""" + frameworkDir + @"""
          env_number_of_processors=""" + numProcessors + @""" />
   <!-- #endif -->
</xml>";
            RunTest(input, settings, expected);
        }

        #endregion BuiltInTest

        #region RecursiveSettingsTest

        // ############################################################

        [Test]
        public void RecursiveSettingsTest()
        {
            Console.WriteLine("** RecursiveSettingsTest **");

            string settings =
@"<settings>
    <property name=""installer"">true</property>
    <property name=""protocol"">http</property>
    <property name=""port"">80</property>
    <property name=""uri1"">${protocol}:${port}//server1</property>
     <property name=""uri2"">${protocol}:${port}//server2</property>
    <property name=""uri3"">${protocol}:${port}//server3</property>
</settings>";

            string input =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
  <!-- #ifdef installer -->
  <!--
  <uri1>${uri1}</uri1>
  <uri2>${uri2}</uri2>
  <uri3>${uri3}</uri3>
  -->
  <!-- #endif -->
</configuration>";

            string expected =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
  <!-- #ifdef installer -->
  <!--
  <uri1>${uri1}</uri1>
  <uri2>${uri2}</uri2>
  <uri3>${uri3}</uri3>
  -->
  <!-- #else -->
  <uri1>http:80//server1</uri1>
  <uri2>http:80//server2</uri2>
  <uri3>http:80//server3</uri3>
  <!-- #endif -->
</configuration>";

            RunTest(input, settings, expected);
        }

        #endregion RecursiveSettingsTest

        #region MissedEndifErrorTest

        // ############################################################

        [Test]
        public void MissedEndifErrorTest()
        {
            Console.WriteLine("** MissedEndifErrorTest **");

            string input =
@"<xml>
    <!-- #ifdef PRODUCTION -->
    <!-- <entry foo=""${PROPERTY}""/> -->
    <!-- #ifdef DEV -->
    <!-- <entry foo=""${PROPERTY}""/> -->
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/d", "PRODUCTION", "/d", "PROPERTY=newvalue" });

            Assert.AreEqual(1, exitCode);
        }

        #endregion MissedEndifErrorTest

        #region EqualInPropertyValueTest

        // ############################################################

        [Test]
        public void EqualInPropertyValueTest()
        {
            Console.WriteLine("** EqualInPropertyValueTest **");

            string input =
                @"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""abc""/>
   <!-- #endif -->

</xml>";

            string expected =
                @"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""newvalue=1""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PRODUCTION", "/d", "PROPERTY=newvalue=1"});

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion EqualInPropertyValueTest

        #region UndefinedTokenTest

        // ############################################################

        [Test]
        public void UndefinedTokenTest()
        {
            Console.WriteLine("** UndefinedTokenTest **");

            string input =
@"<xml>
    <!-- #ifdef PRODUCTION -->
    <!-- <entry foo=""${PROPERTY}""/> -->
    <!-- #endif -->
<FOO>
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] {"/nologo", "/i", inputFile, "/d", "PRODUCTION", "/vs"});

            Assert.AreEqual(2, exitCode);
        }

        #endregion UndefinedTokenTest

        #region UndefinedTokenTestDisregardsWellFormedness

        // ############################################################

        [Test]
        public void UndefinedTokenTestDisregardsWellFormedness()
        {
            Console.WriteLine("** UndefinedTokenTestDisregardsWellFormedness **");

            string input =
@"<xml>
    <!-- #ifdef PRODUCTION -->
    <!-- <entry foo=""${PROPERTY}""/> -->
    <!-- #endif -->
<BADFORM>
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] {"/nologo", "/i", inputFile, "/d", "PRODUCTION", "/d", "PROPERTY", "/vs" });

            Assert.AreEqual(0, exitCode);
        }

        #endregion UndefinedTokenTestDisregardsWellFormedness

        #region NotWellFormedTest

        // ############################################################

        [Test]
        public void NotWellFormedTest()
        {
            Console.WriteLine("** NotWellFormedTest **");

            string input =
@"<xml>
    <!-- #ifdef PRODUCTION -->
    <!-- <entry foo=""${PROPERTY}""/> -->
    <!-- #endif -->
<BADFORM>
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] {"/nologo", "/i", inputFile, "/d", "PRODUCTION", "/vx" });

            Assert.AreEqual(1, exitCode);
        }

        #endregion NotWellFormedTest

        #region ValidateAllTest

        // ############################################################

        [Test]
        public void ValidateAllTest()
        {
            Console.WriteLine("** ValidateAllTest **");

            string input =
@"<xml>
    <!-- #ifdef PRODUCTION -->
    <!-- <entry foo=""${PROPERTY}""/> -->
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] {"/nologo", "/i", inputFile, "/d", "PRODUCTION", "/v" });

            Assert.AreEqual(2, exitCode);
        }

        #endregion ValidateAllTest

        #region ExplicitlyUndefinedTokenTest

        // ############################################################

        [Test]
        public void ExplicitlyUndefinedTokenTest()
        {
            Console.WriteLine("** ExplicitlyUndefinedTokenTest **");

            string input =
@"<xml>
    <!-- #ifdef PRODUCTION -->
    <!-- <entry foo=""${PROPERTY}""/> -->
    <!-- #endif -->
</xml>";

            string expected =
@"<xml>
    <!-- #ifdef PRODUCTION -->
    <!-- <entry foo=""${PROPERTY}""/> -->
    <!-- #else -->
    <!-- #endif -->
</xml>";

            string expected2 =
@"<xml>
    <!-- #ifdef PRODUCTION -->
    <!-- <entry foo=""${PROPERTY}""/> -->
    <!-- #else -->
    <entry foo=""somevalue""/>
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PRODUCTION=#undef","/d", "PROPERTY=somevalue" });

            Assert.AreEqual(expected, ReadFile(inputFile));

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PRODUCTION=false","/d", "PROPERTY=somevalue" });

#if NOFIXFALSE
            Assert.AreEqual(expected, ReadFile(inputFile));
#else
            Assert.AreEqual(expected2, ReadFile(inputFile));
#endif

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PRODUCTION=false","/d", "PROPERTY=somevalue", "/f" });

            Assert.AreEqual(expected2, ReadFile(inputFile));
        }

        #endregion ExplicitlyUndefinedTokenTest

        #region SettingsFileTest

        // ############################################################
        [Test]
        public void SettingsFileTest()
        {
            Console.WriteLine("** SettingsFileTest **");

            string input =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string expected =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""setting1default""/>
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/v", "/s", Path.Combine(_baseDir, "..\\..\\TestSettings.xml") });


            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion SettingsFileTest

        #region SettingsWithEnvFileTest

        // ############################################################
        [Test]
        public void SettingsWithEnvFileTest()
        {
            Console.WriteLine("** SettingsWithEnvFileTest **");

            string input =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string expected =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""setting1default""/>
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/v", "/s", Path.Combine(_baseDir, "..\\..\\TestSettings2.xml"), "/e", "Production"});


            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion SettingsWithEnvFileTest

        #region SettingsWithEnvAndAltNamesFileTest

        // ############################################################
        [Test]
        public void SettingsWithEnvAndAltNamesFileTest()
        {
            Console.WriteLine("** SettingsWithEnvAndAltNamesFileTest **");

            string input =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string expected =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""setting1default""/>
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/v", "/s", Path.Combine(_baseDir, "..\\..\\TestSettings3.xml"), "/e", "Production" });


            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion SettingsWithEnvAndAltNamesFileTest

        #region ExcelFileTest

        // ############################################################
        [Test]
        public void ExcelFileTest()
        {
            Console.WriteLine("** ExcelFileTest **");

            string input =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string expected =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""setting1default""/>
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/v", "/x", Path.Combine(_baseDir, "..\\..\\TestSettings.xls"), "/e", "Local" });


            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion ExcelFileTest

        #region SpreadsheetMLTest

        // ############################################################

        [Test]
        public void SpreadsheetMLTest()
        {
            Console.WriteLine("** SpreadsheetMLTest **");

            string input =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string expected =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""setting1default""/>
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/v", "/x", Path.Combine(_baseDir, "..\\..\\SpreadsheetML.xml"), "/e", "Local" });

            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion SpreadsheetMLTest

        #region SpreadsheetMLBoolTest

        // ############################################################

        [Test]
        public void SpreadsheetMLBoolTest()
        {
            Console.WriteLine("** SpreadsheetMLBoolTest **");

            string input =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${BoolSetting}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string expected =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${BoolSetting}""/> -->
    <!-- #else -->
    <entry foo=""True""/>
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/v", "/x", Path.Combine(_baseDir, "..\\..\\SpreadsheetML.xml"), "/e", "Local" });

            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion SpreadsheetMLBoolTest

        #region SpreadsheetMLWithBlankRowsTest

        // ############################################################

        [Test]
        public void SpreadsheetMLWithBlankRowsTest()
        {
            Console.WriteLine("** SpreadsheetMLWithBlankRowsTest **");

            string input =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string expected =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""setting1default""/>
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/v", "/x", Path.Combine(_baseDir, "..\\..\\SpreadsheetMLWithBlankRows.xml"), "/e", "Local" });

            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion SpreadsheetMLWithBlankRowsTest

        #region PropertyCountTest

        // ############################################################

        [Test]
        public void PropertyCountTest()
        {
            Console.WriteLine("** PropertyCountTest **");

            string input =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!--
    <entry foo=""${Setting1}""/>
    <entry foo=""${Setting1}""/>
    <entry foo=""${Setting2}""/>
    <entry foo=""${Setting3}""/>
    -->
    <!-- #endif -->
    <!-- #ifdef _xml_preprocess -->
    <!--
    <entry foo=""${Setting1}""/>
    <entry foo=""${Setting1}""/>
    <entry foo=""${Setting2}""/>
    <entry foo=""${Setting3}""/>
    -->
    <!-- #endif -->
</xml>";

            string expected =
@"Property, Count
Setting1, 4
Setting2, 2
Setting3, 2
Setting4, 0
Setting5, 0
BoolSetting, 0
";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            string reportFile = Path.Combine(_baseDir, "counts.csv");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] { "/nologo", "/cr", reportFile, "/i", inputFile, "/v", "/x", Path.Combine(_baseDir, "..\\..\\SpreadsheetML.xml"), "/e", "Local" });

            Assert.AreEqual(0, exitCode);
            string report = ReadFile(reportFile);
            Assert.AreEqual(expected, report);
        }

        #endregion PropertyCountTest


        #region PropertyCountNoDirectivesTest

        // ############################################################

        [Test]
        public void PropertyCountNoDirectivesTest()
        {
            Console.WriteLine("** PropertyCountNoDirectivesTest **");

            string input =
@"<xml>
    <entry foo=""${Setting1}""/>
    <entry foo=""${Setting1}""/>
    <entry foo=""${Setting2}""/>
    <entry foo=""${Setting3}""/>
</xml>";

            string expected =
@"Property, Count
Setting1, 2
Setting2, 1
Setting3, 1
Setting4, 0
Setting5, 0
BoolSetting, 0
";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            string reportFile = Path.Combine(_baseDir, "counts.csv");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] { "/nologo", "/n", "/cr", reportFile, "/i", inputFile, "/v", "/x", Path.Combine(_baseDir, "..\\..\\SpreadsheetML.xml"), "/e", "Local" });

            Assert.AreEqual(0, exitCode);
            string report = ReadFile(reportFile);
            Assert.AreEqual(expected, report);
        }

        #endregion PropertyCountNoDirectivesTest

        #region CsvTest

        // ############################################################

        [Test]
        public void CsvTest()
        {
            Console.WriteLine("** CsvTest **");

            string input =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string expected =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""setting1default""/>
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/v", "/x", Path.Combine(_baseDir, "..\\..\\TestSettings.csv"), "/e", "Local" });

            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion CsvTest

        #region EnvironmentBuiltinTest

        // ############################################################

        [Test]
        public void EnvironmentBuiltinTest()
        {
            Console.WriteLine("** EnvironmentBuiltinTest **");

            string input =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${_environment_name}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string expected =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${_environment_name}""/> -->
    <!-- #else -->
    <entry foo=""Local""/>
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/v", "/x", Path.Combine(_baseDir, "..\\..\\SpreadsheetML.xml"), "/e", "Local" });

            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion EnvironmentBuiltinTest

        #region MissingEnvironmentInSpreadsheetTest

        // ############################################################

        [Test]
        public void MissingEnvironmentInSpreadsheetTest()
        {
            Console.WriteLine("** MissingEnvironmentInSpreadsheetTest **");

            string input =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/v", "/x", Path.Combine(_baseDir, "..\\..\\SpreadsheetML.xml"), "/e", "FOO" });

            Assert.AreEqual(1, exitCode);
        }

        #endregion MissingEnvironmentInSpreadsheetTest

        #region MultipleInputFileTest

        // ############################################################

        [Test]
        public void MultipleInputFileTest()
        {
            Console.WriteLine("** MultipleInputFileTest **");

            string input1 =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string inputFile1 = Path.Combine(_baseDir, "input1.xml");
            WriteFile(input1, inputFile1);


            string input2 =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string inputFile2 = Path.Combine(_baseDir, "input2.xml");
            WriteFile(input2, inputFile2);


            int exitCode = Run(new string[] { "/nologo", "/i", inputFile1, "/i", inputFile2, "/v", "/x", Path.Combine(_baseDir, "..\\..\\SpreadsheetML.xml"), "/e", "Local" });

            Assert.AreEqual(0, exitCode);
        }

        #endregion MultipleInputFileTest

        #region MultipleInputAndOutputFileTest

        // ############################################################

        [Test]
        public void MultipleInputAndOutputFileTest()
        {
            Console.WriteLine("** MultipleInputAndOutputFileTest **");

            string input1 =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string inputFile1 = Path.Combine(_baseDir, "input1.xml");
            WriteFile(input1, inputFile1);


            string input2 =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string inputFile2 = Path.Combine(_baseDir, "input2.xml");
            WriteFile(input2, inputFile2);


            int exitCode = Run(new string[] { "/nologo", "/i", inputFile1, "/i", inputFile2, "/o", inputFile1, "/o", inputFile2, "/v", "/x", Path.Combine(_baseDir, "..\\..\\SpreadsheetML.xml"), "/e", "Local" });

            Assert.AreEqual(0, exitCode);
        }

        #endregion MultipleInputAndOutputFileTest

        #region MultipleInputAndOutputFileWithSemicolonsTest

        // ############################################################

        [Test]
        public void MultipleInputAndOutputFileWithSemicolonsTest()
        {
            Console.WriteLine("** MultipleInputAndOutputFileWithSemicolonsTest **");

            string input1 =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string inputFile1 = Path.Combine(_baseDir, "input1.xml");
            WriteFile(input1, inputFile1);


            string input2 =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string inputFile2 = Path.Combine(_baseDir, "input2.xml");
            WriteFile(input2, inputFile2);


            int exitCode = Run(new string[] { "/nologo", "/i", inputFile1 + ";" + inputFile2, "/o", inputFile1 + ";" + inputFile2, "/v", "/x", Path.Combine(_baseDir, "..\\..\\SpreadsheetML.xml"), "/e", "Local" });

            Assert.AreEqual(0, exitCode);
        }

        #endregion MultipleInputAndOutputFileWithSemicolonsTest

        #region NormalizeArrayAlgorithmTest

        // ############################################################

        [Test]
        public void NormalizeArrayAlgorithmTest()
        {
            Console.WriteLine("** NormalizeArrayAlgorithmTest **");

            // this is "suboptimal" to be sure, but because of the way the unit test are
            // currently set up to only run xml preprocessor out of process, I wanted to make sure
            // the little algorithm I wrote to normalize the input and output arrays
            // was working properly. I just pasted the function right into the unit test
            ArrayList a;
            ArrayList b;
            
            a = new ArrayList();
            a.Add("1");
            a.Add("2");
            a.Add("3;4;5");
            b = NormalizeFileArray(a);
            Assert.AreEqual(5, b.Count);

            a = new ArrayList();
            a.Add("");
            a.Add("2");
            a.Add("3;4;5");
            b = NormalizeFileArray(a);
        }

        private ArrayList NormalizeFileArray(ArrayList inList)
        {
            ArrayList outList = new ArrayList();

            foreach (string fileParam in inList)
            {
                if (!string.IsNullOrEmpty(fileParam))
                {
                    foreach (string file in fileParam.Split(';'))
                    {
                        outList.Add(file);
                    }
                }
                else
                {
                    outList.Add(fileParam);
                }
            }
            return outList;
        }

        #endregion NormalizeArrayAlgorithmTest

        #region XPathBindingTest

        // ############################################################

        [Test]
        public void XPathBindingTest()
        {
            Console.WriteLine("** XPathBindingTest **");

            string input =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<xml>
   <!-- Comment -->
   <entry foo=""True"" />
   <entry foo=""True"" />
</xml>";

            string expected =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<xml>
   <!-- Comment -->
   <entry
    foo=""bar"" />
   <entry
    foo=""bar"" />
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", @"${XPath=/xml/entry/@foo IncludedFiles=*.xml}=bar" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion XPathBindingTest

        #region XPathBindingTestWithNamespace

        // ############################################################

        [Test]
        public void XPathBindingTestWithNamespace()
        {
            Console.WriteLine("** XPathBindingTestWithNamespace **");

            string input =
@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<a:assembly xmlns:a=""urn:schemas-microsoft-com:asm.v1""
  manifestVersion=""1.0"">
<a:assemblyIdentity
    name=""Application.exe""
    version=""1.0.0.0""
    type=""win32""
    processorArchitecture=""x86"" />
</a:assembly>";

            string expected =
@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<a:assembly xmlns:a=""urn:schemas-microsoft-com:asm.v1""
  manifestVersion=""1.0"">
<a:assemblyIdentity
    name=""Application.exe""
    version=""1.0.1.0""
    type=""win32""
    processorArchitecture=""x86"" />
</a:assembly>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", @"${Xpath=/a:assembly/a:assemblyIdentity/@version}=1.0.1.0" });

            string actual = ReadFile(inputFile);
            Assert.AreEqual(expected, actual);
        }

        #endregion XPathBindingTestWithNamespace

        #region XPathBindingTestWithDefaultNamespace

        // ############################################################

        [Test]
        public void XPathBindingTestWithDefaultNamespace()
        {
            Console.WriteLine("** XPathBindingTestWithDefaultNamespace **");

            string input =
@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<assembly xmlns=""urn:schemas-microsoft-com:asm.v1""
  manifestVersion=""1.0"">
<assemblyIdentity
    name=""Application.exe""
    version=""1.0.0.0""
    type=""win32""
    processorArchitecture=""x86"" />
</assembly>";

            string expected =
@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<assembly xmlns=""urn:schemas-microsoft-com:asm.v1""
  manifestVersion=""1.0"">
<assemblyIdentity
    name=""Application.exe""
    version=""1.0.1.0""
    type=""win32""
    processorArchitecture=""x86"" />
</assembly>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", @"${Xpath=/assembly/assemblyIdentity/@version}=1.0.1.0" });

            string actual = ReadFile(inputFile);
            Assert.AreEqual(expected, actual);
        }

        #endregion XPathBindingTestWithDefaultNamespace

        #region XPathRemoveNodeTest

        // ############################################################

        [Test]
        public void XPathRemoveNodeTest()
        {
            Console.WriteLine("** XPathRemoveNodeTest **");

            string input =
@"<xml>
   <SomeTestOnlySetting RemoveMe=""True"" />
</xml>";

            string expected =
@"<xml>
   
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", @"${XPath=//SomeTestOnlySetting}=#remove" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion XPathRemoveNodeTest

        #region XPathRemoveNodeTest2

        // ############################################################

        [Test]
        public void XPathRemoveNodeTest2()
        {
            Console.WriteLine("** XPathRemoveNodeTest2 **");

            string input =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  
  <appSettings>
    <add
      key=""SomeSwitch""
      value=""False"" />
    <add
      key=""SomeSwitchToCompletelyRemove""
      value=""False"" />
  </appSettings>
  
  <connectionStrings>
    <!-- This will connect to the @environment@ database -->
    <add
      name=""LocalSqlServer""
      connectionString=""Database=db;Server=local;Integrated Security=SSPI;MultipleActiveResultSets=true;""
      providerName=""System.Data.SqlClient"" />
  </connectionStrings>

  <system.web>
    <compilation
      debug=""true"" />
    
    <SomePlaceholder />

  </system.web>
  
</configuration>";

            string expected =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  
  <appSettings>
    <add
      key=""SomeSwitch""
      value=""False"" />
    
  </appSettings>
  
  <connectionStrings>
    <!-- This will connect to the @environment@ database -->
    <add
      name=""LocalSqlServer""
      connectionString=""Database=db;Server=local;Integrated Security=SSPI;MultipleActiveResultSets=true;""
      providerName=""System.Data.SqlClient"" />
  </connectionStrings>

  <system.web>
    <compilation
      debug=""true"" />
    
    <SomePlaceholder />

  </system.web>
  
</configuration>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", @"${Xpath=/configuration/appSettings/add[@key ='SomeSwitchToCompletelyRemove']}=#remove" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion XPathRemoveNodeTest2

        #region XPathRemoveNodeTestFromIfBranch

        // ############################################################

        [Test]
        public void XPathRemoveNodeTestFromIfBranch()
        {
            Console.WriteLine("** XPathRemoveNodeTestFromIfBranch **");

            string input =
@"<xml>
    <appSettings>
        <!-- #ifdef _xml_preprocess -->
        <!--
        <add key=""Remove1"" value=""Remove1""/>
        <add key=""Keep1"" value=""Keep1""/>
        <add key=""Remove2"" value=""Remove2""/>
        <add key=""Keep2"" value=""Keep2""/>
        <add key=""Remove3"" value=""Remove3""/>
        <add key=""Keep3"" value=""Keep3""/>
        -->
        <!-- #endif -->
    </appSettings>
</xml>";

            string expected =
@"<xml>
    <appSettings>
        <!-- #ifdef _xml_preprocess -->
        <!--
        <add key=""Remove1"" value=""Remove1""/>
        <add key=""Keep1"" value=""Keep1""/>
        <add key=""Remove2"" value=""Remove2""/>
        <add key=""Keep2"" value=""Keep2""/>
        <add key=""Remove3"" value=""Remove3""/>
        <add key=""Keep3"" value=""Keep3""/>
        -->
        <!-- #else -->
        
        <add
      key=""Keep1""
      value=""Keep1"" />
        
        <add
      key=""Keep2""
      value=""Keep2"" />
        
        <add
      key=""Keep3""
      value=""Keep3"" />
        <!-- #endif -->
    </appSettings>
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", @"${XPath=//add[starts-with(@key,'Remove')]}=#remove" });
            //Run(new string[] { "/nologo", "/i", inputFile, "/d", @"${XPath=//add[starts-with(@key,'Remove')]}=#undef" });

            string actual = ReadFile(inputFile);
            Assert.AreEqual(expected, actual);
        }

        #endregion XPathRemoveNodeTestFromIfBranch

        #region XPathInsertXMLTest

        // ############################################################

        [Test]
        public void XPathInsertXMLTest()
        {
            Console.WriteLine("** XPathInsertXMLTest **");

            string input =
@"<xml>
   <SomeTestOnlySetting/>
</xml>";

            string expected =
@"<xml>
   <SomeTestOnlySetting><SomeChildXml><AndMore /></SomeChildXml></SomeTestOnlySetting>
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", @"${XPath=//SomeTestOnlySetting}=<SomeChildXml><AndMore/></SomeChildXml>" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion XPathInsertXMLTest

        #region XPathNoMatchTest

        // ############################################################

        [Test]
        public void XPathNoMatchTest()
        {
            Console.WriteLine("** XPathNoMatchTest **");

            string input =
@"<xml>
   <SomeTestOnlySetting />
</xml>";

            string expected =
@"<xml>
   <SomeTestOnlySetting />
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", @"${XPath=//Foo}=<SomeChildXml><AndMore/></SomeChildXml>" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion XPathNoMatchTest

        #region XPathBindingNoXmlDeclarationTest

        // ############################################################

        [Test]
        public void XPathBindingNoXmlDeclarationTest()
        {
            Console.WriteLine("** XPathBindingNoXmlDeclarationTest **");

            string input =
@"<xml>
   <!-- Comment -->
   <entry foo=""True"" />
   <entry foo=""True"" />
</xml>";

            string expected =
@"<xml>
   <!-- Comment -->
   <entry
    foo=""bar"" />
   <entry
    foo=""bar"" />
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", @"${XPath=/xml/entry/@foo IncludedFiles=*.xml}=bar" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion XPathBindingNoXmlDeclarationTest

        #region RegExBindingTest

        // ############################################################

        [Test]
        public void RegExBindingTest()
        {
            Console.WriteLine("** RegExBindingTest **");

            string input =
@"<xml>
   <entry foo=""True"" />
   <entry foo=""True"" />
</xml>";

            string expected =
@"<xml>
   <bar foo=""True"" />
   <bar foo=""True"" />
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", @"${Regex=entry}=bar" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion RegExBindingTest

        #region RegistryBindingTest

        // ############################################################

        [Test]
        public void RegistryBindingTest()
        {
            Console.WriteLine("** RegistryBindingTest **");

            string input =
@"<xml>
   <entry foo=""${Registry=HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework\InstallRoot}"" />
   <entry foo=""${Registry=HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework\MissingValueNoDefault}"" />
   <entry foo=""${Registry=HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework\MissingValueNoDefault,}"" />
   <entry foo=""${Registry=HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework\MissingValueWithDefault,Default}"" />
   <entry foo=""${Registry=HKEY_LOCAL_MACHINE\MissingSubkey\FOO,Default}"" />
</xml>";

            string expected =
@"<xml>
   <entry foo=""C:\Windows\Microsoft.NET\Framework\"" />
   <entry foo="""" />
   <entry foo="""" />
   <entry foo=""Default"" />
   <entry foo=""Default"" />
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/n" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion RegistryBindingTest

        #region TestWildcard

        // ############################################################

        [Test]
        public void TestWildcard()
        {
            Console.WriteLine("** TestWildcard **");

            // this is "suboptimal" to be sure, but because of the way the unit test are
            // currently set up to only run xml preprocessor out of process, I wanted to make sure
            // this little algorithm was working properly. I just pasted the function right into the unit test

            Assert.IsTrue(ShouldProcess("abc.config", "abc.config"));
            Assert.IsFalse(ShouldProcess("abc.config", "def.config"));
            Assert.IsTrue(ShouldProcess("*.config", "abc.config"));
            Assert.IsFalse(ShouldProcess("*.config", "abc.xml"));
            Assert.IsFalse(ShouldProcess("*.config", "abc.xml"));
            Assert.IsTrue(ShouldProcess("*.config", @"c:\temp\abc.config"));
            Assert.IsTrue(ShouldProcess("*EntLib.config", @"c:\temp\ClientEntLib.config"));
            Assert.IsFalse(ShouldProcess("*EntLib.config", @"c:\temp\ClientEntLibx.config"));

        }

        protected bool ShouldProcess(string _fileSpec, string fileNameBeingProcessed)
        {
            if (string.IsNullOrEmpty(_fileSpec))
                return true;

            foreach (string filePattern in _fileSpec.Split(';'))
            {
                Regex r = new Regex("^" + Regex.Escape(filePattern).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase);
                if (r.IsMatch(Path.GetFileName(fileNameBeingProcessed)))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion TestWildcard

        #region TestFileMatch

        // ############################################################

        [Test]
        public void TestFileMatch()
        {
            Console.WriteLine("** TestFileMatch **");

            // this is "suboptimal" to be sure, but because of the way the unit test are
            // currently set up to only run xml preprocessor out of process, I wanted to make sure
            // this little algorithm was working properly. I just pasted the function right into the unit test

            string files;
            string path;

            GetFilesAndPath(@"XPath=configuration/loggingConfiguration/listeners/add[@name='Rolling Flat File Trace Listener']/@fileName IncludedFiles=ServerEntLib.config", out files, out path);
            Assert.AreEqual(@"ServerEntLib.config", files);
            Assert.AreEqual(@"configuration/loggingConfiguration/listeners/add[@name='Rolling Flat File Trace Listener']/@fileName", path);

            GetFilesAndPath(@"XPath=configuration/loggingConfiguration/listeners/add[@name='Rolling Flat File Trace Listener']/@fileName   IncludedFiles = ServerEntLib.config  ", out files, out path);
            Assert.AreEqual(@"ServerEntLib.config", files);
            Assert.AreEqual(@"configuration/loggingConfiguration/listeners/add[@name='Rolling Flat File Trace Listener']/@fileName", path);

            GetFilesAndPath(@"XPath=configuration/loggingConfiguration/listeners/add[@IncludedFiles = ServerEntLib.config ]/@fileName IncludedFiles = ServerEntLib.config;ClientEntLib.config", out files, out path);
            Assert.AreEqual(@"ServerEntLib.config;ClientEntLib.config", files);
            Assert.AreEqual(@"configuration/loggingConfiguration/listeners/add[@IncludedFiles = ServerEntLib.config ]/@fileName", path);
        }

        protected void GetFilesAndPath(string bindingValue, out string files, out string path)
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

        #endregion TestFileMatch

        #region NonWellFormedTest

        // ############################################################

        [Test]
        public void NonWellFormedTest()
        {
            Console.WriteLine("** NonWellFormedTest **");

            string input =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${PROPERTY}""""/> -->
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/d", "PROPERTY", "/v" });

            Assert.AreEqual(1, exitCode);
        }

        #endregion NonWellFormedTest

        #region MissingEnvironmentTest

        // ############################################################
 
        [Test]
        public void MissingEnvironmentTest()
        {
            Console.WriteLine("** MissingEnvironmentTest **");

            string input =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/q", "/v", "/x", Path.Combine(_baseDir, "..\\..\\SpreadsheetML.xml"), "/e", "" });

            Assert.AreEqual(1, exitCode);
        }

        #endregion MissingEnvironmentTest

        #region ListSettingTest

        // ############################################################

        [Test]
        public void ListSettingTest()
        {
            Console.WriteLine("** ListSettingTest **");

            int exitCode = Run(new string[] { "/nologo", "/x", Path.Combine(_baseDir, "..\\..\\SpreadsheetML.xml"), "/e", "Local" , "/p", "Setting4" });

            Assert.AreEqual(0, exitCode);
        }

        #endregion ListSettingTest

        #region ListSettingValuesTest

        // ############################################################

        [Test]
        public void ListSettingValuesTest()
        {
            Console.WriteLine("** ListSettingValuesTest **");

            int exitCode = Run(new string[] { "/nologo", "/x", Path.Combine(_baseDir, "..\\..\\SpreadsheetML.xml"), "/l", "/p", "Setting5" });

            Assert.AreEqual(0, exitCode);
        }

        #endregion ListSettingValuesTest

        #region ListSettingWithMacrosValuesTest

        // ############################################################

        [Test]
        public void ListSettingWithMacrosValuesTest()
        {
            Console.WriteLine("** ListSettingWithMacrosValuesTest **");

            int exitCode = Run(new string[] { "/nologo", "/x", Path.Combine(_baseDir, "..\\..\\SpreadsheetML.xml"), "/p", "Setting5", "/e", "Test" });

            Assert.AreEqual(0, exitCode);
        }

        #endregion ListSettingWithMacrosValuesTest

        #region ListSettingWithMacrosValuesTest

        // ############################################################

        [Test]
        public void ListSettingsWithMacrosValuesTest()
        {
            Console.WriteLine("** ListSettingWithMacrosValuesTest **");

            int exitCode = Run(new string[] { "/nologo", "/x", Path.Combine(_baseDir, "..\\..\\SpreadsheetML.xml"), "/l", "/p", "Setting5" });

            Assert.AreEqual(0, exitCode);
        }

        #endregion ListSettingsWithMacrosValuesTest

        #region DefineTest

        // ############################################################

        [Test]
        public void DefineTest()
        {
            Console.WriteLine("** DefineTest **");

            string input =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<xml>
   <!-- #define ${XPath=/xml/entry/@foo} = ${sometoken} -->
   <entry foo=""JUNK VALUE"" />
</xml>";

            string expected =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<xml>
   <!-- #define ${XPath=/xml/entry/@foo} = ${sometoken} -->
   <entry
    foo=""bar"" />
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", @"sometoken=bar" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion DefineTest

        #region StaticDefineTestInline

        // ############################################################

        [Test]
        public void StaticDefineTestInline()
        {
            Console.WriteLine("** StaticDefineTestInline **");

            string input =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<xml>
   <!-- #define SomeNewValue = ""sometoken"" -->
   <!-- #ifdef _xml_preprocess -->
   <!--
   <entry foo=""${SomeNewValue}"" />
   -->
   <!-- #endif -->
</xml>";

            string expected =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<xml>
   <!-- #define SomeNewValue = ""sometoken"" -->
   <!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${SomeNewValue}"" /> -->
   <!-- #else -->
   <entry foo=""sometoken"" />
   <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion StaticDefineTestInline

        #region MultilineDefineTest

        // ############################################################

        [Test]
        public void MultilineDefineTest()
        {
            Console.WriteLine("** MultilineDefineTest **");

            string input =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<xml>
   <!-- #define some_text = I
have
multiple
lines -->
   <!-- #ifdef _xml_preprocess -->
   <!--
   <entry>${some_text}</entry>
   -->
   <!-- #endif -->
</xml>";

            string expected =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<xml>
   <!-- #define some_text = I
have
multiple
lines -->
   <!-- #ifdef _xml_preprocess -->
   <!-- <entry>${some_text}</entry> -->
   <!-- #else -->
   <entry>I
have
multiple
lines</entry>
   <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion MultilineDefineTest

        #region StaticDefineTest

        // ############################################################

        [Test]
        public void StaticDefineTest()
        {
            Console.WriteLine("** StaticDefineTest **");

            string input =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<xml>
   <!-- #define SomeNewValue = ${sometoken} -->
   <!-- #ifdef _xml_preprocess -->
   <!--
   <entry foo=""${SomeNewValue}"" />
   -->
   <!-- #endif -->
</xml>";

            string expected =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<xml>
   <!-- #define SomeNewValue = ${sometoken} -->
   <!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${SomeNewValue}"" /> -->
   <!-- #else -->
   <entry foo=""bar"" />
   <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", @"sometoken=bar" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion StaticDefineTest

        #region ExpressionTest

        // ############################################################

        [Test]
        public void ExpressionTest()
        {
            Console.WriteLine("** ExpressionTest **");

            string input =
@"<xml>
   <entry foo=""${Script=}"" />
   <entry foo=""${Script= GetProperty(""val1"")==""abc"" ? ""yes"" : GetProperty(""val3"") }"" />
   <entry foo=""${ script= GetProperty(""val1"")==""xyz"" ? ""yes"" : GetProperty(""val3"") }"" />
   <entry foo=""${script = GetProperty(""val1"").IndexOf(""a"") > -1 ? ""yes"" : GetProperty(""val3"") }"" />
   <entry foo=""${script=GetProperty(""val1"") + GetProperty(""val3"") }"" />
</xml>";

            string expected1 =
@"<xml>
   <entry foo="""" />
   <entry foo=""yes"" />
   <entry foo=""no"" />
   <entry foo=""yes"" />
   <entry foo=""abcno"" />
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", @"val1=abc", "/d", @"val3=no", "/n"});

            Assert.AreEqual(expected1, ReadFile(inputFile));
        }

        #endregion ExpressionTest

        #region IncludesTest

        // ############################################################

        [Test]
        public void IncludesTest()
        {
            Console.WriteLine("** IncludesTest **");

            string input =
@"<xml>
   <!-- #include ""included-${_environment_name}.xml"" -->
</xml>";

            string includedFileContents =
@"<!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${val1}"" /> -->
   <!-- #endif -->";

            string expected1 =
@"<xml>
   <!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${val1}"" /> -->
   <!-- #else -->
   <entry foo=""abc"" />
   <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            string includedFile = Path.Combine(_baseDir, "included-Production.xml");
            WriteFile(includedFileContents, includedFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", @"val1=abc", "/e", "Production" });

            Assert.AreEqual(expected1, ReadFile(inputFile));
        }

        #endregion IncludesTest

        #region IncludesTest2

        // ############################################################

        [Test]
        public void IncludesTest2()
        {
            Console.WriteLine("** IncludesTest2 **");

            string input =
@"<xml>
   <!-- #include ""${filename}"" -->
</xml>";

            string includedFileContents =
@"<!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${val1}"" /> -->
   <!-- #endif -->";

            string expected1 =
@"<xml>
   <!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${val1}"" /> -->
   <!-- #else -->
   <entry foo=""abc"" />
   <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            string includedFile = Path.Combine(_baseDir, "included-Production.xml");
            WriteFile(includedFileContents, includedFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", @"val1=abc", "/d", "filename=included-Production.xml" });

            Assert.AreEqual(expected1, ReadFile(inputFile));
        }

        #endregion IncludesTest2

        #region UndefinedIncludesTest

        // ############################################################

        [Test]
        public void UndefinedIncludesTest()
        {
            Console.WriteLine("** UndefinedIncludesTest **");

            string input =
@"<xml>
   <!-- #include ""${filename}"" -->
</xml>";

            string includedFileContents =
@"<!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${val1}"" /> -->
   <!-- #endif -->";

            string expected1 =
@"<xml>
   <!-- #include ""${filename}"" -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            string includedFile = Path.Combine(_baseDir, "included-Production.xml");
            WriteFile(includedFileContents, includedFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/v", "/d", @"val1=abc", "/d", "filenameX=included-Production.xml" });

            Assert.AreEqual(expected1, ReadFile(inputFile));
        }

        #endregion UndefinedIncludesTest

        #region IncludeWithDefinedFileName

        // ############################################################

        [Test]
        public void IncludeWithDefinedFileName()
        {
            Console.WriteLine("** IncludeWithDefinedFileName **");

            string input =
@"<xml>
<!-- #define inputFile=""something.xml"" -->
<!-- #include ""${inputFile}"" -->
</xml>";

            string includedFileContents =
@"<property name=""something"" value=""another"" />";

            string expected1 =
@"<xml>
<!-- #define inputFile=""something.xml"" -->
<property name=""something"" value=""another"" />
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            string includedFile = Path.Combine(_baseDir, "something.xml");
            WriteFile(includedFileContents, includedFile);

            Run(new string[] { "/nologo", "/v", "/q", "/c", "/i", inputFile });

            Assert.AreEqual(expected1, ReadFile(inputFile));
        }

        #endregion IncludeWithDefinedFileName

        #region IncludesWithXPathTest

        // ############################################################

        [Test]
        public void IncludesWithXPathTest()
        {
            Console.WriteLine("** IncludesWithXPathTest **");

            string input =
@"<xml>
  <!-- #include ""included-${_environment_name}.xml"" xpath=""/environments/environment[@name='${_environment_name}']"" -->
</xml>";

            string includedFileContents =
@"<environments>

  <environment name=""Development"">
    <connectionStrings>
      <developmentConnectionStrings/>
    </connectionStrings>
  </environment>

  <environment name=""Production"">
    <!-- COMMENT -->
    <connectionStrings>
      <productionConnectionStrings/>
    </connectionStrings>
  </environment>

</environments>";

            string expected1 =
@"<xml>
  
    <!-- COMMENT -->
    <connectionStrings>
      <productionConnectionStrings />
    </connectionStrings>
  
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            string includedFile = Path.Combine(_baseDir, "included-Production.xml");
            WriteFile(includedFileContents, includedFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", @"val1=abc", "/e", "Production" });

            string actual = ReadFile(inputFile);
            Assert.AreEqual(expected1, actual);
        }

        #endregion IncludesWithXPathTest

        #region NestedCommentTest

        // ############################################################

        [Test]
        public void NestedCommentTest()
        {
            Console.WriteLine("** NestedCommentTest **");

            string input =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!--
   < ! - - This is a nested comment - - >
   <entry foo=""${PROPERTY}""/>
   -->
   <!-- #else -->
   <entry foo=""abc""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!--
   < ! - - This is a nested comment - - >
   <entry foo=""${PROPERTY}""/>
   -->
   <!-- #else -->
   <!-- This is a nested comment -->
   <entry foo=""newvalue""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PRODUCTION", "/d", "PROPERTY=newvalue" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion NestedCommentTest

        #region EscapedValueTest

        // ############################################################

        [Test]
        public void EscapedValueTest()
        {
            Console.WriteLine("** EscapedValueTest **");

            string input =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!--
   <entry foo=""${PROPERTY}""/>
   <entry foo=""${newvalue}""/>
   -->
   <!-- #else -->
   <entry foo=""abc""/>
   <entry foo=""def""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!--
   <entry foo=""${PROPERTY}""/>
   <entry foo=""${newvalue}""/>
   -->
   <!-- #else -->
   <entry foo=""${newvalue}""/>
   <entry foo=""test""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PRODUCTION", "/d", @"PROPERTY=$${{newvalue}}", "/d", "newvalue=test" });

            string actual = ReadFile(inputFile);
            Assert.AreEqual(expected, actual);
        }

        #endregion EscapedValueTest

        #region EscapedValueTestEmbedded

        // ############################################################

        [Test]
        public void EscapedValueTestEmbedded()
        {
            Console.WriteLine("** EscapedValueTestEmbedded **");

            string input =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!--
   <entry foo=""${PROPERTY}""/>
   <entry foo=""${newvalue}""/>
   -->
   <!-- #else -->
   <entry foo=""abc""/>
   <entry foo=""def""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!--
   <entry foo=""${PROPERTY}""/>
   <entry foo=""${newvalue}""/>
   -->
   <!-- #else -->
   <entry foo=""Something ${newvalue} And More""/>
   <entry foo=""test""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PRODUCTION", "/d", @"PROPERTY=Something $${{newvalue}} And More", "/d", "newvalue=test" });

            string actual = ReadFile(inputFile);
            Assert.AreEqual(expected, actual);
        }

        #endregion EscapedValueTestEmbedded

        #region SimpleIfTest

        // ############################################################

        [Test]
        public void SimpleIfTest()
        {
            Console.WriteLine("** SimpleIfTest **");

            string input =
@"<xml>

   <!-- #if defined(""PRODUCTION"") -->
   <!-- <entry foo=""${PRODUCTION}""/> -->
   <!-- #else -->
   <entry foo=""False""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #if defined(""PRODUCTION"") -->
   <!-- <entry foo=""${PRODUCTION}""/> -->
   <!-- #else -->
   <entry foo=""""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PRODUCTION" });

            string actual = ReadFile(inputFile);
            Assert.AreEqual(expected, actual);
        }

        #endregion SimpleIfTest

        #region ComplexIfTest

        // ############################################################

        [Test]
        public void ComplexIfTest()
        {
            Console.WriteLine("** ComplexIfTest **");

            string input =
@"<xml>

   <!-- #if defined(""DoServerCheck"") && (GetProperty(""PrimaryServers"").IndexOf(GetProperty(""Machine""), StringComparison.OrdinalIgnoreCase) > -1) -->
   <!-- <entry foo=""${SomeVal}""/> -->
   <!-- #else -->
   <entry foo=""False""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #if defined(""DoServerCheck"") && (GetProperty(""PrimaryServers"").IndexOf(GetProperty(""Machine""), StringComparison.OrdinalIgnoreCase) > -1) -->
   <!-- <entry foo=""${SomeVal}""/> -->
   <!-- #else -->
   <entry foo=""abc""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "SomeVal=abc", "/d", "DoServerCheck=True", "/d", "PrimaryServers=server1;server2", "/d", "Machine=SERVER1" });

            string actual = ReadFile(inputFile);
            Assert.AreEqual(expected, actual);
        }

        #endregion ComplexIfTest

        #region ForEachSimpleTest

        // ############################################################

        [Test]
        public void ForEachSimpleTest()
        {
            Console.WriteLine("** ForEachSimpleTest **");

            string input =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- #foreach(SomeVal) <entry foo=""${SomeVal}""/> -->
   <!-- #else -->
   <entry foo=""One""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- #foreach(SomeVal) <entry foo=""${SomeVal}""/> -->
   <!-- #else -->
   <entry foo=""One""/>
   <entry foo=""Two""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "SomeVal=One;Two" });

            string actual = ReadFile(inputFile);
            Assert.AreEqual(expected, actual);
        }

        #endregion ForEachSimpleTest

        #region ForEachWithMultipleValuesTest

        // ############################################################

        [Test]
        public void ForEachWithMultipleValuesTest()
        {
            Console.WriteLine("** ForEachWithMultipleValuesTest **");

            string input =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- #foreach(SomeVal,SomeOtherVal) <entry foo=""${SomeVal}"" bar=""${SomeOtherVal}""/> -->
   <!-- #else -->
   <entry foo=""One"" bar=""Alpha""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- #foreach(SomeVal,SomeOtherVal) <entry foo=""${SomeVal}"" bar=""${SomeOtherVal}""/> -->
   <!-- #else -->
   <entry foo=""One"" bar=""Alpha""/>
   <entry foo=""Two"" bar=""Beta""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "SomeVal=One;Two", "/d", "SomeOtherVal=Alpha;Beta" });

            string actual = ReadFile(inputFile);
            Assert.AreEqual(expected, actual);
        }

        #endregion ForEachWithMultipleValuesTest

        #region ForEachWithRaggedMultipleValuesTest

        // ############################################################

        [Test]
        public void ForEachWithRaggedMultipleValuesTest()
        {
            Console.WriteLine("** ForEachWithRaggedMultipleValuesTest **");

            string input =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- #foreach(SomeVal,SomeOtherVal) <entry foo=""${SomeVal}"" bar=""${SomeOtherVal}""/> -->
   <!-- #else -->
   <entry foo=""One"" bar=""Alpha""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- #foreach(SomeVal,SomeOtherVal) <entry foo=""${SomeVal}"" bar=""${SomeOtherVal}""/> -->
   <!-- #else -->
   <entry foo=""One"" bar=""Alpha""/>
   <entry foo=""Two"" bar=""""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "SomeVal=One;Two", "/d", "SomeOtherVal=Alpha" });

            string actual = ReadFile(inputFile);
            Assert.AreEqual(expected, actual);
        }

        #endregion ForEachWithRaggedMultipleValuesTest

        #region ForEachWithMultipleLinesTest

        // ############################################################

        [Test]
        public void ForEachWithMultipleLinesTest()
        {
            Console.WriteLine("** ForEachWithMultipleLinesTest **");

            string input =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- #foreach(SomeVal,SomeOtherVal)
   <wrapper>
     <entry foo=""${SomeVal}""
            bar=""${SomeOtherVal}""/>
   </wrapper>
   -->
   <!-- #else -->
   <wrapper>
     <entry foo=""One""
            bar=""Alpha""/>
   </wrapper>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!--
   #foreach(SomeVal,SomeOtherVal)
   <wrapper>
     <entry foo=""${SomeVal}""
            bar=""${SomeOtherVal}""/>
   </wrapper>
   -->
   <!-- #else -->
   <wrapper>
     <entry foo=""One""
            bar=""Alpha""/>
   </wrapper>
   <wrapper>
     <entry foo=""Two""
            bar=""""/>
   </wrapper>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "SomeVal=One;Two", "/d", "SomeOtherVal=Alpha" });

            string actual = ReadFile(inputFile);
            Assert.AreEqual(expected, actual);
        }

        #endregion ForEachWithMultipleLinesTest

        #region ForEachWithMultipleCommentLinesTest

        // ############################################################

        [Test]
        public void ForEachWithMultipleCommentLinesTest()
        {
            Console.WriteLine("** ForEachWithMultipleCommentLinesTest **");

            string input =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- #foreach(SomeVal,SomeOtherVal) -->
   <!-- <wrapper> -->
   <!--   <entry foo=""${SomeVal}"" bar=""${SomeOtherVal}""/> -->
   <!-- </wrapper> -->
   <!-- #else -->
   <wrapper>
     <entry foo=""One"" bar=""Alpha""/>
   </wrapper>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!--
   #foreach(SomeVal,SomeOtherVal)
   <wrapper>
   <entry foo=""${SomeVal}"" bar=""${SomeOtherVal}""/>
   </wrapper>
   -->
   <!-- #else -->
   <wrapper>
   <entry foo=""One"" bar=""Alpha""/>
   </wrapper>
   <wrapper>
   <entry foo=""Two"" bar=""""/>
   </wrapper>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "SomeVal=One;Two", "/d", "SomeOtherVal=Alpha" });

            string actual = ReadFile(inputFile);
            Assert.AreEqual(expected, actual);
        }

        #endregion ForEachWithMultipleCommentLinesTest

        #region MultipleFileTest

        // ############################################################

        [Test]
        public void MultipleFileTest()
        {
            Console.WriteLine("** MultipleFileTest **");

            string input =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""abc""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""newvalue""/>
   <!-- #endif -->

</xml>";

            string inputFile1 = Path.Combine(_baseDir, "input1.xml");
            string inputFile2 = Path.Combine(_baseDir, "input2.xml");
            WriteFile(input, inputFile1);
            WriteFile(input, inputFile2);

            Run(new string[] { "/nologo", "/i", inputFile1 + ";" + inputFile2, "/d", "PRODUCTION", "/d", "PROPERTY=newvalue" });

            Assert.AreEqual(expected, ReadFile(inputFile1));
            Assert.AreEqual(expected, ReadFile(inputFile2));
        }

        #endregion MultipleFileTest

        #region MultipleFileWithEmptyFileTest

        // ############################################################

        [Test]
        public void MultipleFileWithEmptyFileTest()
        {
            Console.WriteLine("** MultipleFileWithEmptyFileTest **");

            string input =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""abc""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""newvalue""/>
   <!-- #endif -->

</xml>";

            string inputFile1 = Path.Combine(_baseDir, "input1.xml");
            string inputFile2 = Path.Combine(_baseDir, "input2.xml");
            WriteFile(input, inputFile1);
            WriteFile(input, inputFile2);

            int exitCode = Run(new string[] { "/nologo", "/i", inputFile1 + ";" + inputFile2 + ";", "/d", "PRODUCTION", "/d", "PROPERTY=newvalue" });
            Assert.AreEqual(0, exitCode, "Preprocessor returned error");

            Assert.AreEqual(expected, ReadFile(inputFile1));
            Assert.AreEqual(expected, ReadFile(inputFile2));
        }

        #endregion MultipleFileWithEmptyFileTest

        #region CustomDataSourceTest

        // ############################################################
        [Test]
        public void CustomDataSourceTest()
        {
            Console.WriteLine("** CustomDataSourceTest **");

            string input =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string expected =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""setting1default""/>
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            string command = Path.Combine(_baseDir, "..\\..\\CustomScript.bat") + " > @tempFile@";
            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/cx", command, "/e", "Local" });

            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion CustomDataSourceTest

        #region WildcardSimpleTest

        // ############################################################

        [Test]
        public void WildcardSimpleTest()
        {
            Console.WriteLine("** WildcardSimpleTest **");

            string input =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""abc""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef PRODUCTION -->
   <!-- <entry foo=""${PROPERTY}""/> -->
   <!-- #else -->
   <entry foo=""newvalue""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", Path.Combine(_baseDir, "*.xml"), "/d", "PRODUCTION", "/d", "PROPERTY=newvalue" });


            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion WildcardSimpleTest

        #region ZeroTokenTest

        // ############################################################

        [Test]
        public void ZeroTokenTest()
        {
            Console.WriteLine("** ZeroTokenTest **");

            string input =
@"<configuration>
  <appSettings>
   <add key=""foo"" value=""${PROPERTY}""/>
   <add key=""bar"" value=""abc${0}""/>
  </appSettings>
</configuration>";

            string expected =
@"<configuration>
  <appSettings>
   <add key=""foo"" value=""newvalue""/>
   <add key=""bar"" value=""abc${0}""/>
  </appSettings>
</configuration>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            // there ought to be a cleaner way to do this, but for now
            // defining a setting named "0", with a value of "$${{0}}" works
            Run(new string[] { "/nologo", "/n", "/i", inputFile, "/d", "PRODUCTION", "/d", "0=$${{0}}", "/d", "PROPERTY=newvalue" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion ZeroTokenTest



        // INTEGRATION TESTS - THESE REQUIRE MACHINE SETUP

        #region DbTest

        // ############################################################
        [Test]
        [Ignore("Set up required")]
        public void DbTest()
        {
            Console.WriteLine("** DbTest **");
            Console.WriteLine("- Test setup required: Must create database (see samples\\database for scripts)");

            string input =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string expected =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""setting1default""/>
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            //string connectionString = "Data Source=localhost;Initial Catalog=ConfigSettings;Integrated Security=SSPI;";
            string connectionString = "Server=localhost;Database=ConfigSettings;Trusted_Connection=True;";
            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/v", "/db", connectionString, "/e", "Local" });


            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion DbTest

        #region SettingsFileUrlTest

        // ############################################################
        [Test]
        public void SettingsFileUrlTest()
        {
            Console.WriteLine("** SettingsFileUrlTest **");
            Console.WriteLine("- Test setup required: Must copy TestSettings.xml to C:\\inetpub\\wwwroot");

            string input =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string expected =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""setting1default""/>
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/v", "/s", "http://localhost/TestSettings.xml" });

            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion SettingsFileUrlTest

        #region ExcelFileUrlTest (not supported)
        // Loading Excel file with URL not supported by OLEDB driver

//        // ############################################################
//        [Test]
//        public void ExcelFileUrlTest()
//        {
//            Console.WriteLine("** ExcelFileUrlTest **");

//            string input =
//@"<xml>
//    <!-- #ifdef _xml_preprocess -->
//    <!-- <entry foo=""${Setting1}""/> -->
//    <!-- #else -->
//    <entry foo=""tcp""/>
//    <!-- #endif -->
//</xml>";

//            string expected =
//@"<xml>
//    <!-- #ifdef _xml_preprocess -->
//    <!-- <entry foo=""${Setting1}""/> -->
//    <!-- #else -->
//    <entry foo=""setting1default""/>
//    <!-- #endif -->
//</xml>";

//            string inputFile = Path.Combine(_baseDir, "input.xml");
//            WriteFile(input, inputFile);

//            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/v", "/x", "http://localhost/TestSettings.xls", "/e", "Local" });


//            Assert.AreEqual(0, exitCode);
//            Assert.AreEqual(expected, ReadFile(inputFile));
//        }

        #endregion ExcelFileUrlTest (not supported

        #region SpreadsheetMLUrlTest

        // ############################################################

        [Test]
        public void SpreadsheetMLUrlTest()
        {
            Console.WriteLine("** SpreadsheetMLUrlTest **");
            Console.WriteLine("- Test setup required: Must copy SpreadsheetML.xml to C:\\inetpub\\wwwroot");

            string input =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string expected =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""setting1default""/>
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/v", "/x", "http://localhost/SpreadsheetML.xml", "/e", "Local" });

            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion SpreadsheetMLUrlTest

        #region CsvUrlTest

        // ############################################################

        [Test]
        public void CsvUrlTest()
        {
            Console.WriteLine("** CsvUrlTest **");
            Console.WriteLine("- Test setup required: Must copy TestSettings.csv to C:\\inetpub\\wwwroot");

            string input =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""tcp""/>
    <!-- #endif -->
</xml>";

            string expected =
@"<xml>
    <!-- #ifdef _xml_preprocess -->
    <!-- <entry foo=""${Setting1}""/> -->
    <!-- #else -->
    <entry foo=""setting1default""/>
    <!-- #endif -->
</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            int exitCode = Run(new string[] { "/nologo", "/i", inputFile, "/v", "/x", "http://localhost/TestSettings.csv", "/e", "Local" });

            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion CsvUrlTest

        #region NestedPropertyTest

        // ############################################################

        [Test]
        public void NestedPropertyTest()
        {
            Console.WriteLine("** NestedPropertyTest **");

            string input =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${PROPERTY_${MACHINE}}""/> -->
   <!-- #else -->
   <entry foo=""abc""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${PROPERTY_${MACHINE}}""/> -->
   <!-- #else -->
   <entry foo=""prop2""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "MACHINE=FOO", "/d", "PROPERTY=newvalue", "/d", "PROPERTY_FOO=prop2" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion NestedPropertyTest

        #region FallbackPropertyTest

        // ############################################################

        [Test]
        public void FallbackPropertyTest()
        {
            Console.WriteLine("** FallbackPropertyTest **");

            string input =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${PROPERTY1;PROPERTY2}""/> -->
   <!-- #else -->
   <entry foo=""abc""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!-- <entry foo=""${PROPERTY1;PROPERTY2}""/> -->
   <!-- #else -->
   <entry foo=""newvalue""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "PROPERTY2=newvalue" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion FallbackPropertyTest

        #region NestedFallbackPropertyTest

        // ############################################################

        [Test]
        public void NestedFallbackPropertyTest()
        {
            Console.WriteLine("** NestedFallbackPropertyTest **");

            string input =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!--
   <entry foo=""${PROPERTY_${MACHINE};PROPERTY}""/>
   <entry foo=""${PROPERTY_BAR;PROPERTY}""/>
   -->
   <!-- #else -->
   <entry foo=""abc""/>
   <entry foo=""abc""/>
   <!-- #endif -->

</xml>";

            string expected =
@"<xml>

   <!-- #ifdef _xml_preprocess -->
   <!--
   <entry foo=""${PROPERTY_${MACHINE};PROPERTY}""/>
   <entry foo=""${PROPERTY_BAR;PROPERTY}""/>
   -->
   <!-- #else -->
   <entry foo=""prop2""/>
   <entry foo=""newvalue""/>
   <!-- #endif -->

</xml>";

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            Run(new string[] { "/nologo", "/i", inputFile, "/d", "MACHINE=FOO", "/d", "PROPERTY=newvalue", "/d", "PROPERTY_FOO=prop2" });

            Assert.AreEqual(expected, ReadFile(inputFile));
        }

        #endregion NestedFallbackPropertyTest

        #region Utilities

        private int RunTest(string input, string settings, string expected)
        {
            int exitCode = 0;

            string inputFile = Path.Combine(_baseDir, "input.xml");
            WriteFile(input, inputFile);

            string settingsFile = Path.Combine(_baseDir, "settings.xml");
            WriteFile(settings, settingsFile);

            exitCode = Run(new string[] {"/nologo", "/i", inputFile, "/s", settingsFile});

            Assert.AreEqual(expected, ReadFile(inputFile));

            return exitCode;
        }

        private int Run(string[] args)
        {
            string assemblyPath = Path.Combine(_baseDir, "XmlPreprocess.exe");
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            Type type = assembly.GetType("XmlPreprocess.XmlPreprocessorMain");
            MethodInfo methInfo = type.GetMethod("Main", new Type[] { typeof(string[]) });
            return (int)methInfo.Invoke(null, new object[] { args });
        }

        private void WriteFile(string text, string fileName)
        {
            StreamWriter sw = File.CreateText(fileName);
            sw.Write(text);
            sw.Close();
        }

        private string ReadFile(string fileName)
        {
            string contents = null;
            using (StreamReader sr = File.OpenText(fileName))
            {
                contents = sr.ReadToEnd();
            }
            return contents;
        }

        #endregion Utilities
    }
}
