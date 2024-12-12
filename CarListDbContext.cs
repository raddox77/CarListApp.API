using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarListApp.API;

public class CarListDbContext : IdentityDbContext
{
    public CarListDbContext(DbContextOptions<CarListDbContext> options) : base(options)
    {
    }

    public DbSet<Car> Cars { get; set; }    

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
    }
}
