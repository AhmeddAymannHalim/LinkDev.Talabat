using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LinkDev.Talabat.APIs.Controllers.Errors
{
    public class ApiExceptionResponse : ApiResponse
    {
        public string? Details { get; set; }

        public ApiExceptionResponse
            (int statuscode,
            string? message = null,
            string? details = null)
            : base(statuscode, message)
        {
            Details = details;
        }
        public override string ToString() => JsonSerializer.Serialize(this, new JsonSerializerOptions(){ PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    }
}
