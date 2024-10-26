
namespace PolarisGateway.Services.Artefact;

// Would be ideal to be able to just say that CachingArtefactService is an IArtefactService, but
//  DI would struggle to differentiate between the two. Might be an elegant/standard way around this.
public interface ICachingArtefactService : IArtefactService { }