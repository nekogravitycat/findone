using Azure.AI.Vision.ImageAnalysis;
using Azure;
using Microsoft.AspNetCore.Mvc;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageAnalysisController : Controller
    {
        public class ImageRequest
        {
            public string Base64Image { get; set; }
        }
    }
}