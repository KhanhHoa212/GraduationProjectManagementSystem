using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPMS.Web.Controllers;

[Authorize]
public class LecturerController : Controller
{
    public IActionResult Dashboard() => View();
    
    public IActionResult Projects() => View();
    
    public IActionResult ProjectGroupDetail(string id = "GRP-01") 
    {
        ViewBag.GroupId = id;
        return View();
    }
    
    public IActionResult FeedbackApprovals() => View();
    
    public IActionResult FeedbackApprovalDetail(string id = "FB-100") 
    {
        ViewBag.FeedbackId = id;
        return View();
    }
    
    public IActionResult ReviewAssignments() => View();
    
    public IActionResult EvaluationForm(string id = "EV-200") 
    {
        ViewBag.EvaluationId = id;
        return View();
    }
}
