using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Album.Models
{
    public class FileMetadata
    {
    }

    [ModelMetadataType(typeof(FileMetadata))]
    public partial class File
    {
        public static async Task<File> CreateFile(AlbumContext context, IFormFile file, string createdBy)
        {
            var fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var File = new File
            {
                FileName = fileName,
                FilePath = "/uploads/" + fileName,
                CreateBy = createdBy,
                CreateDate = DateTime.Now,
                IsDelete = false
            };

            context.Files.Add(File);
            await context.SaveChangesAsync();

            return File;
        }
        public static async Task<File> EditFile(AlbumContext context, IFormFile file, string updatedBy)
        {
            var fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileEntity = new File
            {
                FileName = fileName,
                FilePath = "/uploads/" + fileName,
                CreateBy = updatedBy,
                UpdateBy = updatedBy,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                IsDelete = false
            };

            context.Files.Add(fileEntity);
            await context.SaveChangesAsync();

            return fileEntity;
        }
    }
}
