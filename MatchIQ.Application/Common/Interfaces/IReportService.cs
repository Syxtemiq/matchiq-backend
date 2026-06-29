namespace MatchIQ.Application.Common.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateCompanyReportAsync(int companyUserId);
    Task<byte[]> GenerateAdminReportAsync();
}
