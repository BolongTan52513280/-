using Amazoom.Data;
using Amazoom.Data.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Amazoom.Models
{
    public class NewItemVM
    {
        public int Id { get; set; }

        [Display(Name = "Item name")]
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Display(Name = "Item description")]
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Display(Name = "Price in $")]
        [Required(ErrorMessage = "Price is required")]
        public double Price { get; set; }

        [Display(Name = "Item poster URL")]
        [Required(ErrorMessage = "Item poster URL is required")]
        public string ImageURL { get; set; }

        [Display(Name = "volume")]
        [Required(ErrorMessage = "volume is required")]
        public double volume { get; set; }

        [Display(Name = "stock")]
        [Required(ErrorMessage = "stock is required")]
        public int stock { get; set; }

        [Display(Name = "weight")]
        [Required(ErrorMessage = "weight is required")]
        public double weight { get; set; }

    }
}
