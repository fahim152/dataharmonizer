using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Harmonizer.Data;
using Harmonizer.DTOs;
using Harmonizer.Models;
using System.Text.RegularExpressions;

namespace Harmonizer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly HarmonizerDbContext _context;

    public TenantsController(HarmonizerDbContext context)
    {
        _context = context;
    }

    // GET: api/Tenants
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TenantDto>>> GetAll()
    {
        var tenants = await _context.Tenants
            .Include(t => t.Namespaces)
            .OrderBy(t => t.Name)
            .Select(t => new TenantDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                Description = t.Description,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                NamespaceCount = t.Namespaces.Count
            })
            .ToListAsync();
        return Ok(tenants);
    }

    // GET: api/Tenants/hierarchy
    [HttpGet("hierarchy")]
    public async Task<ActionResult<IEnumerable<TenantHierarchyDto>>> GetHierarchy()
    {
        var tenants = await _context.Tenants
            .Where(t => t.IsActive)
            .Include(t => t.Namespaces.Where(n => n.IsActive))
                .ThenInclude(n => n.Teams.Where(team => team.IsActive))
            .OrderBy(t => t.Name)
            .Select(t => new TenantHierarchyDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                Namespaces = t.Namespaces.Select(n => new NamespaceHierarchyDto
                {
                    Id = n.Id,
                    Name = n.Name,
                    Slug = n.Slug,
                    Teams = n.Teams.Select(team => new TeamHierarchyDto
                    {
                        Id = team.Id,
                        Name = team.Name,
                        Slug = team.Slug
                    }).ToList()
                }).ToList()
            })
            .ToListAsync();
        return Ok(tenants);
    }

    // GET: api/Tenants/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TenantDto>> GetById(int id)
    {
        var tenant = await _context.Tenants
            .Include(t => t.Namespaces)
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (tenant == null) return NotFound();
        
        return Ok(new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Slug = tenant.Slug,
            Description = tenant.Description,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt,
            NamespaceCount = tenant.Namespaces.Count
        });
    }

    // POST: api/Tenants
    [HttpPost]
    public async Task<ActionResult<TenantDto>> Create([FromBody] TenantCreateDto dto)
    {
        var slug = dto.Slug ?? GenerateSlug(dto.Name);
        
        if (await _context.Tenants.AnyAsync(t => t.Slug == slug))
        {
            return BadRequest(new { error = "A tenant with this slug already exists" });
        }

        var tenant = new Tenant
        {
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = tenant.Id }, new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Slug = tenant.Slug,
            Description = tenant.Description,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt,
            NamespaceCount = 0
        });
    }

    // DELETE: api/Tenants/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null) return NotFound();

        _context.Tenants.Remove(tenant);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }
}

[ApiController]
[Route("api/[controller]")]
public class NamespacesController : ControllerBase
{
    private readonly HarmonizerDbContext _context;

    public NamespacesController(HarmonizerDbContext context)
    {
        _context = context;
    }

    // GET: api/Namespaces?tenantId=1
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NamespaceDto>>> GetAll([FromQuery] int? tenantId)
    {
        var query = _context.Namespaces
            .Include(n => n.Tenant)
            .Include(n => n.Teams)
            .AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(n => n.TenantId == tenantId.Value);
        }

        var namespaces = await query
            .OrderBy(n => n.Tenant.Name)
            .ThenBy(n => n.Name)
            .Select(n => new NamespaceDto
            {
                Id = n.Id,
                Name = n.Name,
                Slug = n.Slug,
                Description = n.Description,
                IsActive = n.IsActive,
                CreatedAt = n.CreatedAt,
                TenantId = n.TenantId,
                TenantName = n.Tenant.Name,
                TeamCount = n.Teams.Count
            })
            .ToListAsync();
        return Ok(namespaces);
    }

    // POST: api/Namespaces
    [HttpPost]
    public async Task<ActionResult<NamespaceDto>> Create([FromBody] NamespaceCreateDto dto)
    {
        var tenant = await _context.Tenants.FindAsync(dto.TenantId);
        if (tenant == null) return BadRequest(new { error = "Tenant not found" });

        var slug = dto.Slug ?? GenerateSlug(dto.Name);
        
        if (await _context.Namespaces.AnyAsync(n => n.TenantId == dto.TenantId && n.Slug == slug))
        {
            return BadRequest(new { error = "A namespace with this slug already exists in this tenant" });
        }

        var ns = new Namespace
        {
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            TenantId = dto.TenantId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Namespaces.Add(ns);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new NamespaceDto
        {
            Id = ns.Id,
            Name = ns.Name,
            Slug = ns.Slug,
            Description = ns.Description,
            IsActive = ns.IsActive,
            CreatedAt = ns.CreatedAt,
            TenantId = ns.TenantId,
            TenantName = tenant.Name,
            TeamCount = 0
        });
    }

    // DELETE: api/Namespaces/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ns = await _context.Namespaces.FindAsync(id);
        if (ns == null) return NotFound();

        _context.Namespaces.Remove(ns);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }
}

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly HarmonizerDbContext _context;

    public TeamsController(HarmonizerDbContext context)
    {
        _context = context;
    }

    // GET: api/Teams?namespaceId=1
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeamDto>>> GetAll([FromQuery] int? namespaceId)
    {
        var query = _context.Teams
            .Include(t => t.Namespace)
                .ThenInclude(n => n.Tenant)
            .Include(t => t.Workflows)
            .AsQueryable();

        if (namespaceId.HasValue)
        {
            query = query.Where(t => t.NamespaceId == namespaceId.Value);
        }

        var teams = await query
            .OrderBy(t => t.Namespace.Tenant.Name)
            .ThenBy(t => t.Namespace.Name)
            .ThenBy(t => t.Name)
            .Select(t => new TeamDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                Description = t.Description,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                NamespaceId = t.NamespaceId,
                NamespaceName = t.Namespace.Name,
                TenantName = t.Namespace.Tenant.Name,
                WorkflowCount = t.Workflows.Count
            })
            .ToListAsync();
        return Ok(teams);
    }

    // POST: api/Teams
    [HttpPost]
    public async Task<ActionResult<TeamDto>> Create([FromBody] TeamCreateDto dto)
    {
        var ns = await _context.Namespaces
            .Include(n => n.Tenant)
            .FirstOrDefaultAsync(n => n.Id == dto.NamespaceId);
        if (ns == null) return BadRequest(new { error = "Namespace not found" });

        var slug = dto.Slug ?? GenerateSlug(dto.Name);
        
        if (await _context.Teams.AnyAsync(t => t.NamespaceId == dto.NamespaceId && t.Slug == slug))
        {
            return BadRequest(new { error = "A team with this slug already exists in this namespace" });
        }

        var team = new Team
        {
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            NamespaceId = dto.NamespaceId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            Slug = team.Slug,
            Description = team.Description,
            IsActive = team.IsActive,
            CreatedAt = team.CreatedAt,
            NamespaceId = team.NamespaceId,
            NamespaceName = ns.Name,
            TenantName = ns.Tenant.Name,
            WorkflowCount = 0
        });
    }

    // DELETE: api/Teams/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null) return NotFound();

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }
}
