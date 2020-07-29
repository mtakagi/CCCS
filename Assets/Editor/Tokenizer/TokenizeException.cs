namespace CCCS
{
    public class TokenizeException : System.Exception
    {

        private string mMessage;

        public override string Message
        {
            get
            {
                return mMessage;
            }
        }

        public TokenizeException(string filename, int lineNo, int index, char token)
        {
            this.mMessage = $"{filename}:{lineNo}:{index}: Invalid token \"{token}\" found.";
        }

        public TokenizeException(int lineNo, int index, char token)
        {
            this.mMessage = $"{lineNo}:{index}: Invalid token \"{token}\" found.";
        }

        public TokenizeException(string filename, int lineNo, int index, string line, char token)
        {
            var position = "^".PadLeft(index);
            this.mMessage = $"{filename}:{lineNo}:{index}: Invalid token \"{token}\" found.\n{line}\n{position}";
        }


        public TokenizeException(int lineNo, int index, string line, char token)
        {
            var position = "^".PadLeft(index);
            this.mMessage = $"{lineNo}:{index}: Invalid token \"{token}\" found.\n{line}\n{position}";
        }
    }

}
