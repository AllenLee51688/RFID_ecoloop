namespace EcoloopSystem.Server.Models
{
    public class User
    {
        public int Id { get; set; }
        public string CardId { get; set; } = null!; // RFID UID
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
    }
}
