using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using TradeHubAnalyst.Models;

namespace TradeHubAnalyst.Libraries
{
    public class SqliteDataAccess
    {
        public static List<ItemModel> LoadItems()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<ItemModel>("SELECT * FROM items", new DynamicParameters());
                return output.ToList();
            }
        }

        public static ItemModel LoadSingleItem(string type_id)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<ItemModel>("SELECT type_id, name, volume, sell_price, trade_volume, updated_at FROM items WHERE type_id = " + type_id);
                return output.FirstOrDefault();
            }
        }

        public static void SaveItem(ItemModel item)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("INSERT INTO items (type_id, name, volume, sell_price, trade_volume, updated_at) VALUES (@type_id, @name, @volume, @sell_price, @trade_volume, @updated_at)", item);
            }
        }

        public static void UpdateItem(ItemModel item)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("UPDATE items SET name = @name, volume = @volume, sell_price = @sell_price, trade_volume = @trade_volume, updated_at = @updated_at WHERE type_id = @type_id", item);
            }
        }

        public static Int32 CountItems()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Int32>("SELECT COUNT(*) FROM items", new DynamicParameters());
                return output.FirstOrDefault();
            }
        }

        public static List<StationModel> LoadStations()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<StationModel>("SELECT * FROM stations", new DynamicParameters());
                return output.ToList();
            }
        }

        public static StationModel LoadSingleStation(string id)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<StationModel>("SELECT id, type_id, name FROM stations WHERE id = " + id);
                return output.FirstOrDefault();
            }
        }

        public static void SaveStation(StationModel station)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("INSERT INTO stations (type_id, name) VALUES (@type_id, @name)", station);
            }
        }

        public static void UpdateStation(StationModel station)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("UPDATE stations SET type_id=@type_id, name=@name WHERE id=@id", station);
            }
        }

        public static void DeleteStation(StationModel station)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("DELETE FROM stations WHERE id = @id", station);
            }
        }

        public static ItemFiltersModel LoadItemFilters()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<ItemFiltersModel>("SELECT * FROM item_filters ORDER BY ROWID ASC LIMIT 1", new DynamicParameters());
                return output.FirstOrDefault();
            }
        }

        public static void SaveItemFilters(ItemFiltersModel filters)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute(@"
                    INSERT INTO item_filters (id, max_volume, max_price, min_trade_volume, ignore_zero, filtered_items,
                    selected_hauling_station_id, user_cargo_capacity, user_available_money, selected_station_trading_station_id, updated_item_max_age, max_async_tasks, user_brokers_fee, user_sales_tax)
                    VALUES (@id, @max_volume, @max_price, @min_trade_volume, @ignore_zero, @filtered_items, @selected_hauling_station_id,
                    @user_cargo_capacity, @user_available_money, @selected_station_trading_station_id, @updated_item_max_age, @max_async_tasks, @user_brokers_fee, @user_sales_tax)", filters);
            }
        }

        public static void UpdateItemFilters(ItemFiltersModel filters)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute(@"UPDATE item_filters
                    SET max_volume=@max_volume, max_price=@max_price, min_trade_volume=@min_trade_volume, ignore_zero=@ignore_zero,
                    filtered_items=@filtered_items, selected_hauling_station_id=@selected_hauling_station_id, user_cargo_capacity=@user_cargo_capacity,
                    user_available_money=@user_available_money, selected_station_trading_station_id=@selected_station_trading_station_id,
                    updated_item_max_age=@updated_item_max_age, max_async_tasks=@max_async_tasks, user_brokers_fee=@user_brokers_fee, user_sales_tax=@user_sales_tax
                    WHERE id=@id", filters);
            }
        }

        private static string LoadConnectionString(string name = "Default")
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }
    }
}