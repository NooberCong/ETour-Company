using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Company.Controllers
{
    [AllowAnonymous]
    public class StatusController: Controller
    {
        public IActionResult Index(string code)
        {
            return View($"Views/Status/{code}.cshtml");
        }
    }
}
