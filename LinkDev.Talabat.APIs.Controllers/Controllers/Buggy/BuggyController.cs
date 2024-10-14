using LinkDev.Talabat.APIs.Controllers.Exceptions;
using LinkDev.Talabat.APIs.Controllers.Controllers.Base;
using LinkDev.Talabat.APIs.Controllers.Errors;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LinkDev.Talabat.APIs.Controllers.Controllers.Buggy
{
    public class BuggyController : ApiControllerBase
    {
        [HttpGet("notfound")]
        public IActionResult GetNotFoundRequest()
        {
            //return NotFound(new ApiResponse(404)); //404
            throw new NotFoundException();
        }

         [HttpGet("badrequest")]
            public IActionResult GetBadRequest()
        {
           return BadRequest(new ApiResponse(400)); //400
        }

        [HttpGet("badrequest/{id}")] // GetNotFound Endpoint
        public IActionResult GetValidationError(int id) // ValidationError From BadRequest !!  => 400 (return BadRequest())
        {

            return Ok(); //400
        }


        [HttpGet("servererror")]
        public IActionResult GetServerError()
        {
            //var product = new ProductToReturnDto() { Name = string.Empty ,Description = string.Empty};

            //product = null;

            //var ProductName = product.Name;

            throw new Exception();
        }

      

        [HttpGet("unauthorized")]
        public IActionResult GetUnAuthorizedError()
        {
            return Unauthorized(new ApiResponse(401)); 
        }

        [HttpGet("forbidden")]
        public IActionResult GetUnForBiddenRequest()
        {
            return Forbid();
        }

        [Authorize]
        [HttpGet("authorized")]
        public IActionResult GetAuthorizedRequest()
        {
            return Ok();
        }
        
    }
}
