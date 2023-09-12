# class ScanErrorLog
Utility class for Logging errors while scanning and then produce formated error output in various forms.<br/>
> If the error is logged via one of the Scanners then the ScriptSegment and position is logged by the scanner.

Error output has the following basic form:

```con
Sample Error                              // Error Heading 
 FuncName(prm1, 'prm2')  sample body }    // ScriptSegment
-------------------------^(Ln: 1 Ch: 26)  // Error position in segment
Filename: C:\somefilename.txt             // Error File name (if defined)
Parse error: { expected                   // Error Context : Error Message
```

| Members | Description |
| :---- | :------ |
| ***Properties:*** |  |
| ``P: bool IsError`` | Get/Set Error status.<br/> |
| ``P: string ErrorMessage`` | Get/Set Error message.<br/> |
| ``P: string ErrorContext`` | Get/Set Error context.<br/> |
| ``P: string ScriptSegment`` | Get/Set Error script segment (if applicable).<br/> |
| ``P: string ErrorFileName`` | Get/Set the Error filename:<br/>- For certain situations it may be convenient to record a filename with an error when processing multiple files.<br/>- In the above case: the ErrorFileName may be set and is then associated with any subsequent error, until it is changed.<br/> |
| ``P: int Ln`` | Get/Set Error line number in a script (1..n).<br/> |
| ``P: int Col`` | Get/Set Error column number (1..n).<br/> |
