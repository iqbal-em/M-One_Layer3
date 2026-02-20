namespace M_One_Layer3.Domain
{
    public class Session
    {
        public Guid Id { get; set; }

        public SessionType Type { get; set; }

        public string OfficerId { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } // IN_PROGRESS / COMPLETED / FAILED

        public string Result { get; set; } // APPROVED / REJECTED / etc
    }
}
