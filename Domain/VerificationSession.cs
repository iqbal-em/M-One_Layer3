using System;

namespace M_One_Layer3.Domain
{
    public class VerificationSession
    {
        public int Id { get; set; }

        public int? PersonId { get; set; }

        public Person Person { get; set; }

        public string DeviceId { get; set; }

        public bool IsMatch { get; set; }

        public double? MatchScore { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}