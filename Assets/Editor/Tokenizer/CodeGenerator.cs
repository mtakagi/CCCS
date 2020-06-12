using System.Text;

namespace CCCS
{

    public class CodeGenerator
    {
        private static string GenLVar(Node node)
        {
            if (node.Kind != NodeKind.LeftVariable)
            {
                throw new System.Exception();
            }

            return $"  mov rax, rbp\n  sub rax, {node.Offset}\n  push rax\n";
        }

        public static string CodeGen(Node node)
        {
            switch (node.Kind)
            {
                case NodeKind.Nunber:
                    return $"  push {node.IntValue}\n";
                case NodeKind.LeftVariable:
                    return $"{GenLVar(node)}  pop rax\n  mov rax, [rax]\n  push rax\n";
                case NodeKind.Assign:
                    return $"{GenLVar(node.Lhs)}{CodeGen(node.Rhs)}  pop rdi\n  pop rax\n  mov [rax], rdi\n  push rdi\n";
            }

            var sb = new System.Text.StringBuilder();

            sb.Append(CodeGen(node.Lhs));
            sb.Append(CodeGen(node.Rhs));

            sb.Append("  pop rdi\n");
            sb.Append("  pop rax\n");

            switch (node.Kind)
            {
                case NodeKind.Add:
                    sb.Append("  add rax, rdi\n");
                    break;
                case NodeKind.Sub:
                    sb.Append("  sub rax, rdi\n");
                    break;
                case NodeKind.Mul:
                    sb.Append("  imul rax, rdi\n");
                    break;
                case NodeKind.Div:
                    sb.Append("  cqo\n");
                    sb.Append("  idiv rdi\n");
                    break;
                case NodeKind.Equal:
                    sb.Append("  cmp rax, rdi\n");
                    sb.Append("  sete al\n");
                    sb.Append("  movzx rax, al\n");
                    break;
                case NodeKind.NotEqual:
                    sb.Append("  cmp rax, rdi\n");
                    sb.Append("  setne al\n");
                    sb.Append("  movzx rax, al\n");
                    break;
                case NodeKind.LesserThan:
                    sb.Append("  cmp rax, rdi\n");
                    sb.Append("  setl al\n");
                    sb.Append("  movzx rax, al\n");
                    break;
                case NodeKind.LesserThanOrEqual:
                    sb.Append("  cmp rax, rdi\n");
                    sb.Append("  setle al\n");
                    sb.Append("  movzx rax, al\n");
                    break;
            }

            sb.Append("  push rax\n");

            return sb.ToString();
        }
    }
}
