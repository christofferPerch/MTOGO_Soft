using Dapper;
using MTOGO.Services.DataAccess;
using MTOGO.Services.RestaurantAPI.Models;
using MTOGO.Services.RestaurantAPI.Models.Dto;
using MTOGO.Services.RestaurantAPI.Services.IServices;
using System.Data;
using Microsoft.Extensions.Logging;

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

        public async Task<int> AddRestaurant(RestaurantDto restaurant)
        {
            try
            {
                var menuItemsTable = new DataTable();
                menuItemsTable.Columns.Add("Name", typeof(string));
                menuItemsTable.Columns.Add("Description", typeof(string));
                menuItemsTable.Columns.Add("Price", typeof(decimal));

                foreach (var item in restaurant.MenuItems)
                {
                    menuItemsTable.Rows.Add(item.Name, item.Description, item.Price);
                }

                var feeStructuresTable = new DataTable();
                feeStructuresTable.Columns.Add("MinimumOrderAmount", typeof(decimal));
                feeStructuresTable.Columns.Add("MaximumOrderAmount", typeof(decimal));
                feeStructuresTable.Columns.Add("FeePercentage", typeof(decimal));

                foreach (var fee in restaurant.FeeStructures)
                {
                    feeStructuresTable.Rows.Add(fee.MinimumOrderAmount, fee.MaximumOrderAmount, fee.FeePercentage);
                }

                var parameters = new DynamicParameters();
                parameters.Add("@Name", restaurant.Name);
                parameters.Add("@Street", restaurant.Address.Street);
                parameters.Add("@City", restaurant.Address.City);
                parameters.Add("@State", restaurant.Address.State);
                parameters.Add("@PostalCode", restaurant.Address.PostalCode);
                parameters.Add("@Country", restaurant.Address.Country);
                parameters.Add("@ContactInformation", restaurant.ContactInformation);
                parameters.Add("@OpeningTime", restaurant.OperatingHours.OpeningTime);
                parameters.Add("@ClosingTime", restaurant.OperatingHours.ClosingTime);
                parameters.Add("@MenuItems", menuItemsTable.AsTableValuedParameter("TVP_MenuItem"));
                parameters.Add("@FeeStructures", feeStructuresTable.AsTableValuedParameter("TVP_FeeStructure"));
                parameters.Add("@RestaurantId", dbType: DbType.Int32, direction: ParameterDirection.Output);

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
                    INSERT INTO MenuItem (RestaurantId, Name, Description, Price)
                    VALUES (@RestaurantId, @Name, @Description, @Price);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    RestaurantId = menuItemDto.RestaurantId,
                    Name = menuItemDto.Name,
                    Description = menuItemDto.Description,
                    Price = menuItemDto.Price
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
                var sql = @"
                    UPDATE a
                    SET a.Street = @Street, 
                        a.City = @City, 
                        a.State = @State, 
                        a.PostalCode = @PostalCode, 
                        a.Country = @Country
                    FROM Address a
                    INNER JOIN Restaurant r ON r.AddressId = a.Id
                    WHERE r.Id = @RestaurantId;

                    UPDATE o
                    SET o.OpeningTime = @OpeningTime, 
                        o.ClosingTime = @ClosingTime
                    FROM OperatingHours o
                    INNER JOIN Restaurant r ON r.OperatingHoursId = o.Id
                    WHERE r.Id = @RestaurantId;

                    UPDATE f
                    SET f.MinimumOrderAmount = @MinimumOrderAmount, 
                        f.MaximumOrderAmount = @MaximumOrderAmount, 
                        f.FeePercentage = @FeePercentage
                    FROM FeeStructure f
                    WHERE f.RestaurantId = @RestaurantId;

                    UPDATE Restaurant
                    SET Name = @Name, 
                        ContactInformation = @ContactInformation
                    WHERE Id = @RestaurantId;";

                var parameters = new
                {
                    RestaurantId = updateRestaurantDto.Id,
                    updateRestaurantDto.Address.Street,
                    updateRestaurantDto.Address.City,
                    updateRestaurantDto.Address.State,
                    updateRestaurantDto.Address.PostalCode,
                    updateRestaurantDto.Address.Country,
                    updateRestaurantDto.OperatingHours.OpeningTime,
                    updateRestaurantDto.OperatingHours.ClosingTime,
                    updateRestaurantDto.FeeStructure.MinimumOrderAmount,
                    updateRestaurantDto.FeeStructure.MaximumOrderAmount,
                    updateRestaurantDto.FeeStructure.FeePercentage,
                    updateRestaurantDto.Name,
                    updateRestaurantDto.ContactInformation
                };

                return await _dataAccess.Update(sql, parameters);
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

        public async Task<int> RemoveMenuItem(int menuItemId)
        {
            try
            {
                var sql = "DELETE FROM MenuItem WHERE Id = @Id";
                return await _dataAccess.Delete(sql, new { Id = menuItemId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting menu item with ID {menuItemId}");
                throw;
            }
        }

        public async Task<RestaurantDto?> GetRestaurantById(int id)
        {
            try
            {
                var sql = @"
                    SELECT r.*, a.*, o.*
                    FROM Restaurant r
                    INNER JOIN Address a ON r.AddressId = a.Id
                    INNER JOIN OperatingHours o ON r.OperatingHoursId = o.Id
                    WHERE r.Id = @Id;";

                var restaurant = await _dataAccess.GetById<RestaurantDto>(sql, new { Id = id });

                if (restaurant == null)
                {
                    return null;
                }

                var menuItemsSql = "SELECT * FROM MenuItem WHERE RestaurantId = @RestaurantId;";
                var menuItems = await _dataAccess.GetAll<MenuItemDto>(menuItemsSql, new { RestaurantId = id });
                restaurant.MenuItems = menuItems;

                var feeStructuresSql = "SELECT * FROM FeeStructure WHERE RestaurantId = @RestaurantId;";
                var feeStructures = await _dataAccess.GetAll<FeeStructureDto>(feeStructuresSql, new { RestaurantId = id });
                restaurant.FeeStructures = feeStructures;

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
                var sql = @"
            SELECT r.Id, r.Name, r.ContactInformation, r.AddressId, r.OperatingHoursId
            FROM Restaurant r";


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

                    var feeStructuresSql = "SELECT * FROM FeeStructure WHERE RestaurantId = @RestaurantId;";
                    var feeStructures = await _dataAccess.GetAll<FeeStructureDto>(feeStructuresSql, new { RestaurantId = restaurant.Id });
                    restaurant.FeeStructures = feeStructures;
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
