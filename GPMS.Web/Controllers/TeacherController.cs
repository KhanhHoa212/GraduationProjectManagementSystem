using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPMS.Web.Controllers;

[Authorize(Roles = "Lecturer")]
public class TeacherController : Controller
{
    public IActionResult Index() => View();
}
