using System.IO;

namespace CCCS
{
    public class TokenReader : System.IDisposable
    {
        private StreamReader mReader;
        private string mLine;
        private int mMaxLength;

        public int LineNumber { get; private set; }

        public int CurrentIndex { get; private set; }

        public Token CurrentToken { get; private set; }

        public string Identifier { get; private set; }

        public TokenReader(string path)
        {
            mReader = new StreamReader(path);
            this.ReadLine();
        }

        public TokenReader(Stream stream)
        {
            mReader = new StreamReader(stream);
            this.ReadLine();
        }

        private void ReadLine()
        {
            while ((this.mLine = this.mReader.ReadLine()) != null)
            {
                this.LineNumber++;

                if ((this.mMaxLength = this.mLine.Length) > 0)
                {
                    this.CurrentIndex = 0;
                    break;
                }
            }
        }

        private char ReadChar()
        {
            if (this.CurrentIndex < this.mMaxLength)
            {
                return this.mLine[this.CurrentIndex];
            }
            else
            {
                return ' ';
            }
        }

        public bool IsNoneDigit(char c)
        {
            return this.IsAscii(c) || (c == '_');
        }

        private bool IsAscii(char c)
        {
            return ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z');
        }

        private bool IsNumber(char c)
        {
            return ('0' <= c && c <= '9');
        }

        private bool IsAlphaNum(char c)
        {
            return this.IsNoneDigit(c) || this.IsNumber(c);
        }

        public Token NextToken()
        {
            if (this.mLine == null)
            {
                return Token.NewToken(TokenKind.EOF, this.CurrentToken, "\0");
            }

            char nextChar;

            do
            {
                if (this.CurrentIndex == this.mMaxLength)
                {
                    this.ReadLine();

                    if (this.mLine == null)
                    {
                        return Token.NewToken(TokenKind.EOF, this.CurrentToken, "\0");
                    }
                }

                nextChar = this.ReadChar();
                this.CurrentIndex++;
            } while (char.IsWhiteSpace(nextChar));

            switch (nextChar)
            {
                case '!':
                    if (this.ReadChar() == '=')
                    {
                        this.CurrentIndex++;
                        return (this.CurrentToken = Token.NewToken(TokenKind.Reserved, this.CurrentToken, "!="));
                    }
                    else
                    {
                        throw new System.Exception();
                    }
                case '<':
                case '>':
                case '=':
                    var prevChar = nextChar;
                    nextChar = this.ReadChar();
                    if (nextChar == '=' || nextChar == '>' || nextChar == '<')
                    {
                        this.CurrentIndex++;
                        return (this.CurrentToken = Token.NewToken(TokenKind.Reserved, this.CurrentToken, $"{prevChar}{nextChar}"));
                    }
                    else
                    {
                        nextChar = prevChar;
                        goto case '+';
                    }
                case '/':
                    if (this.ReadChar() == '/')
                    {
                        this.ReadLine();
                        return this.NextToken();
                    }
                    goto case '+';
                case '+':
                case '-':
                case '*':
                case '%':
                case '&':
                case '(':
                case ')':
                case '{':
                case '}':
                case '[':
                case ']':
                case '?':
                case ':':
                case ',':
                case ';':
                    return (this.CurrentToken = Token.NewToken(TokenKind.Reserved, this.CurrentToken, nextChar.ToString()));
                // TODO: Support control character
                case '"':
                    {
                        var sb = new System.Text.StringBuilder();
                        nextChar = this.ReadChar();
                        this.CurrentIndex++;

                        while (nextChar != '"')
                        {
                            sb.Append(nextChar);
                            nextChar = this.ReadChar();
                            this.CurrentIndex++;
                        }

                        sb.Append('\0');

                        return (this.CurrentToken = Token.NewToken(TokenKind.String, this.CurrentToken, sb.ToString()));
                    }
                // TODO: Support control character
                case '\'':
                    {
                        var c = this.ReadChar();
                        this.CurrentIndex++;
                        nextChar = this.ReadChar();
                        this.CurrentIndex++;

                        if (nextChar != '\'')
                        {
                            throw new TokenizeException(this.LineNumber, this.CurrentIndex, this.mLine, nextChar);
                        }

                        var token = Token.NewToken(TokenKind.Number, this.CurrentToken, c.ToString());
                        token.IntValue = c;
                        return (this.CurrentToken = token);
                    }
                case char letter when this.IsNoneDigit(letter):
                    var start = this.CurrentIndex - 1;

                    while (this.IsAlphaNum(this.ReadChar()))
                    {
                        this.CurrentIndex++;
                    }
                    // TODO: Support C99 keyword
                    this.Identifier = this.mLine.Substring(start, this.CurrentIndex - start);
                    switch (this.Identifier)
                    {
                        // storage specifier
                        case "auto":
                        case "register":
                        case "static":
                        case "extern":
                        case "typedef":
                        // type specifier
                        case "void":
                        case "char":
                        case "short":
                        case "int":
                        case "long":
                        case "float":
                        case "double":
                        case "signed":
                        case "unsigned":
                        // type qualifier
                        case "const":
                        case "volatile":
                        // struct & union
                        case "struct":
                        case "union":
                        // selection statement
                        case "if":
                        case "else":
                        case "switch":
                        // label statement
                        case "case":
                        case "default":
                        // iteration statement
                        case "while":
                        case "do":
                        case "for":
                        // Jump statement
                        case "goto":
                        case "break":
                        case "continue":
                        case "return":
                            return (this.CurrentToken = Token.NewToken(TokenKind.Reserved, this.CurrentToken, this.Identifier));
                        default:
                            return (this.CurrentToken = Token.NewToken(TokenKind.Identifier, this.CurrentToken, this.Identifier));
                    }
                // TODO: Support float etc
                case char digit when this.IsNumber(digit):
                    int value = 0;
                    for (value = int.Parse(nextChar.ToString()); this.IsNumber((nextChar = this.ReadChar())); this.CurrentIndex++)
                    {
                        value = value * 10 + int.Parse(nextChar.ToString());
                    }

                    this.CurrentToken = Token.NewToken(TokenKind.Number, this.CurrentToken, value.ToString());
                    this.CurrentToken.IntValue = value;

                    return this.CurrentToken;
                case char punc when char.IsPunctuation(punc):
                    return (this.CurrentToken = Token.NewToken(TokenKind.Reserved, this.CurrentToken, punc.ToString()));
                default:
                    throw new TokenizeException(this.LineNumber, this.CurrentIndex, this.mLine, nextChar);
            }
        }

        public void Dispose()
        {
            if (mReader != null)
            {
                mReader.Dispose();
            }
        }
    }
}