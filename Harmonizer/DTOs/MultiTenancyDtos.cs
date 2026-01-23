namespace Harmonizer.DTOs;

// Tenant DTOs
public class TenantDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int NamespaceCount { get; set; }
}

public class TenantCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
}

// Namespace DTOs
public class NamespaceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public int TeamCount { get; set; }
}

public class NamespaceCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public int TenantId { get; set; }
}

// Team DTOs
public class TeamDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int NamespaceId { get; set; }
    public string NamespaceName { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public int WorkflowCount { get; set; }
    
    // Full path for display
    public string FullPath => $"{TenantName} / {NamespaceName} / {Name}";
}

public class TeamCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public int NamespaceId { get; set; }
}

// Hierarchy response for dropdowns
public class TenantHierarchyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public List<NamespaceHierarchyDto> Namespaces { get; set; } = new();
}

public class NamespaceHierarchyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public List<TeamHierarchyDto> Teams { get; set; } = new();
}

public class TeamHierarchyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}
