namespace CCCS
{
    public class Tokenizer
    {
        private string str;

        public Tokenizer(string str)
        {
            this.str = str;
        }

        public Token Tokenize()
        {
            using (var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(this.str)))
            using (var reader = new TokenReader(stream))
            {
                var head = reader.NextToken();
                var current = head;

                while (current.Kind != TokenKind.EOF)
                {
                    current = reader.NextToken();
                }
                return head;
            }
        }
    }
}

