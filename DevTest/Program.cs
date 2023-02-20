using Psw.Scanners;
using static System.Console;

//TestScanBlock();
TestScanList();

void TestScanBlock() {
    var scn = new ScriptScanner();

    WriteLine("Test ScanBlock:");

    //var block = "{body it's /* comment } */ {nested} ` } ` }";
    var block = "{/* } */}";
//    var block = @"{line 1 it's
//line 2 // after }
//' } '
//line 2 {nested}
//}";
    scn.SetSource(block);

    if (scn.ScanBlock()) {
        WriteLine($"ScanBlock: <<{block}>> = Pass, Token = <<{scn.Token}>>");
        WriteLine($"StripComments: <<{scn.TokenStripComments}>>");
    }
    else WriteLine(scn.ErrorLog.AsConsoleError("ScanBlock Error:"));
}

void TestScanList() {
    var scn = new ScriptScanner();

    WriteLine("Test ScanList:");

    var listSource = "( one, two, ,three, 'Literal with comma ,' , after, [Block with comma ,] )";
//    var listSource = @"( one, two, ,three, 'Literal with comma ,'
//after, [Block with comma ,] 
//Line 1
//Line 2)";

    scn.SetSource(listSource);

    var list = scn.ScanList();

    if (list != null) {
        list.ForEach(x => WriteLine(x));
    }
    else WriteLine(scn.ErrorLog.AsConsoleError("ScanList Error:"));
}

