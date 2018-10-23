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

            IProductService productService = GetProductServiceViaDependencyInjection();

            var searchCriteria = new SearchCriteria();
            searchCriteria.PageIndex = 1;
            searchCriteria.PageSize = 10;
            searchCriteria.SearchQuery = "";
            searchCriteria.SortField = "CreatedDate";
            searchCriteria.SortOrder = "DESC";

            var products = productService.GetProducts(searchCriteria);

            return new JsonResult(products);
        }

        private IProductService GetProductServiceViaDependencyInjection()
        {
            return new ProductService();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public interface IProductService
    {
        IEnumerable<ProductModel> GetProducts(SearchCriteria searchCriteria);
    }

    public class ProductService : IProductService
    {
        public IEnumerable<ProductModel> GetProducts(SearchCriteria searchCriteria)
        {

            var param1 = new SqlParameter
            {
                ParameterName = "RecordsPerPage",
                Value = searchCriteria.PageSize,
                DbType = DbType.Int32
            };
            var param2 = new SqlParameter
            {
                ParameterName = "PageNo",
                Value = searchCriteria.PageIndex,
                DbType = DbType.Int32
            };
            var param3 = new SqlParameter
            {
                ParameterName = "KeyWord",
                Value = searchCriteria.SearchQuery,
                DbType = DbType.String
            };
            var param4 = new SqlParameter
            {
                ParameterName = "SortBy",
                Value = searchCriteria.SortField + " " + searchCriteria.SortOrder,
                DbType = DbType.String
            };

            var productsDataTable = GetDataResults("SearchProducts", Startup.ProductsConnectionString, param1, param2, param3, param4);
            //var products = GetDataResults("SearchProducts @RecordsPerPage, @PageNo, @KeyWord, @SortBy", Startup.ProductsConnectionString, param1, param2, param3, param4);





            return GetProductModels(productsDataTable);
        }


        IEnumerable<ProductModel> GetProductModels(DataTable productsDataTable)
        {
            foreach (DataRow row in productsDataTable.Rows)
            {
                yield return PopulateProduct(row);
            }

        }

        DataTable GetDataResults(string sqlQuery, string connectionString, params object[] parameters)
        {



            var results = new DataTable();

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {


                    using (var command = new SqlCommand(sqlQuery, conn))
                    {


                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddRange(parameters);


                        using (var dataAdapter = new SqlDataAdapter(command))
                        {


                            dataAdapter.Fill(results);


                        }
                    }

                }

            //var resultRows = results.AsEnumerable().ToList();
            }
            catch (Exception ex){

                var error = ex.Message;
                var innerError = ex.InnerException;

                results = new DataTable();
            }


            return results;

        }



        ProductModel PopulateProduct(DataRow row)
        {

            var product = new ProductModel
            {
                Id = int.Parse(row["Id"].ToString()),
                TotalRows = int.Parse(row["RECORDCOUNT"].ToString()),
                RowNumber = int.Parse(row["ROW"].ToString()),
                ProductCode = row["ProductCode"].ToString(),
                Description = row["Description"].ToString(),
                CreatedDate = DateTime.Parse(row["CreatedDate"].ToString()),
            };


            return product;
        }
    }

    public class ProductModel
    {
        public int TotalRows { get; set; }
        public int RowNumber { get; set; }
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
