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

                if (c == '=' || c == '!' || c == '>' || c == '<')
                {
                    var c2 = str[i + 1];
                    if (c2 == '=' || c2 == '>' || c2 == '<')
                    {
                        current = Token.NewToken(TokenKind.Reserved, current, $"{c}{c2}");
                        i++;
                        continue;
                    }
                }

                if (c == '+' || c == '-' || c == '*' || c == '/' || c == '(' || c == ')' || c == '<' || c == '>' || c == ';' || c == '=')
                {
                    current = Token.NewToken(TokenKind.Reserved, current, c.ToString());
                    continue;
                }

                if ('a' <= c && c <= 'z')
                {
                    current = Token.NewToken(TokenKind.Identifier, current, c.ToString());
                    continue;
                }

                if (char.IsDigit(c))
                {
                    var start = i;
                    var val = Tokenizer.StrtoL(str, ref i);
                    var s = str.Substring(start, i - start);
                    i--;
                    current = Token.NewToken(TokenKind.Number, current, c.ToString());
                    current.IntValue = val;
                }
            }

            Token.NewToken(TokenKind.EOF, current, "\0");

            return head.Next;
        }
    }
}

