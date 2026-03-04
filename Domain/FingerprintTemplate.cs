namespace M_One_Layer3.Domain
{
    public class FingerprintTemplate
    {
        Guid Id { get; set; }
        public string TemplateBase64 { get; set; }
        public int? FingerIndex { get; set; }
        public int QualityScore { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
