using Artillery.Data.Models.Enums;
using Artillery.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Artillery.DataProcessor.ImportDto
{
    public class ImportGunDto
    {
        public int ManufacturerId { get; set; }

        [Required]
        [Range(100, 1_350_000)]
        public int GunWeight { get; set; }

        [Required]
        [Range(2.00, 35.00)]
        public double BarrelLength { get; set; }

        public int? NumberBuild { get; set; }

        [Required]
        [Range(1, 100_000)]
        public int Range { get; set; }

        [Required]
        public string GunType { get; set; }

        [Required]
        public int ShellId { get; set; }

        //[JsonArray("Countries")]
        public Countries[] Countries { get; set; }
    }

    public class Countries
    {
        public int Id { get; set; }
    }
}
