using System.ComponentModel.DataAnnotations;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Auth;

namespace LinkDev.Talabat.Tests
{
    public class RegisterDtoValidationTests
    {
        private static bool IsValid(string password)
        {
            var dto = new RegisterDto
            {
                DisplayName = "New", UserName = "new", Email = "new@test.com", Phone = "01000000000", Password = password,
            };
            return Validator.TryValidateObject(dto, new ValidationContext(dto), new List<ValidationResult>(), validateAllProperties: true);
        }

        [Theory]
        [InlineData("P@ssw0rd!")]
        [InlineData("Abc12&")]
        [InlineData("Abc12\"")]
        [InlineData("Abc12>")]
        [InlineData("Abc12<")]
        public void Password_WithUpperLowerDigitAndSymbol_IsValid(string password)
        {
            Assert.True(IsValid(password));
        }

        [Theory]
        [InlineData("Passw0rda")]   // letters only from the old "&amp;" / "&quot;" entities used to count as symbols
        [InlineData("Passw0rdq")]
        [InlineData("Passw0rdt")]
        [InlineData("p@ssw0rd")]    // no upper-case letter
        [InlineData("P@SSW0RD")]    // no lower-case letter
        [InlineData("P@ssword")]    // no digit
        [InlineData("P@1a")]        // shorter than 6
        [InlineData("P@ssw0rd!xy")] // longer than 10
        [InlineData("P@ss w0rd")]   // contains a space
        public void Password_BreakingTheRule_IsInvalid(string password)
        {
            Assert.False(IsValid(password));
        }
    }
}
