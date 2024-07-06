using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace ShareNet.Models
{
    public class File
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public string FileName { get; set;}
        public string FilePath { get; set;}
        public long Size {  get; set;}
        [DefaultValue(false)]
        public bool isShared {  get; set; }
    }
}
