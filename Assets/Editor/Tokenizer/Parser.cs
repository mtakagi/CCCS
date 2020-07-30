using System.Collections.Generic;
using System.Linq;

namespace CCCS
{
    public class Parser
    {
        private TokenLexer lexer;
        public List<Node> Code { get; private set; }
        public VariableList Locals { get; private set; }

        public List<Function> Func { get; private set; }

        public Parser(TokenLexer lexer)
        {
            this.lexer = lexer;
            this.Code = new List<Node>();
            this.Func = new List<Function>();
        }

        public void Program()
        {
            while (!this.lexer.IsEOF())
            {
                this.Func.Add(this.Function());
            }
        }

        public LocalVariable ParameterDeclaration(string name, Type type)
        {
            var val = new LocalVariable(name, 0);
            var list = new VariableList();

            val.Type = type;
            list.Var = val;
            list.Next = this.Locals;

            this.Locals = list;

            return val;
        }

        public Type BaseType()
        {
            this.lexer.Expect("int");
            var type = Type.IntType;

            while (this.lexer.Consume("*"))
            {
                type = Type.PointerTo(type);
            }

            return type;
        }

        public VariableList FunctionParam()
        {
            var list = new VariableList();
            var type = this.BaseType();
            var token = this.lexer.ConsumeIdentifier();

            list.Var = this.ParameterDeclaration(token.StrValue, type);

            return list;
        }

        public VariableList ParameterList()
        {
            if (this.lexer.Consume(")"))
            {
                return null;
            }

            var head = this.FunctionParam();
            var cur = head;


            while (!this.lexer.Consume(")"))
            {
                this.lexer.Expect(",");
                cur.Next = this.FunctionParam();
                cur = cur.Next;
            }

            return head;
        }

        public Function Function()
        {
            var node = new Node();
            var func = new Function();

            var head = node;

            this.Locals = null;
            this.BaseType();

            var token = this.lexer.ConsumeIdentifier();

            this.lexer.Expect("(");
            func.Params = this.ParameterList();
            this.lexer.Expect("{");


            while (!this.lexer.Consume("}"))
            {
                node.Next = this.Statement();
                node = node.Next;
            }


            func.Name = token.StrValue;
            func.Node = head.Next;
            func.Locals = this.Locals;

            return func;
        }

        public Node Declaration()
        {
            var token = this.lexer.token;
            var type = this.BaseType();
            var val = this.ParameterDeclaration(this.lexer.ConsumeIdentifier().StrValue, type);

            if (this.lexer.Consume(";"))
            {
                return new Node(NodeKind.Null);
            }

            this.lexer.Expect("=");

            var lhs = new Node(val);
            var rhs = this.Expr();

            this.lexer.Expect(";");

            var node = new Node(NodeKind.Assign, lhs, rhs);

            return new Node(NodeKind.ExpressionStatement, node, null);
        }

        public Node Statement()
        {
            Node node = null;

            if (this.lexer.Consume("return"))
            {
                node = new Node(NodeKind.Return, this.Expr(), null);
            }
            else if (this.lexer.Consume("if"))
            {
                this.lexer.Expect("(");
                var cond = this.Expr();
                this.lexer.Expect(")");
                var then = this.Statement();
                Node els = null;

                if (this.lexer.Consume("else"))
                {
                    els = this.Statement();
                }

                return new Node(cond, then, els);
            }
            else if (this.lexer.Consume("while"))
            {
                this.lexer.Expect("(");
                var cond = this.Expr();
                this.lexer.Expect(")");
                var then = this.Statement();

                return new Node(cond, then);
            }
            else if (this.lexer.Consume("for"))
            {
                Node init = null;
                Node cond = null;
                Node inc = null;
                this.lexer.Expect("(");

                if (!this.lexer.Consume(";"))
                {
                    init = this.Expr();
                    this.lexer.Expect(";");
                }

                if (!this.lexer.Consume(";"))
                {
                    cond = this.Expr();
                    this.lexer.Expect(";");
                }

                if (!this.lexer.Consume(")"))
                {
                    inc = this.Expr();
                    this.lexer.Expect(")");
                }

                var then = this.Statement();

                return new Node(init, cond, inc, then);
            }
            else if (this.lexer.Consume("{"))
            {
                var head = new Node();
                node = head;
                while (!this.lexer.Consume("}"))
                {
                    head.Next = this.Statement();
                    head = head.Next;
                }

                return node;
            }
            else if (this.lexer.Peek("int") != null)
            {
                return this.Declaration();
            }
            else
            {
                node = this.Expr();
            }

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
            else if (this.lexer.Consume("&"))
            {
                return new Node(NodeKind.Address, this.Unary(), null);
            }
            else if (this.lexer.Consume("*"))
            {
                return new Node(NodeKind.Dereference, this.Unary(), null);
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
                Node node = null;

                if (this.lexer.Consume("("))
                {
                    node = new Node(token.StrValue);
                    if (this.lexer.Consume(")"))
                    {
                        return node;
                    }

                    var head = this.Assign();
                    var cur = head;

                    while (this.lexer.Consume(","))
                    {
                        cur.Next = this.Assign();
                        cur = cur.Next;
                    }
                    this.lexer.Expect(")");
                    node.Args = head;

                    return node;
                }


                var local = this.Locals == null ? null : this.Locals.Find(obj => obj.Name == token.StrValue);

                if (local == null)
                {
                    // local = this.ParameterDeclaration(token.StrValue);
                    throw new System.Exception();
                }
                node = new Node(local);

                return node;
            }

            return new Node(this.lexer.ExpectNumber());
        }

    }
}
