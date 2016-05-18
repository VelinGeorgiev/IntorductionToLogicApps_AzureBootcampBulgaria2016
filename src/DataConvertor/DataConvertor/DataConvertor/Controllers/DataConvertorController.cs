using System.Net;
using System.Net.Http;
using System.Web.Http;
using TRex.Metadata;
using DataConvertor.Models;

namespace DataConvertor.Controllers
{
    public class DataConvertorController : ApiController
    {
        /// <summary>
        /// Converts game data to more meaningful data object.
        /// </summary>
        /// <param name="gameScore">Game score object</param>
        /// <returns>FormattedData</returns>
        [Swashbuckle.Swagger.Annotations.SwaggerResponse(HttpStatusCode.OK, Type = typeof(FormattedData))]
        [Metadata("Data Convertor", "Formatted data")]
        public HttpResponseMessage Post([FromBody]GameScore gameScore)
        {
            var values = gameScore.Data.Trim().Split(new[] { "#;" }, System.StringSplitOptions.RemoveEmptyEntries);
            if (values.Length < 2)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var score = 0;
            int.TryParse(values[1], out score);
            if (score == 0)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var totalScore = 0;
            int.TryParse(values[2], out totalScore);
            
            var formattedData = new FormattedData(values[0], score, totalScore);

            return Request.CreateResponse(formattedData);
        }
    }
}
