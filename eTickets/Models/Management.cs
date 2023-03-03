using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Amazoom.Models
{
    [Table("Items")]
    public class Management
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public double Price { get; set; }

        public string ImageURL { get; set; }
        public double weight { get; set; }
        public double volume { get; set; }
        public int stock { get; set; }
    }
}
