using System.Text;

namespace CCCS
{
    public class Token
    {
        public TokenKind Kind { get; private set; }
        public Token Next { get; private set; }
        public int IntValue { get; internal set; }
        public char StrValue { get; internal set; }

        private Token(TokenKind kind, char str)
        {
            this.Kind = kind;
            this.StrValue = str;
        }

        public bool IsEOF() => this.Kind == TokenKind.EOF;

        public static Token NewToken()
        {
            return new Token(TokenKind.NOP, '\0');
        }

        public static Token NewToken(TokenKind kind, Token current, char str)
        {
            var token = new Token(kind, str);
            current.Next = token;

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

