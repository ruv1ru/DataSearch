using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProductSearchMvc.Models;
using System.Configuration;
using System.Net;
using System.Text;
using System.Web;

namespace ProductSearchMvc.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("GetProducts");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult GetProducts()
        {
            int totalRows;
            var products = new ProductService().GetProducts(new SearchCriteria(), out totalRows);
            return new JsonResult(products);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public interface IProductService
    {
        IEnumerable<ProductModel> GetProducts(SearchCriteria searchCriteria, out int totalRows);
    }

    public class ProductService : IProductService
    {
        public IEnumerable<ProductModel> GetProducts(SearchCriteria searchCriteria, out int totalRows)
        {
            totalRows = 1;
            searchCriteria.PageIndex = 1;
            searchCriteria.PageSize = 5;
            searchCriteria.SearchQuery = "Ch";

            var param1 = new SqlParameter
            {
                ParameterName = "RecordsPerPage",
                Value = 5,
                DbType = DbType.Int32
            };
            var param2 = new SqlParameter
            {
                ParameterName = "PageNo",
                Value = 1,
                DbType = DbType.Int32
            };
            var param3 = new SqlParameter
            {
                ParameterName = "KeyWord",
                Value = "Ch",
                DbType = DbType.String
            };
            var param4 = new SqlParameter
            {
                ParameterName = "SortBy",
                Value = "",
                DbType = DbType.String
            };

            var products = GetDataResults("SearchProducts @RecordsPerPage, @PageNo, @KeyWord, @SortBy", Startup.ProductsConnectionString, param1, param2, param3, param4);

            return new List<ProductModel>();
        }

        private DataTable GetDataResults(string sqlQuery, string connectionString, params object[] parameters)
        {

            var results = new DataTable();

            using (var conn = new SqlConnection(connectionString)){
                using (var command = new SqlCommand(sqlQuery, conn)){
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddRange(parameters);
                    using (var dataAdapter = new SqlDataAdapter(command)){
                        dataAdapter.Fill(results);
                    }
                }

            }


            //using (var command = new SqlCommand(sqlQuery, conn))
            //using (var dataAdapter = new SqlDataAdapter(command))
                //dataAdapter.Fill(results);


            /*
             using (SqlCommand cmd = new SqlCommand("sp_Add_contact", con)) {
      cmd.CommandType = CommandType.StoredProcedure;

      cmd.Parameters.Add("@FirstName", SqlDbType.VarChar).Value = txtFirstName.Text;
      cmd.Parameters.Add("@LastName", SqlDbType.VarChar).Value = txtLastName.Text;

      con.Open();
      cmd.ExecuteNonQuery();
    }
*/
            return results;

        }



        ProductModel PopulateProduct(DataRow row)
        {

            var product = new ProductModel
            {
                ProductCode = row["ProductCode"].ToString(),
                Description = row["Description"].ToString(),
                CreatedDate = DateTime.Parse(row["CreatedDate"].ToString())
            };


            return product;
        }
    }

    public class ProductModel
    {
        public int Id { get; set; }
        public string ProductCode { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class SearchCriteria
    {
        public string SearchQuery { get; set; }
        public int PageIndex { get; set; } 
        public int PageSize { get; set; }
        public string SortField { get; set; }
        public string SortOrder { get; set; }
    }
}
