using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using project_pastebin.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Ambil "Kunci Rahasia" dari appsettings.json
var jwtIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>();
var jwtKey = builder.Configuration.GetSection("Jwt:Token").Get<string>();

// 2. Konfigurasi JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddJwtBearer(options =>
 {
     options.TokenValidationParameters = new TokenValidationParameters
     {
         // Cek apakah tokennya diterbitkan oleh server kita (Validasi Tanda Tangan)
         ValidateIssuerSigningKey = true,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

         // Untuk development, kita matikan dulu validasi Issuer & Audience biar gampang
         ValidateIssuer = false,
         ValidateAudience = false,

         // Biar token expired-nya pas waktunya (gak ada toleransi waktu tambahan)
         ClockSkew = TimeSpan.Zero
     };
 });

//
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


builder.Services.AddDbContext<ApiDbContext>((option) =>
{
    option.UseSqlServer(connectionString);
});
//

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
