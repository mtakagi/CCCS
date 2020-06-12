using System.Collections.Generic;

namespace CCCS
{
    public class TokenLexer
    {
        private Token token;
        public List<Node> Code { get; private set; }

        public TokenLexer(Token token)
        {
            this.token = token;
            this.Code = new List<Node>();
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

        public Token ConsumeIdentifier()
        {
            if (token.Kind != TokenKind.Identifier)
            {
                return null;
            }

            var result = token;

            token = token.Next;

            return result;
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

        public void Program()
        {
            while (!this.IsEOF())
            {
                this.Code.Add(Statement());
            }
        }

        public Node Statement()
        {
            var node = this.Expr();
            Expect(";");

            return node;
        }

        public Node Expr()
        {
            return this.Assign();
        }

        public Node Assign()
        {
            var node = this.Equality();

            if (this.Consume("="))
            {
                node = new Node(NodeKind.Assign, node, this.Assign());
            }

            return node;
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

            var token = this.ConsumeIdentifier();

            if (token != null)
            {
                var node = new Node((token.StrValue[0] - 'a' + 1) * 8, NodeKind.LeftVariable);

                return node;
            }

            return new Node(this.ExpectNumber());
        }
    }
}