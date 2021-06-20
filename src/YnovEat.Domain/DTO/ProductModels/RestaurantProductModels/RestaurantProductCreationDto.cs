using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using YnovEat.Domain.ModelsAggregate.RestaurantAggregate;
using YnovEat.DomainShared.RestaurantAggregate.Enums;

namespace YnovEat.Domain.DTO.ProductModels.RestaurantProductModels
{
    public class RestaurantProductCreationDto
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public double Price { get; set; }
        public int? Quantity { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? ExpirationDateTime { get; set; }
        [Range(0, ProductFamiliesUtils.Count, ErrorMessage = "ProductFamily is out of range")]
        public ProductFamilies? ProductFamily { get; set; }
    }
}
