using Grpc.Core;
using MeterReaderWeb.Data;
using MeterReaderWeb.Data.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MeterReaderWeb.Services
{
    public class MeterService : MeterReadingService.MeterReadingServiceBase
    {
        private readonly ILogger<MeterService> _logger;
        private readonly IReadingRepository _repository;
        public MeterService(ILogger<MeterService> logger, IReadingRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async override Task<StatusMessage> AddReading(ReadingPacket request, ServerCallContext context)
        {
            var result = new StatusMessage();

            if (request.Successful == ReadingStatus.Success)
            {
                try
                {
                    foreach (ReadingMessage item in request.Readings)
                    {
                        // Save to the database
                        var reading = new MeterReading()
                        {
                            Value = item.ReadingValue,
                            ReadingDate = item.ReadingTime.ToDateTime(),
                            CustomerId = item.CustomerId,
                        };

                        _repository.AddEntity(reading);
                    }

                    if(await _repository.SaveAllAsync())
                    {
                        result.Success = ReadingStatus.Success;
                    }

                }
                catch (Exception ex)
                {
                    result.Message = "Exception thrown during process";
                    _logger.LogError($"Exception thrown during saving of readings: {ex.Message}");
                };

            }

            return result;
        }
    }
}
