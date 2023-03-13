using Psw.Scanners;
using static System.Console;

ScanSample();

// Block samples: -----------------------------------------
var block1 = "{body it's /* comment } */ {nested} ` } `}";

var block2 = @"{ // Should retain newlines
    body it's /* comment } */ {nested} ` } `
}";

var block3 = @"{|Basic Html and 'controls'
                |>@LabelItem('Forms:', 'Vertical, Horizontal, Inline and advanced layouts')
                |NavBar, Menus, Tab and Grid layouts, Cards, Svg etc.
                |A whole new way... <b>it's a game changer :)</b> }";

TestScanBlock(block1);

// List samples: ------------------------------------------
var list1 = "( one, two, ,three, 'Literal text' , after, [Block with comma , and )] [block 2], [block 3] )";

var list2 = @"(one, 
               two,
               three,
               'literal , )',
               [
block
])";

var list3 = @"{Basic Html and 'controls'
               |>@LabelItem('Forms:', 'Vertical, Horizontal, Inline and advanced layouts')
               |NavBar, Menus, Tab and Grid layouts, Cards, Svg etc.
               |A whole new way... <b>it's a game changer :)</b> }";

var list4 = @"([Basic Html and 'controls'
                |>@LabelItem('Forms:', 'Vertical, Horizontal, Inline and advanced layouts')
                |NavBar, Menus, Tab and Grid layouts, Cards, Svg etc.
                |A whole new way... <b>it's a game changer :)</b>])";

TestScanList(list2);
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



