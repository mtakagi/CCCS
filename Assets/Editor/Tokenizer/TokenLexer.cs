using System.Collections.Generic;

namespace CCCS
{
    public class TokenLexer
    {
        public Token token { get; private set; }

        public TokenLexer(Token token)
        {
            this.token = token;
        }

        public Token Peek(string s)
        {
            if (token.Kind != TokenKind.Reserved || s != token.StrValue
                    || s.Length != token.Length || s != token.StrValue)
            {
                return null;
            }

            return token;
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

    }
}