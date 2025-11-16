using Partlyx.Services.Dtos;

namespace Partlyx.Services.ServiceInterfaces
{
    public interface ISettingsService
    {
        Task<List<OptionDto>> GetAllOptionsAsync();
        Task<OptionDto?> GetOptionAsync(string settingKey);
        Task<TValue?> GetSettingValueAsync<TValue>(string settingKey);
        Task SetSettingValueAsync(string settingKey, object? value);
    }
}