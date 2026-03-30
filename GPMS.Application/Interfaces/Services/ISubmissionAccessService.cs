namespace GPMS.Application.Interfaces.Services;

public interface ISubmissionAccessService
{
    Task<(byte[] content, string fileName, string contentType)?> GetSubmissionFileAsync(int submissionId);
    Task<bool> CanUserAccessSubmissionAsync(string userId, int submissionId, string role);
}
