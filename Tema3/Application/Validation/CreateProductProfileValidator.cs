using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Tema3.Application.Requests;
using Tema3.Domain.Enums;

namespace Tema3.Application.Validation;

public class CreateProductProfileValidator
{
    private readonly ApplicationContext _ctx;
    private readonly ILogger<CreateProductProfileValidator> _logger;

    // liste existente
    private static readonly string[] InappropriateWords =
        { "offensive", "nsfw", "obscene", "vulgar" };

    private static readonly string[] HomeRestrictedWords =
        { "weapon", "explosive", "toxic", "hazard" };

    private static readonly HashSet<string> ImageExts =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    private static readonly Regex BrandRegex =
        new(@"^[A-Za-z0-9\s\-\.'’]+$", RegexOptions.Compiled);

    private static readonly Regex SkuRegex =
        new(@"^[A-Za-z0-9\-]{5,20}$", RegexOptions.Compiled);

    // noi: cuvinte pentru produse tech
    private static readonly string[] TechKeywords =
    {
        "tech","smart","ai","ml","gpu","cpu","processor","chip","ssd","nvme",
        "wireless","bluetooth","usb-c","hdmi","4k","8k","oled","lcd","monitor",
        "laptop","notebook","desktop","headphones","earbuds","camera","router","wifi",
        "gaming","controller","console","keyboard","mouse","battery","charger","dock"
    };

    // noi: filtre extra pentru Home
    private static readonly string[] HomeInappropriateWordsExtra =
        { "violence","adult","nsfw" };

    public CreateProductProfileValidator(ApplicationContext ctx,
        ILogger<CreateProductProfileValidator> logger)
    {
        _ctx = ctx;
        _logger = logger;
    }

    public async Task ValidateAndThrowAsync(CreateProductProfileRequest r, CancellationToken ct)
    {
        var errors = new List<string>();

        // Name
        if (string.IsNullOrWhiteSpace(r.Name))
            errors.Add("Name is required.");
        if (r.Name?.Length < 1 || r.Name?.Length > 200)
            errors.Add("Name length must be 1..200.");
        if (!BeValidName(r.Name))
            errors.Add("Name contains inappropriate content.");
        if (!await BeUniqueName(r.Name, r.Brand, ct))
            errors.Add("Name must be unique per brand.");

        // Brand
        if (string.IsNullOrWhiteSpace(r.Brand))
            errors.Add("Brand is required.");
        if (r.Brand?.Length < 2 || r.Brand?.Length > 100)
            errors.Add("Brand length must be 2..100.");
        if (!BeValidBrandName(r.Brand))
            errors.Add("Brand contains invalid characters.");

        // SKU
        if (string.IsNullOrWhiteSpace(r.SKU))
            errors.Add("SKU is required.");
        if (!BeValidSKU(r.SKU))
            errors.Add("SKU must be alphanumeric with hyphens, 5..20 length.");
        if (!await BeUniqueSKU(r.SKU, ct))
            errors.Add("SKU must be unique.");

        // Category
        if (!Enum.IsDefined(typeof(ProductCategory), r.Category))
            errors.Add("Category is invalid.");

        // Price
        if (r.Price <= 0)
            errors.Add("Price must be greater than 0.");
        if (r.Price >= 10000)
            errors.Add("Price must be less than 10000.");

        // ReleaseDate
        if (r.ReleaseDate > DateTime.UtcNow)
            errors.Add("ReleaseDate cannot be in the future.");
        if (r.ReleaseDate < new DateTime(1900, 1, 1))
            errors.Add("ReleaseDate cannot be before 1900.");

        // StockQuantity
        if (r.StockQuantity < 0)
            errors.Add("StockQuantity cannot be negative.");
        if (r.StockQuantity > 100000)
            errors.Add("StockQuantity cannot exceed 100000.");

        // ImageUrl
        if (!BeValidImageUrl(r.ImageUrl))
            errors.Add("ImageUrl must be HTTP/HTTPS and end with a valid image extension.");

        // === CONDITIONAL VALIDATION (Task 3.3) ===
        switch (r.Category)
        {
            case ProductCategory.Electronics:
                if (r.Price < 50m)
                    errors.Add("Electronics price must be at least $50.00.");
                if (!ContainTechnologyKeywords(r.Name))
                    errors.Add("Electronics Name must contain technology keywords.");
                if (!ReleasedWithinLastYears(r.ReleaseDate, 5))
                    errors.Add("Electronics must be released within the last 5 years.");
                break;

            case ProductCategory.Home:
                if (r.Price > 200m)
                    errors.Add("Home product price must be at most $200.00.");
                if (!BeAppropriateForHome(r.Name))
                    errors.Add("Home product Name is not appropriate.");
                break;

            case ProductCategory.Clothing:
                if (string.IsNullOrWhiteSpace(r.Brand) || r.Brand.Trim().Length < 3)
                    errors.Add("Clothing Brand must be at least 3 characters.");
                break;
        }

        // Cross-field
        if (r.Price > 100m && r.StockQuantity > 20)
            errors.Add("Expensive products (> $100) must have limited stock (≤ 20 units).");

        if (r.Category == ProductCategory.Electronics && !ReleasedWithinLastYears(r.ReleaseDate, 5))
            errors.Add("Electronics must be recent (released within 5 years).");
        // === end conditional ===

        // Business Rules
        var business = await PassBusinessRules(r, ct);
        errors.AddRange(business);

        if (errors.Count > 0)
        {
            foreach (var e in errors) _logger.LogWarning("Validation error: {Message}", e);
            throw new ArgumentException(string.Join(" | ", errors));
        }
    }

