using Microsoft.AspNetCore.Mvc;

namespace M_One_Layer3.Controllers
{
    [ApiController]
    [Route("api/fingerprint")]
    public class FingerprintController : ControllerBase
    {
        private readonly FingerprintService_Bridge _bridge;

        public FingerprintController(FingerprintService_Bridge bridge)
        {
            _bridge = bridge;
        }

        [HttpPost("opendevice")]
        public async Task<IActionResult> OpenDevice()
        {
            var result = await _bridge.OpenDeviceAsync();
            return Ok(result);
        }

        [HttpPost("closedevice")]
        public async Task<IActionResult> CloseDevice()
        {
            var result = await _bridge.CloseDeviceAsync();
            return Ok(result);
        }

        [HttpPost("startcapture")]
        public async Task<IActionResult> StartCapture([FromBody] StartCaptureRequest body)
        {
            try
            {
                int mode = body.mode;
                int missing = body.nMissingFinger;
                var result = await _bridge.StartCaptureAsync(mode, missing);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[StartCapture Error] " + ex);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("stopcapture")]
        public async Task<IActionResult> StopCapture()
        {
            var result = await _bridge.StopCaptureAsync();
            return Ok(result);
        }

        [HttpPost("startenroll")]
        public async Task<IActionResult> StartEnroll([FromBody] EnrollRequest req)
        {
            var result = await _bridge.StartEnrollAsync(
                req.UserId,
                req.CaptureType,
                req.MissingFinger,
                req.FeatureFormat);

            return Ok(result);
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyRequest req)
        {
            var result = await _bridge.VerifyAsync(
                req.UserId,
                req.CaptureType,
                req.MissingFinger,
                req.FeatureFormat);
            return Ok(result);
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SearchRequest req)
        {
            var result = await _bridge.SearchAsync(
                req.CaptureType,
                req.MissingFinger,
                req.FeatureFormat);
            return Ok(result);
        }
        public class EnrollRequest
        {
            public int UserId { get; set; }

            public int MissingFinger { get; set; }
            public int CaptureType { get; set; }
            
            public int FeatureFormat { get; set; }
        }


        public class StartCaptureRequest
        {
            public int mode { get; set; }
            public int nMissingFinger { get; set; }

        }

        public class VerifyRequest
        {
            public int UserId { get; set; }

            public int CaptureType { get; set; }

            public int MissingFinger { get; set; }
            public int FeatureFormat { get; set; }


        }

        public class SearchRequest
        {
            public int CaptureType { get; set; }

            public int MissingFinger {get; set; }
            public int FeatureFormat { get; set; }
        }
    }
}
