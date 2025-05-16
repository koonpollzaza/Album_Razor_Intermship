using Microsoft.AspNetCore.Mvc;

namespace Album.Models
{
    public class AlbumMetadata
    {
        
    }
    [ModelMetadataType(typeof(AlbumMetadata))]
    public partial class Album
    {

    }
}
