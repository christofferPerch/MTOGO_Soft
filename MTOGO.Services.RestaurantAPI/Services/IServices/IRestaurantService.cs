using MTOGO.Services.RestaurantAPI.Models;
using MTOGO.Services.RestaurantAPI.Models.Dto;

namespace MTOGO.Services.RestaurantAPI.Services.IServices
{
    public interface IRestaurantService
    {
        Task<RestaurantDto?> GetRestaurantById(int id);
        Task<List<RestaurantDto>> GetAllRestaurants();
        Task<int> AddRestaurant(RestaurantDto restaurant);
        Task<int> UpdateRestaurant(UpdateRestaurantDto updateRestaurantDto);
        Task<int> DeleteRestaurant(int id);
        Task<int> AddMenuItem(AddMenuItemDto menuItemDto);
        Task<int> RemoveMenuItem(int menuItemId);
    }
}
