using Dapper;
using MTOGO.Services.DataAccess;
using MTOGO.Services.RestaurantAPI.Models.Dto;
using MTOGO.Services.RestaurantAPI.Services.IServices;
using System.Data;

namespace MTOGO.Services.RestaurantAPI.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IDataAccess _dataAccess;
        private readonly ILogger<RestaurantService> _logger;

        public RestaurantService(IDataAccess dataAccess, ILogger<RestaurantService> logger)
        {
            _dataAccess = dataAccess;
            _logger = logger;
        }

        public async Task<int> AddRestaurant(AddRestaurantDto restaurantDto)
        {
            try
            {
                #region parameters
                var parameters = new DynamicParameters();
                parameters.Add("@RestaurantName", restaurantDto.RestaurantName);
                parameters.Add("@LegalName", restaurantDto.LegalName);
                parameters.Add("@VATNumber", restaurantDto.VATNumber);
                parameters.Add("@RestaurantDescription", restaurantDto.RestaurantDescription);
                parameters.Add("@FoodCategory", (int)restaurantDto.FoodCategory);
                parameters.Add("@ContactEmail", restaurantDto.ContactEmail);
                parameters.Add("@ContactPhone", restaurantDto.ContactPhone);

                parameters.Add("@AddressLine1", restaurantDto.Address.AddressLine1);
                parameters.Add("@AddressLine2", restaurantDto.Address.AddressLine2);
                parameters.Add("@City", restaurantDto.Address.City);
                parameters.Add("@ZipCode", restaurantDto.Address.ZipCode);
                parameters.Add("@Country", restaurantDto.Address.Country);

                parameters.Add("@MondayOpening", restaurantDto.OperatingHours.MondayOpening);
                parameters.Add("@MondayClosing", restaurantDto.OperatingHours.MondayClosing);
                parameters.Add("@TuesdayOpening", restaurantDto.OperatingHours.TuesdayOpening);
                parameters.Add("@TuesdayClosing", restaurantDto.OperatingHours.TuesdayClosing);
                parameters.Add("@WednesdayOpening", restaurantDto.OperatingHours.WednesdayOpening);
                parameters.Add("@WednesdayClosing", restaurantDto.OperatingHours.WednesdayClosing);
                parameters.Add("@ThursdayOpening", restaurantDto.OperatingHours.ThursdayOpening);
                parameters.Add("@ThursdayClosing", restaurantDto.OperatingHours.ThursdayClosing);
                parameters.Add("@FridayOpening", restaurantDto.OperatingHours.FridayOpening);
                parameters.Add("@FridayClosing", restaurantDto.OperatingHours.FridayClosing);
                parameters.Add("@SaturdayOpening", restaurantDto.OperatingHours.SaturdayOpening);
                parameters.Add("@SaturdayClosing", restaurantDto.OperatingHours.SaturdayClosing);
                parameters.Add("@SundayOpening", restaurantDto.OperatingHours.SundayOpening);
                parameters.Add("@SundayClosing", restaurantDto.OperatingHours.SundayClosing);

                parameters.Add("@RestaurantId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                #endregion

                await _dataAccess.ExecuteStoredProcedure<int>("AddRestaurant", parameters);

                return parameters.Get<int>("@RestaurantId");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new restaurant");
                throw;
            }
        }

        public async Task<int> AddMenuItem(AddMenuItemDto menuItemDto)
        {
            try
            {
                var sql = @"
                    INSERT INTO MenuItem (RestaurantId, Name, Description, Price, Image)
                    VALUES (@RestaurantId, @Name, @Description, @Price, @Image);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    RestaurantId = menuItemDto.RestaurantId,
                    Name = menuItemDto.Name,
                    Description = menuItemDto.Description,
                    Price = menuItemDto.Price,
                    Image = menuItemDto.Image
                };

                int newMenuItemId = (await _dataAccess.InsertAndGetId<int?>(sql, parameters)) ?? 0;
                return newMenuItemId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new menu item");
                throw;
            }
        }

        public async Task<int> UpdateRestaurant(UpdateRestaurantDto updateRestaurantDto)
        {
            try
            {
                #region parameters
                var parameters = new DynamicParameters();
                parameters.Add("@RestaurantId", updateRestaurantDto.Id);
                parameters.Add("@RestaurantName", updateRestaurantDto.RestaurantName);
                parameters.Add("@LegalName", updateRestaurantDto.LegalName);
                parameters.Add("@VATNumber", updateRestaurantDto.VATNumber);
                parameters.Add("@RestaurantDescription", updateRestaurantDto.RestaurantDescription);
                parameters.Add("@FoodCategory", updateRestaurantDto.FoodCategory.HasValue ? (int)updateRestaurantDto.FoodCategory : (object)DBNull.Value);
                parameters.Add("@ContactEmail", updateRestaurantDto.ContactEmail);
                parameters.Add("@ContactPhone", updateRestaurantDto.ContactPhone);

                if (updateRestaurantDto.Address != null)
                {
                    parameters.Add("@AddressLine1", updateRestaurantDto.Address.AddressLine1);
                    parameters.Add("@AddressLine2", updateRestaurantDto.Address.AddressLine2);
                    parameters.Add("@City", updateRestaurantDto.Address.City);
                    parameters.Add("@ZipCode", updateRestaurantDto.Address.ZipCode);
                    parameters.Add("@Country", updateRestaurantDto.Address.Country);
                }

                if (updateRestaurantDto.OperatingHours != null)
                {
                    parameters.Add("@MondayOpening", updateRestaurantDto.OperatingHours.MondayOpening);
                    parameters.Add("@MondayClosing", updateRestaurantDto.OperatingHours.MondayClosing);
                    parameters.Add("@TuesdayOpening", updateRestaurantDto.OperatingHours.TuesdayOpening);
                    parameters.Add("@TuesdayClosing", updateRestaurantDto.OperatingHours.TuesdayClosing);
                    parameters.Add("@WednesdayOpening", updateRestaurantDto.OperatingHours.WednesdayOpening);
                    parameters.Add("@WednesdayClosing", updateRestaurantDto.OperatingHours.WednesdayClosing);
                    parameters.Add("@ThursdayOpening", updateRestaurantDto.OperatingHours.ThursdayOpening);
                    parameters.Add("@ThursdayClosing", updateRestaurantDto.OperatingHours.ThursdayClosing);
                    parameters.Add("@FridayOpening", updateRestaurantDto.OperatingHours.FridayOpening);
                    parameters.Add("@FridayClosing", updateRestaurantDto.OperatingHours.FridayClosing);
                    parameters.Add("@SaturdayOpening", updateRestaurantDto.OperatingHours.SaturdayOpening);
                    parameters.Add("@SaturdayClosing", updateRestaurantDto.OperatingHours.SaturdayClosing);
                    parameters.Add("@SundayOpening", updateRestaurantDto.OperatingHours.SundayOpening);
                    parameters.Add("@SundayClosing", updateRestaurantDto.OperatingHours.SundayClosing);
                }
                #endregion

                var result = await _dataAccess.ExecuteStoredProcedure<int>("UpdateRestaurant", parameters);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating restaurant with ID {updateRestaurantDto.Id}");
                throw;
            }
        }

        public async Task<int> DeleteRestaurant(int id)
        {
            try
            {
                var sql = @"
                        DELETE FROM MenuItem WHERE RestaurantId = @Id;

                        DELETE FROM Restaurant WHERE Id = @Id;

                        DELETE FROM Address WHERE Id = (SELECT AddressId FROM Restaurant WHERE Id = @Id);

                        DELETE FROM OperatingHours WHERE Id = (SELECT OperatingHoursId FROM Restaurant WHERE Id = @Id);";

                return await _dataAccess.Delete(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting restaurant with ID {id}");
                throw;
            }
        }

        public async Task<int> RemoveMenuItem(int restaurantId, int menuItemId)
        {
            try
            {
                var sql = "DELETE FROM MenuItem WHERE Id = @Id AND RestaurantId = @RestaurantId";
                return await _dataAccess.Delete(sql, new { Id = menuItemId, RestaurantId = restaurantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting menu item with ID {menuItemId} for restaurant ID {restaurantId}");
                throw;
            }
        }

        public async Task<RestaurantDto?> GetRestaurantById(int id)
        {
            try
            {
                var sql = @"
                        SELECT * FROM Restaurant 
                        WHERE Id = @Id";  

                RestaurantDto restaurant = await _dataAccess.GetById<RestaurantDto>(sql, new { Id = id });

                if (restaurant == null)
                {
                    return null;
                }

                var menuItemsSql = "SELECT * FROM MenuItem WHERE RestaurantId = @RestaurantId;";
                var menuItems = await _dataAccess.GetAll<MenuItemDto>(menuItemsSql, new { RestaurantId = id });
                restaurant.MenuItems = menuItems;

                var addressSql = "SELECT * FROM Address WHERE Id = @AddressId;";
                var address = await _dataAccess.GetById<AddressDto>(addressSql, new { AddressId = restaurant.AddressId });
                restaurant.Address = address;

                var operatingHoursSql = "SELECT * FROM OperatingHours WHERE Id = @OperatingHoursId;";
                var operatingHours = await _dataAccess.GetById<OperatingHoursDto>(operatingHoursSql, new { OperatingHoursId = restaurant.OperatingHoursId });
                restaurant.OperatingHours = operatingHours;

                return restaurant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving restaurant with ID {id}");
                throw;
            }
        }

        public async Task<List<RestaurantDto>> GetAllRestaurants()
        {
            try
            {
                var sql = @"SELECT * FROM Restaurant";

                var restaurants = await _dataAccess.GetAll<RestaurantDto>(sql);

                foreach (var restaurant in restaurants)
                {
                    var addressSql = "SELECT * FROM Address WHERE Id = @AddressId;";
                    var address = await _dataAccess.GetById<AddressDto>(addressSql, new { AddressId = restaurant.AddressId });
                    restaurant.Address = address;

                    var operatingHoursSql = "SELECT * FROM OperatingHours WHERE Id = @OperatingHoursId;";
                    var operatingHours = await _dataAccess.GetById<OperatingHoursDto>(operatingHoursSql, new { OperatingHoursId = restaurant.OperatingHoursId });
                    restaurant.OperatingHours = operatingHours;

                    var menuItemsSql = "SELECT * FROM MenuItem WHERE RestaurantId = @RestaurantId;";
                    var menuItems = await _dataAccess.GetAll<MenuItemDto>(menuItemsSql, new { RestaurantId = restaurant.Id });
                    restaurant.MenuItems = menuItems ?? new List<MenuItemDto>();
                }

                return restaurants;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all restaurants");
                throw;
            }
        }
    }
}
