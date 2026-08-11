namespace YamanCam.Web.Services;

public interface IAppLogService
{
    Task WriteInfoAsync(
        string category,
        string eventName,
        string message,
        int? statusCode = null,
        long? durationMs = null,
        string? oldValue = null,
        string? newValue = null,
        CancellationToken cancellationToken = default);

    Task WriteErrorAsync(
        string category,
        string eventName,
        string message,
        int? statusCode = null,
        long? durationMs = null,
        string? oldValue = null,
        string? newValue = null,
        CancellationToken cancellationToken = default);
}
