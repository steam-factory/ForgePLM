using ForgePLM.Contracts.PartCategories;

namespace ForgePLM.Runtime.Services
{
    public interface IPartCategoryService
    {
        Task<List<PartCategoryDto>> GetPartCategoriesAsync();
    }
}