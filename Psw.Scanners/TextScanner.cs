﻿// -----------------------------------------------------------------------------
// Copyright (c) Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Psw.Scanners
{
    /// <group>Class</group>
    /// <summary>
    /// A comprehensive low level core Scanner suitable for any text parsing and/or Lexer construction.
    /// </summary>
    /// <mdoc>
    /// The Text Scanner is used to extract *Tokens* from text, check for strings or characters (delimiters), skip over text etc. 
    /// 
    /// > **Incorporates the following basic operations:**
    /// > - Maintains an *Index* (a scan pointer) for all scanning operations.
    /// > - Check for characters or string at the current Index (or Index + offset).
    /// > - Scan up to characters or strings. 
    /// > - Scanned text is recored in *Token* for later access.
    /// > - Several Skipping operations.
    /// > - Some applicable operations record a delimiter in *Delim*
    /// > - Error logging via [*ScanErrorLog*](Docs\scaneerrorlog.md) (which also records the position of the error for later reporting)
    /// 
    /// > **In the documentation below the following abbreviations are used:**
    /// > - *Eos:* End of Source (or string)
    /// > - *Eol:* End of Line
    /// </mdoc>
    public class TextScanner : ILogScanError
    {

        public string Source { get; private set; } = "";   // Source String

        /// <summary>
        /// Get last delimiter logged (when applicable)
        /// </summary>
        public char Delim { get; private set; }            // Last Delimiter logged
        /// <summary>
        /// Get the matching string for the last IsAnyString or SkipToAnyStr method call.
        /// </summary>
        public string Match { get; private set; }          

        protected char _Current { get; private set; }      // Current char at index

        protected int _index;                              // Current scan index position (0..n)
        protected int _length => Source.Length;            // Length of source

        protected static string _nl = Environment.NewLine; // newline: \r\n or just \n
        protected static char _Eos = '\0';                 // End of source character

        /// <summary>
        /// Get/Set the bound ScanErroLog
        /// </summary>
        public ScanErrorLog ErrorLog { get; set; }         // To record scan/other errors

        /// <group>Constructor</group>
        /// <summary>
        /// Create a TextScanner with given source string and optional 'external' ScanErrorLog (else an internal one is created)
        /// </summary>
        public TextScanner(string source, ScanErrorLog errorLog = null) {
            ErrorLog = errorLog ?? new ScanErrorLog();
            SetSource(source);
        }

        // Token ==============================================================

        protected int _tokenStartIndex, // Index where last Token starts (inclusive)
                      _tokenEndIndex;   // Index where last Token ends (exclusive)     

        /// <sgroup>Token operations</sgroup>
        /// <summary>
        /// Query if a Token currently exists
        /// </summary>
        public bool IsToken => _tokenEndIndex > _tokenStartIndex;

        /// <summary>
        /// Returns the current Token else string.Empty for none
        /// </summary>
        public string Token => IsToken ? Source[_tokenStartIndex.._tokenEndIndex] : string.Empty;

        /// <summary>
        /// Check if the current Token is not null or WhiteSpace
        /// </summary>
        public bool ValidToken() {
            if (IsToken) {
                var i = _tokenStartIndex;
                while (i < _tokenEndIndex) {
                    if (!char.IsWhiteSpace(Source[i])) return true;
                    i++;
                }
            }
            return false;
        }

        /// <summary>
        /// Return current Token Trimmed
        /// </summary>
        public string TrimToken => Token.Trim();

        public string TokenStripComments => ScriptScanner.StripComments(Token);

        // Source Management ==================================================

        /// <sgroup>Source Management</sgroup>
        /// <summary>
        /// Set the Scanner Source from a String and reset Index
        /// </summary>
        public void SetSource(string source) {
            Source = source ?? "";
            _index = 0;
            ResetAdvance();
        }

        /// <summary>
        /// Insert text at the current Index, and continue scanning from there
        /// </summary>
        public void Insert(string text) {
            Source = Source.Insert(_index, text);
            ResetAdvance();
        }

        /// <summary>
        /// Insert text and newline (\r\n or \n) at the current Index, and continue scanning from there
        /// </summary>
        public void InsertLine(string text) => Insert(text + _nl);

        /// <summary>
        /// Remove a section of the Source string, from startIndex up to, but excluding, current Index
        /// </summary>
        public void Remove(int startIndex) {
            var removeLen = _index - startIndex;
            if (removeLen > 0) {
                Source = Source.Remove(startIndex, removeLen);
                _index = startIndex;
                ResetAdvance();
            }
        }

        // Index Management ===================================================

        /// <group>Index Management</group>
        /// <summary>
        /// Get: current scan index<br/> 
        /// Set: scan index (0 = start, &lt; 0 or &gt; length = end, else intermediate)
        /// </summary>
        public int Index {
            get => _index;
            set {
                _index = value < 0 || value > _length ? _length : value;
                ResetAdvance();
            }
        }


        // Core Utilities =====================================================

        /// <summary>
        /// Set current scanner state
        /// </summary>
        protected void ResetAdvance() {
            IsEos = _index >= _length;
            _Current = IsEos ? _Eos : Source[_index];
            if (IsEos) _index = _length;
        }

        /// <summary>
        /// Advance Index by count and set state
        /// </summary>
        protected void Advance(int count = 1) {
            if (!IsEos) {
                _index += count;
                ResetAdvance();
            }
        }


        /// <sgroup>Core Utilities</sgroup>
        /// <summary>
        /// Query if Index is at End of Source
        /// </summary>
        public bool IsEos { get; private set; }

        /// <summary>
        /// Advance Index to Eos
        /// </summary>
        public void ToEos() => Index = -1;

        /// <summary>
        /// Query if Index is at End of Line
        /// </summary>
        public bool IsEol => _Current == _nl[0];

        /// <summary>
        /// Query if Index is at Eos or Eol
        /// </summary>
        public bool IsEosOrEol => IsEos || IsEol;

        /// <summary>
        /// Returns character at Index and increments Index, or '0' for Eos.
        /// </summary>
        public char NextCh() {
            char c = _Current;
            Advance();
            return c;
        }

        /// <summary>
        /// Returns character at Index (without advancing), or '0' for Eos.
        /// </summary>
        public char PeekCh() => _Current;

        /// <summary>
        /// Returns character at relative offset to Index (without changing Index), or '0' Eos if out of range
        /// </summary>
        public char PeekNextCh(int offset = 1) {
            var peekIndex = _index + offset;
            return peekIndex >= _length || peekIndex < 0 ? _Eos : Source[peekIndex];
        }

        /// <summary>
        /// Returns true (and advances Index) if the character at Index matches c else false and Index unchanged.
        /// </summary>
        public bool IsCh(char c) {
            if (!IsPeekCh(c)) return false;
            Advance();
            return true;
        }

        /// <summary>
        /// Returns count of consecutive matching characters and advances Index
        /// </summary>
        public int CountCh(char c) {
            int count = 0;
            while (IsCh(c)) count++;
            return count;
        }

        /// <summary>
        /// Query if character at Index is in chars
        /// </summary>
        /// <returns>
        /// True: if found, advances the Index and logs the char in Delim<br/>
        /// False: if not found and Index is unchanged
        /// </returns>
        public bool IsAnyCh(string chars) {
            if (!IsPeekAnyCh(chars)) return false;
            Delim = _Current;
            Advance();
            return true;
        }

        /// <summary>
        /// Returns true if character at Index (without advancing) matches c else false
        /// </summary>
        public bool IsPeekCh(char c) => _Current == c;

        /// <summary>
        /// Returns true if character at Index (without advancing) is in chars else false
        /// </summary>
        public bool IsPeekAnyCh(string chars) => chars.Contains(_Current);

        /// <summary>
        /// Return true if character at Index + offset (without advancing) matches c else false
        /// </summary>
        public bool IsPeekNextCh(char c, int offset) => PeekNextCh(offset) == c;

        /// <summary>
        /// Returns true if character at Index + offset (without advancing) is in chars else false
        /// </summary>
        public bool IsPeekNextAnyCh(string chars, int offset) => chars.Contains(PeekNextCh(offset));

        /// <summary>
        /// Check if text at Index equals matchString and optionally advance Index if it matches.
        /// </summary>
        /// <param name="comp">Comparison type (default = StringComparison.InvariantCultureIgnoreCase)</param>
        public bool IsString(string matchString, bool advanceIndex = true, StringComparison comp = StringComparison.InvariantCultureIgnoreCase) {
            if (IsEos || string.IsNullOrEmpty(matchString)) return false;

            var sSpan = matchString.AsSpan();
            var bufferSpan = Source.AsSpan(Index);

            if (bufferSpan.StartsWith(sSpan, comp)) {
                if (advanceIndex) Advance(matchString.Length);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if text at Index equals any string in matchStrings and optionally advance Index if it matches.<br/>
        /// - Match contains the matching string.
        /// </summary>
        /// <param name="matchStrings">Enumerable set of strings</param>
        /// <param name="advanceIndex">Advance Index to just after match (default) else not</param>
        /// <param name="comp">Comparison type (default = StringComparison.InvariantCultureIgnoreCase)</param>
        public bool IsAnyString(IEnumerable<string> matchStrings, bool advanceIndex = true, StringComparison comp = StringComparison.InvariantCultureIgnoreCase) {
            if (IsEos) return false;

            foreach (var str in matchStrings) {
                if (IsString(str, advanceIndex, comp)) {
                    Match = str;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if text at Index equals any string in delimited matchStrings and optionally advance Index if it matches.<br/>
        /// - Match contains the matching string.
        /// </summary>
        /// <param name="matchStrings">Delimited strings and first character must be the delimiter (e.g. "|s1|s2|...")</param>
        /// <param name="advanceIndex">Advance Index to just after match (default) else not</param>
        /// <param name="comp">Comparison type (default = StringComparison.InvariantCultureIgnoreCase)</param>
        public bool IsAnyString(string matchStrings, bool advanceIndex = true, StringComparison comp = StringComparison.InvariantCultureIgnoreCase) {
            if (IsEos || string.IsNullOrEmpty(matchStrings)) return false;
            return IsAnyString(matchStrings.Split(matchStrings[0], StringSplitOptions.RemoveEmptyEntries), advanceIndex, comp);
        }

        // Skip Operations ====================================================

        /// <group>Skipping Operations</group>
        /// <summary>
        /// Skip while character is skipChar
        /// </summary>
        /// <returns>
        /// True if not Eos after skipping else false
        /// </returns>
        public bool Skip(char skipChar) {
            while (_Current == skipChar) Advance();
            return !IsEos;
        }

        /// <summary>
        /// Skip while character is any of the skipChars
        /// </summary>
        /// <returns>
        /// True if not Eos after skipping else false
        /// </returns>
        public bool SkipAny(string skipChars) {
            if (string.IsNullOrEmpty(skipChars)) return true;
            while (skipChars.Contains(_Current)) Advance();
            return !IsEos;
        }

        /// <summary>
        /// Skip until the termChar is found:<br/>
        /// - Optionally skip over the delimiter if skipOver is true.
        /// </summary>
        /// <returns>
        ///   True: Found and Index at matching char or next if skipOver = true<br/>
        ///   False: Not found or Eos. Index not changed
        /// </returns>
        public bool SkipTo(char termChar, bool skipOver = false) {
            int dpos;
            if (IsEos || (dpos = Source.IndexOf(termChar, _index)) == -1) return false;
            Index = dpos + (skipOver ? 1 : 0);
            return true;
        }

        /// <summary>
        /// Skip until any one of the termChars is found.<br/>
        /// - Delim contains the matching character<br/>
        /// - Optionally skip over the delimiter if skipOver is true.
        /// </summary>
        /// <returns>
        ///   True: Found and Index at matching char or next if skipOver = true<br/>
        ///   False: Not found or Eos. Index not changed
        /// </returns>
        public bool SkipToAny(string termChars, bool skipOver = false) {
            int dpos;
            if (IsEos || string.IsNullOrEmpty(termChars) || (dpos = Source.IndexOfAny(termChars.ToCharArray(), _index)) == -1) return false;
            Delim = Source[dpos];
            Index = dpos + (skipOver ? 1 : 0);
            return true;
        }

        /// <summary>
        /// Skip up to given text and optionally skip over it if skipOver is true
        /// </summary>
        /// <param name="comp">Comparison type (default = StringComparison.InvariantCultureIgnoreCase)</param>
        /// <returns>
        ///   True: Found and Index at start of matching text or just after if skipOver = true<br/>
        ///   False: Not found or Eos. Index unchanged
        /// </returns>
        public bool SkipToStr(string text, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase) {
            int dpos;
            if (IsEos || string.IsNullOrEmpty(text)
                      || (dpos = Source.IndexOf(text, _index, comp)) == -1) return false;
            Index = dpos + (skipOver ? text.Length : 0);
            return true;
        }

        /// <summary>
        /// Skip up to first occurrence of any string in matchStrings and optionally skip over the matching string.<br/>
        /// - Match contains the matching string.
        /// </summary>
        /// <param name="matchStrings">Enumerable set of strings</param>
        /// <param name="skipOver">Advance Index to just after match (default = false) else not</param>
        /// <param name="comp">Comparison type (default = StringComparison.InvariantCultureIgnoreCase)</param>
        /// <returns>
        ///   True: Found and Index at start of matching text or just after if skipOver = true<br/>
        ///   False: Not found or Eos. Index unchanged
        /// </returns>
        public bool SkipToAnyStr(IEnumerable<string> matchStrings, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase) {
            if (IsEos) return false;
            int pos, start = -1, len = 0;

            foreach (var str in matchStrings) {
                if ((pos = Source.IndexOf(str, _index, comp)) != -1) {
                    if (start == -1 || pos < start) {
                        start = pos;
                        len = str.Length;
                    }
                }
            }

            if (start != -1) {
                Match = Source.Substring(start, len);
                Index = start + (skipOver ? len : 0);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Skip up to first occurrence of any string in delimited matchStrings and optionally skip over the matching string.<br/>
        /// - Match contains the matching string
        /// </summary>
        /// <param name="matchStrings">Delimited string and first character must be the delimiter (e.g. "|s1|s2|...")</param>
        /// <param name="skipOver">Advance Index to just after match (default = false) else not</param>
        /// <param name="comp">Comparison type (default = StringComparison.InvariantCultureIgnoreCase)</param>
        /// <returns>
        ///   True: Found and Index at start of matching text or just after if skipOver = true<br/>
        ///   False: Not found or Eos. Index unchanged
        /// </returns>
        public bool SkipToAnyStr(string matchStrings, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase) {
            if (IsEos || string.IsNullOrEmpty(matchStrings)) return false;
            return SkipToAnyStr(matchStrings.Split(matchStrings[0], StringSplitOptions.RemoveEmptyEntries), skipOver, comp);
        }

        /// <summary>
        /// Skip to Eol or Eos (last line)<br/>
        /// - Optionally skip over the Eol if skipOver is true.
        /// </summary>
        /// <returns> False if started at Eos else True</returns>
        public bool SkipToEol(bool skipOver = true) {
            if (IsEos) return false;

            if (SkipTo(_nl[0])) {
                if (skipOver) Advance(_nl.Length);
            }
            else ToEos(); // Go to Eos
            return true;
        }

        /// <summary>
        /// Skip one NewLine. Must currently be at the newline (else ignored)
        /// </summary>
        /// <returns>
        /// True if not Eos after skipping else false
        /// </returns>
        public bool SkipEol() {
            if (_Current == _nl[0]) Advance(_nl.Length);
            return !IsEos;
        }

        /// <summary>
        /// Skip All consecutive NewLines. Must currently be at a newline (else ignored)
        /// </summary>
        /// <returns>
        /// True if not Eos after skipping else false
        /// </returns>
        public bool SkipConsecEol() => SkipAny(_nl);

        /// <summary>
        /// Skip all characters while a predicate matches
        /// </summary>
        public void SkipWhile(Func<char, bool> predicate) {
            int len = _length;

            while (_index < len && predicate(Source[_index])) { _index++; }

            ResetAdvance();
        }

        // Scanning Operations ================================================

        /// <group>Scanning Operations</group>
        /// <summary>
        /// Scans up to the delim or to Eos (if orToEos it true):<br/>
        /// - Optionally skip over the delimiter if skipOver is true.<br/>
        /// - Token contains the intermediate text (excluding delimiter)
        /// </summary>
        /// <returns>
        /// True: Delimiter found or orToEos is true. Index at Eos, delimiter or after delimiter if skipOver<br/>
        /// False: Started at Eos or delimiter not found (and orToEos is false). Index unchanged
        /// </returns>
        public bool ScanTo(char delim, bool orToEos = false, bool skipOver = false) {
            if (IsEos) return false;   // Scan pointer at Eos

            _tokenStartIndex = _index;

            var dpos = Source.IndexOf(delim, _index);

            if (dpos == -1) {
                if (orToEos) ToEos();
                else return false;
            }
            else _index = dpos;

            _tokenEndIndex = _index;
            Advance(skipOver ? 1 : 0);
            return true;
        }

        /// <summary>
        /// Scans up to any character in delims or to Eos (if orToEos it true):<br/>
        /// - Token contains the intermediate text (excluding delimiter)
        /// </summary>
        /// <returns>
        /// True: Delimiter found or orToEos is true. Index at delimiter or Eos<br/>
        /// False: Started at Eos, delimiter not found (and orToEos is false) or delims is blank. Index unchanged
        /// </returns>
        public bool ScanToAny(string delims, bool orToEos = false) {
            if (IsEos || string.IsNullOrEmpty(delims)) return false;   // Scan pointer at end or no delims

            _tokenStartIndex = _index;

            var dpos = Source.IndexOfAny(delims.ToCharArray(), _index);

            if (dpos == -1) {
                if (orToEos) ToEos();
                else return false;
            }
            else _index = dpos;

            _tokenEndIndex = _index;
            Advance(0);
            return true;
        }

        /// <summary>
        /// Scan up to a match of findString:<br/> 
        /// - Token contains the intermediate text (excluding findString)
        /// </summary>
        /// <param name="comp">Comparison type (default = StringComparison.InvariantCultureIgnoreCase)</param>
        /// <returns>
        ///   True:  findString found and Index directly after findString
        ///   False: findString not found and Index remains at original position<br/>
        /// </returns>
        public bool ScanToStr(string findString, StringComparison comp = StringComparison.InvariantCultureIgnoreCase) {
            if (IsEos || 0 == findString.Length) return false;
            var pos = Source.IndexOf(findString, _index, comp);

            if (-1 == pos) return false;  // not found

            _tokenStartIndex = _index;
            _tokenEndIndex = pos;
            Index = pos + findString.Length;
            return true;
        }

        /// <summary>
        /// Scan up to first occurrence of any string in matchStrings.<br/>
        /// - Token contains the intermediate text (excluding matching string).<br/>
        /// - Match contains the matching string.
        /// </summary>
        /// <param name="matchStrings">Enumerable set of strings</param>
        /// <param name="skipOver">Advance Index to just after match (default = false) else not</param>
        /// <param name="comp">Comparison type (default = StringComparison.InvariantCultureIgnoreCase)</param>
        /// <returns>
        ///   True: Found and Index at start of matching text or just after if skipOver = true<br/>
        ///   False: Not found or Eos. Index unchanged
        /// </returns>
        public bool ScanToAnyStr(IEnumerable<string> matchStrings, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase) {
            _tokenStartIndex = Index;

            if (!SkipToAnyStr(matchStrings, skipOver, comp)) return false;

            _tokenEndIndex = Index - (skipOver ? Match.Length : 0);
            return true;
        }

        /// <summary>
        /// Scan up to first occurrence of any string in delimited matchStrings.<br/>
        /// - Token contains the intermediate text (excluding matching string).<br/>
        /// - Match contains the matching string.
        /// </summary>
        /// <param name="matchStrings">Delimited string and first character must be the delimiter (e.g. "|s1|s2|...")</param>
        /// <param name="skipOver">Advance Index to just after match (default = false) else not</param>
        /// <param name="comp">Comparison type (default = StringComparison.InvariantCultureIgnoreCase)</param>
        /// <returns>
        ///   True: Found and Index at start of matching text or just after if skipOver = true<br/>
        ///   False: Not found or Eos. Index unchanged
        /// </returns>
        public bool ScanToAnyStr(string matchStrings, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase) {
            if (IsEos || string.IsNullOrEmpty(matchStrings)) return false;
            return ScanToAnyStr(matchStrings.Split(matchStrings[0], StringSplitOptions.RemoveEmptyEntries), skipOver, comp);
        }

        /// <summary>
        /// Scan to Eol and optionally skip over Eol:<br/>
        /// - Handles intermediate or last line (with no Eol)<br/>
        /// - Token contains the intermediate text (excluding the newline)
        /// </summary>
        /// <returns>False if started at Eos or Eol else true</returns>
        public bool ScanToEol(bool skipEol = true) {
            if (!IsEol && ScanTo(_nl[0], true)) {
                if (skipEol) SkipEol();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Scan a value (token) to Eol and optionally skip over Eol:<br/>
        /// - Handles intermediate or last line (with no Eol)<br/>
        /// - Token contains the intermediate text (excluding the newline)
        /// </summary>
        /// <returns>False if started at Eos or a non-valid Token else true</returns>
        public bool ValueToEol(bool skipEol = true) {
            if (ScanTo(_nl[0], true)) {
                if (skipEol) SkipEol();
                return ValidToken();
            }
            return false;
        }

        /// <summary>
        /// Return the remainder of the current line without changing the Index position.
        /// (typically used for debugging)
        /// </summary>
        public string LineRemainder() {
            var curPos = Index;
            string line = string.Empty;
            if (ScanTo(_nl[0], true)) line = $"[{Token}]";
            Index = curPos;
            return line;
        }

        /// <summary>
        /// Scan all characters while a predicate matches:<br/>
        /// - Predicate = Func: &lt;this, current char, 0..n index from scan start, bool&gt;<br/>
        /// - Token contains the scanned characters
        /// </summary>
        /// <returns>True if any characters are scanned (scan pointer after last match) else false (scan pointer unchanged)</returns>
        public bool ScanWhile(Func<TextScanner, char, int, bool> predicate) {
            _tokenStartIndex = _index;
            int len = _length, i = 0;
            //while (_index < len && predicate(this, Source[_index], i)) { _index++; i++; }
            while (_index < len && predicate(this, Source[_index], i)) { Advance(); i++; }

            if (_index > _tokenStartIndex) {
                _tokenEndIndex = _index;
                Advance(0);
                return true;
            }
            return false;
        }


        // Type Operations ====================================================

        /// <group>Type Operations</group>
        /// <summary>
        /// Check if current character matches a predicate (without advancing Index)
        /// </summary>
        public bool IsChType(Func<char, bool> predicate) => !IsEos && predicate(_Current);

        /// <summary>
        /// Check if current character is a Digit (via char.IsDigit()) (without advancing Index)
        /// </summary>
        public bool IsDigit() => IsChType(c => char.IsDigit(c));

        /// <summary>
        /// Check if current character is a Letter (via char.IsLetter()) (without advancing Index)
        /// </summary>
        public bool IsLetter() => IsChType(c => char.IsLetter(c));

        /// <summary>
        /// Check if current character is a LetterOrDigit (via char.IsLetterOrDigit()) (without advancing Index)
        /// </summary>
        public bool IsLetterOrDigit() => IsChType(c => char.IsLetterOrDigit(c));

        /// <summary>
        /// Check if current character is a Decimal digit (IsDigit || '.') (without advancing Index)
        /// </summary>
        public bool IsDecimal() => IsDigit() || _Current == '.';

        /// <summary>
        /// Scan a decimal value of the form n*.n* 
        /// </summary>
        /// <returns>True and output double else false</returns>
        public bool NumDecimal(out double value) {
            var dot = false;
            _tokenStartIndex = _index;

            while (IsDigit() || !dot && (dot = _Current == '.')) Advance();

            _tokenEndIndex = _index;
            if (IsToken) {
                if (!double.TryParse(Token, out value)) return false;
                return true;
            }
            value = 0;
            return false;
        }

        /// <summary>
        /// Scan an integer value of the form n* 
        /// </summary>
        /// <returns>True and output int else false</returns>
        public bool NumInt(out int value) {
            if (ScanWhile((scn, ch, i) => char.IsDigit(ch))) {
                if (!int.TryParse(Token, out value)) return false;
                return true;
            }
            value = 0;
            return false;
        }

        /// <summary>
        /// Get current character, into Delim, if it is a digit and advance Index. Else return false and Index unchanged.
        /// </summary>
        public bool GetDigit() {
            if (IsDigit()) {
                Delim = NextCh();
                return true;
            }
            return false;
        }

        // Error Logging ======================================================

        /// <group>Error Logging</group>
        /// <summary>
        ///   Return Line and column number for given or current position
        /// </summary>
        /// <param name="pos">
        ///   Position (index) to get line and column for.
        ///   If this value is -1 use the current scan pointer position
        /// </param>
        /// <returns>
        ///   anonymous type: line   = line (1..n),
        ///                   col    = column (1..n),
        ///                   offset, 
        ///                   astext = "Ln l+1  Col c+1"
        /// </returns>
        public (int line, int col, int offset, string astext) GetLineAndColumn(int pos = -1) {
            int ipos = pos < 0 ? _index : pos;
            int spos = 0, lpos = 0;
            int line = 0, col = 0;

            while (spos <= ipos) {
                spos = Source.IndexOf('\n', spos);
                if (-1 == spos) { // Last/Only line so just set column
                    col = ipos - lpos;
                    break;
                }

                if (spos >= ipos) {  // Past our target
                    col = ipos - lpos;
                    break;
                }
                line++;
                spos++;   // Skip \n
                lpos = spos; // Where next line starts
            }

            return (line + 1, col + 1, ipos, $"Ln {line + 1} Col {col + 1}");
        }

        /// <summary>
        /// Get all text up to and including the line containing pos (excluding NL)<br/>
        /// - Optionally only get the lastNoofLines if > 0
        /// </summary>
        /// <param name="pos">Position or -1 for current position</param>
        public string GetUptoLine(int pos = -1, int lastNoofLines = 0) {
            int indexPos = pos < 0 ? _index : pos;
            int endPos = Source.IndexOf(_nl[0], indexPos); // Next newline after indexPos

            if (endPos == -1) { // EOF
                endPos = Source.Length;
            }

            if (lastNoofLines <= 0) return Source[..endPos];

            int startPos = endPos;

            while (startPos >= 0 && lastNoofLines > 0) {
                startPos = Source.LastIndexOf(_nl[0], startPos - 1);
                lastNoofLines--;
            }

            return Source[(startPos < 0 ? 0 : startPos + _nl.Length)..endPos];
        }

        /// <summary>
        /// Log an Error (see ScanErrorLog) with given erroMsg and errorContext:<br/>
        /// - At current Index position (default errIndex = -1) or at given errIndex ( >= 0 )<br/>
        /// - Records the last 10 lines and Line and Column no in ErrorLog - for later display
        /// </summary>
        /// <returns>False always - so can use to return false from caller</returns>
        public bool LogError(string errorMsg, string errorContext = "Parse error", int errIndex = -1) {
            var errPos = GetLineAndColumn(errIndex);
            return ErrorLog.LogError(errorMsg, errorContext, GetUptoLine(errIndex, 10), errPos.line, errPos.col);
        }

        /// <summary>
        /// Return Error status
        /// </summary>
        public bool IsError() => ErrorLog.IsError;

    }
}