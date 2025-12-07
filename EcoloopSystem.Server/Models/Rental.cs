namespace EcoloopSystem.Server.Models
{
    public class Rental
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int TablewareId { get; set; }
        public Tableware? Tableware { get; set; }
        public DateTime BorrowedAt { get; set; } = DateTime.Now;
        public DateTime? ReturnedAt { get; set; }
        public int StationId { get; set; }
    }
}