    // Methods required

    public bool BeValidName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        var n = name.ToLowerInvariant();
        return InappropriateWords.All(w => !n.Contains(w));
    }

    public async Task<bool> BeUniqueName(string name, string brand, CancellationToken ct)
    {
        var exists = await _ctx.NameExistsForBrandAsync(name, brand, ct);
        _logger.LogInformation("Unique name check: Name={Name}, Brand={Brand}, Exists={Exists}", name, brand, exists);
        return !exists;
    }

    public bool BeValidBrandName(string brand) => BrandRegex.IsMatch(brand);

    public bool BeValidSKU(string sku) => SkuRegex.IsMatch(sku);

    public async Task<bool> BeUniqueSKU(string sku, CancellationToken ct)
    {
        var exists = await _ctx.SkuExistsAsync(sku, ct);
        _logger.LogInformation("Unique SKU check: SKU={SKU}, Exists={Exists}", sku, exists);
        return !exists;
    }

    public bool BeValidImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true; // opțional
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return false;
        var ext = Path.GetExtension(uri.LocalPath);
        return !string.IsNullOrEmpty(ext) && ImageExts.Contains(ext);
    }

    public async Task<List<string>> PassBusinessRules(CreateProductProfileRequest r, CancellationToken ct)
    {
        var errors = new List<string>();

        // Rule 1: daily cap
        var todayCount = await _ctx.CountCreatedTodayAsync(ct);
        if (todayCount >= 500)
        {
            _logger.LogWarning("Daily limit exceeded: {Count}", todayCount);
            errors.Add("Daily product addition limit reached (500).");
        }

        // Rule 2: electronics minimum price
        if (r.Category == ProductCategory.Electronics && r.Price < 50m)
        {
            _logger.LogWarning("Electronics min price violated. Price={Price}", r.Price);
            errors.Add("Electronics must be priced at least $50.00.");
        }

        // Rule 3: Home restricted words in Name
        if (r.Category == ProductCategory.Home)
        {
            var n = r.Name?.ToLowerInvariant() ?? "";
            if (HomeRestrictedWords.Any(w => n.Contains(w)))
            {
                _logger.LogWarning("Home content restriction violated. Name={Name}", r.Name);
                errors.Add("Home category has restricted content in Name.");
            }
        }

        // Rule 4: high-value stock cap
        if (r.Price > 500m && r.StockQuantity > 10)
        {
            _logger.LogWarning("High value stock limit violated. Price={Price} Stock={Stock}", r.Price, r.StockQuantity);
            errors.Add("High-value product stock limit: max 10 units for price > $500.");
        }

        return errors;
    }

    // helpers noi

    private static bool ContainTechnologyKeywords(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        var n = name.ToLowerInvariant();
        return TechKeywords.Any(k => n.Contains(k));
    }

    private static bool BeAppropriateForHome(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        var n = name.ToLowerInvariant();
        if (!InappropriateWords.All(w => !n.Contains(w))) return false;
        if (!HomeRestrictedWords.All(w => !n.Contains(w))) return false;
        if (!HomeInappropriateWordsExtra.All(w => !n.Contains(w))) return false;
        return true;
    }

    private static bool ReleasedWithinLastYears(DateTime date, int years)
    {
        var limit = DateTime.UtcNow.AddYears(-years);
        return date >= limit;
    }
}