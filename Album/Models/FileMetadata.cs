using Microsoft.AspNetCore.Mvc;

namespace Album.Models
{
    public class FileMetadata
    {
    }
    [ModelMetadataType(typeof(FileMetadata))]
    public partial class File
    {

    }
}
