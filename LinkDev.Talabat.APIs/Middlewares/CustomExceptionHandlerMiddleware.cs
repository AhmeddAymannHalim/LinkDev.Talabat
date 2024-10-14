using Azure;
using LinkDev.Talabat.APIs.Controllers.Errors;
using LinkDev.Talabat.APIs.Controllers.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace LinkDev.Talabat.APIs.Middlewares
{

    //Convension Middle Ware : Must Class End WithMiddleware
    public class CustomExceptionHandlerMiddleware 
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public CustomExceptionHandlerMiddleware(RequestDelegate next, ILogger<CustomExceptionHandlerMiddleware> logger,IWebHostEnvironment env) // For next()
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                //Logix Executed With The Request 
                await _next(httpContext);
                //Logix Executed With The Response 

            }
            catch (Exception ex)
            {
                        ApiResponse response;
                switch (ex)
                {
                         case NotFoundException:
                        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        httpContext.Response.ContentType = "application/json";
                        response = new ApiResponse(404, ex.Message);
                        await httpContext.Response.WriteAsync(response.ToString());

                        break;
                    default:
                        if (_env.IsDevelopment())

                        {
                            //Development Mode
                            _logger.LogError(ex, ex.Message);
                            response = new ApiExceptionResponse((int)HttpStatusCode.InternalServerError, ex.Message, ex.StackTrace?.ToString());

                        }
                        else
                        {

                            //Production Mode
                            //Log Exception in Database || File (text,json)
                            response = new ApiExceptionResponse((int)HttpStatusCode.InternalServerError);


                        }

                        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        httpContext.Response.ContentType = "application/json";

                        await httpContext.Response.WriteAsync(response.ToString());
                        break;
                }

            }
        }
    }
}
