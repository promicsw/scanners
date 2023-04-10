# class ScriptScanner : TextScanner

Extends TextScanner with methods useful for script scanning and parsing.

> **Incorporates the following:**
> - Handles *comments*: Line `//...` and block `/*...*/` (handles nesting)
> - Skip whitespace and comments
> - Scan delimited strings and blocks
> - Standard identifier scanning.
> - Scan list structures.

|Member|Description|
|----|------|
|**Constructor:**||
|`C: ScriptScanner(string source = "", ScanErrorLog errorLog = null)`|Create a ScripScanner with given source string and optional external ScanErrorLog (else an internal one is created)<br/>|
|**Delimited Strings:**||
|`P: string StringDelim`|Get or Set the String Delimiters to use (default "'\`)<br/>|
|`M: bool IsStringDelim()`|Query if character at Index is one of the StringDelim values<br/>|
|**Script utilities:**||
|`M: bool StrLit()`|Scan a delimited String Literal:<br/>- Current Index must be at the starting delimiter ("\`' etc).<br/>- NOTE: A string literal may NOT span a line.<br/>- Token contains the string (excluding delimiters).<br/><br/>**Returns:**<br/>True: if there was a string literal and Index positioned after ending delimiter<br/>False: for no string or Eos - Index unchanged<br/>|
|`M: bool ValueOrStrLit(string termChars, bool orToEos = false)`|Scan either a StrLit or result of ScanToAny(termChars, orToEos) and ValidToken()<br/>- If Index is at a StringDelim - returns the result of StrLit()<br/>- Else returns the result of ScanToAny(termChars, orToEos) and ValidToken() (i.e non-blank value)<br/>- Use Token to retrieve the value<br/><br/>**Returns:**<br/>Success of the scan<br/>|
|`M: bool StdIdent()`|Scan Standard Identifier of the form: (letter \| _)\* (letterordigit \| _)\*<br/><br/>**Returns:**<br/>True for valid identifier (available via Token) else false<br/>|
|`M: bool StdIdent2()`|Scan Standard Identifier of the form: (letter \| _)\* (letterordigit \| _ \| -)\*<br/><br/>**Returns:**<br/>True for valid identifier (available via Token) else false<br/>|
|`M: bool ScanBlock(string blockDelims = "{}", bool isOpen = false)`|Scan a block delimited by blockDelims (E.g "\{\}" or "()" or "[]" etc.) <br/>- Handles Nesting and ignores any block delimiters inside comments or string literals (delimited by StringDelim)<br/>- Token contains the block content excluding the block delimiters.<br/><br/>**Parameters:**<br/><code>blockDelims:</code> String with opening and closing block delimiters (default = "\{\})<br/><code>isOpen:</code> False - current Index at start of block else Index just inside block<br/><br/>**Returns:**<br/>True for a valid block (Index positioned after block) else false and Logs an error (Index unchanged)<br/>|
|`M: string StripComments(string source)`|Static method: Return the source string with all line and block comments removed.<br/>|
|`M: List<string> ScanList(string delims = "()", char separator = ',', string block = "[]")`|Scan a List of the form: ( item1, item 2, ...)<br/>- Note: The next non-whitespace character must be the Opening list delimiter.<br/>- Item type 1: All text up to closing delimiter or separator (logged trimmed)<br/>- Item type 2: A string literal - may NOT span a line! (logged verbatim excluding string delimiters)<br/>- Item type 3: Block delimited text (logged verbatim excluding block delimiters) - use for multi-line text. <br/>- Blank items are not recorded.<br/><br/>**Parameters:**<br/><code>delims:</code> Opening and closing list delimiter (default = "()")<br/><code>separator:</code> List item separator (default = ,)<br/><code>block:</code> Opening and closing Block delimiters (default = "[]")<br/><br/>**Returns:**<br/>List of strings else null and error logged in ErrorLog<br/>|
|`M: bool ScanList(out List<string> list, string delims = "()", char separator = ',', string block = "[]")`|Scan a List of the form: ( item1, item 2 ... )<br/>- Note: The next non-whitespace character must be the Opening list delimiter.<br/>- Item type 1: All text up to closing delimiter or separator (logged trimmed)<br/>- Item type 2: A string literal - may NOT span a line! (logged verbatim  excluding string delimiters)<br/>- Item type 3: Block delimited text (logged verbatim excluding block delimiters) - use for multi-line text. <br/>- Blank items are not recorded.<br/><br/>**Parameters:**<br/><code>delims:</code> Opening and closing list delimiter (default = "()")<br/><code>separator:</code> List item separator (default = ,)<br/><code>block:</code> Opening an closing Block delimiters (default = "[]")<br/><br/>**Returns:**<br/>True and List of strings in out list (Index after list), else false and error logged in ErrorLog<br/>|
|**Whitespace and Comment skipping:**||
|`M: bool SkipSp()`|Skip Spaces (" \t")<br/><br/>**Returns:**<br/>True if not at Eos after skipping else False<br/>|
|`M: bool SkipWS(string wsChars = " \r\n\t")`|Skip given White Space characters (default: " \r\n\t")<br/><br/>**Returns:**<br/>True if not at Eos after skipping else False<br/>|
|`M: bool SkipWSC(bool termNL = false, string spaceChars = " \t")`|Skip White Space characters and comments //... or /\*..\*/ (handles nested comments)<br/>- White space: spaceChars + "\r\n" if termNL is false<br/>- Set termNL to position Index at the next newline not inside a block comment (/\*..\*/), else the newlines are also skipped.<br/><br/>**Parameters:**<br/><code>spaceChars:</code> Space characters to build the white space characters from (default: " \t")<br/><br/>**Returns:**<br/>True: Whitespace and comments skipped and Index directly after<br/>  False: Eos or comment error (missing \*/ logged as error) - Index unchanged<br/>|
|`M: bool IsComment()`|Query if Index is currently at a comment<br/>|
|`M: bool SkipComment(bool termNL = false, bool commentConfirmed = false)`|Skip continuous sequence of comments: <br/>- Line comment ( //..Eol/Eos ), Block comment ( /\*..\*/ ) and handles nested block comments:<br/>- NOTE: Index must currently be positioned at the start of a comment /<br/>- Set termNL to true to position Index at the newline after a line comment ( // ), else the newline is skipped<br/><br/>**Returns:**<br/>True: Comment skipped and Index positioned after comment<br/>False: Eos or comment error (missing \*/ logged as error) - Index unchanged<br/>|

