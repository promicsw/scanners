// -----------------------------------------------------------------------------
// Copyright (c) 2023 Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

namespace Psw.Scanners
{
    /// <summary>
    /// Interface defining a LogError method that can be used for error logging during scanning/parsing/other operations.<br/>
    /// 
    /// > TextScanner (and all derivatives) implement this interface on the bound [ScanErrorLog](Docs/ScanErrorLog.md)
    /// </summary>
    public interface ILogScanError
    {
        /// <summary>
        /// Log an error while scanning/parsing/other.
        /// </summary>
        /// <param name="errorMsg">Error message to log</param>
        /// <param name="errorContext">Error context to log</param>
        /// <param name="errIndex">Error Index in source to log (-1 = current else specific)</param>
        /// <returns>Typically should be implemented to always return false so that the caller may return this as an error condition.</returns>
        public bool LogError(string errorMsg, string errorContext = "Parse error", int errIndex = -1);
    }
}
