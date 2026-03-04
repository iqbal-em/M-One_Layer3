using Device.Core.Interfaces;
using Device.Core.Models;
using Device.Core.SDK.Bioslim10;
using M_One_Layer3.Domain;
using M_One_Layer3.Hubs;
using M_One_Layer3.Infrastructure.Database;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Buffers.Text;
using System.Diagnostics.Eventing.Reader;
using System.Reflection;

namespace M_One_Layer3
{
    public class FingerprintService_Bridge
    {
        private readonly IFingerPrintService _sdkService;
        private readonly IHubContext<FingerprintHub> _hub;
        //private readonly AppDbContext _db;

        private readonly IServiceScopeFactory _scopeFactory;
        //private readonly AppDbContext _context;
        private readonly List<(int fingerindex, int _capturemode, byte[] template)> _currentEnrollTemplates = new();
        private readonly List<(bool success, int UserId)> _currentSearch = new();
        private List<FingerprintFingerResult> _capturedFingers = new();
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
        public FingerprintService_Bridge(IFingerPrintService sdkService, IHubContext<FingerprintHub> hub, IServiceScopeFactory scopeFactory)
        {
            _sdkService = sdkService;
            _hub = hub;
            _scopeFactory = scopeFactory;

            RegisteredEvents();
            //
        }

        private void RegisteredEvents()
        {
            //Subscribe event dari SDK
            _sdkService.PreviewFrameUpdated += frame =>
            {

                string base64 = RawToBmp(frame.ImageBytes, frame.Width, frame.Height);

                _hub.Clients.All.SendAsync("PreviewUpdated", base64);
            };

            _sdkService.FingerCaptured += finger =>
            {

                _capturedFingers.Add(finger);
                _hub.Clients.All.SendAsync("FingerCompleted", new
                {
                    fingerIndex = finger.FingerIndex,
                    nfiq = finger.Nfiq,
                    image = RawToBmp(finger.ImageBytes, finger.Width, finger.Height),
                });
            };

            _sdkService.FingerEnrollTemplated += template =>
            {
                string base64 = Convert.ToBase64String(template.TemplateBytes);

                //Console.WriteLine("Template received: " + template.FingerIndex);

                _currentEnrollTemplates.Add((template.FingerIndex, template.CaptureMode, template.TemplateBytes));

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

                _currentSearch.Add((result.Found,result.UserId));
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
                        DeviceResult.Fail(-1, "StartCapture tidak mendukung TenFinger mode"));
                }

                _sdkService.StartCapture(mode, missingFinger);

