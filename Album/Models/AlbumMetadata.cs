using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Album.Models
{
    public class AlbumMetadata
    {

    }
    [ModelMetadataType(typeof(AlbumMetadata))]
    public partial class Album
    {
        public async Task<bool> Create(AlbumContext albumContext)
        {
            IsDelete = true;
            CreateBy = "pon";
            CreateDate = DateTime.Now;
            albumContext.Albums.Add(this);
            await albumContext.SaveChangesAsync();

            return true;
        }
        public List<Album> GetAll(AlbumContext context, string searchString)
        {
            List<Album> albums = context.Albums
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

            return albums;
        }
        public async Task<bool> Create(AlbumContext context,IFormFile coverPhoto, List<string> songNames,string createdBy = "pon")
        {
            // Validate cover photo
            if (coverPhoto == null || coverPhoto.Length == 0)
                throw new ArgumentException("กรุณาเลือกภาพปก");

            // Upload file
            var file = await File.CreateFile(context, coverPhoto, createdBy);
            FileId = file.Id;

            // Set album metadata
            CreateBy = createdBy;
            CreateDate = DateTime.Now;
            UpdateDate = DateTime.Now;
            IsDelete = false;

            context.Albums.Add(this);
            await context.SaveChangesAsync();

            // Add songs
            await Song.CreateSong(context, this.Id, songNames, createdBy);

            return true;
        }
        public async Task<bool> Edit(AlbumContext context, IFormFile? newCoverPhoto, List<string> newSongNames, bool removeOldPhoto, string updatedBy = "pon")
        {
            var dbAlbum = await context.Albums
                .Include(a => a.File)
                .Include(a => a.Songs)
                .FirstOrDefaultAsync(a => a.Id == this.Id && a.IsDelete == false);

            if (dbAlbum == null)
                throw new Exception("ไม่พบข้อมูลอัลบั้ม");

            dbAlbum.Name = this.Name;
            dbAlbum.Description = this.Description;
            dbAlbum.UpdateDate = DateTime.Now;
            dbAlbum.UpdateBy = updatedBy;

            if (removeOldPhoto && dbAlbum.File != null)
            {
                dbAlbum.File.IsDelete = true;
                dbAlbum.FileId = null;
            }

            if (newCoverPhoto != null && newCoverPhoto.Length > 0)
            {
                var updatedFile = await File.EditFile(context, newCoverPhoto, updatedBy);
                dbAlbum.FileId = updatedFile.Id;
            }

            await Song.EditSong(context, dbAlbum.Id, dbAlbum.Songs.ToList(), newSongNames, updatedBy);

            await context.SaveChangesAsync();
            return true;
        }

    }
}
