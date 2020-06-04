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
    }
}