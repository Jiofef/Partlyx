using Partlyx.Core.Settings;

namespace Partlyx.Infrastructure.Data.Interfaces
{
    public interface ISettingsRepository
    {
        Task<int> AddOptionAsync(OptionEntity option);
        Task DeleteOptionAsync(string key);
        Task DeleteOptionByIdAsync(int id);
        Task<List<OptionEntity>> GetAllOptionsAsync();
        Task<string?> GetDeserializedOptionValueStringAsync(string key);
        Task<OptionEntity?> GetOptionAsync(string key);
        Task<OptionEntity?> GetOptionByIdAsync(int id);
        Task<string?> GetOptionValueAsync(string key);
        Task<OptionEntity?> SetOptionJsonValueAndGetItAsync(string key, string value);
        Task SetOptionJsonValueAsync(string key, string value);
    }
}