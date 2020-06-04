using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

public class TokenTest
{

    [MenuItem("CCCS/Test")]
    public static void Test()
    {
        Debug.Log(Compile("1+2+3-4"));
    }

    public static string Compile(string str)
    {
        var tokenizer = new CCCS.Tokenizer(str);
        var token = tokenizer.Tokenize();
        var lexer = new CCCS.TokenLexer(token);
        var sb = new StringBuilder();

        Debug.Log(token);

        sb.Append(".intel_syntax noprefix\n");
        sb.Append(".globl _main\n");
        sb.Append("_main:\n");

        sb.Append($"  mov rax, {lexer.ExpectNumber()}\n");

        while (!lexer.IsEOF())
        {
            if (lexer.Consume('+'))
            {
                sb.Append($"  add rax, {lexer.ExpectNumber()}\n");
            }
            else
            {
                lexer.Expect('-');
                sb.Append($"  sub rax, {lexer.ExpectNumber()}\n");
            }
        }

        sb.Append("  ret\n");

        return sb.ToString();
    }
}
