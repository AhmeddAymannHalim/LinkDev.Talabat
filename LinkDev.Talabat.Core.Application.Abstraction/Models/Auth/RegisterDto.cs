using System.ComponentModel.DataAnnotations;

namespace LinkDev.Talabat.Core.Application.Abstraction.Models.Auth
{
    public class RegisterDto
    {
        [Required] 
        public required string DisplayName { get; set; }

        [Required]
        public required string UserName{ get; set; }

        [Required]
        [EmailAddress]
        public required string Email{ get; set; }


        [Required] 
        public required string Phone{ get; set; }

        [Required]      
        [RegularExpression("(?=^.{6,10}$)(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#%^&()_+}{\":;'?>.<,])(?!.*\\s).*$",
        ErrorMessage = "Password must have 1 UpperCase, 1 LowerCase, 1 number , 1 symbol, be 6 to 10 characters long and contain no spaces.")]
        public required string Password { get; set; }



    }
}
