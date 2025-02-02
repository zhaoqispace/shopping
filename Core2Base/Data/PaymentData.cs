﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Core2Base.Data
{
    public class PaymentData:Data
    {
        public static int InsertCardInfo(string cardNumber, string userID)
        {

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"SELECT * FROM [UserPayment] WHERE UserID = @UserID and CreditCardNum=@CardNumber";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("UserID", userID);
                cmd.Parameters.AddWithValue("CardNumber", cardNumber);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    conn.Close();
                    return 0;
                }
                else
                {
                    conn.Close();

                    string query = "INSERT INTO [UserPayment](CreditCardNum,UserID) values (@CardNumber,@UserID)";
                    SqlCommand cmd2 = new SqlCommand(query, conn);
                    cmd2.Parameters.AddWithValue("CardNumber", cardNumber);
                    cmd2.Parameters.AddWithValue("UserID", userID);
                    conn.Open();
                    int i = cmd2.ExecuteNonQuery();
                    conn.Close();

                    return i;
                }
            }
        }

        public static void InsertOrderDetails(string userID)
        {
            //inserting order table
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd"));
                string query = "INSERT INTO [Order](UserID,DateOfPurchase) values (@UserID,@DateOfPurchase)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("UserID", userID);
                cmd.Parameters.AddWithValue("DateOfPurchase", DateTime.Now.ToString("yyyy-MM-dd"));
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            string orderID = "";
            using (SqlConnection conn2 = new SqlConnection(connectionString))
            {
                conn2.Open();
                Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd"));
                string query = "select TOP (1)OrderID from [Order] where UserID=@userID and DateOfPurchase=@date order by OrderID DESC";
                SqlCommand cmd2 = new SqlCommand(query, conn2);
                cmd2.Parameters.AddWithValue("userID", userID);
                cmd2.Parameters.AddWithValue("date", DateTime.Now.ToString("yyyy-MM-dd"));
                SqlDataReader reader = cmd2.ExecuteReader();
                while (reader.Read())
                {
                    orderID = reader["OrderID"].ToString();
                }
                conn2.Close();
            }

            
            //inserting orderdetail table
            using (SqlConnection conn3 = new SqlConnection(connectionString))
            {
                conn3.Open();
                //query data from Cartdetail table
                string sql = @"SELECT * FROM [CartDetails] WHERE UserID =@UserID";

                SqlCommand cmd3 = new SqlCommand(sql, conn3);
                cmd3.Parameters.AddWithValue("UserID", userID);
                SqlDataReader reader1 = cmd3.ExecuteReader();
                while (reader1.Read())
                {
                    string productID = reader1["ProductID"].ToString();
                    Debug.WriteLine(productID);
                    int qty = (int)reader1["Qty"];
                    double price = 0.0;
                    using (SqlConnection conn5 = new SqlConnection(connectionString))
                    {
                        conn5.Open();
                        Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd"));
                        string query = "select  Price from [Product] where ProductID=@ProductID";
                        SqlCommand cmd5 = new SqlCommand(query, conn5);
                        cmd5.Parameters.AddWithValue("ProductID", productID);
                        SqlDataReader reader = cmd5.ExecuteReader();
                        while (reader.Read())
                        {
                            price = (double)reader["Price"];
                        }
                        conn5.Close();
                    }
                    for (int i=0; i < qty; i++)
                    {
                        using (SqlConnection conn4 = new SqlConnection(connectionString))
                        {
                            conn4.Open();
                            string query = "INSERT INTO [OrderDetails](ProductID,BuyPrice,OrderID) values (@ProductID,@BuyPrice,@OrderID)";
                            SqlCommand cmd4 = new SqlCommand(query, conn4);
                            cmd4.Parameters.AddWithValue("ProductID", productID);
                            cmd4.Parameters.AddWithValue("BuyPrice", price);
                            cmd4.Parameters.AddWithValue("OrderID", orderID);
                            cmd4.ExecuteNonQuery();
                            conn4.Close();
                        }

                    }
                }
            }

        }

        public static void DeleteOrderFromCart(string userID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "Delete from [CartDetails] where UserID=@UserID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("UserID", userID);
                cmd.ExecuteNonQuery();
                conn.Close();
            }

        }
    }
}
