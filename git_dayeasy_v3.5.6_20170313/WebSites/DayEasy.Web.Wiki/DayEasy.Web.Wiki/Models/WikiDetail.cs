using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Web.Wiki.Models
{
    [Table("TWikiDetail")]
    public class WikiDetail : DEntity<string>
    {
        [Key, StringLength(16)]
        public override string Id { get; set; }
        [Required, StringLength(32)]
        [ForeignKey("DWiki")]
        public string WikiId { get; set; }

        [Required, StringLength(64)]
        public string Title { get; set; }
        [Required]
        public string Detail { get; set; }
        [Required, DefaultValue(0)]
        public int Sort { get; set; }

        public string Version { get; set; }

        public virtual Wiki DWiki { get; set; }
    }
}