using System.Collections;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_pastebin.Data;
using project_pastebin.Models;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasteController : ControllerBase
    {
        private readonly ApiDbContext _context;
        public PasteController(ApiDbContext context)
        {
            _context = context;
        }


        // [HttpGet("{q}")]
        // [Authorize]
        // public async Task<ActionResult<IEnumerable>> GetAllPasteVulnSql(string q)
        // {
        //     string sql = $"SELECT * FROM PasteContents WHERE author LIKE '%{q}%'";

        //     //dangerous nigga
        //     var Result = await _context.PasteContents.FromSqlRaw(sql).ToListAsync();

        //     if (Result != null)
        //     {
        //         return Ok(Result);
        //     }
        //     else
        //     {
        //         return NotFound();
        //     }

        // }

        [HttpGet]
        [Authorize] // 2. PASANG GEMBOK DI SINI
        public async Task<ActionResult<List<PasteContent>>> GetAllPaste()
        {
            return await _context.PasteContents.ToListAsync();
        }

        [HttpGet("{ID}")]
        public async Task<ActionResult<IEnumerable<PasteContent>>> GetSinglePaste(int ID)
        {
            var pasteContent = await _context.PasteContents.FindAsync(ID);

            if (pasteContent != null)
            {
                return Ok(pasteContent);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost] // Menandakan ini dipanggil lewat HTTP POST
        public async Task<ActionResult<PasteContent>> Post(PasteContent newPaste)
        {
            // STEP 1: Masukkan ke Memori (Belum ke DB)
            _context.PasteContents.Add(newPaste);

            // STEP 2: "Push" ke SQL Server
            // Di sinilah SQL Query "INSERT INTO..." dijalankan.
            await _context.SaveChangesAsync();

            // STEP 3: Return Standar Internasional (HTTP 201 Created)
            // Ini best practice REST API.
            // Daripada cuma bilang "OK", kita bilang "Barang berhasil dibuat,
            // dan ini lho alamat/ID barunya."
            return CreatedAtAction(nameof(GetSinglePaste), new { id = newPaste.Id }, newPaste);
        }

    }
}
