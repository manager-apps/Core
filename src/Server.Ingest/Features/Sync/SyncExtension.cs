namespace Server.Ingest.Features.Sync;

public static class SyncExtension
{
  public static void AddSyncServices(this IServiceCollection services)
  {
    services.AddScoped<ISyncHandler, SyncHandler>();
  }

  public static void MapSyncEndpoints(this IEndpointRouteBuilder app)
  {
    app.MapSyncEndpoint();
  }
}
