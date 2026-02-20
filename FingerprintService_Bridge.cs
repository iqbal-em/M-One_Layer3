using Device.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;
using M_One_Layer3.Hubs;
using Device.Core.Models;
using Device.Core.SDK.Bioslim10;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;

namespace M_One_Layer3
{
    public class FingerprintService_Bridge
    {
        private readonly IFingerPrintService _sdkService;
        private readonly IHubContext<FingerprintHub> _hub;
        private List<string> _currentEnrollTemplates = new();

        public enum FingerMode
        {
            LeftFinger = 0,
            RightFinger = 1,
            BothThumb = 2,
            OneFinger = 3,
            Roll = 4,
            TenFinger = 5
        }

        //private IFingerprintWrapper wrapper = new IFingerprintWrapper();
        public FingerprintService_Bridge(IFingerPrintService sdkService, IHubContext<FingerprintHub> hub)
        {
            _sdkService = sdkService;
            _hub = hub;

            // Subscribe event dari SDK
            _sdkService.PreviewFrameUpdated += frame =>
            {

                string base64 = RawToBmp(frame.ImageBytes, frame.Width, frame.Height);

                _hub.Clients.All.SendAsync("PreviewUpdated",base64);
            };

            _sdkService.FingerCaptured += finger =>
            {
                _hub.Clients.All.SendAsync("FingerCompleted", new
                {
                    fingerIndex = finger.FingerIndex,
                    nfiq = finger.Nfiq,      
                    image = RawToBmp(finger.ImageBytes, finger.Width, finger.Height),
                });
            };

            _sdkService.FingerEnrollTemplated += template =>
            {
                _hub.Clients.All.SendAsync("EnrollTemplate", Convert.ToBase64String(template.TemplateBytes));
            };

            _sdkService.FingerVerified += result =>
            {
                _hub.Clients.All.SendAsync("VerifyResult", new
                {
                    success = result.Success,
                    userId = result.UserId,
                    message = result.Success ? "Verify success" : "Verify failed"
                });
            };

            _sdkService.FingerSearchCompleted += result =>
            {
                _hub.Clients.All.SendAsync("SearchResult", new
                {
                    success = result.Found,
                    userId = result.UserId,
                    message = result.Success ? "Match found" : "No match found"
                });
            };

        }



        public Task<DeviceResult> OpenDeviceAsync()
        {
            return Task.FromResult(_sdkService.OpenDevice());
        }

        public Task<DeviceResult> CloseDeviceAsync()
        {
            return Task.FromResult(_sdkService.CloseDevice());
        }

        public Task<DeviceResult> StartCaptureAsync(int mode, int missingFinger)
        {
            try
            {
                if (mode == (int)FingerMode.TenFinger)
                {
                    return Task.FromResult(
                        DeviceResult.Fail("StartCapture tidak mendukung TenFinger mode"));
                }

                _sdkService.StartCapture(mode, missingFinger);

                return Task.FromResult(DeviceResult.Ok("Capture started"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    DeviceResult.Fail("StartCapture failed: " + ex.Message));
            }
        }
        public Task<DeviceResult> StopCaptureAsync()
        {
            _sdkService.CancelCapture();
            return Task.FromResult(DeviceResult.Ok("Capture stopped"));
        }

        public async Task<object> StartEnrollAsync(
     int userId,
     int captureType,
     int missingFinger,
     int featureFormat)
        {
            try
            {
                if (captureType == (int)FingerMode.OneFinger)
                {
                    return new
                    {
                        success = false,
                        message = "Enroll tidak mendukung mode Onefinger",
                        templates = (object)null
                    };
                }

                _currentEnrollTemplates.Clear();

                var result = await _sdkService.StartEnrollAsync(
                    userId,
                    captureType,
                    missingFinger,
                    featureFormat);

                if (!result.Success)
                {
                    await _hub.Clients.All.SendAsync("EnrollFailed", result.Message);

                    return new
                    {
                        success = false,
                        message = result.Message,
                        templates = (object)null
                    };
                }

                await _hub.Clients.All.SendAsync("EnrollSuccess");

                return new
                {
                    success = true,
                    message = "Enroll success",
                    templates = _currentEnrollTemplates
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    message = ex.Message,
                    templates = (object)null
                };
            }
        }

        public Task<DeviceResult> VerifyAsync(int UserId, int nCapturetype, int missingFinger, int ifeatureformat)
        {
            try
            {
                _sdkService.StartVerifyAsync(UserId,nCapturetype,missingFinger,ifeatureformat);
                return Task.FromResult(DeviceResult.Ok("Verify started"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(DeviceResult.Fail("Verify failed: " + ex.Message));
            }
        }

        public Task<DeviceResult> SearchAsync(int nCaptureType, int missingFinger, int ifeatureformat)
        {
            try
            {
                _sdkService.Start_SearchAsync(nCaptureType, missingFinger, ifeatureformat);
                return Task.FromResult(DeviceResult.Ok("Search started"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(DeviceResult.Fail("Search failed: " + ex.Message));
            }
        }




        private string RawToBmp(byte[] raw, int width, int height)
        {
            // Boost brightness sederhana
            byte[] adjusted = new byte[raw.Length];

            for (int i = 0; i < raw.Length; i++)
            {
                int bright = raw[i] + 30;   // tambah brightness
                if (bright > 255) bright = 255;
                adjusted[i] = (byte)bright;
            }

            using var image = Image.LoadPixelData<L8>(adjusted, width, height);

            using var ms = new MemoryStream();
            image.SaveAsBmp(ms); // BMP format

            return Convert.ToBase64String(ms.ToArray());
        }
    }
}
