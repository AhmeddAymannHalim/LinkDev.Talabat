using Azure;
using LinkDev.Talabat.APIs.Controllers.Errors;
using LinkDev.Talabat.Core.Application;
using LinkDev.Talabat.Core.Application.Exceptions;
using System.Net;

namespace LinkDev.Talabat.APIs.Middlewares
{

    //Convension Middle Ware : Must Class End WithMiddleware
    public class ExceptionHandlerMiddleware 
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger,IWebHostEnvironment env) // For next()
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
                #region Logging : TODO

                if (_env.IsDevelopment())
                {
                    _logger.LogError(ex, ex.Message);

                }
                else
                {

                }  
                #endregion

                await HandleExceptionAsync(httpContext, ex);

            }
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
        {
                 ApiResponse response; 
            switch (ex)
            {
                case NotFoundException:
                    httpContext.Response.StatusCode = (int) HttpStatusCode.NotFound;
                    httpContext.Response.ContentType = "application/json";

                    response = new ApiResponse(404, ex.Message);

                    await httpContext.Response.WriteAsync(response.ToString());
                    break;

                case BadRequestException:
                    httpContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                    httpContext.Response.ContentType = "application/json";
                    response = new ApiResponse(400,ex.Message);

                    await httpContext.Response.WriteAsync(response.ToString());
                    break;

                case UnAuthorizedException:
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    httpContext.Response.ContentType = "application/json";
                    response = new ApiResponse(401, "Invalid Email Or Password ! Please Try agian ...");

                    await httpContext.Response.WriteAsync(response.ToString());
                    break;
                default:

                    response = _env.IsDevelopment() ? response = new ApiExceptionResponse((int)HttpStatusCode.InternalServerError, ex.Message, ex.StackTrace?.ToString())
                      :
                    response = new ApiExceptionResponse((int)HttpStatusCode.InternalServerError);


                    httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    httpContext.Response.ContentType = "application/json";
                    await httpContext.Response.WriteAsJsonAsync(response.ToString());
                    break;
            }
        }


    }
}
