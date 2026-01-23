namespace Harmonizer.Models;

/// <summary>
/// Represents a tenant/organization in the multi-tenant system
/// </summary>
public class Tenant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty; // URL-friendly identifier
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Namespace> Namespaces { get; set; } = new List<Namespace>();
}

/// <summary>
/// Represents a namespace within a tenant (e.g., "production", "staging", "department-x")
/// </summary>
public class Namespace
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign key
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    // Navigation properties
    public ICollection<Team> Teams { get; set; } = new List<Team>();
}

/// <summary>
/// Represents a team within a namespace
/// </summary>
public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign key
    public int NamespaceId { get; set; }
    public Namespace Namespace { get; set; } = null!;
    
    // Navigation properties
    public ICollection<Workflow> Workflows { get; set; } = new List<Workflow>();
}
