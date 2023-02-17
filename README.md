# Script Utilities
A set of high performance C# utilities incorporating Scanners for text parsing, 
structured Text Writing
and sophisticated Parser building (see [Flow Expressions](#id-fex)).


## Scanners
A Scanner is used to scan over and extract *Tokens* from text, check for strings or characters (delimiters), skip over text etc.

> Tokens represent basic units of meaning in the input source code, such as keywords, identifiers, operators and special characters.

The library provides associated utilities and low level scanners (extensible) with a comprehensive set of methods:
- [TextScanner](Docs/TextScanner.md): Primary core text scanning facilities
- [ScriptScanner](Docs/ScriptScanner.md): Extends TextScanner with facilities useful for scanning *scripts* 
- [ScanErrorLog](Docs/ScanErrorLog.md): Class to log and output scan error messages
- [ILogScanError](Docs/ILogScanError.md): Interface to allow external entities to log errors in ScanErrorLog

**Simple scanning example:**
```csharp
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
    var body = scn.TrimToken();

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
--------------------------^ (ln:1 Ch:27)
Parse error: { expected
```

As a teaser, the above parsing can be implemented using [Flow Expressions](#id-fex)
```csharp
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

## Structured text writer
Provides methods for writing/generating structured text output (to an internal or external StringBuffer) with automatic indentation management via a fluent API.

Includes some methods for writing structured HTML with automatic tag management, and may be further extended to generate any kind of output.
- [IndentTextWriter](Docs/IndentTextWriter.md): Reference document.

Example text writing:
```csharp
void WriterSample() {
    var w = new IndentTextWriter();
    w.WL("class ClassName")
     .WL("{").Indent()
     .WL("public int SomeValue { get; set; }")
     .WL()
     .WL("public int SomeMethod(int value) {").Indent()
     .WL("return value * 2;")
     .Outdent().WL("}")
     .Outdent().WL("}");

    Console.WriteLine(w.AsString());
}
```
```con
class ClassName
{
    public int SomeValue { get; set; }

    public int SomeMethod(int value) {
        return value * 2;
    }
}
```
Sample Html writing:
```csharp
void WriterHtmlSample() {
    var w = new IndentTextWriter();
    w.HtmlTag("div", "class='some-class'", c =>
        c.HtmlLineTag("p", "Some paragraph text")
         .HtmlTag("div", c => c.WL("Inner text"))
    );
    Console.WriteLine(w.AsString());
}
```
```html
<div class='some-class'>
    <p>Some paragraph text</p>
    <div>
        Inner text
    </div>
</div>
```
<a id="id-fex"></a>
## Flow Expression
Powerful and novel mechanism for building inline Parsers and other logical flow expressions.
Flow Expressions are fully described in a separate repo [flow-expressions](https://github.com/PromicSW/flow-expressions)

The following example is a complete **Expression Parser**, including evaluation and error reporting:

```csharp
public static void ExpressionEval() {
    /*
     * Expression Grammar:
     * expression     => factor ( ( '-' | '+' ) factor )* ;
     * factor         => unary ( ( '/' | '*' ) unary )* ;
     * unary          => ( '-' ) unary | primary ;
     * primary        => NUMBER | "(" expression ")" ;
    */

    // Number Stack for calculations
    Stack<double> numStack = new Stack<double>();

    var expr1 = "9 - (5.5 + 3) * 6 - 4 / ( 9 - 1 )";

    Console.WriteLine($"Evaluating expression: {expr1}");

    var fex = new FexParser(expr1);

    var expr = fex.Seq(s => s
        .Ref("factor")
        .RepOneOf(0, -1, r => r
            .Seq(s => s.Ch('+').Ref("factor").Act(c => numStack.Push(numStack.Pop() + numStack.Pop())))
            .Seq(s => s.Ch('-').Ref("factor").Act(c => numStack.Push(-numStack.Pop() + numStack.Pop())))
         ));

    var factor = fex.Seq(s => s.RefName("factor")
        .Ref("unary")
        .RepOneOf(0, -1, r => r
            .Seq(s => s.Ch('*').Ref("unary").Act(c => numStack.Push(numStack.Pop() * numStack.Pop())))
            .Seq(s => s.Ch('/').Ref("unary")
                       .Op(c => numStack.Peek() != 0).OnFail("Division by 0") // Trap division by 0
                       .Act(c => numStack.Push(1/numStack.Pop() * numStack.Pop())))
         ));

    var unary = fex.Seq(s => s.RefName("unary")
        .OneOf(o => o
            .Seq(s => s.Ch('-').Ref("unary").Act(a => numStack.Push(-numStack.Pop())))
            .Ref("primary")
         ).OnFail("Primary expected"));

    var primary = fex.Seq(s => s.RefName("primary")
        .OneOf(o => o
            .Seq(e => e.Ch('(').Fex(expr).Ch(')').OnFail(") expected"))
            .Seq(s => s.NumDecimal(n => numStack.Push(n)))
         ));

    var exprEval = fex.Seq(s => s.GlobalPreOp(c => c.SkipSp()).Fex(expr).IsEos().OnFail("invalid expression"));

    Console.WriteLine(fex.Run(exprEval, () => $"Passed = {numStack.Pop():F4}", e => e.AsConsoleError("Expression Error:")));
}
```
