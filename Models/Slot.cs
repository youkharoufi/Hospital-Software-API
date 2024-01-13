namespace Hospital_Software.Models
{
    public class Slot
    {
        public string Id { get; set; }

        public List<DateTime> SlotTime { get; set; } = new List<DateTime>();

        public string DoctorId { get; set; }

        public string? patientId { get; set; }

        public bool Booked { get; set; }
    }
}
