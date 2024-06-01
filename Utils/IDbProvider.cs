using MyDotNetAPP.Utils;
namespace MyDotNetAPP.Utils
{
    public interface IDbProvider
    {
        public Task<IDb> GetDb(String? connectionString = null);
    }
}
