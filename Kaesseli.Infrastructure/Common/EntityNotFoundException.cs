namespace Kaesseli.Infrastructure.Common;

public class EntityNotFoundException(Type entityType, Guid entityId) :
    Exception(message: $"Entity {entityType} with id {entityId} not found.");