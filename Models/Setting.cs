namespace POS.Models
{
    public class Setting : BaseEntity
    {
        public string SettingKey { get; set; } = string.Empty;
        public string? SettingValue { get; set; }
        public string? Description { get; set; }
        public string? Group { get; set; }
        public bool IsEncrypted { get; set; } = false;
    }
}