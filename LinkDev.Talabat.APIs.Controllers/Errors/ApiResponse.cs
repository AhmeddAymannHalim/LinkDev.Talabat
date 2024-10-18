using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LinkDev.Talabat.APIs.Controllers.Errors
{
    public class ApiResponse
    {
        public int StatusCode { get; set; }

        public string? Message { get; set; }

        public ApiResponse(int statuscode,string? message = null)
        {
            StatusCode = statuscode;
            Message = message ?? GetDefaultMessageForStatusCode(statuscode);
        }

        private string? GetDefaultMessageForStatusCode(int statuscode)
        {
           

            return statuscode switch
            {

                400 => "Badrequest, you have made",
                401 => "UnAuthorized",
                404 => "Resource is not found ",
                500 => "Errors are the path to the dark side. Errors lead to anger. Anger leads to hate. Hate leads to career change",
                _ => null
            };
        }

        public override string ToString() => JsonSerializer.Serialize(this , new JsonSerializerOptions() { PropertyNamingPolicy=JsonNamingPolicy.CamelCase});
    }
}
