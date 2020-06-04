namespace CCCS
{
    public class Tokenizer
    {
        private string str;

        public Tokenizer(string str)
        {
            this.str = str;
        }

        private static int StrtoL(string str, ref int index)
        {
            var start = index;
            for (; index < str.Length; index++)
            {
                var c = str[index];

                if (!char.IsDigit(c))
                {
                    return int.Parse(str.Substring(start, index - start));
                }
            }

            return int.Parse(str.Substring(start, index - start));
        }

        public Token Tokenize()
        {
            var head = Token.NewToken();
            var current = head;

            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                if (c == '+' || c == '-' || c == '*' || c == '/' || c == '(' || c == ')')
                {
                    current = Token.NewToken(TokenKind.Reserved, current, c);
                    continue;
                }

                if (char.IsDigit(c))
                {
                    var val = Tokenizer.StrtoL(str, ref i);
                    i--;
                    current = Token.NewToken(TokenKind.Number, current, c);
                    current.IntValue = val;
                }
            }

            Token.NewToken(TokenKind.EOF, current, '\0');

            return head.Next;
        }
    }
}

