using API.Models;

namespace API.Services.Labels;

public interface ILabelsService
{
    Task<List<LabelTemplate>> GetAllAsync();
    Task<LabelTemplate?> GetByIdAsync(int id);
    Task<LabelTemplate> CreateAsync(LabelTemplate label);
    Task<LabelTemplate?> UpdateAsync(int id, LabelTemplate label);
    Task<bool> DeleteAsync(int id);
}
