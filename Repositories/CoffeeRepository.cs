﻿using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using CoffeeShop.Models;

namespace CoffeeShop.Repositories
{
    public class CoffeeRepository : ICoffeeRepository
    {
        private readonly string _connectionString;
        public CoffeeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private SqlConnection Connection
        {
            get { return new SqlConnection(_connectionString); }
        }

        public List<Coffee> GetAll()
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id, c.Title, c.BeanVarietyId, bv.Name AS 'BeanVariety_Name' 
                                       FROM Coffee c
                                       JOIN BeanVariety bv ON c.BeanVarietyId = bv.Id;";
                    var reader = cmd.ExecuteReader();
                    var coffees = new List<Coffee>();
                    while (reader.Read())
                    {
                        var coffee = new Coffee()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            BeanVarietyId = reader.GetInt32(reader.GetOrdinal("BeanVarietyId")),
                            BeanVariety = new BeanVariety
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("BeanVarietyId")),
                                Name = reader.IsDBNull(reader.GetOrdinal("BeanVariety_Name")) ? null : reader.GetString(reader.GetOrdinal("BeanVariety_Name"))
                            }
                        };

                        reader.Close();

                        return coffees;
                    }
                }
            }
        }

            public Coffee Get(int id)
            {
                using (var conn = Connection)
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                                       SELECT c.Id, c.Title, c.BeanVarietyId, bv.Name AS 'BeanVariety_Name' 
                                       FROM Coffee c
                                       JOIN BeanVariety bv ON c.BeanVarietyId = bv.Id
                                       WHERE Id = @id;";
                        cmd.Parameters.AddWithValue("@id", id);

                        var reader = cmd.ExecuteReader();

                        Coffee coffee = null;
                        if (reader.Read())
                        {
                            coffee = new Coffee()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                BeanVarietyId = reader.GetInt32(reader.GetOrdinal("BeanVarietyId")),
                                BeanVariety = new BeanVariety
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("BeanVarietyId")),
                                    Name = reader.IsDBNull(reader.GetOrdinal("BeanVariety_Name")) ? null : reader.GetString(reader.GetOrdinal("BeanVariety_Name"))
                                }
                            };
                        }

                        reader.Close();

                        return coffee;
                    }
                }
            }

        public void Add(Coffee coffee)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO Coffee (Title, BeanVarietyId)
                        OUTPUT INSERTED.ID
                        VALUES (@title, @beanvarietyid)";
                    cmd.Parameters.AddWithValue("@name", coffee.Title);
                    cmd.Parameters.AddWithValue("@region", coffee.BeanVarietyId);

                    coffee.Id = (int)cmd.ExecuteScalar();
                }
            }
        }

        public void Update(Coffee coffee)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        UPDATE Coffee 
                           SET Title = @title, 
                               BeanVarietyId = @beanvarietyid
                         WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@id", coffee.Id);
                    cmd.Parameters.AddWithValue("@name", coffee.Title);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Coffee WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

    }
}
