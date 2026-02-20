namespace M_One_Layer3.Domain
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        public Guid SessionId { get; set; }

        public string Action { get; set; }

        public string Device { get; set; }

        public string Status { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
