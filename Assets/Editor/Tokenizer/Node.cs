namespace CCCS
{
    public class Node
    {
        public NodeKind Kind { get; private set; }
        public Node Lhs { get; private set; }
        public Node Rhs { get; private set; }
        public int IntValue { get; private set; }

        public Node(NodeKind kind, Node lhs, Node rhs)
        {
            this.Kind = kind;
            this.Lhs = lhs;
            this.Rhs = rhs;
        }

        public Node(int value)
        {
            this.Kind = NodeKind.Nunber;
            this.IntValue = value;
        }

        override public string ToString()
        {
            if (this.Kind == NodeKind.Nunber)
            {
                return $"Kind: Number {this.IntValue}\n";
            }
            var sb = new System.Text.StringBuilder();

            if (this.Lhs != null)
            {
                sb.Append(this.Lhs.ToString());
            }

            if (this.Rhs != null)
            {
                sb.Append(this.Rhs.ToString());
            }

            switch (this.Kind)
            {
                case NodeKind.Add:
                    sb.Append("Kind: Add\n");
                    break;
                case NodeKind.Sub:
                    sb.Append("Kind: Sub\n");
                    break;
                case NodeKind.Mul:
                    sb.Append("Kind: Mul\n");
                    break;
                case NodeKind.Div:
                    sb.Append("Kind: Div\n");
                    break;
                case NodeKind.Equal:
                    sb.Append("Kind: Equal\n");
                    break;
                case NodeKind.NotEqual:
                    sb.Append("Kind: NotEqual\n");
                    break;
                case NodeKind.LesserThan:
                    sb.Append("Kind: LesserThan\n");
                    break;
                case NodeKind.LesserThanOrEqual:
                    sb.Append("Kind: LesserThanOrEqual\n");
                    break;
            }

            return sb.ToString();
        }
    }
}