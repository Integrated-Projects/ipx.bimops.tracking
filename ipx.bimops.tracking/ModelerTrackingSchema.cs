
public class ModelerTrackingSchema
{
    public required string id_project { get; set; }
    public required string id_document { get; set; }
    public required string id_user { get; set; }
    public required string id_user_ip_address { get; set; }
    public required long timestamp { get; set; }
    public required string? id_element { get; set; }
    public required string? type_element { get; set; }
    public string? title_document { get; set; }
    public required int? duration { get; set; }
    public required action_project action_project { get; set; }
    public required action_element? action_element { get; set; }
}

public enum action_project
{
    OPEN,
    MODIFY,
    SUBMIT,
    CLOSE,
    IDLE_BEGIN,
    IDLE_END
}
public enum action_element
{
    CREATE,
    MODIFY,
    DELETE
}