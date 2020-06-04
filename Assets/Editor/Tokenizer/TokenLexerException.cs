using System;

public class TokenLexerException : Exception
{
    private int pos;
    private string str;

    public TokenLexerException(int pos, string str)
    {
        this.pos = pos;
        this.str = str;
    }
}
