using CarListApp.API;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSetting:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
    .RequireAuthenticatedUser()
    .Build();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.MapOpenApi();
    app.UseSwagger();;
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
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
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var roles = await _userManager.GetRolesAsync(user);
    var claims = await _userManager.GetClaimsAsync(user);

    // Create token claims for use with token creation
    var tokenClaims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim("email_confirmed", user.EmailConfirmed.ToString())
    }.Union(claims)
    .Union(roles.Select(role => new Claim(ClaimTypes.Role, role)));

    // Create token
    var securityToken = new JwtSecurityToken(
        issuer: builder.Configuration["JwtSettings:Issuer"],
        audience: builder.Configuration["JwtSettings:Audience"],
        claims: tokenClaims,
        expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(builder.Configuration["JwtSettings:DurationInMinutes"])),
        signingCredentials: credentials
        );

    var accessToken = new JwtSecurityTokenHandler().WriteToken(securityToken);

    // Create a response to send back to client
    var response = new AuthResponseDto
    {
        UserId = user.Id,
        Username = loginDto.Username,
        Token = accessToken
    };

    return Results.Ok(response);
}).AllowAnonymous();

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
