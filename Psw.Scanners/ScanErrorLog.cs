// -----------------------------------------------------------------------------
// Copyright (c) Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Psw.Scanners
{
    /// <summary>
    /// Utility class for Logging errors while scanning and then produce formated error output in various forms.
    /// 
    /// > If the error is logged via one of the Scanners then the ScriptSegment and position is logged by the scanner.
    /// 
    /// Error output has the following basic form:
    /// </summary>
    /// <code lang="con">
    /// Sample Error                              // Error Heading 
    ///  FuncName(prm1, 'prm2')  sample body }    // ScriptSegment
    /// -------------------------^(ln: 1 Ch: 26)  // Error position in segment
    /// Filename: C:\somefilename.txt             // Error File name (if defined)
    /// Parse error: { expected                   // Error Context : Error Message
    /// </code>
    public class ScanErrorLog
    {
        /// <group>Properties</group>
        /// <summary>
        /// Get/Set Error status
        /// </summary>
        public bool IsError { get; set; }
        /// <summary>
        /// Get/Set Error massage
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
        /// <summary>
        /// Get/Set Error context
        /// </summary>
        public string ErrorContext { get; set; } = string.Empty;
        /// <summary>
        /// Get/Set Error script segment (if applicable)
        /// </summary>
        public string ScriptSegment { get; set; }
        public string ErrorFile { get; private set; }
        /// <summary>
        /// Get/Set Error line number in a script (1..n)
        /// </summary>
        public int Ln { get; set; }
        /// <summary>
        /// Get/Set Error column number (1..n)
        /// </summary>
        public int Col { get; set; }
        /// <summary>
        /// Get/Set Error filename (if applicable - and subsequent errors will be logged with this filename until it is changed) 
        /// </summary>
        public string FileName { get; set; }

        /// <group>Methods</group>
        /// <summary>
        /// Log an Error and set IsError to true
        /// </summary>
        /// <param name="errMsg">Error message</param>
        /// <param name="errContext">Error context</param>
        /// <param name="scriptSegment">Optional script segment to use when formatting the error output</param>
        /// <param name="ln">Line number in script segment</param>
        /// <param name="col">Column number in script segment</param>
        /// <returns>False always - so it can return false from caller (if required)</returns>
        public bool LogError(string errMsg, string errContext = "Parse error", string? scriptSegment = null, int ln = 1, int col = 1) {
            (IsError, ErrorMessage, ErrorContext, ScriptSegment, Ln, Col, ErrorFile) = (true, errMsg, errContext, scriptSegment, ln, col, FileName);
            return false;
        }

        /// <summary>
        /// Return error string with given heading, formatted as Html.
        /// </summary>
        public string AsHtmlError(string heading) {
            var sb = new StringBuilder();
            sb.AppendLine($"<span class='se-heading'>{heading}</span>");

            if (!string.IsNullOrWhiteSpace(ScriptSegment)) {
                sb.AppendLine($"<span class='se-code'>{ScriptSegment}</span>");
            }

            sb.AppendLine($"<span class='se-pos'>{new string('-', Col - 1)}^ (ln:{Ln} Ch:{Col})</span>");
            if (!string.IsNullOrEmpty(ErrorFile)) sb.AppendLine($"<span class='se-pos'>Filename: {ErrorFile}</span>");
            sb.AppendLine($"<span class='se-msg'>{ErrorContext}: {ErrorMessage}</span>");
            return sb.ToString();
        }

        /// <summary>
        /// Return error string with given heading, formatted as a plain string.
        /// </summary>
        public string AsTextError(string heading) {
            var sb = new StringBuilder();
            sb.AppendLine(heading);

            if (!string.IsNullOrWhiteSpace(ScriptSegment)) {
                sb.AppendLine(ScriptSegment);
            }

            sb.AppendLine($"{new string('-', Col - 1)}^ (ln:{Ln} Ch:{Col})");
            if (!string.IsNullOrEmpty(ErrorFile)) sb.AppendLine($"Filename: {ErrorFile}");
            sb.AppendLine($"{ErrorContext}: {ErrorMessage}");
            return sb.ToString();
        }

        /// <summary>
        /// Return error string with given heading, formatted for console output with embedded color directives. 
        /// </summary>
        public string AsConsoleError(string heading) {
            var s = new StringBuilder();

            int cheading = 35, // magenta
                cscript = 36,  // cyan
                cpos = 33,     // yellow
                cerror = 31;   // red

            string ccode(int color) => $"\u001b[{color};1m";

            void WriteLn(int color, string text) => s.AppendLine($"{ccode(color)}{text}");

            WriteLn(cheading, heading);
            if (IsError) {
                if (!string.IsNullOrWhiteSpace(ScriptSegment)) WriteLn(cscript, ScriptSegment);
                WriteLn(cpos, $"{new string('-', Col - 1)}^ (ln:{Ln} Ch:{Col})");
                if (!string.IsNullOrEmpty(ErrorFile)) WriteLn(cpos, $"Filename: {ErrorFile}");
                WriteLn(cerror, $"{ErrorContext}: {ErrorMessage}");
            }
            s.Append(ccode(0));
            return s.ToString();
        }
    }
}
