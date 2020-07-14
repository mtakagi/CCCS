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

            foreach (var func in parser.Func)
            {
                var offset = 0;
                for (var val = func.Locals; val != null; val = val.Next)
                {
                    offset += 8;
                    val.Offset = offset;
                }
                func.StackSize = offset;

                if (offset == 0)
                {
                    func.StackSize = 208;
                }
            }

            sb.Append(".intel_syntax noprefix\n");
            foreach (var func in parser.Func)
            {
                sb.Append($".globl _{func.Name}\n");
                sb.Append($"_{func.Name}:\n");

                sb.Append("  push rbp\n");
                sb.Append("  mov rbp, rsp\n");
                sb.Append($"  sub rsp, {func.StackSize}\n");

                for (var node = func.Node; node != null; node = node.Next)
                {
                    sb.Append(CodeGenerator.CodeGen(node, func.Name));
                }

                sb.Append($".L.return.{func.Name}:\n");
                sb.Append("  mov rsp, rbp\n");
                sb.Append("  pop rbp\n");
                sb.Append("  ret\n");
            }

            return sb.ToString();
        }
    }
}
