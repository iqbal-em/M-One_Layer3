namespace M_One_Layer3.Domain
{
    public class Person
    {
        public Guid Id { get; set; }

        public string FullName { get; set; }
        public string Nationality { get; set; }
        public DateTime DateOfBirth { get; set; }

        public string PassportNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<BiometricTemplate> Biometrics { get; set; }

    }
}
