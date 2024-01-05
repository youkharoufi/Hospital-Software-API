namespace Hospital_Software.Models
{
    public class Slot
    {
        public int Id { get; set; }

        public DateOnly Date { get; set; }

        public TimeOnly Time { get; set; }

        public string DoctorId { get; set; }

        public string? patientId { get; set; }

        public bool Booked { get; set; }
    }
}
