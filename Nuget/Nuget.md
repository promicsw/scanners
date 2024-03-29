# Scanners
A set of high performance Scanners implemented in C# for low-level text/script parsing.  

A Scanner is used to scan over and extract *Tokens* from text, check for strings or characters (delimiters), skip over text etc.


**Simple scanning example:**
```csharp
using Psw.Scanners;

bool ScanSample() {
    var sample = " FuncName (prm1, 'prm2') { sample body }"; // Text to parse
    var scn = new ScriptScanner(sample);

    // Use the scanner ErrorLog for formatted error reporting
    bool Error(string msg) {
        scn.LogError(msg);
        Console.WriteLine(scn.ErrorLog.AsConsoleError("Sample Error"));
        return false;
    }

    scn.SkipSp();
    if (!scn.StdIdent()) return Error("Function name expected");
    var funcName = scn.Token;

    scn.SkipSp();
    if (!scn.IsPeekCh('(')) return Error("( expected");

    var prm = scn.ScanList();
    if (prm == null) return Error("Parameters expected");

    scn.SkipSp();
    if (!scn.IsPeekCh('{')) return Error("{ expected");

    if (!scn.ScanBlock()) return Error("body expected");
    var body = scn.TrimToken;

    Console.WriteLine($"Result: {funcName}({string.Join(',', prm)}) {{{body}}}");
    return true;
}
```
**Sample output:**
```con
Result: FuncName(prm1,prm2) {sample body}

Or for an Error: 
Sample Error
 FuncName (prm1, 'prm2')  sample body }
--------------------------^ (Ln:1 Ch:27)
Parse error: { expected
```

### FYI: the above parsing can be implemented using [Flow Expressions](https://github.com/PromicSW/flow-expressions)
Flow Expressions are a powerful and novel mechanism for building complex inline Parsers and other logical flow systems.
```csharp
using Psw.FlowExpressions

void FexScanSample() {
    var fex = new FexParser(" FuncName (prm1, 'prm2') { sample body }");
    string funcName = "", body = "";
    List<string> prm = null;

    var funcParse = fex.Seq(s => s.GlobalPreOp(c => c.SkipSp())
        .StdIdent().ActToken(t => funcName = t).OnFail("Function name expected")
        .PeekCh('(').OnFail("( expected")
        .ScanList().ActValue<List<string>>(v => prm = v).OnFail("Parameters expected")
        .PeekCh('{').OnFail("{ expected")
        .ScanBlock().ActTrimToken(t => body = t));

    Console.WriteLine(fex.Run(funcParse, () => $"Result: {funcName}({string.Join(',', prm)}) {{{body}}}", e => e.AsConsoleError("Error")));
}
```


