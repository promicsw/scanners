# interface ILogScanError
Interface for Logging and Querying errors during scanning/parsing/other operations.<br/>
> All Scanner derivatives implement this interface on the bound [ScanErrorLog](Docs/ScanErrorLog.md)

| Members | Description |
| :---- | :------ |
| ***Implementation:*** |  |
| ``P: bool IsError`` | Query if an error is currently logged.<br/> |
| ``M: bool LogError(string errorMsg, string errorContext = "Parse error", int errIndex = -1)`` | Log an error while scanning/parsing/other.<br/><br/>**Parameters:**<br/><code>errorMsg:</code> Error message to log.<br/><code>errorContext:</code> Error context to log.<br/><code>errIndex:</code> Error Index in source (-1 = current else specific)<br/><br/>**Returns:**<br/>Typically should be implemented to always return false so that the caller may return this as an error condition. |
