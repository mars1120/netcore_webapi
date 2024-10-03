using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

public partial class CurrencyDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public CurrencyDbContext()
    {
    }

    public CurrencyDbContext(DbContextOptions<CurrencyDbContext> options)
        : base(options)
    {
    }

    public CurrencyDbContext(DbContextOptions<CurrencyDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public virtual DbSet<Currency> Currencies { get; set; }
    public virtual DbSet<CurrentLangCurrency> CurrentLangCurrencies { get; set; }
    public virtual DbSet<Language> Languages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(_configuration?.GetConnectionString("DefaultConnection") ??
                                        "Server=(localdb)\\MSSQLLocalDB;Database=CurrencyDB;Trusted_Connection=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Currenci__3213E83FCD188D15");

            entity.HasIndex(e => e.Code, "UQ__Currenci__357D4CF94EAFFF68").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Rate)
                .HasColumnType("decimal(20, 6)")
                .HasColumnName("rate");
            entity.Property(e => e.RateFloat)
                .HasColumnType("decimal(20, 6)")
                .HasColumnName("rate_float");
            entity.Property(e => e.Symbol)
                .HasMaxLength(10)
                .HasColumnName("symbol");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<CurrentLangCurrency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CurrentL__3213E83F5149238E");

            entity.ToTable("CurrentLangCurrency");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.CurrentLang).HasMaxLength(10);
            entity.Property(e => e.LangId).HasColumnName("lang_id");
            entity.Property(e => e.LangTitle)
                .HasMaxLength(100)
                .HasColumnName("langTitle");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__language__3213E83F1A607B55");

            entity.ToTable("languages");

            entity.HasIndex(e => e.LangCode, "UQ__language__A0138498833EB5D1").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.LangCode)
                .HasMaxLength(10)
                .HasColumnName("langCode");
            entity.Property(e => e.LangName)
                .HasMaxLength(50)
                .HasColumnName("langName");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}