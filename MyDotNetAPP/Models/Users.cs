namespace MyDotNetAPP.Models
{
    public class Users
    {
        public long id { get; set; }
        public string username { get; set; }
        public string mobilenumber { get; set; }
        public string passwordhash { get; set; }
        public string passwordsalt { get; set; }
        public string status { get; set; }
        public DateTime createdat { get; set; }
        public bool isactive { get; set; }
    }

    public class UserUpdateReq
    {
        public long id { get; set; }
        public string username { get; set; }
        public string status { get; set; }
    }

    public class UserSelectReq
    {
        public long id { get; set; }
        public string mobilenumber { get; set; }

    }

    public class UserLoginReq
    {
        public string mobilenumber { get; set; }
        public string password { get; set; }
    }

    public class UserLoginRes
    {
        public long id { get; set; }
        public string accesstoken { get; set; }
        public string mobilenumber { get; set; }

    }

    public class UserGenerateJwtTokenReq
    {
        public long id { get; set; }
        public string mobilenumber { get; set; }

    }

    public class UserSignUpReq
    {
        public long id { get; set; }
        public string mobilenumber { get; set; }
        public string password { get; set; }
    }
}
