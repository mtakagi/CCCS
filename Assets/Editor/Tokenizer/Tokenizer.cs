﻿namespace CCCS
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
                switch (c)
                {
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        continue;
                    case '!':
                        var c2 = str[i + 1];
                        if (c2 == '=' || c2 == '>' || c2 == '<')
                        {
                            current = Token.NewToken(TokenKind.Reserved, current, $"{c}{c2}");
                            i++;
                            continue;
                        }
                        else
                        {
                            throw new System.Exception();
                        }
                    case '<':
                    case '>':
                    case '=':
                        var c3 = str[i + 1];
                        if (c3 == '=' || c3 == '>' || c3 == '<')
                        {
                            current = Token.NewToken(TokenKind.Reserved, current, $"{c}{c3}");
                            i++;
                            continue;
                        }
                        else
                        {
                            goto case '+';
                        }
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '(':
                    case ')':
                    case ';':
                        current = Token.NewToken(TokenKind.Reserved, current, c.ToString());
                        continue;
                    case char letter when this.IsLetter(c):
                        current = Token.NewToken(TokenKind.Identifier, current, this.Name(ref i));
                        continue;
                    case char digit when char.IsDigit(digit):
                        current = this.Digit(current, ref i);
                        continue;
                }
            }
            Token.NewToken(TokenKind.EOF, current, "\0");

            return head.Next;
        }

        private bool IsLetter(char c)
        {
            return ('a' <= c && c <= 'z') || c == '_';
        }

        private string Name(ref int i)
        {
            var sb = new System.Text.StringBuilder();
            var c = this.str[i];
            do
            {
                sb.Append(c);
                i++;
                c = this.str[i];
            } while (this.IsLetter(c));

            i--;

            return sb.ToString();
        }

        private Token Digit(Token current, ref int i)
        {
            var start = i;
            var val = Tokenizer.StrtoL(str, ref i);
            var s = str.Substring(start, i - start);
            i--;
            var token = Token.NewToken(TokenKind.Number, current, s);
            token.IntValue = val;

            return token;
        }
    }
}

