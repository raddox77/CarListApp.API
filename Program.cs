using CarListApp.API;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Cross Origin Resource Services
builder.Services.AddCors(o => {
    o.AddPolicy("AllowAll", a=> a.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
});
builder.Services.AddDbContext<CarListDbContext>(options => 
    {
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    });

builder.Services.AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<CarListDbContext>();


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.MapOpenApi();
    app.UseSwagger();;
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseCors("AllowAll");


app.MapGet("/cars", async (CarListDbContext db) => await db.Cars.ToListAsync());

app.MapGet("/cars/{id}", async (int id, CarListDbContext db) => 
    await db.Cars.FindAsync(id) is Car car ? Results.Ok(car) : Results.NotFound()
    );

app.MapPut("/cars/{id}", async (int id,[FromBody] Car car, CarListDbContext db) =>
{
    var record = await db.Cars.FindAsync(id);
    if (record is null)
    {
        return Results.NotFound();
    }
    record.Make = car.Make;
    record.Model = car.Model;
    record.Vin = car.Vin;

    await db.SaveChangesAsync();
    return Results.NoContent(); 
});

app.MapDelete("/cars/{id}", async (int id, CarListDbContext db) =>
{
    var record = await db.Cars.FindAsync(id);
    if (record is null)
    {
        return Results.NotFound();
    }
    db.Remove(record);
    await db.SaveChangesAsync();
    return Results.NoContent(); 
});

app.MapPost("/cars", async ([FromBody] Car car, CarListDbContext db) =>
{
    await db.AddAsync(car);
    await db.SaveChangesAsync();
    return Results.Created($"/cars/{car.Id}", car);
});

app.MapPost("/login", async (LoginDto loginDto, UserManager<IdentityUser> _userManager) => 
{ 
    var user = await _userManager.FindByNameAsync(loginDto.Username);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var isFalidPassword = await _userManager.CheckPasswordAsync(user, loginDto.Password);
    if (!isFalidPassword)
    {
        return Results.Unauthorized();
    }

    // Generate an access token
    var response = new AuthResponseDto
    {
        UserId = user.Id,
        Username = loginDto.Username,
        Token = "AccessTokenHere"
    };
    return Results.Ok(response);
});

app.Run();

internal class LoginDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}

internal class AuthResponseDto
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public string Token { get; set; }
}
