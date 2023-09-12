// -----------------------------------------------------------------------------
// Copyright (c) 2023 Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

namespace Psw.Scanners
{
    /// <summary>
    /// Interface for Logging and Querying errors during scanning/parsing/other operations.<br/>
    /// > All Scanner derivatives implement this interface on the bound [ScanErrorLog](Docs/ScanErrorLog.md)
    /// </summary>
    public interface ILogScanError
    {
        /// <summary>
        /// Query if an error is currently logged.
        /// </summary>
        public bool IsError { get; }

        /// <summary>
        /// Log an error while scanning/parsing/other.
        /// </summary>
        /// <param name="errorMsg">Error message to log.</param>
        /// <param name="errorContext">Error context to log.</param>
        /// <param name="errIndex">Error Index in source (-1 = current else specific)</param>
        /// <returns>Typically should be implemented to always return false so that the caller may return this as an error condition.</returns>
        public bool LogError(string errorMsg, string errorContext = "Parse error", int errIndex = -1);
    }
}
