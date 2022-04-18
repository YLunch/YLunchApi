using Mapster;
using YLunchApi.Domain.CommonAggregate.Services;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Filters;
using YLunchApi.Domain.RestaurantAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;
using YLunchApi.Domain.RestaurantAggregate.Services;

namespace YLunchApi.Application.RestaurantAggregate;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IRestaurantService _restaurantService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public OrderService(IOrderRepository orderRepository, IDateTimeProvider dateTimeProvider, IProductRepository productRepository, IRestaurantRepository restaurantRepository, IRestaurantService restaurantService)
    {
        _orderRepository = orderRepository;
        _dateTimeProvider = dateTimeProvider;
        _productRepository = productRepository;
        _restaurantRepository = restaurantRepository;
        _restaurantService = restaurantService;
    }

    public async Task<OrderReadDto> CreateOrder(string customerId, string restaurantId, OrderCreateDto orderCreateDto)
    {
        var restaurant = await _restaurantRepository.GetRestaurantById(restaurantId);
        if (!_restaurantService.IsOpenToOrder(restaurant, (DateTime)orderCreateDto.ReservedForDateTime!))
        {
            throw new ReservedForDateTimeOutOfOpenToOrderOpeningTimesException();
        }

        var notFoundProductIds = new List<string>();

        var products = orderCreateDto.ProductIds!
                                     .Aggregate(new List<Product>(), (acc, productId) =>
                                     {
                                         var product = _productRepository
                                                       .ProductsQueryBase
                                                       .Where(x => x.RestaurantId == restaurantId)
                                                       .FirstOrDefault(x => x.Id == productId);
                                         if (product == null)
                                         {
                                             notFoundProductIds.Add(productId);
                                         }
                                         else
                                         {
                                             acc.Add(product);
                                         }

                                         return acc;
                                     })
                                     .ToList();
        if (notFoundProductIds.Count > 0)
        {
            throw new EntityNotFoundException($"Products: {string.Join(" and ", notFoundProductIds)} not found.");
        }

        var totalPrice = products.Sum(x => x.Price);
        var order = orderCreateDto.Adapt<Order>();
        order.CreationDateTime = _dateTimeProvider.UtcNow;
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
                                                UserId = customerId,
                                                RestaurantId = x.RestaurantId,
                                                Name = x.Name,
                                                Description = x.Description,
                                                Price = x.Price,
                                                CreationDateTime = x.CreationDateTime,
                                                ExpirationDateTime = x.ExpirationDateTime,
                                                ProductType = x.ProductType,
                                                Image = x.Image,
                                                Allergens = string.Join(",", x.Allergens.OrderBy(y => y.Name).Select(y => y.Name)),
                                                ProductTags = string.Join(",", x.ProductTags.OrderBy(y => y.Name).Select(y => y.Name))
                                            };
                                            return orderedProduct;
                                        })
                                        .ToList();

        await _orderRepository.CreateOrder(order);
        var orderDb = await _orderRepository.GetOrderById(order.Id);
        return orderDb.Adapt<OrderReadDto>();
    }

    public async Task<OrderReadDto> GetOrderByIdForCustomer(string customerId, string orderId)
    {
        var order = await _orderRepository.GetOrderById(orderId);
        if (order.UserId != customerId)
        {
            throw new EntityNotFoundException();
        }

        return order.Adapt<OrderReadDto>();
    }

    public async Task<OrderReadDto> GetOrderByIdForRestaurantAdmin(string restaurantAdminId, string orderId)
    {
        var order = await _orderRepository.GetOrderById(orderId);

        var restaurantsOfAdmin = await _restaurantRepository.GetRestaurants(new RestaurantFilter
        {
            RestaurantAdminId = restaurantAdminId
        });

        if (restaurantsOfAdmin.All(x => x.Id != order.RestaurantId))
        {
            throw new EntityNotFoundException();
        }

        return order.Adapt<OrderReadDto>();
    }

    public async Task<ICollection<OrderReadDto>> GetOrders(OrderFilter orderFilter)
    {
        var orders = await _orderRepository.GetOrders(orderFilter);
        return orders.Adapt<ICollection<OrderReadDto>>();
    }

    public async Task<ICollection<OrderReadDto>> AddStatusToOrders(string restaurantId, AddStatusToOrdersDto addStatusToOrdersDto)
    {
        var orders = await _orderRepository.AddStatusToOrders(restaurantId, addStatusToOrdersDto.OrderIds!, (OrderState)addStatusToOrdersDto.OrderState!);
        return orders.Adapt<ICollection<OrderReadDto>>();
    }
}
