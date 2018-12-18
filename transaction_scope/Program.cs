using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Transactions;

namespace transaction_scope
{
    class Program
    {
        static void Main(string[] args)
        {
            string strCon = ConfigurationManager.AppSettings["ConnectionString"];
            List<Doctor> doctors = new List<Doctor>
                {
                    new Doctor
                    {
                        Email="ivan@mail.ru",
                        FirstName="Ivan",
                        LastName="Kable",
                        Kabinet="23B",
                        Status="Main doctor"
                    },
                    new Doctor
                    {
                        Email="peter@mail.ru",
                        FirstName="Petro",
                        LastName="Gugle",
                        Kabinet="185B",
                        Status="Travma"
                    },
                    new Doctor
                    {
                        Email="jan@gmail.com",
                        FirstName="Janna",
                        LastName="Stuart",
                        Kabinet="23B",
                        Status="Med.sister"
                    }
                };
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (SqlConnection con = new SqlConnection(strCon))
                    {
                        con.Open();
                        SqlCommand command = new SqlCommand();
                        command.Connection = con;
                        string query = "";
                        foreach (var doctor in doctors)
                        {

                            int statusId = 0;
                            query = "SELECT Id FROM tblStatus " +
                                $"WHERE Name='{doctor.Status}'";
                            command.CommandText = query;
                            SqlDataReader reader = command.ExecuteReader();
                            if (reader.Read())
                            {
                                statusId = int
                                    .Parse(reader["Id"].ToString());
                            }
                            reader.Close();
                            if (statusId == 0)
                            {
                                query = "INSERT INTO [dbo].[tblStatus] " +
                                    $"([Name]) VALUES ('{doctor.Status}')";
                                command.CommandText = query;
                                var count = command.ExecuteNonQuery();
                                if (count == 1)
                                {
                                    query = "SELECT SCOPE_IDENTITY() AS LastId";
                                    command.CommandText = query;
                                    reader = command.ExecuteReader();
                                    if (reader.Read())
                                    {
                                        statusId = int
                                            .Parse(reader["LastId"].ToString());
                                        Console.WriteLine("LastId = {0}", statusId);
                                    }
                                    reader.Close();
                                }
                                else { throw new Exception($"Проблема при добавлені статуса {doctor.FirstName}"); }
                            }

                            if (string.IsNullOrEmpty(doctor.Email))
                                throw new Exception($"Помилка при добавелі лікаря {doctor.Email}");
                            query = "INSERT INTO [dbo].[tblDoctor] " +
                                "([StatusId],[FirstName],[LastName],[Email],[Kabinet]) " +
                                $"VALUES ({statusId},'{doctor.FirstName}'," +
                                $"'{doctor.LastName}','{doctor.Email}','{doctor.Kabinet}')";
                            command.CommandText = query;
                            var res = command.ExecuteNonQuery();
                            if (res != 1)
                                throw new Exception($"Помилка при добавелі лікаря {doctor.Email}");
                        }


                    }

                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }
    }
}
