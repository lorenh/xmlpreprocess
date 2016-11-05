/*
 * Copyright (c) 2004-2016 Loren M Halvorson
 * This source is subject to the Microsoft Public License (Ms-PL).
 * See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
 * All other rights reserved.
 * Portions copyright 2002-2007 The Genghis Group (http://www.genghisgroup.com/)
 * Portions copyright 2007-08 Thomas F. Abraham.
 */

using System;

namespace XmlPreprocess.Util
{
	/// <summary>
	/// Writes colored lines to the console
	/// </summary>
    public class ConsoleUtils
    {
        /// <summary>
        /// Writes colored lines to the console
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="text">The text.</param>
        public static void WriteLine(ConsoleColor color, string text)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = originalColor;
        }

        /// <summary>
        /// Writes colored lines to the console
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="text">The text.</param>
        public static void Write(ConsoleColor color, string text)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = originalColor;
        }
    }
}
