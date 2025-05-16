using Microsoft.AspNetCore.Mvc;

namespace Album.Models
{
    public class SongMetadata
    {
    }
    [ModelMetadataType(typeof(SongMetadata))]
    public partial class Song
    { 
    
    }

}
