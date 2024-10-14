using LinkDev.Talabat.APIs.Controllers.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.APIs.Controllers.Controllers.Buggy
{
    public class BuggyController : ApiControllerBase
    {
        [HttpGet("notfound")]
        public IActionResult GetNotFoundRequest()
        {
            return NotFound(new {StatusCode = 404,Message = "NotFound Page!!"}); //404
        }
         [HttpGet("badrequest")]
            public IActionResult GetBadRequest()
        {
           return BadRequest(new { StatusCode = 400, Message = "BadRequest!!" }); //400
        }
        [HttpGet("servererror")]
        public IActionResult GetServerError()
        {
            throw new Exception(); //500
        }

        [HttpGet("badrequest/{id}")]
        public IActionResult GetValidationError(int id) // ValidationError From BadRequest !!
            // => 400 (return BadRequest())
        {
            return Ok(); //400
        }


        [HttpGet("unauthorized")]
        public IActionResult GetUnAuthorizedError()
        {
            return Unauthorized(new { StatusCode = 401, Message = "unathorized!!" }); 
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
