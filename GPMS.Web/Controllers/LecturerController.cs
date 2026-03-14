using GPMS.Application.DTOs.Lecturer;
using GPMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GPMS.Web.Controllers;

[Authorize]
public class LecturerController : Controller
{
    private readonly ILecturerService _lecturerService;

    public LecturerController(ILecturerService lecturerService)
    {
        _lecturerService = lecturerService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    public async Task<IActionResult> Dashboard()
    {
        var model = await _lecturerService.GetDashboardDataAsync(GetUserId());
        return View(model);
    }

    public async Task<IActionResult> Projects()
    {
        var model = await _lecturerService.GetMentoredProjectsAsync(GetUserId());
        return View(model);
    }

    public async Task<IActionResult> ProjectGroupDetail(int id)
    {
        var model = await _lecturerService.GetProjectGroupDetailAsync(id);
        return View(model);
    }

    public async Task<IActionResult> FeedbackApprovals()
    {
        var model = await _lecturerService.GetPendingApprovalsAsync(GetUserId());
        return View(model);
    }

    public async Task<IActionResult> FeedbackApprovalDetail(int id)
    {
        var model = await _lecturerService.GetFeedbackApprovalDetailAsync(id);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveFeedback(int feedbackId, string decision, string? comments)
    {
        await _lecturerService.ApproveFeedbackAsync(feedbackId, decision, comments ?? string.Empty);
        TempData["SuccessMessage"] = decision == "Reject"
            ? "Feedback has been rejected and returned to the reviewer."
            : "Feedback approved and released to students.";
        return RedirectToAction(nameof(FeedbackApprovals));
    }

    public async Task<IActionResult> ReviewAssignments()
    {
        var model = await _lecturerService.GetReviewAssignmentsAsync(GetUserId());
        return View(model);
    }

    public async Task<IActionResult> EvaluationForm(int id)
    {
        var model = await _lecturerService.GetEvaluationFormAsync(id);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitEvaluation(LecturerEvaluationFormViewModel model)
    {
        var success = await _lecturerService.SubmitEvaluationAsync(model);
        if (success)
        {
            TempData["SuccessMessage"] = "Evaluation submitted successfully!";
            return RedirectToAction(nameof(ReviewAssignments));
        }
        TempData["ErrorMessage"] = "Failed to submit evaluation. Please try again.";
        return View("EvaluationForm", model);
    }
}
