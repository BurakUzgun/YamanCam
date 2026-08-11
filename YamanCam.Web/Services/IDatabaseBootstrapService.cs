namespace YamanCam.Web.Services;

public interface IDatabaseBootstrapService
{
    Task EnsureInitializedIfNeededAsync(CancellationToken cancellationToken = default);
}
