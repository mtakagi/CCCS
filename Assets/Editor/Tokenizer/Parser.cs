using System.Collections.Generic;
using System.Linq;

namespace CCCS
{
    public class Parser
    {
        private TokenLexer lexer;
        public List<Node> Code { get; private set; }
        public List<LocalVariable> Locals { get; private set; }

        public Parser(TokenLexer lexer)
        {
            this.lexer = lexer;
            this.Code = new List<Node>();
            this.Locals = new List<LocalVariable>();
        }

        public void Program()
        {
            while (!this.lexer.IsEOF())
            {
                this.Code.Add(Statement());
            }
        }

        public Node Statement()
        {
            var node = this.Expr();
            this.lexer.Expect(";");

            return node;
        }

        public Node Expr()
        {
            return this.Assign();
        }

        public Node Assign()
        {
            var node = this.Equality();

            if (this.lexer.Consume("="))
            {
                node = new Node(NodeKind.Assign, node, this.Assign());

                var local = this.Locals.Find(obj => obj.Name == this.lexer.token.StrValue);

                if (local != null)
                {
                    node.Offset = local.Offset;
                }
                else
                {
                    var offset = this.Locals.Count != 0 ? this.Locals.Last().Offset : 8;
                    local = new LocalVariable(this.lexer.token.StrValue, offset);
                    node.Offset = local.Offset;
                    this.Locals.Add(local);
                }
            }

            return node;
        }

        public Node Equality()
        {
            var node = this.Relational();

            for (; ; )
            {
                if (this.lexer.Consume("=="))
                {
                    node = new Node(NodeKind.Equal, node, this.Relational());
                }
                else if (this.lexer.Consume("!="))
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
                if (this.lexer.Consume("<"))
                {
                    node = new Node(NodeKind.LesserThan, node, this.Add());
                }
                else if (this.lexer.Consume("<="))
                {
                    node = new Node(NodeKind.LesserThanOrEqual, node, this.Add());
                }
                else if (this.lexer.Consume(">"))
                {
                    node = new Node(NodeKind.LesserThan, this.Add(), node);
                }
                else if (this.lexer.Consume(">="))
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
                if (this.lexer.Consume("+"))
                {
                    node = new Node(NodeKind.Add, node, this.Mul());
                }
                else if (this.lexer.Consume("-"))
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
                if (this.lexer.Consume("*"))
                {
                    node = new Node(NodeKind.Mul, node, this.Unary());
                }
                else if (this.lexer.Consume("/"))
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
            if (this.lexer.Consume("+"))
            {
                return this.Primary();
            }
            else if (this.lexer.Consume("-"))
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
            if (this.lexer.Consume("("))
            {
                var node = this.Expr();
                this.lexer.Expect(")");

                return node;
            }

            var token = this.lexer.ConsumeIdentifier();

            if (token != null)
            {
                var node = new Node((token.StrValue[0] - 'a' + 1) * 8, NodeKind.LeftVariable);

                return node;
            }

            return new Node(this.lexer.ExpectNumber());
        }

    }
}
