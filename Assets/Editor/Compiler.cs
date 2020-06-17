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
            var parser = new CCCS.Parser(lexer);
            var sb = new StringBuilder();

            parser.Program();

            sb.Append(".intel_syntax noprefix\n");
            sb.Append(".globl _main\n");
            sb.Append("_main:\n");

            sb.Append("  push rbp\n");
            sb.Append("  mov rbp, rsp\n");
            sb.Append("  sub rsp, 208\n");

            foreach (var node in parser.Code)
            {
                sb.Append(CodeGenerator.CodeGen(node));
                sb.Append("  pop rax\n");
            }

            sb.Append("  mov rsp, rbp\n");
            sb.Append("  pop rbp\n");
            sb.Append("  ret\n");

            return sb.ToString();
        }
    }
}
