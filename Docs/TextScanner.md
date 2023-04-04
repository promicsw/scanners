# class TextScanner : ILogScanError

A comprehensive low level core Scanner suitable for any text parsing and/or Lexer construction.

The Text Scanner is used to extract *Tokens* from text, check for strings or characters (delimiters), skip over text etc. 

> **Incorporates the following basic operations:**
> - Maintains an *Index* (a scan pointer) for all scanning operations.
> - Check for characters or string at the current Index (or Index + offset).
> - Scan up to characters or strings. 
> - Scanned text is recored in *Token* for later access.
> - Several Skipping operations.
> - Some applicable operations record a delimiter in *Delim*
> - Error logging via [*ScanErrorLog*](Docs\scaneerrorlog.md) (which also records the position of the error for later reporting)

> **In the documentation below the following abbreviations are used:**
> - *Eos:* End of Source (or string)
> - *Eol:* End of Line

|Member|Description|
|----|------|
|`P: char Delim`|Get last delimiter logged (when applicable)<br/>|
|`P: string Match`|Get the matching string for the last IsAnyString or SkipToAnyStr method call.<br/>|
|`P: ScanErrorLog ErrorLog`|Get/Set the bound ScanErroLog<br/>|
|**Constructor:**||
|`C: TextScanner(string source, ScanErrorLog errorLog = null)`|Create a TextScanner with given source string and optional 'external' ScanErrorLog (else an internal one is created)<br/>|
|**Token operations:**||
|`P: bool IsToken`|Check if a Token currently exists<br/>|
|`P: string StripToken`|Get current token stripped of comments<br/>|
|`P: string Token`|Get the current Token else string.Empty for none<br/>|
|`P: string TrimStripToken`|Get current token trimmed and stripped of comments<br/>|
|`P: string TrimToken`|Get current token Trimmed<br/>|
|`M: bool ValidToken()`|Check if the current Token is not null or WhiteSpace<br/>|
|**Source Management:**||
|`M: void Insert(string text)`|Insert text at the current Index, and continue scanning from there<br/>|
|`M: void InsertLine(string text)`|Insert text and newline (\r\n or \n) at the current Index, and continue scanning from there<br/>|
|`M: void Remove(int startIndex)`|Remove a section of the Source string, from startIndex up to, but excluding, current Index<br/>|
|`M: void SetSource(string source)`|Set the Scanner Source from a String and reset Index<br/>|
|**Index Management:**||
|`P: int Index`|Get: current scan index <br/>Set: scan index (0 = start, &lt; 0 or &gt; length = end, else intermediate)<br/>|
|**Core Utilities:**||
|`M: int CountCh(char c)`|Get count of consecutive matching characters and advances Index<br/>|
|`M: bool IsAnyCh(string chars)`|Check if character at Index is one of the chars<br/><br/>**Returns:**<br/>True: if found, advances the Index and logs the char in Delim<br/>False: if not found and Index is unchanged<br/>|
|`M: bool IsAnyString(IEnumerable<string> matchStrings, bool advanceIndex = true, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`|Check if text at Index equals any string in matchStrings and optionally advance the Index if it matches.<br/>- Match contains the matching string.<br/><br/>**Parameters:**<br/><code>matchStrings:</code> Enumerable set of strings<br/><code>advanceIndex:</code> Advance Index to just after match (default) else not<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/>|
|`M: bool IsAnyString(string matchStrings, bool advanceIndex = true, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`|Check if text at Index equals any string in delimited matchStrings and optionally advance the Index if it matches.<br/>- Match contains the matching string.<br/><br/>**Parameters:**<br/><code>matchStrings:</code> Delimited strings and first character must be the delimiter (e.g. "\|s1\|s2\|...")<br/><code>advanceIndex:</code> Advance Index to just after match (default) else not<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/>|
|`M: bool IsCh(char c)`|Check if the character at Index matches c and advance Index if true.<br/>|
|`P: bool IsEol`|Check if Index is at End of Line<br/>|
|`P: bool IsEos`|Check if Index is at End of Source<br/>|
|`P: bool IsEosOrEol`|Query if Index is at Eos or Eol<br/>|
|`M: bool IsPeekAnyCh(string chars)`|Check if character at Index is one of the chars (index unchanged).<br/>|
|`M: bool IsPeekCh(char c)`|Check if character at Index matches c (index unchanged).<br/>|
|`M: bool IsPeekNextAnyCh(string chars, int offset)`|Check character at Index + offset is one of the chars (index unchanged).<br/>|
|`M: bool IsPeekNextCh(char c, int offset)`|Check if character at Index + offset matches c (index unchanged).<br/>|
|`M: bool IsString(string matchString, bool advanceIndex = true, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`|Check if text at Index equals matchString and optionally advance the Index if it matches.<br/><br/>**Parameters:**<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/>|
|`M: char NextCh()`|Get character at Index and increments Index, or '0' for Eos.<br/>|
|`M: char PeekCh()`|Get character at Index (index unchanged), or '0' for Eos.<br/>|
|`M: char PeekNextCh(int offset = 1)`|Get character at relative offset to Index (index unchanged), or '0' Eos if out of range<br/>|
|`M: void ToEos()`|Advance Index to Eos<br/>|
|**Skipping Operations:**||
|`M: bool Skip(char skipChar)`|Skip while character is skipChar<br/><br/>**Returns:**<br/>True if not Eos after skipping else false<br/>|
|`M: bool SkipAny(string skipChars)`|Skip while character is any of the skipChars<br/><br/>**Returns:**<br/>True if not Eos after skipping else false<br/>|
|`M: bool SkipTo(char termChar, bool skipOver = false)`|Skip until the termChar is found:<br/>- Optionally skip over the delimiter if skipOver is true.<br/><br/>**Returns:**<br/>True: Found and Index at matching char or next if skipOver = true<br/>  False: Not found or Eos. Index not changed<br/>|
|`M: bool SkipToAny(string termChars, bool skipOver = false)`|Skip until any one of the termChars is found.<br/>- Delim contains the matching character<br/>- Optionally skip over the delimiter if skipOver is true.<br/><br/>**Returns:**<br/>True: Found and Index at matching char or next if skipOver = true<br/>  False: Not found or Eos. Index not changed<br/>|
|`M: bool SkipToStr(string text, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`|Skip up to given text and optionally skip over it if skipOver is true<br/><br/>**Parameters:**<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/><br/>**Returns:**<br/>True: Found and Index at start of matching text or just after if skipOver = true<br/>  False: Not found or Eos. Index unchanged<br/>|
|`M: bool SkipToAnyStr(IEnumerable<string> matchStrings, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`|Skip up to first occurrence of any string in matchStrings and optionally skip over the matching string.<br/>- Match contains the matching string.<br/><br/>**Parameters:**<br/><code>matchStrings:</code> Enumerable set of strings<br/><code>skipOver:</code> Advance Index to just after match (default = false) else not<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/><br/>**Returns:**<br/>True: Found and Index at start of matching text or just after if skipOver = true<br/>  False: Not found or Eos. Index unchanged<br/>|
|`M: bool SkipToAnyStr(string matchStrings, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`|Skip up to first occurrence of any string in delimited matchStrings and optionally skip over the matching string.<br/>- Match contains the matching string<br/><br/>**Parameters:**<br/><code>matchStrings:</code> Delimited string and first character must be the delimiter (e.g. "\|s1\|s2\|...")<br/><code>skipOver:</code> Advance Index to just after match (default = false) else not<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/><br/>**Returns:**<br/>True: Found and Index at start of matching text or just after if skipOver = true<br/>  False: Not found or Eos. Index unchanged<br/>|
|`M: bool SkipToEol(bool skipOver = true)`|Skip to Eol or Eos (last line)<br/>- Optionally skip over the Eol if skipOver is true.<br/><br/>**Returns:**<br/>False if started at Eos else True<br/>|
|`M: bool SkipEol()`|Skip one NewLine. Must currently be at the newline (else ignored)<br/><br/>**Returns:**<br/>True if not Eos after skipping else false<br/>|
|`M: bool SkipConsecEol()`|Skip All consecutive NewLines. Must currently be at a newline (else ignored)<br/><br/>**Returns:**<br/>True if not Eos after skipping else false<br/>|
|`M: void SkipWhile(Func<char, bool> predicate)`|Skip all characters while a predicate matches<br/>|
|**Scanning Operations:**||
|`M: bool ScanTo(char delim, bool orToEos = false, bool skipOver = false)`|Scans up to the delim or to Eos (if orToEos it true):<br/>- Optionally skip over the delimiter if skipOver is true.<br/>- Token contains the intermediate text (excluding delimiter)<br/><br/>**Returns:**<br/>True: Delimiter found or orToEos is true. Index at Eos, delimiter or after delimiter if skipOver<br/>False: Started at Eos or delimiter not found (and orToEos is false). Index unchanged<br/>|
|`M: bool ScanToAny(string delims, bool orToEos = false)`|Scans up to any character in delims or to Eos (if orToEos it true):<br/>- Token contains the intermediate text (excluding delimiter)<br/><br/>**Returns:**<br/>True: Delimiter found or orToEos is true. Index at delimiter or Eos<br/>False: Started at Eos, delimiter not found (and orToEos is false) or delims is blank. Index unchanged<br/>|
|`M: bool ScanToStr(string findString, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`|Scan up to a match of findString: <br/>- Token contains the intermediate text (excluding findString)<br/><br/>**Parameters:**<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/><br/>**Returns:**<br/>True:  findString found and Index directly after findString<br/>  False: findString not found and Index remains at original position<br/>|
|`M: bool ScanToAnyStr(IEnumerable<string> matchStrings, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`|Scan up to first occurrence of any string in matchStrings.<br/>- Token contains the intermediate text (excluding matching string).<br/>- Match contains the matching string.<br/><br/>**Parameters:**<br/><code>matchStrings:</code> Enumerable set of strings<br/><code>skipOver:</code> Advance Index to just after match (default = false) else not<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/><br/>**Returns:**<br/>True: Found and Index at start of matching text or just after if skipOver = true<br/>  False: Not found or Eos. Index unchanged<br/>|
|`M: bool ScanToAnyStr(string matchStrings, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`|Scan up to first occurrence of any string in delimited matchStrings.<br/>- Token contains the intermediate text (excluding matching string).<br/>- Match contains the matching string.<br/><br/>**Parameters:**<br/><code>matchStrings:</code> Delimited string and first character must be the delimiter (e.g. "\|s1\|s2\|...")<br/><code>skipOver:</code> Advance Index to just after match (default = false) else not<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/><br/>**Returns:**<br/>True: Found and Index at start of matching text or just after if skipOver = true<br/>  False: Not found or Eos. Index unchanged<br/>|
|`M: bool ScanToEol(bool skipEol = true)`|Scan to Eol and optionally skip over Eol:<br/>- Handles intermediate or last line (with no Eol)<br/>- Token contains the intermediate text (excluding the newline)<br/><br/>**Returns:**<br/>False if started at Eos or Eol else true<br/>|
|`M: bool ValueToEol(bool skipEol = true)`|Scan a value (token) to Eol and optionally skip over Eol:<br/>- Handles intermediate or last line (with no Eol)<br/>- Token contains the intermediate text (excluding the newline)<br/><br/>**Returns:**<br/>False if started at Eos or a non-valid Token else true<br/>|
|`M: string LineRemainder()`|Return the remainder of the current line without changing the Index position.<br/>(typically used for debugging)<br/>|
|`M: bool ScanWhile(Func<TextScanner, char, int, bool> predicate)`|Scan all characters while a predicate matches:<br/>- Predicate = Func: &lt;this, current char, 0..n index from scan start, bool&gt;<br/>- Token contains the scanned characters<br/><br/>**Returns:**<br/>True if any characters are scanned (scan pointer after last match) else false (scan pointer unchanged)<br/>|
|**Type Operations:**||
|`M: bool IsChType(Func<char, bool> predicate)`|Check if current character matches a predicate (without advancing Index)<br/>|
|`M: bool IsDigit()`|Check if current character is a Digit (via char.IsDigit()) (without advancing Index)<br/>|
|`M: bool IsLetter()`|Check if current character is a Letter (via char.IsLetter()) (without advancing Index)<br/>|
|`M: bool IsLetterOrDigit()`|Check if current character is a LetterOrDigit (via char.IsLetterOrDigit()) (without advancing Index)<br/>|
|`M: bool IsDecimal()`|Check if current character is a Decimal digit (IsDigit \|\| '.') (without advancing Index)<br/>|
|`M: bool NumDecimal(out double value)`|Scan a decimal value of the form n\*.n\*<br/><br/>**Returns:**<br/>True and output double else false<br/>|
|`M: bool NumInt(out int value)`|Scan an integer value of the form n\*<br/><br/>**Returns:**<br/>True and output int else false<br/>|
|`M: bool GetDigit()`|Get current character, into Delim, if it is a digit and advance Index. Else return false and Index unchanged.<br/>|
|**Error Logging:**||
|`M: (int line, int col, int offset, string astext) GetLineAndColumn(int pos = -1)`|Return Line and column number for given or current position<br/><br/>**Parameters:**<br/><code>pos:</code> Position (index) to get line and column for.<br/>  If this value is -1 use the current scan pointer position<br/><br/>**Returns:**<br/>anonymous type: line   = line (1..n),<br/>                  col    = column (1..n),<br/>                  offset, <br/>                  astext = "Ln l+1  Col c+1"<br/>|
|`M: string GetUptoLine(int pos = -1, int lastNoofLines = 0)`|Get all text up to and including the line containing pos (excluding NL)<br/>- Optionally only get the lastNoofLines if > 0<br/><br/>**Parameters:**<br/><code>pos:</code> Position or -1 for current position<br/>|
|`M: bool LogError(string errorMsg, string errorContext = "Parse error", int errIndex = -1)`|Log an Error (see ScanErrorLog) with given erroMsg and errorContext:<br/>- At current Index position (default errIndex = -1) or at given errIndex ( >= 0 )<br/>- Records the last 10 lines and Line and Column no in ErrorLog - for later display<br/><br/>**Returns:**<br/>False always - so can use to return false from caller<br/>|
|`M: bool IsError()`|Return Error status<br/>|

