using System.Text;

namespace CCCS
{
    public class Compiler
    {
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
}
