using M_One_Layer3.Domain;
using Microsoft.AspNetCore.Mvc;
using M_One_Layer3.Infrastructure.Database;

namespace Prototype_API.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly AppDbContext _context;

        public RegistrationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(Person model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            _context.Persons.Add(model);
            _context.SaveChanges();

            // Redirect ke fingerprint enrollment
            return RedirectToAction("Enroll", "Fingerprint", new { personId = model.Id });
        }
    }
}