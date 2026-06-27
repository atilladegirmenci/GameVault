using Microsoft.AspNetCore.Mvc;

namespace GameDBWebApi.Controllers
{
    public class GameController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
