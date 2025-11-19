using Microsoft.AspNetCore.Mvc;

namespace APIConsultorio.Controllers
{
    public class MedicosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
