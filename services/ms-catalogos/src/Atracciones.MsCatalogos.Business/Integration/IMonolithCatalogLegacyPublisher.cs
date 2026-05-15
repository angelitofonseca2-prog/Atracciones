namespace Atracciones.MsCatalogos.Business.Integration;

public interface IMonolithCatalogLegacyPublisher
{
    Task PublishAsync(CatalogMirrorBatch batch, CancellationToken cancellationToken = default);
}
