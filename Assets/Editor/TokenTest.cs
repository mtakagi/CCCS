using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System.Diagnostics;

public class TokenTest
{

    private static string OutputFileName = "test.s";
    private static string OutputExeFileName = "test";

    [MenuItem("CCCS/Test")]
    public static void Test()
    {
        var path = Path.GetTempPath();
        var asm = Compile("1+2*3-4");

        File.WriteAllText(Path.Combine(path, "test.s"), asm, Encoding.UTF8);

        var info = new ProcessStartInfo("cc", $"-o {OutputExeFileName} {OutputFileName}");

        info.WorkingDirectory = path;

        using (var proc = Process.Start(info))
        {
            proc.WaitForExit();
        }
        using (var proc = Process.Start(Path.Combine(path, "test")))
        {
            proc.WaitForExit();
            UnityEngine.Debug.Log(proc.ExitCode);
        }
    }

    public static string Compile(string str)
    {
        var tokenizer = new CCCS.Tokenizer(str);
        var token = tokenizer.Tokenize();
        var lexer = new CCCS.TokenLexer(token);
        var node = lexer.Expr();
        var sb = new StringBuilder();

        sb.Append(".intel_syntax noprefix\n");
        sb.Append(".globl _main\n");
        sb.Append("_main:\n");

        sb.Append(lexer.CodeGen(node));
        sb.Append("  pop rax\n");
        sb.Append("  ret\n");

        return sb.ToString();
    }
}
