namespace Company.Domain.Common.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an entity is not found.
/// </summary>
public class EntityNotFoundException : DomainException
{
    /// <summary>
    /// Gets the type of entity that was not found.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Gets the identifier that was used to search for the entity.
    /// </summary>
    public string EntityId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
    /// </summary>
    /// <param name="entityType">The type of entity that was not found.</param>
    /// <param name="entityId">The identifier that was used to search for the entity.</param>
    public EntityNotFoundException(string entityType, string entityId)
        : base("NotFound", $"{entityType} with identifier '{entityId}' was not found")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class with a <see cref="Guid"/> identifier.
    /// </summary>
    /// <param name="entityType">The type of entity that was not found.</param>
    /// <param name="entityId">The <see cref="Guid"/> identifier that was used to search for the entity.</param>
    public EntityNotFoundException(string entityType, Guid entityId)
        : this(entityType, entityId.ToString())
    {
    }
}