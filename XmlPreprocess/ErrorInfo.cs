/*
 * Copyright (c) 2004-2016 Loren M Halvorson
 * This source is subject to the Microsoft Public License (Ms-PL).
 * See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
 * All other rights reserved.
 * Portions copyright 2002-2007 The Genghis Group (http://www.genghisgroup.com/)
 * Portions copyright 2007-08 Thomas F. Abraham.
 */

using System;

namespace XmlPreprocess
{
    /// <summary>
    /// Possible Error codes
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>
        /// An exception ocurred
        /// </summary>
        ErrorException = 100,

        /// <summary>
        /// File not found
        /// </summary>
        ErrorFileNotFound = 101,

        /// <summary>
        /// Missing Token
        /// </summary>
        ErrorMissingToken = 102,

        /// <summary>
        /// Not well-formed
        /// </summary>
        ErrorNotWellFormed = 103,

    }

    /// <summary>
    /// Holds errors that need to be conveyed to the user
    /// </summary>
    public class ErrorInfo
    {
        private ErrorCode _errorNumber;
        private string _message;
        private string _inputFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorInfo"/> class.
        /// </summary>
        /// <param name="errorNumber">The error number.</param>
        /// <param name="inputFile">The file.</param>
        public ErrorInfo(ErrorCode errorNumber, string inputFile)
        {
            _errorNumber = errorNumber;
            _inputFile = inputFile;
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        /// <summary>
        /// Gets or sets the error number.
        /// </summary>
        /// <value>The error number.</value>
        public ErrorCode ErrorCode
        {
            get { return _errorNumber; }
            set { _errorNumber = value; }
        }

        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>The file.</value>
        public string InputFile
        {
            get { return _inputFile; }
            set { _inputFile = value; }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(InputFile))
                return string.Format("Error XMLPP{0}: {1} Input File: {2}", (int)ErrorCode, Message, InputFile);
            else
                return string.Format("Error XMLPP{0}: {1}", (int)ErrorCode, Message);
        }
    }
}
