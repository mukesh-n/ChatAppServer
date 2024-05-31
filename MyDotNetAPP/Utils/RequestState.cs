using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using MyDotNetAPP.Models;
using MyDotNetAPP.Services;

namespace MyDotNetAPP.Utils
{
    public class RequestState
    {
        private readonly IHttpContextAccessor _accessor;
        private ApplicationEnvironment _applicationEnvironment;
        private String _dbConnectionString;
        ILogger<RequestState> _logger;
        public RequestState(IHttpContextAccessor accessor, IOptions<ApplicationEnvironment> applicationEnvironment, ILogger<RequestState> logger)
        {
            _accessor = accessor;
            _applicationEnvironment = applicationEnvironment.Value;
            _logger = logger;
        }
        public async Task<String> GetDBConnectionString()
        {

            _dbConnectionString = _applicationEnvironment.postgresqlconnection;

            return _dbConnectionString;


        }


        public UserLoginRes _usercontext { get; set; }
        public UserLoginRes usercontext
        {
            get
            {
                if (_usercontext == null)
                {
                    UserLoginRes? result = null;
                    try
                    {
                        result = (UserLoginRes)_accessor.HttpContext.Items["User"];
                    }
                    catch (Exception e)
                    {
                        _logger.LogInformation(e.Message);
                        _logger.LogInformation(e.StackTrace);
                    }
                    if (result != null)
                    {
                        _usercontext = result;

                    }
                    else
                    {
                        _usercontext = new UserLoginRes { id = -1 };
                    }
                }
                return _usercontext;

            }
        }



    }
}
