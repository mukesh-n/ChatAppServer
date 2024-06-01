﻿using MyDotNetAPP.Utils;
using System.Net;
using System.Text.Json;

namespace MyDotNetAPP.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        ILogger _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                switch (error)
                {
                    case AppException e:
                        // custom application error
                        _logger.LogError("[ HANDLED EXCEPTION ] {1}", e.ToString());
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        await response.WriteAsync(error.ToString());
                        break;
                    default:
                        //// unhandled error
                        //response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        //await response.WriteAsync(JsonSerializer.Serialize(new { message = error?.Message }));
                        _logger.LogError("[ UNHANDLED EXCEPTION ] {1}", error.ToString());
                        throw error;
                        break;
                }
            }
        }
    }
}
