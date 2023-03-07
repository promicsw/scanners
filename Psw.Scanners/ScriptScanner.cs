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
    /// Extends TextScanner with methods useful for script scanning and parsing.
    /// </summary>
    /// <mdoc>
    /// > **Incorporates the following:**
    /// > - Handles *comments*: Line `//...` and block `/*...*/` (handles nesting)
    /// > - Skip whitespace and comments
    /// > - Scan delimited strings and blocks
    /// > - Standard identifier scanning.
    /// > - Scan list structures.
    /// </mdoc>
    public class ScriptScanner : TextScanner
    {
        /// <group>Constructor</group>
        /// <summary>
        /// Create a ScripScanner with given source string and optional external ScanErrorLog (else an internal one is created)
        /// </summary>
        public ScriptScanner(string source = "", ScanErrorLog errorLog = null) : base(source, errorLog) { }

        // String Delimiter ===================================================

        /// <group>Delimited Strings</group>
        /// <summary>
        /// Get or Set the String Delimiters to use (default "'`)
        /// </summary>
        public string StringDelim { get; set; } = "\"'`";

        /// <summary>
        /// Query if character at Index is one of the StringDelim values
        /// </summary>
        public bool IsStringDelim() => StringDelim.Contains(_Current);

        // Script Scanning ====================================================

        /// <group> Script utilities</group>
        /// <summary>
        /// Scan a delimited String Literal:<br/>
        /// - Current Index must be at the starting delimiter ("`' etc).<br/>
        /// - NOTE: A string literal may NOT span a line.<br/>
        /// - Token contains the string (excluding delimiters).
        /// </summary>
        /// <returns>
        /// True: if there was a string literal and Index positioned after ending delimiter<br/>
        /// False: for no string or Eos - Index unchanged
        /// </returns>
        public bool StrLit() {
            char delim = NextCh();

            if (!ScanToAny(delim + _nl)) return LogError($"Unterminated string literal starting with {delim}", "Scan StrLit");
            if (IsEol) return LogError($"String literal starting with {delim} may not span a line", "Scan StrLit");
            Advance(); // Skip over delim
            return true;
        }

        /// <summary>
        /// Scan either a StrLit or result of ScanToAny(termChars, orToEos) and ValidToken()<br/>
        /// - If Index is at a StringDelim - returns the result of StrLit()<br/>
        /// - Else returns the result of ScanToAny(termChars, orToEos) and ValidToken() (i.e non-blank value)<br/>
        /// - Use Token to retrieve the value
        /// </summary>
        /// <returns>Success of the scan</returns>
        public bool ValueOrStrLit(string termChars, bool orToEos = false)
            => IsStringDelim() ? StrLit() : ScanToAny(termChars, orToEos) && ValidToken();

        /// <summary>
        /// Scan Standard Identifier of the form: (letter | _)* (letterordigit | _)*
        /// </summary>
        /// <returns>True for valid identifier (available via Token) else false</returns>
        public bool StdIdent() => ScanWhile((scn, ch, i) => char.IsLetter(ch) || '_' == ch || i > 0 && char.IsLetterOrDigit(ch));

        /// <summary>
        /// Scan Standard Identifier of the form: (letter | _)* (letterordigit | _ | -)*
        /// </summary>
        /// <returns>True for valid identifier (available via Token) else false</returns>
        public bool StdIdent2() => ScanWhile((scn, ch, i) => char.IsLetter(ch) || '_' == ch || i > 0 && ('-' == ch || char.IsLetterOrDigit(ch)));

        private static bool ValidBlockDelims(string delims) => delims != null && delims.Length > 1;

        /// <summary>
        /// Scan a block delimited by blockDelims E.g "{}" or "()" or "[]" etc.<br /> 
        /// - Handles Nesting and ignores any block delimiters inside comments or string literals (delimited by StringDelim)<br/>
        /// - Token contains the block content excluding the block delimiters.
        /// </summary>
        /// <param name="blockDelims">String with opening and closing block delimiters (default = "{})</param>
        /// <param name="isOpen">False - current Index at start of block else Index just inside block</param>
        /// <returns>True for a valid block (Index positioned after block) else false and Logs an error (Index unchanged)</returns>
        public bool ScanBlock(string blockDelims = "{}", bool isOpen = false) {
            if (!ValidBlockDelims(blockDelims)) return LogError($"Invalid Block delimiters \"{blockDelims}\" defined in call to ScanBlock", "Scan Block");

            char blockStart = blockDelims[0], blockEnd = blockDelims[1];
            string delims = StringDelim + blockStart + blockEnd + '/';
            int level = 0;
            int startIndex = Index;

            if (isOpen) level++;

            while (SkipWSC() && SkipToAny(delims)) {
                if (IsCh(blockStart)) {
                    if (level == 0) startIndex = Index;
                    level++; continue;
                }

                if (IsCh(blockEnd)) {
                    level--;
                    if (level <= 0) break;
                    else continue;
                }

                if (IsPeekCh('/')) {
                    if (IsComment()) continue; // SkipWSC will skip it
                    else NextCh(); // skip over
                }

                if (IsStringDelim()) { // Skip over string that doesn't span a line else ignore
                    var delim = NextCh();
                    int pos = Index;
                    SkipToAny(delim+_nl);
                    if (!IsCh(delim)) Index = pos;
                }  
            }

            if (level == 0 && _index > startIndex) {
                _tokenStartIndex = startIndex;
                _tokenEndIndex = Index - 1;
                return true;
            }

            Index = startIndex;  // Failed: Restore Position
            return LogError($"Invalid Block {blockStart}...{blockEnd} - matching closing '{blockEnd}' not found", "Scan Block");
        }

        /// <summary>
        /// Static method: Return the source string with all line and block comments removed.
        /// </summary>
        public static string StripComments(string source) {
            var res = new StringBuilder();
            var scn = new ScriptScanner(source);

            while (!scn.IsEos) {
                if (scn.ScanTo('/', true)) {
                    res.Append(scn.Token);
                    if (!scn.IsEos) {
                        if (scn.IsComment()) scn.SkipComment(true);
                        else res.Append(scn.NextCh());
                    }
                }
            }

            return res.ToString();
        }

        /// <summary>
        /// Scan a List of the form: ( item1, item 2, ...)<br/>
        /// - Note: The next non-whitespace character must be the Opening list delimiter.<br/>
        /// - Item type 1: All text up to next closing delim or separator (logged trimmed)
        /// - Item type 2: A string literal - may NOT span a line! (logged verbatim excluding string delimiters)
        /// - Item type 3: Block delimited text (logged verbatim excluding block delimiters) - use for multi-line text. 
        /// - Blank items are not recorded.
        /// </summary>
        /// <param name="delims">Opening and closing list delimiter (default = "()") </param>
        /// <param name="separator">List item separator (default = ,)</param>
        /// <param name="block">Opening and closing Block delimiters (default = "[]")</param>
        /// <returns>List of strings else null and error logged in ErrorLog</returns>
        public List<string>? ScanList(string delims = "()", char separator = ',', string block = "[]") {

            if (!ValidBlockDelims(delims)) {
                LogError($"Invalid delimiters \"{delims}\" defined in call to ScanList", "Scan List");
                return null;
            }

            var list = new List<string>();
            char cOpen = delims[0], cClose = delims[1];
            bool checkBlock = ValidBlockDelims(block);

            SkipWSC();
            if (!IsCh(cOpen)) { LogError($"{cOpen} expected", "ScanList"); return null; }

            while (!IsEos) {
                SkipWS();
                if (IsCh(cClose)) break;       // Done
                if (IsCh(separator)) continue; // Absorb separator

                if (checkBlock && IsPeekCh(block[0])) {
                    if (ScanBlock(block)) list.Add(Token); 
                    else return null;
                }
                else if (IsStringDelim()) {
                    if (StrLit()) list.Add(Token);
                    else return null;
                }
                else if (ScanToAny("" + separator + cClose)) {
                    list.Add(TrimToken);
                }
                else {
                    LogError($"{cClose} or {separator} expected", "ScanList");
                    return null;
                }
            }
            
            return list;

        }

        /*
        public List<string>? ScanList(string delims = "()", char separator = ',', string block = "[]") {

            if (!ValidBlockDelims(delims)) {
                LogError($"Invalid delimiters \"{delims}\" defined in call to ScanList", "Scan List");
                return null;
            }

            var list = new List<string>();
            char cOpen = delims[0], cClose = delims[1];
            bool checkBlock = ValidBlockDelims(block);

            SkipWSC();
            if (!IsCh(cOpen)) { LogError($"{cOpen} expected", "ScanList"); return null; }

            while (!IsEos) {
                SkipWS();
                if (IsCh(cClose)) break;       // Done
                if (IsCh(separator)) continue; // Absorb separator

                if (checkBlock && IsPeekCh(block[0])) {
                    if (ScanBlock(block)) list.Add(TrimToken); 
                    //LogError($"Error scanning block {block}", "ScanList"); Use ScanBlock error
                    else return null;
                }
                else if (IsStringDelim()) {
                    if (StrLit()) list.Add(Token);
                    //LogError("Unterminated string literal", "Scan List"); Use StrLit error message
                    else return null;
                }
                else if (ScanToAny(_nl + separator + cClose)) {
                    list.Add(TrimToken);
                }
                else {
                    LogError($"{cClose} or {separator} expected", "ScanList");
                    return null;
                }
            }
            
            return list;

        }
        */

        /// <summary>
        /// Scan a List of the form: ( item1, item 2 ... )<br/>
        /// - Note: The next non-whitespace character must be the Opening list delimiter.<br/>
        /// - Item type 1: All text up to next closing delim or separator (logged trimmed)
        /// - Item type 2: A string literal - may NOT span a line! (logged verbatim  excluding string delimiters)
        /// - Item type 3: Block delimited text (logged verbatim excluding block delimiters) - use for multi-line text. 
        /// - Blank items are not recorded.
        /// </summary>
        /// <param name="delims">Opening and closing list delimiter (default = "()") </param>
        /// <param name="separator">List item separator (default = ,)</param>
        /// <param name="block">Opening an closing Block delimiters (default = "[]")</param>
        /// <returns>True and List of strings in out list (Index after list), else false and error logged in ErrorLog</returns>
        public bool ScanList(out List<string> list, string delims = "()", char separator = ',', string block = "[]") {
            list = ScanList(delims, separator, block);
            return list != null;
        }

        // Whitespace and Comment Skipping ====================================

        /// <group>Whitespace and Comment skipping</group>
        /// <summary>
        /// Skip Spaces (" \t") 
        /// </summary>
        /// <returns>True if not at Eos after skipping else False</returns>
        public bool SkipSp() => SkipAny(" \t");

        /// <summary>
        /// Skip given White Space characters (default: " \r\n\t") 
        /// </summary>
        /// <returns>True if not at Eos after skipping else False</returns>
        public bool SkipWS(string wsChars = " \r\n\t") => SkipAny(wsChars);

        /// <summary>
        /// Skip White Space characters and comments //... or /*..*/ (handles nested comments)<br/>
        /// - White space: spaceChars + "\r\n" if termNL is false<br/>
        /// - Set termNL to position Index at the next newline not inside a block comment (/*..*/), else the newlines are also skipped.
        /// </summary>
        /// <param name="spaceChars">Space characters to build the white space characters from (default: " \t")</param>
        /// <returns>
        ///   True: Whitespace and comments skipped and Index directly after<br/>
        ///   False: Eos or comment error (missing */ logged as error) - Index unchanged
        /// </returns>
        public bool SkipWSC(bool termNL = false, string spaceChars = " \t") {
            var delim = spaceChars + (termNL ? "" : "\r\n");
            if (!SkipAny(delim)) return false;  // Eos

            while (IsComment()) {
                if (!SkipComment(termNL, true)) return false;
                if (termNL && IsEol) return true;
                if (!SkipAny(delim)) return false;
            }
            return !IsEos;
        }

        /// <summary>
        /// Query if Index is currently at a comment
        /// </summary>
        public bool IsComment() => _Current == '/' && "/*".Contains(PeekNextCh(1));

        /// <summary>
        /// Skip continuous sequence of comments:<br/> 
        /// - Line comment ( //..Eol/Eos ), Block comment ( /*..*/ ) and handles nested block comments:<br/>
        /// - NOTE: Index must currently be positioned at the start of a comment /<br/>
        /// - Set termNL to true to position Index at the newline after a line comment ( // ), else the newline is skipped
        /// </summary>
        /// <returns>
        /// True: Comment skipped and Index positioned after comment<br/>
        /// False: Eos or comment error (missing */ logged as error) - Index unchanged
        /// </returns>
        public bool SkipComment(bool termNL = false, bool commentConfirmed = false) {
            bool isComment = commentConfirmed || IsComment();

            while (isComment) {
                if ('/' == PeekNextCh(1)) { // Line comment
                    SkipToEol(!termNL);
                    if (termNL) return !IsEos;
                }
                
                else {  // Block comment /*
                    Advance(2);              // Skip over /*
                    int restorePos = _index; // Restore position on failure
                    int nestLevel = 1;       // To handle nesting

                    while (nestLevel > 0) {
                        if (!SkipToAnyStr("|*/|/*", true)) { // No closing */ found
                            LogError("No matching closing block comment */ found - may also be due to bad nesting", "SkipComment");
                            Index = restorePos;
                            return false;
                        }

                        if (Match == "*/") nestLevel--;
                        else nestLevel++;  // Nesting
                    }
                }

                isComment = IsComment();
            }

            return !IsEos;
        }

    }
}
