using Mapster;
using YLunchApi.Domain.CommonAggregate.Services;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;
using YLunchApi.Domain.RestaurantAggregate.Services;

namespace YLunchApi.Application.RestaurantAggregate;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public OrderService(IOrderRepository orderRepository, IDateTimeProvider dateTimeProvider, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _dateTimeProvider = dateTimeProvider;
        _productRepository = productRepository;
    }

    public async Task<OrderReadDto> Create(string customerId, string restaurantId, OrderCreateDto orderCreateDto)
    {
        var products = orderCreateDto.ProductIds!
                                     .Select(productId =>
                                     {
                                         var product = _productRepository
                                                       .ProductsQueryBase
                                                       .Where(x => x.RestaurantId == restaurantId)
                                                       .First(x => x.Id == productId);
                                         if (product == null)
                                         {
                                             throw new EntityNotFoundException($"Product:{productId}");
                                         }

                                         return product;
                                     })
                                     .ToList();

        var totalPrice = products.Sum(x => x.Price);
        var order = orderCreateDto.Adapt<Order>();
        order.UserId = customerId;
        order.RestaurantId = restaurantId;
        order.OrderStatuses = new List<OrderStatus>
        {
            new()
            {
                OrderId = order.Id,
                DateTime = _dateTimeProvider.UtcNow,
                State = OrderState.Idling
            }
        };
        order.IsDeleted = false;
        order.TotalPrice = totalPrice;

        order.OrderedProducts = products.Select(x =>
                                        {
                                            var orderedProduct = new OrderedProduct
                                            {
                                                OrderId = order.Id,
                                                ProductId = x.Id,
                                                RestaurantId = x.RestaurantId,
                                                Name = x.Name,
                                                Description = x.Description,
                                                Price = x.Price,
                                                CreationDateTime = x.CreationDateTime,
                                                Allergens = string.Join(",", x.Allergens.Select(y => y.Name)),
                                                ProductTags = string.Join(",", x.ProductTags.Select(y => y.Name))
                                            };
                                            return orderedProduct;
                                        })
                                        .ToList();

        await _orderRepository.Create(order);
        var orderDb = await _orderRepository.GetById(order.Id);
        return orderDb.Adapt<OrderReadDto>();
    }
}
