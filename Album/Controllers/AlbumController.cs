using Album.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace Album.Controllers
{
    public class AlbumController : Controller
    {
        private readonly AlbumContext _context;

        public AlbumController(AlbumContext context)
        {
            _context = context;
        }

        // 📋 แสดงรายการอัลบั้ม
        public IActionResult Index(string searchString)
        {
            var albums = _context.Albums
                .Include(a => a.File)
                .Include(a => a.Songs)
                .Where(a => a.IsDelete != true)
                .AsEnumerable();

            if (!string.IsNullOrEmpty(searchString))
            {
                albums = albums.Where(a => a.Name.Contains(searchString.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return View(albums);
        }

        // 📄 หน้าสร้างอัลบั้ม
        public IActionResult Create()
        {
            return View();
        }

        // 📤 เพิ่มอัลบั้มใหม่
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Models.Album album, List<string> Songs, IFormFile CoverPhoto)
        {
            if (CoverPhoto == null || CoverPhoto.Length == 0)
            {
                ModelState.AddModelError("CoverPhoto", "กรุณาเลือกภาพปก");
                return View(album);
            }

            if (ModelState.IsValid)
            {
                // ✔ อัปโหลดภาพปก
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(CoverPhoto.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    await CoverPhoto.CopyToAsync(stream);
                }

                var file = new Models.File
                {
                    FileName = fileName,
                    FilePath = "/uploads/" + fileName
                };

                _context.Files.Add(file);
                await _context.SaveChangesAsync();

                album.FileId = file.Id;
                album.CreateDate = DateTime.Now;
                album.IsDelete = false;

                _context.Albums.Add(album);
                await _context.SaveChangesAsync();

                // 🎵 เพิ่มเพลง
                if (Songs != null && Songs.Count > 0)
                {
                    var songList = Songs.Select(songName => new Song
                    {
                        Name = songName,
                        AlbumId = album.Id
                    }).ToList();

                    _context.Songs.AddRange(songList);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(album);
        }

        // 📄 หน้าสำหรับแก้ไขอัลบั้ม
        public async Task<IActionResult> Edit(int id)
        {
            var album = await _context.Albums
                .Include(a => a.File)
                .Include(a => a.Songs)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // 💾 บันทึกการแก้ไข
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Models.Album album, List<string> Songs, IFormFile? CoverPhoto, string Editphoto)
        {
            if (ModelState.IsValid)
            {
                var dbAlbum = await _context.Albums
                    .Include(a => a.File)
                    .Include(a => a.Songs)
                    .Where(a => a.IsDelete != true)
                    .FirstOrDefaultAsync(a => a.Id == album.Id);

                if (dbAlbum == null)
                {
                    return NotFound();
                }

                // ✔อัปเดตข้อมูลอัลบั้ม
                dbAlbum.Name = album.Name;
                dbAlbum.Description = album.Description;
                dbAlbum.UpdateDate = DateTime.Now;

                // จัดการภาพปก
                if (Editphoto == "true" && dbAlbum.File != null)
                {
                    // ลบไฟล์ภาพจากระบบ
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", dbAlbum.File.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    // ลบข้อมูลไฟล์จากฐานข้อมูล
                    _context.Files.Remove(dbAlbum.File);
                    dbAlbum.FileId = null;
                }
                else if (CoverPhoto != null && CoverPhoto.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(CoverPhoto.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await CoverPhoto.CopyToAsync(stream);
                    }

                    var file = new Models.File
                    {
                        FileName = fileName,
                        FilePath = "/uploads/" + fileName
                    };

                    _context.Files.Add(file);
                    await _context.SaveChangesAsync();

                    dbAlbum.FileId = file.Id;
                }

                // ✔ อัปเดตเพลง
                _context.Songs.RemoveRange(dbAlbum.Songs);
                if (Songs != null && Songs.Count > 0)
                {
                    var songList = Songs.Select(songName => new Song
                    {
                        Name = songName,
                        AlbumId = dbAlbum.Id
                    }).ToList();

                    _context.Songs.AddRange(songList);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(album);
        }

        // ❌ ลบอัลบั้ม
        public async Task<IActionResult> Delete(int id)
        {
            var album = await _context.Albums
                .Include(a => a.File)
                .Include(a => a.Songs)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null)
            {
                return NotFound();
            }

            album.IsDelete = true;
            await _context.SaveChangesAsync();
            //// ลบเพลง
            //_context.Songs.RemoveRange(album.Songs);

            //// ลบไฟล์ภาพ
            //if (album.File != null)
            //{
            //    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", album.File.FilePath.TrimStart('/'));
            //    if (System.IO.File.Exists(filePath))
            //    {
            //        System.IO.File.Delete(filePath);
            //    }

            //    _context.Files.Remove(album.File);
            //}

            //// ลบอัลบั้ม
            //_context.Albums.Remove(album);
            //await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
