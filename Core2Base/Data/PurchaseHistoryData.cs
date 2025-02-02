﻿using Core2Base.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Core2Base.Data
{
    public class PurchaseHistoryData : Data
    {
        public static List<PurchaseHistory> GetPurchaseHistory(string userID)
        {
            //declare purchaseList
            List<PurchaseHistory> purchaseHistories = new List<PurchaseHistory>();
            //declare connection, sqlcommand, reader
            SqlConnection sqlConn = null;
            SqlCommand cmd;
            SqlDataReader reader;

            using (sqlConn = new SqlConnection(connectionString))
            {
                sqlConn.Open();
                //query for product
                string sql = @"Select distinct(p.ProductName),p.ProductID,p.ProductDesc, p.Price,p.ProductImg  from Product p, OrderDetails od , [dbo].[Order] o where p.ProductID=od.ProductID and od.OrderID=o.OrderID and o.UserID =@UserID";

                cmd = new SqlCommand(sql, sqlConn);
                cmd.Parameters.AddWithValue("@UserID", userID);
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    //count for Quantity
                    int count = 0;
                    //declare list of activation and date
                    List<string> activationStatus = new List<string> { };
                    List<string> dateOfPurchase = new List<string> { };
                    bool flag = false;

                    using (SqlConnection conn2 = new SqlConnection(connectionString))
                    {
                        //getting purchase date and activation status
                        string orderSql = "Select CONVERT(varchar(10),o.DateOfPurchase,101) as DateOfPurchase, od.ActivationCode,p.Price from Product p, OrderDetails od , [dbo].[Order] o where p.ProductID=od.ProductID and od.OrderID=o.OrderID and p.ProductID=@ProductID and o.UserID=@UserID";
                        using (SqlCommand cmd_order = new SqlCommand(orderSql, conn2))
                        {
                            SqlParameter param = new SqlParameter();
                            param.ParameterName = "@ProductID";
                            param.Value = reader["ProductID"];

                            // add new parameter to command object
                            cmd_order.Parameters.Add(param);

                            cmd_order.Parameters.AddWithValue("@UserID", userID);

                            //open connection
                            conn2.Open();
                            using (SqlDataReader dr_order = cmd_order.ExecuteReader())
                            {
                                while (dr_order.Read())
                                {
                                    //adding object into list
                                    Debug.WriteLine(dr_order["DateOfPurchase"].ToString());
                                    if (!dateOfPurchase.Contains(dr_order["DateOfPurchase"].ToString()))
                                    {
                                        dateOfPurchase.Add(dr_order["DateOfPurchase"].ToString());
                                    }
                                    if (flag == false)
                                    {
                                        activationStatus.Add(dr_order["ActivationCode"].ToString());
                                        flag = true;
                                    }
                                    else if(dateOfPurchase[0].ToString()== dr_order["DateOfPurchase"].ToString())
                                    {
                                        activationStatus.Add(dr_order["ActivationCode"].ToString());
                                    }
                                    else
                                    {
                                        Debug.WriteLine("There is no list");
                                    }
                                    
                                    count += 1;
                                }
                            }

                        }

                    }
                    //adding data to purchaseHistory model
                    PurchaseHistory product = new PurchaseHistory()
                    {
                        Image = reader["ProductImg"].ToString(),
                        ProductID = Convert.ToString(reader["ProductID"]),
                        ProductName = (string)reader["ProductName"],
                        ProductDescription = (string)reader["ProductDesc"],
                        ActivationStatus = activationStatus,
                        Quantity = count,
                        Price = (double)reader["Price"],
                        DateOfPurchase = dateOfPurchase
                    };
                    purchaseHistories.Add(product);
                }
                reader.Close(); // <- too easy to forget
                reader.Dispose(); // <- too easy to forget
                sqlConn.Close(); // <- too easy to forget

                return purchaseHistories;
            }
        }

        public static List<string> GetDateAndActivation(string date, string productID, string userID)
        {
            SqlConnection sqlConn = null;
            SqlCommand cmd;
            SqlDataReader reader;
            List<string> activationCode = new List<string> { };
            using (sqlConn = new SqlConnection(connectionString))
            {
                //query for date and activation
                string sql = @"select * from OrderDetails od, [dbo].[Order] o where o.OrderID=od.OrderID and o.DateOfPurchase=@Date and od.ProductID=@ProductID and o.UserID=@UserID";

                cmd = new SqlCommand(sql, sqlConn);

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@Date";
                param.Value = date;

                // add new parameter to command object
                cmd.Parameters.Add(param);

                SqlParameter param2 = new SqlParameter();
                param2.ParameterName = "@ProductID";
                param2.Value = productID;
                cmd.Parameters.Add(param2);

                cmd.Parameters.AddWithValue("@UserID", userID);

                sqlConn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    activationCode.Add(reader["ActivationCode"].ToString());
                }
            }

            return activationCode;
        }
    }
}
