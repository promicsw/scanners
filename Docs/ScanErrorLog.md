# class ScanErrorLog

Utility class for Logging errors while scanning and then produce formated error output in various forms.

> If the error is logged via one of the Scanners then the ScriptSegment and position is logged by the scanner.

Error output has the following basic form:

```con
Sample Error                              // Error Heading 
 FuncName(prm1, 'prm2')  sample body }    // ScriptSegment
-------------------------^(ln: 1 Ch: 26)  // Error position in segment
Filename: C:\somefilename.txt             // Error File name (if defined)
Parse error: { expected                   // Error Context : Error Message
```


|Member|Description|
|----|------|
|**Properties:**||
|`P: bool IsError`|Get/Set Error status<br/>|
|`P: string ErrorMessage`|Get/Set Error massage<br/>|
|`P: string ErrorContext`|Get/Set Error context<br/>|
|`P: string ScriptSegment`|Get/Set Error script segment (if applicable)<br/>|
|`P: int Ln`|Get/Set Error line number in a script (1..n)<br/>|
|`P: int Col`|Get/Set Error column number (1..n)<br/>|
|`P: string FileName`|Get/Set Error filename (if applicable - and subsequent errors will be logged with this filename until it is changed)<br/>|
|**Methods:**||
|`M: bool LogError(string errMsg, string errContext = "Parse error", string? scriptSegment = null, int ln = 1, int col = 1)`|Log an Error and set IsError to true<br/><br/>**Parameters:**<br/><code>errMsg:</code> Error message<br/><code>errContext:</code> Error context<br/><code>scriptSegment:</code> Optional script segment to use when formatting the error output<br/><code>ln:</code> Line number in script segment<br/><code>col:</code> Column number in script segment<br/><br/>**Returns:**<br/>False always - so it can return false from caller (if required)<br/>|
|`M: string AsHtmlError(string heading)`|Return error string with given heading, formatted as Html.<br/>|
|`M: string AsTextError(string heading)`|Return error string with given heading, formatted as a plain string.<br/>|
|`M: string AsConsoleError(string heading)`|Return error string with given heading, formatted for console output with embedded color directives.<br/>|
