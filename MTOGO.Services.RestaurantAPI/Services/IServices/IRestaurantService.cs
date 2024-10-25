using MTOGO.Services.RestaurantAPI.Models;
using MTOGO.Services.RestaurantAPI.Models.Dto;

namespace MTOGO.Services.RestaurantAPI.Services.IServices
{
    public interface IRestaurantService
    {
        Task<int> AddRestaurant(RestaurantDto restaurant);
        Task<int> AddMenuItem(AddMenuItemDto menuItemDto);
        Task<int> UpdateRestaurant(UpdateRestaurantDto updateRestaurantDto);
        Task<int> RemoveMenuItem(int menuItemId);
        Task<int> DeleteRestaurant(int id);
        Task<RestaurantDto?> GetRestaurantById(int id);
        Task<List<RestaurantDto>> GetAllRestaurants();
    }
}
