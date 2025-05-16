using Album.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Album.Controllers
{
    public class AlbumController : Controller
    {
        private readonly AlbumContext _context;

        public AlbumController(AlbumContext context)
        {
            _context = context;
        }

        public IActionResult Index(string searchString)
        {
            List<Models.Album> albums = _context.Albums
                .Include(a => a.File)
                .Include(a => a.Songs.Where(a => a.IsDelete != true))
                .Where(a => a.IsDelete != true)
                .ToList();

            if (!string.IsNullOrEmpty(searchString))
            {
                albums = albums
                    .Where(a => a.Name.Contains(searchString.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return View(albums);
        }

        public IActionResult Create()
        {
            return View();
        }

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
                // อัปโหลดภาพ
                // สร้างชื่อไฟล์จากวันที่และเวลาปัจจุบัน
                var fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + Path.GetExtension(CoverPhoto.FileName);
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

                //เพิ่มเพลง
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
        public async Task<IActionResult> Edit(int id)
        {
            var album = await _context.Albums
                .Include(a => a.File)
                .Include(a => a.Songs.Where(a => a.IsDelete != true))
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Models.Album album, List<string> Songs, IFormFile? CoverPhoto, string Editphoto)
        {
            if (ModelState.IsValid)
            {
                var dbAlbum = await _context.Albums
                    .Include(a => a.File)
                    .Include(a => a.Songs.Where(a => a.IsDelete != true))
                    .Where(a => a.IsDelete != true)
                    .FirstOrDefaultAsync(a => a.Id == album.Id);

                if (dbAlbum == null)
                {
                    return NotFound();
                }

                // อัปเดตข้อมูลอัลบั้ม
                dbAlbum.Name = album.Name;
                dbAlbum.Description = album.Description;
                dbAlbum.UpdateDate = DateTime.Now;

                // ✔ อัปเดตภาพปก
                if (Editphoto == "true" && dbAlbum.File != null)
                {
                    // ลบข้อมูลไฟล์
                    dbAlbum.File.IsDelete = true;
                    dbAlbum.FileId = null;
                }

                if (CoverPhoto != null && CoverPhoto.Length > 0)
                {
                    var fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + Path.GetExtension(CoverPhoto.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await CoverPhoto.CopyToAsync(stream);
                    }

                    var file = new Models.File
                    {
                        FileName = fileName,
                        FilePath = "/uploads/" + fileName,
                        IsDelete = false
                    };

                    _context.Files.Add(file);
                    await _context.SaveChangesAsync();

                    dbAlbum.FileId = file.Id;
                }
                foreach (Song song in dbAlbum.Songs)
                {
                    if (Songs.Contains(song.Name))
                    {
                        song.IsDelete = false;
                        Songs.Remove(song.Name);

                    }
                    else
                    {
                        song.IsDelete = true;
                    }
                }
                // อัปเดตเพลง
                //List<Song> existingSongs = dbAlbum.Songs.ToList();
                //_context.Songs.RemoveRange(existingSongs);

                if (Songs != null)
                {
                    List<Song> newSongs = Songs.Select(songName => new Song
                    {
                        Name = songName,
                        AlbumId = dbAlbum.Id,
                        IsDelete = false
                    }).ToList();

                    _context.Songs.AddRange(newSongs);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(album);
        }

        // ลบอัลบั้ม
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
