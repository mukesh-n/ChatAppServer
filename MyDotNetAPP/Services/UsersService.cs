using MyDotNetAPP.Models;
using System.Data.Common;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyDotNetAPP.Utils;
using System.Data;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace MyDotNetAPP.Services
{
    public class UsersService
    {
        private readonly IDbConnection _dbConnection;
        IDbProvider dbprovider;
        IQueryBuilderProvider querybuilderprovider;
        ApplicationEnvironment applicationenvironment;
        RequestState requeststate;




        public UsersService(IDbProvider dbprovider, IDbConnection dbConnection, IQueryBuilderProvider querybuilderprovider, IOptions<ApplicationEnvironment> applicationenvironment, RequestState requeststate)
        {
            this.dbprovider = dbprovider;
            this.querybuilderprovider = querybuilderprovider;
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            this.applicationenvironment = applicationenvironment.Value;
            this.requeststate = requeststate;

        }

        public async Task<List<Users>> Get()
        {
            List<Users> result = new List<Users>();
            try
            {
                _dbConnection.Open();
                result = await GetTranscation();
                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_dbConnection.State == ConnectionState.Open)
                {
                    _dbConnection.Close();
                }
            }
        }


        public async Task<List<Users>> GetTranscation()
        {

            string query = @"SELECT id, username, mobilenumber,status, createdat FROM Users";
            List<Users> result = new List<Users>();

            using (var command = new NpgsqlCommand(query, (NpgsqlConnection)_dbConnection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32("id");
                        var username = reader.GetString(0);
                        var mobilenumber = reader.GetString(1);
                        var passwordhash = reader.GetString(2);
                        var status = reader.GetString(3);
                        var createdat = reader.GetDateTime(4);

                        var user = new Users
                        {
                            id = id,
                            username = username,
                            mobilenumber = mobilenumber,
                            passwordhash = passwordhash,
                            status = status,
                            createdat = createdat,
                        };

                        result.Add(user);
                    }

                    return result;
                }
            }
        }


        public async Task<List<Users>> Select(UserSelectReq req)
        {
            List<Users> result = null;
            using (IDb db = await dbprovider.GetDb())
            {
                await db.Connect();
                req.id = requeststate.usercontext.id;
                req.mobilenumber = requeststate.usercontext.mobilenumber;
                result = await this.SelectTransaction(db, req);
            }
            return result;
        }
        public async Task<List<Users>> SelectTransaction(IDb db, UserSelectReq req)
        {
            List<Users> result = new List<Users>();
            string query = @"
               SELECT id, username, mobilenumber,status, createdat, isactive,passwordhash, passwordsalt FROM Users
                ";
            var queryBuilder = querybuilderprovider.GetQueryBuilder(query);
            if (req.id > 0)
            {
                queryBuilder.AddParameter("Users.id", "=", "id", req.id, DbTypes.Types.Long);
            }
            if (!string.IsNullOrEmpty(req.mobilenumber))
            {

                queryBuilder.AddParameter("Users.mobilenumber", "=", "mobilenumber", req.mobilenumber, DbTypes.Types.String);
            }

            queryBuilder.AddOrderBy(QueryBuilder.Order.ASC, "Users.id");
            var command = queryBuilder.GetCommand(db);
            using (DbDataReader reader = await db.Execute(command))
            {
                while (await reader.ReadAsync())
                {
                    Users temp = new Users();
                    temp.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    temp.createdat = reader["createdat"] == DBNull.Value ? Base.GetMinimumDate() : Convert.ToDateTime(reader["createdat"]);
                    temp.username = reader["username"] == DBNull.Value ? "" : reader["username"].ToString();
                    temp.passwordhash = reader["passwordhash"] == DBNull.Value ? "" : reader["passwordhash"].ToString();
                    temp.passwordsalt = reader["passwordsalt"] == DBNull.Value ? "" : reader["passwordsalt"].ToString();

                    temp.mobilenumber = reader["mobilenumber"] == DBNull.Value ? "" : reader["mobilenumber"].ToString();
                    temp.status = reader["status"] == DBNull.Value ? "" : reader["status"].ToString();
                    temp.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);

                    result.Add(temp);
                }
            }
            return result;
        }



        public async Task<Users> Insert(Users user)
        {
            using (IDb db = await dbprovider.GetDb())
            {
                await db.Connect();
                await this.InsertTransaction(db, user);
            }
            return user;
        }
        public async Task InsertTransaction(IDb db, Users user)
        {
            String query = @"
                INSERT INTO Users (
                    username,mobilenumber,passwordhash,status,createdat,isactive
                )
                VALUES (
                @username, @mobilenumber, @passwordhash, @status, @createdat, @isactive
                )
                RETURNING id;
                ";
            user.isactive = true;
            user.createdat = DateTime.Now;


            DbCommand command = db.GetCommand(query);

            db.AddParameter(command, "username", DbTypes.Types.String).Value = user.username;
            db.AddParameter(command, "mobilenumber", DbTypes.Types.String).Value = user.mobilenumber;
            db.AddParameter(command, "passwordhash", DbTypes.Types.String).Value = user.passwordhash;
            db.AddParameter(command, "status", DbTypes.Types.String).Value = user.status;
            db.AddParameter(command, "createdat", DbTypes.Types.DateTime).Value = user.createdat;
            db.AddParameter(command, "isactive", DbTypes.Types.Boolean).Value = user.isactive;

            using (DbDataReader reader = await db.Execute(command))
            {
                if (await reader.ReadAsync())
                {
                    user.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                }
            }
        }




        public async Task<bool> Update(UserUpdateReq user)
        {
            bool result = false;

            using (IDb db = await dbprovider.GetDb())
            {
                await db.Connect();
                result = await this.UpdateTransaction(db, user);
            }
            return result;
        }
        public async Task<bool> UpdateTransaction(IDb db, UserUpdateReq user)
        {
            bool result = false;
            String query = @"
                UPDATE Users
                    SET username = @username, status = @status
                       
                ";

            var queryBuilder = querybuilderprovider.GetQueryBuilder(query);

            queryBuilder.AddParameter("id", "=", "id", user.id, DbTypes.Types.Long);
            var command = queryBuilder.GetCommand(db);
            db.AddParameter(command, "id", DbTypes.Types.Long).Value = user.id;
            db.AddParameter(command, "username", DbTypes.Types.String).Value = user.username;
            db.AddParameter(command, "status", DbTypes.Types.String).Value = user.status;

            if (await db.ExecuteNonQuery(command) > 0)
            {
                result = true;
            }
            return result;
        }



        public async Task<UserLoginRes> Login(UserLoginReq req)
        {
            var result = new UserLoginRes();
            var user = (await Select(new UserSelectReq
            {
                mobilenumber = req.mobilenumber,
            })).FirstOrDefault();
            if (user == null)
            {
                throw new AppException(AppException.ErrorCodes.UserNotFound);
            }
            var isverified = VerifyPassword(req.password, user.passwordhash, Convert.FromBase64String(user.passwordsalt));
            if (!isverified)
            {
                throw new AppException(AppException.ErrorCodes.InvalidCredential);
            }
            result.id = user.id;
            result.mobilenumber = user.mobilenumber;
            result.accesstoken = GenerateJwtToken(new UserGenerateJwtTokenReq
            {
                id = result.id,
                mobilenumber = result.mobilenumber
            }); ;
            return result;
        }
        private String GenerateJwtToken(UserGenerateJwtTokenReq req)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(applicationenvironment.jwtsecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim("id", req.id.ToString()),
                    new Claim("mobilenumber", req.mobilenumber.ToString()),

                }),
                Expires = DateTime.UtcNow.AddDays(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        static bool VerifyPassword(string password, string hashedPassword, byte[] salt)
        {
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            byte[] saltFromHash = new byte[16];
            Array.Copy(hashBytes, 0, saltFromHash, 0, 16);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltFromHash, 10000))
            {
                byte[] hash = pbkdf2.GetBytes(20);
                for (int i = 0; i < 20; i++)
                {
                    if (hashBytes[i + 16] != hash[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }


        public async Task Signup(UserSignUpReq req)
        {
            using (IDb db = await dbprovider.GetDb())
            {
                await db.Connect();
                await this.SignupTransaction(db, req);
            }
        }
        public async Task<bool> SignupTransaction(IDb db, UserSignUpReq req)
        {
            bool result = false;
            byte[] passwordsalt = GenerateSalt();
            string base64passwordhash = HashPassword(req.password, passwordsalt);
            string base64passwordsalt = Convert.ToBase64String(passwordsalt);

            String query = @"
                INSERT INTO Users (
                    username,mobilenumber,passwordhash,passwordsalt,status,createdat,isactive
                )
                VALUES (
                @username, @mobilenumber, @passwordhash,@passwordsalt, @status, @createdat, @isactive
                )
                RETURNING id;
                ";


            DbCommand command = db.GetCommand(query);

            db.AddParameter(command, "username", DbTypes.Types.String).Value = "";
            db.AddParameter(command, "mobilenumber", DbTypes.Types.String).Value = req.mobilenumber;
            db.AddParameter(command, "passwordhash", DbTypes.Types.String).Value = base64passwordhash;
            db.AddParameter(command, "passwordsalt", DbTypes.Types.String).Value = base64passwordsalt;
            db.AddParameter(command, "status", DbTypes.Types.String).Value = "";
            db.AddParameter(command, "createdat", DbTypes.Types.DateTime).Value = DateTime.Now;
            db.AddParameter(command, "isactive", DbTypes.Types.Boolean).Value = true;

            using (DbDataReader reader = await db.Execute(command))
            {
                if (await reader.ReadAsync())
                {
                    req.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                }
            }
            return result;
        }

        static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
        static string HashPassword(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                byte[] hash = pbkdf2.GetBytes(20); // 20 is the size of the hash
                byte[] hashBytes = new byte[36];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 20);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
