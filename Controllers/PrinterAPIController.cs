using Device.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace M_One_Layer3.Controllers
{
    [ApiController]
    [Route("api/printer")]
    public class PrinterController : ControllerBase
    {
        private readonly ThermalPrinterService58mm _printer;

        public PrinterController(ThermalPrinterService58mm printer)
        {
            _printer = printer;
        }

        [HttpPost("print-text")]
        public IActionResult PrintText([FromBody] string text)
        {
            var result = _printer.Print_Text(text);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("print-qrcode")]
        public IActionResult PrintQRCode([FromBody] string qr)
        {
            var result = _printer.PrintQRCode(qr, 6);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("print-barcode")]
        public IActionResult PrintBarcode([FromBody] string barcode)
        {
            var result = _printer.PrintBarcode(barcode);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("print-sample")]
        public IActionResult PrintSample()
        {
            var result = _printer.PrintSampleReceipt();

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }

}
