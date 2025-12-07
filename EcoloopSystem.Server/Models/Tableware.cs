namespace EcoloopSystem.Server.Models
{
    public enum TablewareType
    {
        Bowl,
        Cup,
        Chopsticks
    }

    public enum TablewareStatus
    {
        Available,
        Rented,
        Lost,
        Cleaning
    }

    public class Tableware
    {
        public int Id { get; set; }
        public string TagId { get; set; } = null!; // RFID UID
        public TablewareType Type { get; set; }
        public TablewareStatus Status { get; set; } = TablewareStatus.Available;
    }
}
