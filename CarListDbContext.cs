using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CarListApp.API;

public class CarListDbContext : IdentityDbContext
{
    public CarListDbContext(DbContextOptions<CarListDbContext> options) : base(options)
    {
    }

    public DbSet<Car> Cars { get; set; }    

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Suppress the PendingModelChangesWarning
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)   
    {
        base.OnModelCreating(modelBuilder);
        //modelBuilder.Entity<Car>()
            //.HasKey(c => c.Id);

        modelBuilder.Entity<Car>().HasData(
            new Car
            {
                Id = 1,
                Make = "Jeep",
                Model = "Grand Cherokee",
                Vin = "1Z324932843AASRT"
            },
            new Car
            {
                Id = 2,
                Make = "Jeep",
                Model = "Grand Cherokee",
                Vin = "1Z324932843AASRT"
            },
            new Car
            {
                Id = 3,
                Make = "Jeep",
                Model = "Grand Cherokee",
                Vin = "1Z324932843AASRT"
            }
        );

        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "f2f3a702-1e94-40a6-bb13-7acefbfab4fc",
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR"
            },
            new IdentityRole
            {
                Id = "7b129612-c6d5-4838-b925-b6211c574c0e",
                Name = "User",
                NormalizedName = "USER"
            }
        );

        var hasher = new PasswordHasher<IdentityUser>();

        modelBuilder.Entity<IdentityUser>().HasData(
            new IdentityUser
            {
                Id = "6e1548ff-2286-475a-aaab-ecbac9fb1d79",
                Email = "admin@localhost.com",
                NormalizedEmail = "ADMIN@LOCALHOST.COM",
                NormalizedUserName = "ADMIN@LOCALHOST.COM",
                UserName = "admin@localhost.com",
                PasswordHash = hasher.HashPassword(null, "P@ssword1"),
                EmailConfirmed = true,
                AccessFailedCount = 0,
                LockoutEnabled = false,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false
            },
            new IdentityUser
            {
                Id = "f1c7e3c4-ddc0-429a-a38f-47a8c0618ec5",
                Email = "user@localhost.com",
                NormalizedEmail = "USER@LOCALHOST.COM",
                NormalizedUserName = "USER@LOCALHOST.COM",
                UserName = "user@localhost.com",
                PasswordHash = hasher.HashPassword(null, "P@ssword1"),
                EmailConfirmed = true,
                AccessFailedCount = 0,
                LockoutEnabled = false,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false
            }
        );

        modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                RoleId = "f2f3a702-1e94-40a6-bb13-7acefbfab4fc",
                UserId = "6e1548ff-2286-475a-aaab-ecbac9fb1d79"
            },
            new IdentityUserRole<string>
            {
                RoleId = "7b129612-c6d5-4838-b925-b6211c574c0e",
                UserId = "f1c7e3c4-ddc0-429a-a38f-47a8c0618ec5"
            }
        );
    }
}
