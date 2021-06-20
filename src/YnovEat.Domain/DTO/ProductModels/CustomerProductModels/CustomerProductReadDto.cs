using System;
using YnovEat.Domain.DTO.OrderModels;
using YnovEat.Domain.ModelsAggregate.CustomerAggregate;
using YnovEat.Domain.ModelsAggregate.RestaurantAggregate;

namespace YnovEat.Domain.DTO.ProductModels.CustomerProductModels
{
    public class CustomerProductReadDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public DateTime CreationDateTime { get; set; }
        public string RestaurantProductId { get; set; }
        public string RestaurantId { get; set; }
        public string OrderId { get; set; }

        public CustomerProductReadDto(CustomerProduct customerProduct)
        {
            Id = customerProduct.Id;
            Name = customerProduct.Name;
            Description = customerProduct.Description;
            Price = customerProduct.Price;
            CreationDateTime = customerProduct.CreationDateTime;
            RestaurantProductId = customerProduct.RestaurantProductId;
            RestaurantId = customerProduct.RestaurantId;
            OrderId = customerProduct.OrderId;
        }
    }
}
