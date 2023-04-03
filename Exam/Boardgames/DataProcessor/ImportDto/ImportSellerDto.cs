using System.ComponentModel.DataAnnotations;

namespace Boardgames.DataProcessor.ImportDto
{
    public class ImportSellerDto
    {
        [Required]
        [StringLength(20, MinimumLength = 5)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(30, MinimumLength = 2)]
        public string Address { get; set; } = null!;

        [Required]
        public string Country { get; set; } = null!;

        [Required]
        [RegularExpression(@"www\.[a-zA-Z0-9-]+\.com$")]
        public string Website { get; set; } = null!;

        public int[] Boardgames { get; set; }
    }
}
