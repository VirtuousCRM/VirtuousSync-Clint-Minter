using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;

namespace Sync
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Sync().GetAwaiter().GetResult();
        }

        private static async Task Sync()
        {
            var apiKey = "v_eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiN2VhYTBhNTQtYTBiZC00OTNlLWFjNDMtZjNjZGEwZmVlNWQ5IiwiZXhwIjoyMTQ3NDgzNjQ3LCJpc3MiOiJodHRwczovL2FwcC52aXJ0dW91c3NvZnR3YXJlLmNvbSIsImF1ZCI6Imh0dHBzOi8vYXBpLnZpcnR1b3Vzc29mdHdhcmUuY29tIn0.oN0bfmYMS7lPxGtVH3ouEVhD0Kuzoqa2nAnuvPTyPpk";
            var configuration = new Configuration(apiKey);
            var virtuousService = new VirtuousService(configuration);

            var skip = 0;
            var take = 100;
            var maxContacts = 1000;
            var hasMore = true;

            var virtuousContacts = new List<AbbreviatedContact>();

            do
            {
                var contacts = await virtuousService.GetContactsAsync(skip, take);
                skip += take;
                virtuousContacts.AddRange(contacts.List);
                hasMore = skip > maxContacts;
            }
            while (!hasMore);

            //First remove all contacts to ensure duplicates won't cause insert errors
            await DeleteContacts();
            //Save all contacts retrieved from API call
            await SaveContactsToDb(virtuousContacts);
            
        }

        /// <summary>
        /// Retreives connection string for database
        /// This is an Azure SQL database 
        /// </summary>
        /// <returns></returns>
        private static string GetConnectionString()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "clint-dev.database.windows.net";
            builder.UserID = "tcminter";
            builder.Password = "Virtuous_Test24";
            builder.InitialCatalog = "VirtuousDevProject ";

            return builder.ToString();
        }

        private static async Task<bool> DeleteContacts()
        {
            try { 
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                {
                    String sql = "DELETE FROM [dbo].[Contacts]";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        var deletedCount = await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch(SqlException e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

            return true;
        }

        private static async Task<int> SaveContactsToDb(List<AbbreviatedContact> contacts)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                {
                    String sql = "INSERT INTO [dbo].[Contacts](Id, Name, ContactType, ContactName, Address, Email, Phone) " +
                        "VALUES(@Id, @Name, @ContactType, @ContactName, @Address, @Email, @Phone)";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add("@Id", SqlDbType.Int);
                        command.Parameters.Add("@Name", SqlDbType.VarChar, 100);
                        command.Parameters.Add("@ContactType", SqlDbType.VarChar, 25);
                        command.Parameters.Add("@ContactName", SqlDbType.VarChar, 100);
                        command.Parameters.Add("@Address", SqlDbType.VarChar, 250);
                        command.Parameters.Add("@Email", SqlDbType.VarChar, 100);
                        command.Parameters.Add("@Phone", SqlDbType.VarChar, 25);

                        connection.Open();

                        foreach(AbbreviatedContact contact in contacts)
                        {
                            command.Parameters["@Id"].Value = contact.Id;
                            command.Parameters["@Name"].Value = contact.Name;
                            command.Parameters["@ContactType"].Value = contact.ContactType;
                            command.Parameters["@ContactName"].Value = contact.ContactName;
                            command.Parameters["@Address"].Value = contact.Address;
                            command.Parameters["@Email"].Value = contact.Email;
                            command.Parameters["@Phone"].Value = contact.Phone;

                            await command.ExecuteNonQueryAsync();
                        }

                        connection.Close();
                    }
                }

                return contacts.Count;
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            return 0;
        }
    }
}
