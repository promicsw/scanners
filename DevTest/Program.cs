using Psw.Scanners;
using static System.Console;

TestScanBlock();

void TestScanBlock() {
    var scn = new ScriptScanner();

    WriteLine("Test ScanBlock:");

    var block = "{body it's /* comment } */ {nested} ` } ` }";
//    var block = @"{line 1 it's
//line 2 // after }
//' } '
//line 2 {nested}
//}";
    scn.SetSource(block);

    if (scn.ScanBlock()) {
        WriteLine($"ScanBlock: <<{block}>> = Pass, Token = <<{scn.Token}>>");
        WriteLine($"StripComments: <<{ScriptScanner.StripComments(scn.Token)}>>");
    }
    else WriteLine(scn.ErrorLog.AsConsoleError("ScanBlock Error:"));
}

