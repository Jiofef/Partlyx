using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.ServiceInterfaces;
using System.Text.Json;

namespace Partlyx.Services.ServiceImplementations
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _repository;
        private readonly IEventBus _bus;
        public SettingsService(ISettingsRepository repository, IEventBus bus)
        {
            _repository = repository;
            _bus = bus;
        }

        public async Task<TValue?> GetSettingValueAsync<TValue>(string settingKey)
        {
            var jsonValue = await _repository.GetOptionValueAsync(settingKey);

            if (jsonValue == null)
                return default;

            var result = JsonSerializer.Deserialize<TValue>(jsonValue);
            return result;
        }

        public async Task<OptionDto?> GetOptionAsync(string settingKey)
        {
            var option = await _repository.GetOptionAsync(settingKey);
            if (option == null) return null;

            var dto = option.ToDto();
            return dto;
        }

        public async Task SetSettingValueAsync(string settingKey, object? value)
        {
            var jsonValue = JsonSerializer.Serialize(value);
            var option = await _repository.SetOptionJsonValueAndGetItAsync(settingKey, jsonValue);

            if (option == null) return;

            var dto = option.ToDto(value);
            var @event = new SettingChangedEvent(dto);
            _bus.Publish(@event);
        }

        public async Task<List<OptionDto>> GetAllOptionsAsync()
        {
            var options = await _repository.GetAllOptionsAsync();
            var optionsDtoList = options.Select(oEntity => oEntity.ToDto()).ToList();
            return optionsDtoList;
        }
    }
}
