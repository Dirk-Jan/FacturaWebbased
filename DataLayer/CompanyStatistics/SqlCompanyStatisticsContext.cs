﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using Task = Models.Task;

namespace DataLayer
{
    public class SqlCompanyStatisticsContext : DbHandler, ICompanyStatisticsContext
    {
        CompanyStatistics companyStats = new CompanyStatistics();
        List<Customer> customers = new List<Customer>();

        private Task TasksFromDataRow(DataRow datarow)
        {
            return new Task(
                id: Convert.ToInt16(datarow["taskID"]),
                totalAmountOfAllSimilarTasks: Convert.ToDecimal(datarow["totalTaskIncome"])
            );
        }

        private DateTime ReadPaymentDate(string date)
        {
            if (date == string.Empty)
            {
                return new DateTime();
            }
            else
            {
                return DateTime.Parse(date);
            }
        }

        private decimal ReadTotalPrice(string price)
        {
            if (price == string.Empty)
            {
                return 0;
            }
            else
            {
                return Convert.ToDecimal(price);
            }
        }

        private int ReadId(string id)
        {
            if (id == string.Empty)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(id);
            }
        }

        private Invoice StatisticsCustomerInvoice(DataRow datarow)
        {
            var customer = new Customer(
                firstname: datarow["firstname"].ToString(),
                preposition: datarow["prefix"].ToString(),
                lastname: datarow["lastname"].ToString()
            );

            return new Invoice(
                id: ReadId(datarow["id"].ToString()),
                datePayed: ReadPaymentDate(datarow["datePayed"].ToString()),
                totalPrice: ReadTotalPrice(datarow["totalprice"].ToString()),
                customer: customer
                );
        }

        private void StatisticsFromDataRow(DataRow datarow)
        {
            var customer = new Customer(
                    id: Convert.ToInt16(datarow["customerId"]),
                    totalpriceofallinv: Convert.ToDecimal(datarow["TotalAmountByCustomer"])
                );
            customers.Add(customer);
        }


        public CompanyStatistics GetTotalIncomeByYear(int year)
        {
            CompanyStatistics companyStatistics = new CompanyStatistics();
            try
            {
                var dataTable = GetDataByView($"SELECT dbo.funcGetTotalInvoicePriceByYear('{year}')");
                if (dataTable.Rows.Count > 0)
                {
                    string test = dataTable.Rows[0][0].ToString();
                    if (test == "")
                    {
                        companyStatistics.TotalIncomeByYear = 0;
                    }
                    else
                        companyStatistics.TotalIncomeByYear = Convert.ToDecimal(dataTable.Rows[0][0].ToString());
                    
                }
            }
            catch (SqlException sqlEx)
            {
                throw new InvoiceException(
                    $"Neem contact op met de beheerder onder sqldatabase exceptionCode:{sqlEx.Number}");
            }
            catch (Exception ex)
            {
                throw new InvoiceException($"Neem contact op met de beheerder onder exceptionCode:{ex.HResult}");
            }
            return companyStatistics;
        }

        public CompanyStatistics GetTop3Customers()
        {
            try
            {
                var dataTable = GetDataByView($"SELECT * FROM vwGetTotalPricePerCustomer");
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        StatisticsFromDataRow(row);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new InvoiceException(
                    $"Neem contact op met de beheerder onder sqldatabase exceptionCode:{sqlEx.Number}");
            }
            catch (Exception ex)
            {
                throw new InvoiceException($"Neem contact op met de beheerder onder exceptionCode:{ex.HResult}");
            }
            //TODO: kijken naar property
            companyStats.TopCustomers = customers;
            return companyStats;
        }

        public List<Task> GetTop3Tasks(string year)
        {
            var tasks = new List<Task>();
            try
            {
                var dataTable = GetDataByView($"SELECT * FROM funcTop3TasksByYear('{year}')");
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        tasks.Add(TasksFromDataRow(row));
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new InvoiceException(
                    $"Neem contact op met de beheerder onder sqldatabase exceptionCode:{sqlEx.Number}");
            }
            catch (Exception ex)
            {
                throw new InvoiceException($"Neem contact op met de beheerder onder exceptionCode:{ex.HResult}");
            }

            return tasks;
        }

        public CompanyStatistics GetCustomersWithInvoices()
        {
            List<Invoice> invoices = new List<Invoice>();
            try
            {
                var dataTable = GetDataByView($"SELECT * FROM vwCustomersWithInvoice");
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        invoices.Add(StatisticsCustomerInvoice(row));
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new InvoiceException(
                    $"Neem contact op met de beheerder onder sqldatabase exceptionCode:{sqlEx.Number}");
            }
            catch (Exception ex)
            {
                throw new InvoiceException($"Neem contact op met de beheerder onder exceptionCode:{ex.HResult}");
            }
            
            return new CompanyStatistics(invoices); 
        }
    }
}
