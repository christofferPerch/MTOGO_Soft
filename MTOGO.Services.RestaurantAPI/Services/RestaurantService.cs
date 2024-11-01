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
                parameters.Add("@ContactEmail", restaurantDto.ContactEmail);
                parameters.Add("@ContactPhone", restaurantDto.ContactPhone);

                parameters.Add("@AddressLine1", restaurantDto.Address.AddressLine1);
                parameters.Add("@AddressLine2", restaurantDto.Address.AddressLine2);
                parameters.Add("@City", restaurantDto.Address.City);
                parameters.Add("@ZipCode", restaurantDto.Address.ZipCode);
                parameters.Add("@Country", restaurantDto.Address.Country);

                var operatingHoursTable = new DataTable();
                operatingHoursTable.Columns.Add("Day", typeof(int));
                operatingHoursTable.Columns.Add("OpeningHours", typeof(TimeSpan));
                operatingHoursTable.Columns.Add("ClosingHours", typeof(TimeSpan));
                foreach (var hours in restaurantDto.OperatingHours)
                {
                    operatingHoursTable.Rows.Add((int)hours.Day, hours.OpeningHours, hours.ClosingHours);
                }
                parameters.Add("@OperatingHours", operatingHoursTable.AsTableValuedParameter("TVP_OperatingHours"));

                var foodCategoriesTable = new DataTable();
                foodCategoriesTable.Columns.Add("Category", typeof(int));
                foreach (var category in restaurantDto.FoodCategories)
                {
                    foodCategoriesTable.Rows.Add((int)category.Category);
                }
                parameters.Add("@FoodCategories", foodCategoriesTable.AsTableValuedParameter("TVP_FoodCategory"));

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
                    var operatingHoursTable = new DataTable();
                    operatingHoursTable.Columns.Add("Day", typeof(int));
                    operatingHoursTable.Columns.Add("OpeningHours", typeof(TimeSpan));
                    operatingHoursTable.Columns.Add("ClosingHours", typeof(TimeSpan));
                    foreach (var hours in updateRestaurantDto.OperatingHours)
                    {
                        operatingHoursTable.Rows.Add((int)hours.Day, hours.OpeningHours, hours.ClosingHours);
                    }
                    parameters.Add("@OperatingHours", operatingHoursTable.AsTableValuedParameter("TVP_OperatingHours"));
                }

                if (updateRestaurantDto.FoodCategories != null)
                {
                    var foodCategoriesTable = new DataTable();
                    foodCategoriesTable.Columns.Add("Category", typeof(int));
                    foreach (var category in updateRestaurantDto.FoodCategories)
                    {
                        foodCategoriesTable.Rows.Add((int)category.Category);
                    }
                    parameters.Add("@FoodCategories", foodCategoriesTable.AsTableValuedParameter("TVP_FoodCategory"));
                }
                #endregion

                return await _dataAccess.ExecuteStoredProcedure<int>("UpdateRestaurant", parameters);
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

                        DELETE FROM OperatingHours WHERE RestaurantId = @Id;

                        DELETE FROM FoodCategory WHERE RestaurantId = @Id;
            
                        DELETE FROM Restaurant WHERE Id = @Id;

                        DELETE FROM Address 

                        WHERE Id = (
                            SELECT AddressId FROM Restaurant WHERE Id = @Id
                        );";

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
                var sql = "SELECT * FROM Restaurant WHERE Id = @Id";
                var restaurant = await _dataAccess.GetById<RestaurantDto>(sql, new { Id = id });
                if (restaurant == null) return null;

                var menuItemsSql = "SELECT * FROM MenuItem WHERE RestaurantId = @RestaurantId";
                restaurant.MenuItems = await _dataAccess.GetAll<MenuItemDto>(menuItemsSql, new { RestaurantId = id }) ?? new List<MenuItemDto>();

                var addressSql = "SELECT * FROM Address WHERE Id = @AddressId";
                restaurant.Address = await _dataAccess.GetById<AddressDto>(addressSql, new { AddressId = restaurant.AddressId });

                var operatingHoursSql = "SELECT * FROM OperatingHours WHERE RestaurantId = @RestaurantId";
                restaurant.OperatingHours = await _dataAccess.GetAll<OperatingHoursDto>(operatingHoursSql, new { RestaurantId = id }) ?? new List<OperatingHoursDto>();

                var foodCategoriesSql = "SELECT * FROM FoodCategory WHERE RestaurantId = @RestaurantId";
                restaurant.FoodCategories = await _dataAccess.GetAll<FoodCategoryDto>(foodCategoriesSql, new { RestaurantId = id }) ?? new List<FoodCategoryDto>();

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
                var sql = "SELECT * FROM Restaurant";
                var restaurants = await _dataAccess.GetAll<RestaurantDto>(sql) ?? new List<RestaurantDto>();

                foreach (var restaurant in restaurants)
                {
                    var addressSql = "SELECT * FROM Address WHERE Id = @AddressId";
                    restaurant.Address = await _dataAccess.GetById<AddressDto>(addressSql, new { AddressId = restaurant.AddressId });

                    var operatingHoursSql = "SELECT * FROM OperatingHours WHERE RestaurantId = @RestaurantId";
                    restaurant.OperatingHours = await _dataAccess.GetAll<OperatingHoursDto>(operatingHoursSql, new { RestaurantId = restaurant.Id }) ?? new List<OperatingHoursDto>();

                    var menuItemsSql = "SELECT * FROM MenuItem WHERE RestaurantId = @RestaurantId";
                    restaurant.MenuItems = await _dataAccess.GetAll<MenuItemDto>(menuItemsSql, new { RestaurantId = restaurant.Id }) ?? new List<MenuItemDto>();

                    var foodCategoriesSql = "SELECT * FROM FoodCategory WHERE RestaurantId = @RestaurantId";
                    restaurant.FoodCategories = await _dataAccess.GetAll<FoodCategoryDto>(foodCategoriesSql, new { RestaurantId = restaurant.Id }) ?? new List<FoodCategoryDto>();
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
