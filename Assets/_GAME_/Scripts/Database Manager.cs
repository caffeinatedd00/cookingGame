using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using UnityEngine;
using TMPro;

public class DatabaseManager : MonoBehaviour
{
    public string dbPath = System.IO.Path.Combine(Application.streamingAssetsPath, "game-data.db");
    [SerializeField] TMP_Text money;
    public class Ingredients
    {
        public int id;
        public string name;
        public int cost;
    }

    public class StorageSlot
    {
        public int id;
        public string name;
        public int quantity;
        public string description;
    }

    public class Recipe
    {
        public int id;
        public string name;

        public List<int> ingredientsRequired = new List<int>();
        public string station;
        public int cookingTime;
        public string canOrder;
        public int price;
    }

    public List<Ingredients> ingredients;
    public List<StorageSlot> storageSlots;
    public List<Recipe> recipes;

    void Awake()
    {
        dbPath = System.IO.Path.Combine(Application.persistentDataPath, "game-data.db");

        // If the DB doesn't exist in the writable location, copy it from StreamingAssets
        if (!System.IO.File.Exists(dbPath))
        {
            string sourcePath = System.IO.Path.Combine(Application.streamingAssetsPath, "game-data.db");

            if (System.IO.File.Exists(sourcePath))
            {
                System.IO.File.Copy(sourcePath, dbPath);
                Debug.Log("Database copied to: " + dbPath);
            }
            else
            {
                Debug.LogError("Source database not found in StreamingAssets!");
            }
        }

        ingredients = LoadIngredients();
        storageSlots = LoadStorageSlot();
        recipes = LoadRecipes();
        money.text = MoneyAmount().ToString();
    }

