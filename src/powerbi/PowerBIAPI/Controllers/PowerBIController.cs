using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PowerBIAPI.Models;
using System.Text;
using TRex.Metadata;

namespace PowerBIAPI.Controllers
{
    public class PowerBiController : ApiController
    {
        public static AuthResult Authorization;
        public AuthenticationController AuthHelper = new AuthenticationController();

        [HttpPost, Route("api/AddRows")]
        [Metadata("Add Rows (string)", "Add rows to a Power BI dataset from a string")]
        [Swashbuckle.Swagger.Annotations.SwaggerResponse(HttpStatusCode.OK, "Added rows", typeof(PowerBIRows))]
        public async Task<HttpResponseMessage> AddRowsString([FromUri][Metadata("Dataset ID", "The Dataset ID from Power BI", VisibilityType.Default)] string datasetId, 
            [FromUri][Metadata("Table Name", "The name of the Table from Power BI", VisibilityType.Default)] string table, 
            [FromBody][Metadata("Rows", "Comma-separated list of JSON Objects for each row to be inputted")] string rows)
        {
            PowerBIRows pbiRows = ConvertRowStringToPowerBIRows(rows);
            return await AddRowsShared(datasetId, table, pbiRows);
        }

        [HttpPost, Route("api/AddJsonRows")]
        [Metadata("Add Rows (json)", "Add rows to a Power BI dataset from a string")]
        [Swashbuckle.Swagger.Annotations.SwaggerResponse(HttpStatusCode.OK, "Added rows", typeof(PowerBIRows))]
        public async Task<HttpResponseMessage> AddJsonRows([FromUri][Metadata("Dataset ID", "The Dataset ID from Power BI", VisibilityType.Default)] string datasetId,
    [FromUri][Metadata("Table Name", "The name of the Table from Power BI", VisibilityType.Default)] string table,
    [FromBody][Metadata("Rows", "Comma-separated list of JSON Objects for each row to be inputted", VisibilityType.Default)] Row row)
        {
            var rowsString = JsonConvert.SerializeObject(row);
            PowerBIRows pbiRows = ConvertRowStringToPowerBIRows(rowsString);
            return await AddRowsShared(datasetId, table, pbiRows);
        }

        [HttpPost, Route("api/AddRowsArray")]
        [Metadata("Add Rows (array)", "Add rows to a Power BI dataset from an array")]
        [Swashbuckle.Swagger.Annotations.SwaggerResponse(HttpStatusCode.OK, "Added rows", typeof(PowerBIRows))]
        public async Task<HttpResponseMessage> AddRowsString([FromUri][Metadata("Dataset ID", "The Dataset ID from Power BI", VisibilityType.Default)] string datasetId,
            [FromUri][Metadata("Table Name", "The name of the Table from Power BI", VisibilityType.Default)] string table,
            [FromBody][Metadata("Rows Array", "Array of rows to add to Power BI", VisibilityType.Default)] JArray rowsArray)
        {
            PowerBIRows pbiRows = new PowerBIRows { rows = rowsArray.ToList() };
            return await AddRowsShared(datasetId, table, pbiRows);
        }

        private async Task<HttpResponseMessage> AddRowsShared(string datasetId, string table, PowerBIRows pbiRows)
        {
            await AuthHelper.CheckToken();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Authorization.AccessToken);
                var result = await client.PostAsync(
                    $"https://api.powerbi.com/v1.0/myorg/datasets/{datasetId}/tables/{table}/rows", new StringContent(JsonConvert.SerializeObject(pbiRows), Encoding.UTF8, "application/json"));
                if (result.StatusCode == HttpStatusCode.OK)
                    return Request.CreateResponse<PowerBIRows>(HttpStatusCode.OK, pbiRows);
                else
                    return result;
            }
        }

        private PowerBIRows ConvertRowStringToPowerBIRows(string rows)
        {
            string arrayRows = rows;
            if(!rows.StartsWith("["))      
                 arrayRows = "[ " + rows + " ]";
            JArray rowsArray = JArray.Parse(arrayRows);
            return new PowerBIRows { rows = rowsArray.ToList() };            
        }
    }

    public class Row
    {
        public string Name { get; set; }
        public int Score { get; set; }
    }
}
