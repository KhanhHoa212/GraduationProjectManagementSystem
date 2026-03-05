using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPMS.Web.Controllers;

[Authorize(Roles = "HeadOfDept,Admin")]
public class HODController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Projects() => View();
    public IActionResult ProjectDetails(string id) => View();
    public IActionResult Groups() => View();
    public IActionResult GroupDetails(string id) => View();
    public IActionResult AssignSupervisor() => View();
    public IActionResult Import() => View();
}
