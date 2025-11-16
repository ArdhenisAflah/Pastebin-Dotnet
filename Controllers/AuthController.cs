using Microsoft.AspNetCore.Mvc;
using project_pastebin.Data;
using project_pastebin.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace project_pastebin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly IConfiguration _configuration;

        // Kita butuh 'IConfiguration' buat ambil Secret Key dari appsettings.json
        public AuthController(ApiDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // REGISTER USER BARU
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDTO request)
        {
            // 1. Enkripsi Password (Hashing) pakai BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 2. Bikin object User baru
            var user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash // Simpan yang sudah di-hash!
            };

            // 3. Simpan ke Database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        // LOGIN & DAPATKAN TOKEN
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDTO request)
        {
            // 1. Cari user berdasarkan Username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            // 2. Cek apakah user ada?
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            // 3. Cek Password (Verifikasi Hash)
            // Kita bandingkan password polos dari input vs Hash di database
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Wrong password.");
            }

            // 4. Kalau password benar, BUAT TOKEN JWT
            string token = CreateToken(user);

            return Ok(token);
        }

        // FUNGSI RAHASIA PEMBUAT TOKEN
        private string CreateToken(User user)
        {
            // A. Ambil data user yang mau disimpan di dalam token (Claims)
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("UserId", user.Id.ToString()) // Bisa simpan ID juga
            };

            // B. Ambil Kunci Rahasia dari appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("Jwt:Token").Value!));

            // C. Tentukan Algoritma Enkripsi (HmacSha256)
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            // D. Rakit Tokennya
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1), // Token berlaku 1 hari
                signingCredentials: creds
            );

            // E. Tulis jadi string "eyJhbGci..."
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}