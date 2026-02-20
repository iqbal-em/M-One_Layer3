using M_One_Layer3.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace M_One_Layer3.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        [HttpPost("test-person")]
        public async Task<IActionResult> TestInsert([FromServices] AppDbContext db)
        {
            var person = new Person
            {
                Id = Guid.NewGuid(),
                FullName = "Test User",
                Nationality = "IDN",
                PassportNumber = "A1234567",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            db.Persons.Add(person);
            await db.SaveChangesAsync();

            return Ok(person);
        }
    }
}