                return Task.FromResult(DeviceResult.Ok(0, "Capture started"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    DeviceResult.Fail(-1, "StartCapture failed: " + ex.Message));
            }
        }
        public Task<DeviceResult> StopCaptureAsync()
        {
            _sdkService.CancelCapture();
            return Task.FromResult(DeviceResult.Ok(0, "Capture stopped"));
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
                        templates = (object)null,
                        fingerindex = -1,
                        capturemode = -1
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

                Console.WriteLine("ERROR SAVE DB:");
                Console.WriteLine(ex.ToString());
                return new
                {
                    success = false,
                    message = ex.Message,
                    templates = (object)null,
                    fingerindex = -1,
                    capturemode = -1
                };
            }
        }

        public async Task<object> StartEnrolldbAsync(
    int captureType,
    int missingFinger,
    int featureFormat)
        {
            try
            {
                //_currentEnrollTemplates.Clear();

                if (captureType == (int)FingerMode.OneFinger)
                {

                    return new
                    {
                        success = false,
                        message = "Enroll tidak mendukung mode Onefinger",
                        templates = (object)null
                    };
                }

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var allTemplates = await db.BiometricTemplates
        .Include(x => x.Person)   
        .Where(x => x.Type == Domain.BiometricType.Fingerprint)
        .ToListAsync();

                byte[] galleryBuffer = BuildGalleryBuffer(allTemplates);
                //enrollment

                //Console.WriteLine(galleryBuffer.Length);
                //Console.WriteLine(allTemplates.Count);

                var result = await _sdkService.StartEnrolldbAsync(
                    galleryBuffer,
                    captureType,
                    missingFinger,
                    featureFormat,
                    allTemplates.Count);

                if (!result.Success && result.code < 0)
                {
                    await _hub.Clients.All.SendAsync("EnrollFailed", result.Message);

                    return new
                    {
                        success = false,
                        message = result.Message,
                        templates = (Object)null,
                    };

                }
                else if (!result.Success && result.code >= 0)
                {
                    Console.WriteLine("result code : "+ result.code);
                    var matchedTemplate = allTemplates[result.code-1];
                    var matchedPerson = matchedTemplate.Person;

                    await _hub.Clients.All.SendAsync("EnrollFailed - Duplicated");
                    return new
                    {
                        success = false,
                        IsDuplicated = true,
                        message = result.Message,
                        MatchedPersonId = matchedPerson.Id,
                        MatchedPersonName = matchedPerson.FullName,
                        MatchedFingerIndex = matchedTemplate.FingerIndex,
                        templates = (object)null

                    };
                }

                var person = new Person
                {
                    Id = Guid.NewGuid(),
                    Nationality = "IDN",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    PassportNumber = "A" + new Random().Next(1000000, 9999999),
                    FullName = "Dummy User " + DateTime.Now.Ticks,
                    CreatedAt = DateTime.UtcNow
                };

                db.Persons.Add(person);
                await db.SaveChangesAsync();   // WAJIB SAVE DULU supaya FK valid

                Guid tmp = Guid.NewGuid();
                Guid personId = person.Id;

                for (int i = 0; i< _currentEnrollTemplates.Count; i++)
                {
                    db.BiometricTemplates.Add(new BiometricTemplate
                    {
                        Id = Guid.NewGuid(),
                        PersonId = personId,   // ← pakai ID dari Person
                        Type = BiometricType.Fingerprint,
                        ImageData = _capturedFingers[i].ImageBytes,
                        TemplateBase64 = _currentEnrollTemplates[i].template,
                        FingerIndex = _currentEnrollTemplates[i].fingerindex,
                        Width = _capturedFingers[i].Width,
                        Height = _capturedFingers[i].Height,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                

                await db.SaveChangesAsync();
                await _hub.Clients.All.SendAsync("EnrollSuccess");

                return new
                {
                    success = true,
                    message = "Enroll success",
                    personName = person.FullName,
                    personId = person.Id,
                    templates = _currentEnrollTemplates      
                };
            }

            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    message = ex.Message,
                    templates = (object)null,
                    fingerindex = -1,
                    capturemode = -1
                };
            }
        }

        public Task<DeviceResult> VerifyAsync(int UserId, int nCapturetype, int missingFinger, int ifeatureformat)
        {
            try
            {
                _sdkService.StartVerifyAsync(UserId,nCapturetype,missingFinger,ifeatureformat);
                return Task.FromResult(DeviceResult.Ok(0,"Verify started"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(DeviceResult.Fail(-1,"Verify failed: " + ex.Message));
            }
        }

        public async Task<Object> VerifyAsyncdb(string passnum, int nCapturetype, int missingFinger, int ifeatureformat)
        {

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var person = await db.Persons
        .Include(p => p.Biometrics)
        .FirstOrDefaultAsync(p => p.PassportNumber == passnum);

            if (person == null)
                return new
                {
                    success = false,
                    message = "No templates in Database",
                };


            var templates = person.Biometrics
        .Where(x => x.Type == BiometricType.Fingerprint)
        .ToList();

            byte[] galleryBuffer = BuildGalleryBuffer(templates);

            var res = await _sdkService.StartVerifydbAsync(passnum
                    , nCapturetype
                    , missingFinger
                    , ifeatureformat
                    , galleryBuffer
                    , templates.Count);

            if (res.Success)
            {
                
                return new
                {
                    success = true,
                    message = "Verify success",
                    personId = person.Id,
                    Fullname = person.FullName,
                };
            }

            return new
            {
                success = false,
                message = "No fingerprint that matched with " + passnum
            };

        }

        public async Task<Object> SearchAsyncdb(int nCapturetype, int missingFinger, int ifeatureformat)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var templates = await db.BiometricTemplates
            .Include(x => x.Person)
            .Where(x => x.Type == BiometricType.Fingerprint)
            .ToListAsync();
                if (!templates.Any())
                {
                    return new
                    {
                        success = false,
                        message = "No templates in Database",
                    };
                }

                var galleryBuffer = BuildGalleryBuffer(templates);


                var res = await _sdkService.StartSearchdbAsync(galleryBuffer
                        , nCapturetype
                        , missingFinger
                        , ifeatureformat
                        , templates.Count);

                if (res.Success)
                {
                    Console.WriteLine("Search Success, matched index: " + (res.search_code-1));
                    var matchedTemplate = templates[res.search_code-1];
                    var matchedPerson = matchedTemplate.Person;

                    return new
                    {
                        success = true,
                        message = res.Message,
                        MatchedPersonId = matchedPerson.Id,
                        MatchedPersonName = matchedPerson.FullName,
                        MatchedFingerIndex = matchedTemplate.FingerIndex
                    };
                }

                return new
                {
                    success = false,
                    message = "No Any finger matched in database"
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    message = ex.Message
                };
            }
        }
            
        
        public Task<DeviceResult> SearchAsync(int nCaptureType, int missingFinger, int ifeatureformat)
        {
            try
            {
                _sdkService.Start_SearchAsync(nCaptureType, missingFinger, ifeatureformat);
                return Task.FromResult(DeviceResult.Ok(0,"Search started"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(DeviceResult.Fail(-1,"Search failed: " + ex.Message));
            }
        }


        private byte[] BuildGalleryBuffer(
    List<BiometricTemplate> templates)
        {
            int count = templates.Count;
            int templateSize = 1024;
            if (count == 0)
                return Array.Empty<byte>();

            byte[] galleryBuffer = new byte[count * templateSize];

            for (int i = 0; i < count; i++)
            {
                var template = templates[i].TemplateBase64;

                if (template.Length != templateSize)
                    throw new Exception($"Invalid template size at index {i}");

                Buffer.BlockCopy(
                    template,
                    0,
                    galleryBuffer,
                    i * templateSize,
                    templateSize
                );
            }

            return galleryBuffer;
        }
        public class temp_enroll
        {
            public bool success{ get; set; }
            public int code_result { get; set; }

            public string message { get; set; }
            public List<string> templates { get; set; }

            public int fingerindex { get; set; }
            public int capturemode { get; set; }
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
