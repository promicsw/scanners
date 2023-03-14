# class ILogScanError

Interface defining a LogError method that can be used for error logging during scanning/parsing operations.

TextScanner (and all derivatives) implement this interface on the bound [ScanErrorLog](Docs/ScanErrorLog.md)

|Member|Description|
|----|------|
|`M: bool LogError(string errorMsg, string errorContext = "Parse error", int errIndex = -1)`|Log an error while scanning and parsing.<br/><br/>**Parameters:**<br/><code>errorMsg:</code> Error message to log<br/><code>errorContext:</code> Error context to log<br/><code>errIndex:</code> Error Index in source to log (-1 = current else specific)<br/><br/>**Returns:**<br/>Typically should be implemented to always return false so that the caller may return this as an error condition.<br/>|

