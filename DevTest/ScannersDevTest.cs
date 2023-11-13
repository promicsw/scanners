using Psw.Scanners;
using static System.Console;

//ScanSample();

// Block samples: -----------------------------------------
var block1 = "{body it's /* comment } */ {nested} ` } `}";

var block2 = @"{ // Should retain newlines
    body it's /* comment } */ {nested} ` } `
}";

var block3 = @"{|Basic Html and 'controls'
                |>@LabelItem('Forms:', 'Vertical, Horizontal, Inline and advanced layouts')
                |NavBar, Menus, Tab and Grid layouts, Cards, Svg etc.
                |A whole new way... <b>it's a game changer :)</b>}";

TestScanBlock(block2);
//TestBlockScan();
//TestStringBlockScan();

var rawBlock = @"```
Line 1
Line 2
```";

//TestScanRawBlock(rawBlock, "```", "```");

// List samples: ------------------------------------------
var list1 = "( one, two, ,three, 'Literal text' , after, [Block with comma , and )] [block 2], [block 3] )";

var list2 = @"(one, 
               two,
               three,
               'literal , )',
               [block])";

var list3 = @"{Basic Html and 'controls'
               |>@LabelItem('Forms:', 'Vertical, Horizontal, Inline and advanced layouts')
               |NavBar, Menus, Tab and Grid layouts, Cards, Svg etc.
               |A whole new way... <b>it's a game changer :)</b> }";

var list4 = @"([Basic Html and 'controls'
                |>@LabelItem('Forms:', 'Vertical, Horizontal, Inline and advanced layouts')
                |NavBar, Menus, Tab and Grid layouts, Cards, Svg etc.
                |A whole new way... <b>it's a game changer :)</b>])";

//TestScanList(list2);
//TestScanList(list3, "{}", '|');  // list3 needs different delimiters and separator

// Scanner sample
bool ScanSample() {
    var sample = " FuncName (prm1, 'prm2') { sample body }";
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

void TestBlockScan() {
    WriteLine("Test Block Scan:");


    void Test(string block, bool isOpen = false) {
        var scn = new ScriptScanner(block);
        WriteLine();
        WriteLine($"  Test: {block}");
        if (scn.ScanBlock("<>", isOpen)) {
            WriteLine($"  Token: [{scn.Token}]");
            WriteLine($"  StipToken: [{scn.StripToken}]");
            WriteLine($"  Remainder: {scn.LineRemainder()}");
        }
        else WriteLine(scn.ErrorLog.AsConsoleError("Fail:"));
    }

    //Test("<Valid full block>the remainder");
    //Test("Valid open block>the remainder", true);
    //Test("<Valid <nested> block>the remainder");
    Test("<Valid /*with > comment*/ block>the remainder");
    Test("<Valid /*with > /*nested comment*/*/ block>the remainder");
    Test("<Valid 'with > in string'>the remainder");
    Test("<Valid 'with > in string not closed>the remainder");
    Test("<Valid '''with > in string not closed>the remainder");

    Test("<Invalid /*with bad nested comment block>the remainder");
    Test("Invalid not at block start>the remainder");
    Test("<Bad <nested block>the remainder");
    Test("<Bad block the remainder");
}

void TestStringBlockScan() {
    WriteLine("Test String Block Scan:");


    void Test(string block, bool isOpen = false) {
        var scn = new ScriptScanner(block);
        WriteLine();
        WriteLine($"  Test: {block}");
        if (scn.ScanBlock("/*", "*/", isOpen)) {
            WriteLine($"  Pass: [{scn.Token}]");
            WriteLine($"  Remainder: {scn.LineRemainder()}");
            WriteLine($"  Remainder via SubSource: {scn.SubSource(-1, -1)}");
            WriteLine($"  Remainder via SubSource (+1): {scn.SubSource(scn.Index+1, -1)}");
            WriteLine($"  Remainder via SubSource (+2, 4): {scn.SubSource(scn.Index+1, 4)}");
        }
        else WriteLine(scn.ErrorLog.AsConsoleError("Fail:"));
    }

    Test("/*Valid full block*/the remainder");
    Test("Valid open block*/the remainder", true);
    Test("Valid not at block start*/the remainder");

    Test("/*Valid /*nested*/ block*/the remainder");
    Test("/*Bad /*nested block*/the remainder");
    Test("/*Bad block the remainder");
}

// Block scanning tests
void TestScanBlock(string block) {
    var scn = new ScriptScanner();

    WriteLine($"Test ScanBlock: <{block}>");
    scn.SetSource(block);

    if (scn.ScanBlock()) {
        WriteLine("Result: Pass");
        WriteLine($"Token: <{scn.Token}>");
        WriteLine($"StripComments: <{scn.StripToken}>");
    }
    else WriteLine(scn.ErrorLog.AsConsoleError("ScanBlock Error:"));
}

// RowBlock scanning tests
//void TestScanRawBlock(string block, string blockStart, string blockEnd) {
//    var scn = new ScriptScanner();

//    WriteLine($"Test ScanRawBlock: <{block}>");
//    scn.SetSource(block);

//    if (scn.ScanRawBlock(blockStart, blockEnd)) {
//        WriteLine("Result: Pass");
//        WriteLine($"Token: <{scn.Token}>");
//        //WriteLine($"StripComments: <{scn.StripToken}>");
//    }
//    else WriteLine(scn.ErrorLog.AsConsoleError("ScanRawBlock Error:"));
//}

// List scanning tests
void TestScanList(string lst, string delims = "()", char sep = ',') {
    var scn = new ScriptScanner();

    WriteLine($"Test ScanList: {lst}");

    scn.SetSource(lst);

    var list = scn.ScanList(delims, sep);

    if (list != null) {
        list.ForEach(x => WriteLine(x));
    }
    else WriteLine(scn.ErrorLog.AsConsoleError("ScanList Error:"));
}



