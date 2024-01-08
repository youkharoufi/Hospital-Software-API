namespace Hospital_Software.Models
{
    public class Slot
    {
        public string Id { get; set; }

        public DateTime DateTime { get; set; }

        public string DoctorId { get; set; }

        public string? patientId { get; set; }

        public bool Booked { get; set; }
    }
}
