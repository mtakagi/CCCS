using System.Text;

namespace CCCS
{
    public class Token
    {
        public TokenKind Kind { get; private set; }
        public Token Next { get; private set; }
        public int IntValue { get; internal set; }
        public string StrValue { get; internal set; }

        public int Length { get; private set; }

        private Token(TokenKind kind, string str)
        {
            this.Kind = kind;
            this.StrValue = str;
            this.Length = str.Length;
        }

        public bool IsEOF() => this.Kind == TokenKind.EOF;

        public static Token NewToken(TokenKind kind, Token current, string str)
        {
            var token = new Token(kind, str);

            if (current != null)
            {
                current.Next = token;
            }

            return token;
        }

        override public string ToString()
        {
            var sb = new StringBuilder();

            for (var token = this; token.Next != null; token = token.Next)
            {
                sb.AppendFormat($"Kind: {token.Kind.ToString()}, Str:{token.StrValue}\n");
            }

            return sb.ToString();
        }
    }

}

