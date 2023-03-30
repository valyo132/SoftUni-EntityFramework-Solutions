using System.ComponentModel.DataAnnotations;

namespace SoftJail.DataProcessor.ImportDto
{
    public class ImportDepartmentCellDto
    {
        [Required]
        [StringLength(25, MinimumLength = 3)]
        public string Name { get; set; } = null!;

        public ImportCellDto[] Cells { get; set; }
    }

    public class ImportCellDto
    {
        [Required]
        [Range(1, 1000)]
        public int CellNumber { get; set; }

        [Required]
        public bool HasWindow { get; set; }
    }
}
