using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Album.Models
{
    public class SongMetadata
    {
    }

    [ModelMetadataType(typeof(SongMetadata))]
    public partial class Song
    {
        public static async Task<List<Song>> CreateSong(
            AlbumContext context,
            int albumId,
            List<string> songNames,
            string createdBy = "pon")
        {
            if (songNames == null || songNames.Count == 0)
                return new List<Song>();

            var songs = songNames.Select(name => new Song
            {
                Name = name,
                AlbumId = albumId,
                CreateBy = createdBy,
                CreateDate = DateTime.Now,
                IsDelete = false
            }).ToList();

            context.Songs.AddRange(songs);
            await context.SaveChangesAsync();

            return songs;
        }
        public static async Task EditSong(AlbumContext context, int albumId, List<Song> existingSongs, List<string> newSongNames, string updatedBy)
        {
            foreach (var song in existingSongs)
            {
                if (newSongNames.Contains(song.Name))
                {
                    song.IsDelete = false;
                    newSongNames.Remove(song.Name);
                }
                else
                {
                    song.IsDelete = true;
                }
            }

            var newSongs = newSongNames.Select(name => new Song
            {
                Name = name,
                AlbumId = albumId,
                CreateBy = updatedBy,
                CreateDate = DateTime.Now,
                IsDelete = false
            }).ToList();

            context.Songs.AddRange(newSongs);
            await context.SaveChangesAsync();
        }
    }
}
