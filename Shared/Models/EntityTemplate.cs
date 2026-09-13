using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace GIBS.Module.Entity.Models
{
    [Table("GIBS_EntityTemplate")]
    public class EntityTemplate : ModelBase
    {
        [Key]
        public int TemplateId { get; set; }

        [Required]
        public int ModuleId { get; set; }

        public int? EntityTypeId { get; set; }

        [StringLength(200)]
        public string TemplateType { get; set; }

        public string Header { get; set; }

        public string Item { get; set; }
       
        public string Alternate { get; set; }

        [StringLength(200)]
        public string Separator { get; set; }
       
        public string Footer { get; set; }

        [StringLength(200)]
        public string PageTitle { get; set; }

        [StringLength(500)]
        public string PageDescription { get; set; }

        [StringLength(500)]
        public string PageKeywords { get; set; }

        [StringLength(200)]
        public string PageHeader { get; set; }
    }
}
