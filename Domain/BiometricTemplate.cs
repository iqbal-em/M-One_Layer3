namespace M_One_Layer3.Domain
{
    public class BiometricTemplate
    {
        public Guid Id { get; set; }
        public Guid PersonId { get; set; }
        public Person Person { get; set; }
        public BiometricType Type { get; set; }

        public byte[] ImageData { get; set; }
        public byte[] TemplateBase64 { get; set; }
        public int ? FingerIndex { get; set; }
        //public int QualityScore { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public int Width { get; set; }

        public int Height { get; set; }

    }
}
