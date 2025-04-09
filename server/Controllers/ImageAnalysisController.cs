using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ImageAnalysisController : Controller
    {
        private readonly ImageAnalysisService _imageAnalysisService;

        public ImageAnalysisController(ImageAnalysisService imageAnalysisService)
        {
            _imageAnalysisService = imageAnalysisService;
        }

        [HttpPost("image")]
        public async Task<IActionResult> ImageAnalysis([FromBody] ImageRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Base64Image))
                return BadRequest(new { error = "請提供有效的 Base64 圖片" });
            
            try
            {
                var response = await _imageAnalysisService.AnalyzeImage(request.Base64Image);

                if (response == null)
                    return BadRequest(new { error = "無法分析影像" });

                return Ok(new { response });
            }
            catch (FormatException)
            {
                return BadRequest(new { error = "提供的 Base64 字串格式錯誤" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "影像分析失敗", details = ex.Message });
            }
        }
    }
}