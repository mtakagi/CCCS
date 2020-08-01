using System.Text;

namespace CCCS
{

    public class CodeGenerator
    {
        private static string[] reg = { "rdi", "rsi", "rdx", "rcx", "r8", "r9" };
        private static int labelseq = 1;
        private static string functionname = "";

        private static string GenAddress(Node node)
        {
            switch (node.Kind)
            {
                case NodeKind.LeftVariable:
                    return $"  mov rax, rbp\n  sub rax, {node.Offset}\n  push rax\n";
                case NodeKind.Dereference:
                    return CodeGen(node.Lhs);
                default:
                    throw new System.Exception();
            }
        }

        private static string GenLVar(Node node)
        {
            if (node.Type.Kind == TypeKind.Array)
            {
                throw new System.Exception();
            }

            return GenAddress(node);
        }

        private static string GenLoad()
        {
            return "  pop rax\n  mov rax, [rax]\n  push rax\n";
        }

        public static string CodeGen(Node node, string funcname = "")
        {
            if (!string.IsNullOrEmpty(funcname) && funcname != functionname)
            {
                functionname = funcname;
            }
            switch (node.Kind)
            {
                case NodeKind.Null:
                    return "";
                case NodeKind.Nunber:
                    return $"  push {node.IntValue}\n";
                case NodeKind.ExpressionStatement:
                    return $"{CodeGen(node.Lhs)}  add rsp, 8\n";
                case NodeKind.LeftVariable:
                    if (node.Type.Kind != TypeKind.Array)
                    {
                        return $"{GenAddress(node)}{GenLoad()}";
                    }
                    else
                    {
                        return GenAddress(node);
                    }
                case NodeKind.Assign:
                    return $"{GenLVar(node.Lhs)}{CodeGen(node.Rhs)}  pop rdi\n  pop rax\n  mov [rax], rdi\n  push rdi\n";
                case NodeKind.Address:
                    return GenLVar(node.Lhs);
                case NodeKind.Dereference:
                    if (node.Type.Kind != TypeKind.Array)
                    {
                        return $"{CodeGen(node.Lhs)}{GenLoad()}";
                    }
                    else
                    {
                        return CodeGen(node.Lhs);
                    }
                case NodeKind.IF:
                    {
                        var builder = new System.Text.StringBuilder();
                        int seq = labelseq++;

                        if (node.Els != null)
                        {
                            builder.Append(CodeGen(node.Cond));
                            builder.Append("  pop rax\n");
                            builder.Append("  cmp rax, 0\n");
                            builder.Append($"  je  .L.else.{seq}\n");
                            builder.Append(CodeGen(node.Then));
                            builder.Append($"  jmp .L.end.{seq}\n");
                            builder.Append($".L.else.{seq}:\n");
                            builder.Append(CodeGen(node.Els));
                            builder.Append($".L.end.{seq}:\n");
                        }
                        else
                        {
                            builder.Append(CodeGen(node.Cond));
                            builder.Append("  pop rax\n");
                            builder.Append("  cmp rax, 0\n");
                            builder.Append($"  je  .L.end.{seq}\n");
                            builder.Append(CodeGen(node.Then));
                            builder.Append($".L.end.{seq}:\n");
                        }

                        return builder.ToString();
                    }
                case NodeKind.While:
                    {
                        var builder = new System.Text.StringBuilder();
                        var seq = labelseq++;

                        builder.Append($".L.begin.{seq}:\n");
                        builder.Append(CodeGen(node.Cond));
                        builder.Append("  pop rax\n");
                        builder.Append("  cmp rax, 0\n");
                        builder.Append($"  je  .L.end.{seq}\n");
                        builder.Append(CodeGen(node.Then));
                        builder.Append($"  jmp .L.begin.{seq}\n");
                        builder.Append($".L.end.{seq}:\n");

                        return builder.ToString();
                    }
                case NodeKind.For:
                    {
                        var builder = new System.Text.StringBuilder();
                        var seq = labelseq++;

                        if (node.Init != null)
                        {
                            builder.Append(CodeGen(node.Init));
                        }

                        builder.Append($".L.begin.{seq}:\n");

                        if (node.Cond != null)
                        {
                            builder.Append(CodeGen(node.Cond));
                            builder.Append("  pop rax\n");
                            builder.Append("  cmp rax, 0\n");
                            builder.Append($"  je  .L.end.{seq}\n");
                        }

                        builder.Append(CodeGen(node.Then));

                        if (node.Inc != null)
                        {
                            builder.Append(CodeGen(node.Inc));
                        }

                        builder.Append($"  jmp .L.begin.{seq}\n");
                        builder.Append($".L.end.{seq}:\n");

                        return builder.ToString();
                    }
                case NodeKind.Body:
                    {
                        var builder = new StringBuilder();
                        var next = node.Next;
                        while (next != null)
                        {
                            builder.Append(CodeGen(next));
                            next = next.Next;
                        }

                        return builder.ToString();
                    }
                case NodeKind.FunctionCall:
                    {
                        var builder = new StringBuilder();
                        var arglen = 0;

                        for (var arg = node.Args; arg != null; arg = arg.Next)
                        {
                            builder.Append(CodeGen(arg));
                            arglen++;
                        }

                        for (var i = arglen - 1; i >= 0; i--)
                        {
                            builder.Append($"  pop {reg[i]}\n");
                        }

                        var seq = labelseq++;
                        builder.Append("  mov rax, rsp\n");
                        builder.Append("  and rax, 15\n");
                        builder.Append($"  jnz .L.call.{seq}\n");
                        builder.Append("  mov rax, 0\n");
                        builder.Append($"  call _{node.FuncName}\n");
                        builder.Append($"  jmp .L.end.{seq}\n");
                        builder.Append($".L.call.{seq}:\n");
                        builder.Append("  sub rsp, 8\n");
                        builder.Append("  mov rax, 0\n");
                        builder.Append($"  call _{node.FuncName}\n");
                        builder.Append("  add rsp, 8\n");
                        builder.Append($".L.end.{seq}:\n");
                        builder.Append("  push rax\n");

                        return builder.ToString();
                    }
                case NodeKind.Return:
                    return $"{CodeGen(node.Lhs)}  pop rax\n  jmp .L.return.{functionname}\n";
            }

            var sb = new System.Text.StringBuilder();

            sb.Append(CodeGen(node.Lhs));
            sb.Append(CodeGen(node.Rhs));

            sb.Append("  pop rdi\n");
            sb.Append("  pop rax\n");

            switch (node.Kind)
            {
                case NodeKind.Add:
                    if (node.Type.BaseType != null)
                    {
                        sb.Append($"  imul rdi, {node.Type.BaseType.Size}\n");
                    }
                    sb.Append("  add rax, rdi\n");
                    break;
                case NodeKind.Sub:
                    if (node.Type.BaseType != null)
                    {
                        sb.Append($"  imul rdi, {node.Type.BaseType.Size}\n");
                    }
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
