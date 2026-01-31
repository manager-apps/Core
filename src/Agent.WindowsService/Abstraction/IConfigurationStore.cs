using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Abstraction;

public interface IConfigurationStore
{
    /// <summary>
    /// Retrieves the configuration from the store.
    /// </summary>
    Task<Configuration> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores or updates the configuration in the store.
    /// </summary>
    Task SaveAsync(Configuration configuration, CancellationToken cancellationToken = default);
}