    public List<Ingredients> LoadIngredients() //loads ingredients from the database to a list of ingredeint classed objects
    {
        List<Ingredients> IngredientList = new List<Ingredients>();
        if (System.IO.File.Exists(dbPath))
        {
            Debug.Log("Database exists");
            string connectionString = $"Data Source={dbPath};Version=3;";

            using (IDbConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, name, cost FROM Ingredients ORDER BY id";

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Ingredients ingredient = new Ingredients
                            {
                                id = Convert.ToInt32(reader["id"]),
                                name = reader["name"].ToString(),
                                cost = Convert.ToInt32(reader["cost"])
                            };
                            IngredientList.Add(ingredient);
                            //Debug.Log($"Successfully added: {ingredient.name}");
                        }
                    }
                }
            }
            return IngredientList;
        }
        else
        {
            return null;
        }

    }

    public List<StorageSlot> LoadStorageSlot() //loads ingredients that are in the storage from the database to the storage slots when starting the game
    {
        List<StorageSlot> StorageSlotList = new List<StorageSlot>();
        if (System.IO.File.Exists(dbPath))
        {
            Debug.Log("Database exists");
            string connectionString = $"Data Source={dbPath};Version=3;";

            using (IDbConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Storage.ingredient_id, Ingredients.name, Storage.quantity, Ingredients.description  FROM Storage, Ingredients WHERE Ingredients.id = Storage.ingredient_id;";

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            StorageSlot storageSlots = new StorageSlot
                            {
                                id = Convert.ToInt32(reader["ingredient_id"]),
                                name = reader["name"].ToString(),
                                quantity = Convert.ToInt32(reader["quantity"]),
                                description = reader["description"].ToString()
                            };
                            StorageSlotList.Add(storageSlots);
                            //Debug.Log($"Successfully added: {storageSlots.name}");
                        }
                    }
                }
            }
            return StorageSlotList;
        }
        else
        {
            return null;
        }
    }

    public List<Recipe> LoadRecipes() //loads recipes from database into a list of recipe classed objects
    {
        List<Recipe> RecipeList = new List<Recipe>();
        if (System.IO.File.Exists(dbPath))
        {
            Debug.Log("Database exists");
            string connectionString = $"Data Source={dbPath};Version=3;";

            using (IDbConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Recipe.id, name, Station.station, cooking_time, can_order, price FROM Recipe, Station WHERE Recipe.station_id = station.id ORDER BY Recipe.id";

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Recipe recipe = new Recipe
                            {
                                id = Convert.ToInt32(reader["id"]),
                                name = reader["name"].ToString(),
                                station = reader["station"].ToString(),
                                cookingTime = Convert.ToInt32(reader["cooking_time"]),
                                canOrder = reader["can_order"].ToString(),
                                price = Convert.ToInt32(reader["price"])
                            };
                            RecipeList.Add(recipe);
                        }
                    }
                }
                using (IDbCommand command = connection.CreateCommand())
                {
                    for (int i = 0; i < RecipeList.Count; i++)
                    {
                        command.CommandText = $"SELECT ingredient_id FROM RecipeIngredients WHERE recipe_id = {RecipeList[i].id}";

                        using (IDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                RecipeList[i].ingredientsRequired.Add(reader.GetInt32(0));
                            }
                        }
                    }

                }
                connection.Close();
            }

            return RecipeList;
        }
        else
        {
            return null;
        }

    }

    public int FindQuantity(int id) //finds quantity of how much you get of a requested item through database
    {
        if (System.IO.File.Exists(dbPath))
        {
            string connectionString = $"Data Source={dbPath};Version=3;";
            using (IDbConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT buyQuantity FROM Shop, Ingredients WHERE (Shop.ingredient_id = Ingredients.id) AND (Ingredients.id = {id})";

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt32(0);
                        }
                        else
                        {
                            return 0;
                        }
                    }

                }
            }
        }
        else
        {
            return 0;
        }
    }

    public int FindId(string name) //find id of ingredient with their given name through database
    {
        if (System.IO.File.Exists(dbPath))
        {
            string connectionString = $"Data Source={dbPath};Version=3;";
            using (IDbConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT ingredient_id FROM Shop, Ingredients WHERE (Shop.ingredient_id = Ingredients.id) AND (Ingredients.name = \"{name}\")";

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt32(0);
                        }
                        else
                        {
                            return 0;
                        }
                    }

                }
            }
        }
        else
        {
            return 0;
        }
    }

    public void UpdateQuantity(int id, int addAmount)//update quantity of an item with add or reduce amount through database
    {
        if (System.IO.File.Exists(dbPath))
        {
            string connectionString = $"Data Source={dbPath};Version=3;";
            using (IDbConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = $"UPDATE Storage SET quantity = quantity + {addAmount} WHERE ingredient_id = {id}";
                    int rowsAffected = command.ExecuteNonQuery();
                    Debug.Log($"Update executed. Rows affected: {rowsAffected}");
                }
                connection.Close();
            }
        }
    }

    public void ReduceMoney(int cost)//reduce money of player through database
    {
        if (System.IO.File.Exists(dbPath))
        {
            string connectionString = $"Data Source={dbPath};Version=3;";
            using (IDbConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = $"UPDATE Money SET amount = amount - {cost}";
                    int rowsAffected = command.ExecuteNonQuery();
                    Debug.Log($"Update executed. Rows affected: {rowsAffected}");
                }
                connection.Close();
            }
        }
    }

    public void AddMoney(int cost)//adds money of player through database
    {
        if (System.IO.File.Exists(dbPath))
        {
            string connectionString = $"Data Source={dbPath};Version=3;";
            using (IDbConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = $"UPDATE Money SET amount = amount + {cost}";
                    int rowsAffected = command.ExecuteNonQuery();
                    Debug.Log($"Update executed. Rows affected: {rowsAffected}");
                }
                connection.Close();
            }
        }
    }

    public int MoneyAmount()//gets how much money player have through database
    {
        if (System.IO.File.Exists(dbPath))
        {
            string connectionString = $"Data Source={dbPath};Version=3;";
            using (IDbConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT amount FROM Money";

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt32(0);
                        }
                        else
                        {
                            return 0;
                        }
                    }

                }
            }
        }
        else
        {
            return 0;
        }
    }

    public int FindCost(List<DatabaseManager.Ingredients> ingredients, int id, int min, int max)//find cost of an ingredient using bubble sort
    {
        if (min > max)
        {
            return 0;
        }
        int mid = (max + min) / 2;
        if (ingredients[mid].id == id)
        {
            return ingredients[mid].cost;
        }
        else if (ingredients[mid].id < id)
        {
            return FindCost(ingredients, id, mid + 1, max);
        }
        else
        {
            return FindCost(ingredients, id, min, mid - 1);
        }
    }

    public string FindName(List<DatabaseManager.Ingredients> ingredients, int id, int min, int max)//find name of an ingredient by their id using bubble sort
    {
        if (min > max)
        {
            return "non";
        }
        int mid = (max + min) / 2;
        if (ingredients[mid].id == id)
        {
            return ingredients[mid].name;
        }
        else if (ingredients[mid].id < id)
        {
            return FindName(ingredients, id, mid + 1, max);
        }
        else
        {
            return FindName(ingredients, id, min, mid - 1);
        }
    }

    public List<Recipe> LoadStationRecipes(string station) //load recipes that can be cooked on a given station
    {
        List<Recipe> RecipeListForStation = new List<Recipe>();
        if (System.IO.File.Exists(dbPath))
        {
            Debug.Log("Database exists");
            string connectionString = $"Data Source={dbPath};Version=3;";

            using (IDbConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT Recipe.id FROM Recipe, Station WHERE (Recipe.station_id = Station.id) AND Station.station = \"{station}\"";

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = Convert.ToInt32(reader["id"]);
                            Recipe recipe = FindRecipe(recipes, id, 0, recipes.Count - 1);
                            RecipeListForStation.Add(recipe);
                        }
                    }
                }
                connection.Close();
            }

            return RecipeListForStation;
        }
        else
        {
            return null;
        }
    }

    public List<string> LoadStations() //loads the list of existing stations from database
    {
        List<string> StationList = new List<string>();
        if (System.IO.File.Exists(dbPath))
        {
            Debug.Log("Database exists");
            string connectionString = $"Data Source={dbPath};Version=3;";

            using (IDbConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT station FROM Station";

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string station = reader["station"].ToString();
                            StationList.Add(station);
                        }
                    }
                }
                connection.Close();
            }

            return StationList;
        }
        else
        {
            return null;
        }
    }

    public Recipe FindRecipe(List<DatabaseManager.Recipe> recipes, int id, int min, int max) //finds recipe with their given id using bubble sort
    {
        if (min > max)
        {
            return null;
        }
        int mid = (max + min) / 2;
        if (recipes[mid].id == id)
        {
            return recipes[mid];
        }
        else if (recipes[mid].id < id)
        {
            return FindRecipe(recipes, id, mid + 1, max);
        }
        else
        {
            return FindRecipe(recipes, id, min, mid - 1);
        }
    }

    public Recipe order() //gets a random recipe from the database
    {
        bool foundRecipe = false;
        while (!foundRecipe)
        {
            int randomIndex = UnityEngine.Random.Range(0, recipes.Count);
            if ((recipes[randomIndex].canOrder == "YES"))
            {
                foundRecipe = true;
                return recipes[randomIndex];
            }
        }
        return null;
    }
}
 