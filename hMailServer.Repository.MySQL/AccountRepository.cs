﻿using System.Linq;
using System.Threading.Tasks;
using Dapper;
using hMailServer.Entities;
using MySql.Data.MySqlClient;

namespace hMailServer.Repository.MySQL
{
    class AccountRepository : IAccountRepository
    {
        private readonly string _connectionString;

        public AccountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Account> GetByIdAsync(long id)
        {
            using (var sqlConnection = new MySqlConnection(_connectionString))
            {
                sqlConnection.Open();

                var accounts = await sqlConnection.QueryAsync<Account>("SELECT * FROM hm_accounts WHERE accountid = @accountid", new
                {
                    accountid = id
                });

                var account = accounts.SingleOrDefault();

                return account;
            }
        }

        public async Task<Account> GetByNameAsync(string address)
        {
            using (var sqlConnection = new MySqlConnection(_connectionString))
            {
                sqlConnection.Open();

                var accounts = await sqlConnection.QueryAsync<Account>("SELECT * FROM hm_accounts WHERE accountaddress = @accountaddress", new
                    {
                        accountaddress = address
                    });

                var account = accounts.SingleOrDefault();

                return account;
            }
        }
    }
}
