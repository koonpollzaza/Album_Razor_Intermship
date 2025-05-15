using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Album.Models;

[Table("File")]
public partial class File
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string FileName { get; set; } = null!;

    [StringLength(50)]
    public string FilePath { get; set; } = null!;

    [StringLength(50)]
    public string? CreateBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    [StringLength(50)]
    public string? UpdateBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }

    public bool? IsDelete { get; set; }

    [InverseProperty("File")]
    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();
}
