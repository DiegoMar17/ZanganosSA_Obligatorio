using Microsoft.AspNetCore.Mvc;

namespace ColmenaEmpresa.Controllers
{
    public class CalendarioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
