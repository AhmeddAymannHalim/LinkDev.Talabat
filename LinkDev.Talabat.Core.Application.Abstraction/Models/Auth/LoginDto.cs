using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Application.Abstraction.Models.Auth
{
    public class LoginDto
    {

        [Required]
        [EmailAddress] // Validation For The Property to be valid EmailAddress 
        //[DataType(DataType.EmailAddress)] // Ui Hinting For Display The EmailAddress
        public required string Email{ get; set; }

        [Required]
        public required string Password{ get; set; }
    }
}
