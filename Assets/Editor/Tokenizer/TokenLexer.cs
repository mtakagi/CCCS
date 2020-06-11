namespace CCCS
{
    public class TokenLexer
    {
        private Token token;

        public TokenLexer(Token token)
        {
            this.token = token;
        }

        public bool Consume(string op)
        {
            if (token.Kind != TokenKind.Reserved || op != token.StrValue
            || op.Length != token.Length || op != token.StrValue)
            {
                return false;
            }
            token = token.Next;

            return true;
        }

        public void Expect(string op)
        {
            if (token.Kind != TokenKind.Reserved || op != token.StrValue
            || op.Length != token.Length || op != token.StrValue)
            {
                throw new System.Exception();
            }
            token = token.Next;
        }

        public int ExpectNumber()
        {
            if (token.Kind != TokenKind.Number)
            {
                throw new System.InvalidOperationException();
            }

            var val = token.IntValue;

            token = token.Next;

            return val;
        }

        public bool IsEOF() => token.IsEOF();

        public Node Expr()
        {
            return this.Equality();
        }

        public Node Equality()
        {
            var node = this.Relational();

            for (; ; )
            {
                if (this.Consume("=="))
                {
                    node = new Node(NodeKind.Equal, node, this.Relational());
                }
                else if (this.Consume("!="))
                {
                    node = new Node(NodeKind.NotEqual, node, this.Relational());
                }
                else
                {
                    return node;
                }
            }
        }

        public Node Relational()
        {
            var node = this.Add();

            for (; ; )
            {
                if (this.Consume("<"))
                {
                    node = new Node(NodeKind.LesserThan, node, this.Add());
                }
                else if (this.Consume("<="))
                {
                    node = new Node(NodeKind.LesserThanOrEqual, node, this.Add());
                }
                else if (this.Consume(">"))
                {
                    node = new Node(NodeKind.LesserThan, this.Add(), node);
                }
                else if (this.Consume(">="))
                {
                    node = new Node(NodeKind.LesserThanOrEqual, this.Add(), node);
                }
                else
                {
                    return node;
                }
            }
        }

        public Node Add()
        {
            var node = this.Mul();

            for (; ; )
            {
                if (this.Consume("+"))
                {
                    node = new Node(NodeKind.Add, node, this.Mul());
                }
                else if (this.Consume("-"))
                {
                    node = new Node(NodeKind.Sub, node, this.Mul());
                }
                else
                {
                    return node;
                }
            }
        }

        public Node Mul()
        {
            var node = this.Unary();

            for (; ; )
            {
                if (this.Consume("*"))
                {
                    node = new Node(NodeKind.Mul, node, this.Unary());
                }
                else if (this.Consume("/"))
                {
                    node = new Node(NodeKind.Div, node, this.Unary());
                }
                else
                {
                    return node;
                }
            }
        }

        public Node Unary()
        {
            if (this.Consume("+"))
            {
                return this.Primary();
            }
            else if (this.Consume("-"))
            {
                return new Node(NodeKind.Sub, new Node(0), this.Primary());
            }
            else
            {
                return this.Primary();
            }
        }

        public Node Primary()
        {
            if (this.Consume("("))
            {
                var node = this.Expr();
                this.Expect(")");

                return node;
            }

            return new Node(this.ExpectNumber());
        }

        public string CodeGen(Node node)
        {
            if (node.Kind == NodeKind.Nunber)
            {
                return $"  push {node.IntValue}\n";
            }

            var sb = new System.Text.StringBuilder();

            sb.Append(this.CodeGen(node.Lhs));
            sb.Append(this.CodeGen(node.Rhs));

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