using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;

namespace GPMS.Application.Services;

public class SubmissionAccessService : ISubmissionAccessService
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IReviewerAssignmentRepository _assignmentRepository;
    private readonly System.Net.Http.IHttpClientFactory _httpClientFactory;

    public SubmissionAccessService(
        ISubmissionRepository submissionRepository,
        IReviewerAssignmentRepository assignmentRepository,
        System.Net.Http.IHttpClientFactory httpClientFactory)
    {
        _submissionRepository = submissionRepository;
        _assignmentRepository = assignmentRepository;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<(byte[] content, string fileName, string contentType)?> GetSubmissionFileAsync(int submissionId)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId);
        if (submission == null || string.IsNullOrEmpty(submission.FileUrl))
        {
            return null;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(submission.FileUrl);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

            return (content, submission.FileName, contentType);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> CanUserAccessSubmissionAsync(string userId, int submissionId, string role)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId);
        if (submission == null)
        {
            return false;
        }

        if (role == "HeadOfDept")
        {
            return true;
        }

        if (role == "Student")
        {
            return submission.Group.GroupMembers.Any(m => m.UserID == userId);
        }

        if (role == "Lecturer")
        {
            if (submission.Group.Project.ProjectSupervisors.Any(ps => ps.LecturerID == userId))
            {
                return true;
            }

            var assignments = await _assignmentRepository.GetByReviewerAsync(userId);
            return assignments.Any(a => a.GroupID == submission.GroupID && a.ReviewRoundID == submission.Requirement.ReviewRoundID);
        }

        return false;
    }
}
