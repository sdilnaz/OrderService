using MassTransit;
using SharedModels.Events;
using DeliveryService.Data;
using DeliveryService.Mappers;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Consumers
{
    public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<OrderCreatedConsumer> _logger;

        public OrderCreatedConsumer(ApplicationDBContext context, ILogger<OrderCreatedConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var orderId = context.Message.Id;
            try
            {
                var existingDeliveryRequest = await _context.DeliveryRequests
                    .FirstOrDefaultAsync(x => x.OrderId == orderId);
                if (existingDeliveryRequest != null)
                {
                    _logger.LogInformation($"Delivery request already exists for OrderID: {orderId}");
                    return;
                }

                var deliveryRequest = context.Message.ToDeliveryRequest();

                await _context.DeliveryRequests.AddAsync(deliveryRequest);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Delivery request created for OrderID: {orderId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating delivery request for OrderID: {orderId}, Error: {ex.Message}");
                throw;
            }
        }
    }
}