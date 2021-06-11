using System;
using YnovEat.Domain.ModelsAggregate.RestaurantAggregate;

namespace YnovEat.Domain.DTO.RestaurantModels.ClosingDatesModels
{
    public class ClosingDateReadDto : IDtoConverter<ClosingDate>
    {
        public string Id { get; set; }
        public DateTime ClosingDateTime { get; set; }
        public string RestaurantId { get; set; }

        public ClosingDateReadDto(ClosingDate closingDate)
        {
            FromEntity(closingDate);
        }

        public void FromEntity(ClosingDate entity)
        {
            Id = entity.Id;
            ClosingDateTime = entity.ClosingDateTime;
            RestaurantId = entity.RestaurantId;
        }
    }
}
