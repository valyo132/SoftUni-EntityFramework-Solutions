﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Artillery.DataProcessor.ImportDto
{
    [XmlType("Country")]
    public class ImportCountiresDto
    {
        [Required]
        [StringLength(60, MinimumLength = 4)]
        [XmlElement("CountryName")]
        public string CountryName { get; set; } = null!;

        [Required]
        [Range(50_000, 10_000_000)]
        [XmlElement("ArmySize")]
        public int ArmySize { get; set; }
    }
}
