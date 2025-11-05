namespace DiffSpectrumView.Services
{
    public interface IApiComparisonService
    {
        Task ProcessRequestAsync(string sourceRequest, string endpoint, string method);
    }
}
