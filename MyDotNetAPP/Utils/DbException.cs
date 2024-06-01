namespace MyDotNetAPP.Utils
{
    public class DbException : Exception
    {
        public Codes Code { get; set; }

        public DbException(Codes code) : base(code.ToString())
        {
            Code = code;
        }

        public enum Codes
        {
            ConnectionStringNotFound,
            ConnectionNotOpened,
            TransactionNotBegun

        }

    }
}
