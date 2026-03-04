using Device.Core.Models;
using M_One_Layer3.Domain;
using M_One_Layer3.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace M_One_Layer3.Enrollment_db
{
    public class Enrollment_Fingerprint
    {
        private readonly AppDbContext _context;
        private readonly FingerprintService_Bridge _fingerprintServiceBridge;

        public Enrollment_Fingerprint(AppDbContext context, FingerprintService_Bridge fingerprintServiceBridge)
        {

            _context = context;
            _fingerprintServiceBridge = fingerprintServiceBridge;

            //get data from db

        }

        public async Task<Object> process_enrollment(int userId,
      int captureType,
      int missingFinger,
      int featureFormat)
        {
            var allTemplates = await _context.BiometricTemplates
        .Where(x => x.Type == Domain.BiometricType.Fingerprint)
        .ToListAsync();

            byte[] galleryBuffer = BuildGalleryBuffer(allTemplates);
            //enrollment
            var result = await _fingerprintServiceBridge.StartEnrolldbAsync(
                captureType,
                missingFinger,
                featureFormat,
                galleryBuffer,
                allTemplates.Count
            );
            if (!result.success && result.code_result >= 0)
            {
                var matchedTemplate = allTemplates[result.code_result-1];
                var matchedPerson = matchedTemplate.Person;
                return new
                {
                    success = false,
                    IsDuplicated = true,
                    message = result.message,
                    MatchedPersonId = matchedPerson.Id,
                    MatchedPersonName = matchedPerson.FullName,
                    MatchedFingerIndex = matchedTemplate.FingerIndex,
                    templates = (object)null
                    
                };
            }

            // 5️⃣ Save

            for (int i = 0; i < result.templates.Count; i++)
            {

                byte[] templateBytes = Convert.FromBase64String(result.templates[i]);
                _context.BiometricTemplates.Add(new BiometricTemplate
                {
                    PersonId = Guid.NewGuid(),
                    Type = BiometricType.Fingerprint,
                    TemplateBase64 = Convert.FromBase64String(result.templates[i]),
                    FingerIndex = result.fingerindex,
                    //QualityScore = captu.QualityScore
                });

               

            }
            await _context.SaveChangesAsync();

            return new
            {
                success = true,
                ISDuplicated = false,
                message = "Enroll success",
                templates = (object)result.templates,



            };
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

    }     
}

        