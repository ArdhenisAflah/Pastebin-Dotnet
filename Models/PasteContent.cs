namespace project_pastebin.Models;

public class PasteContent
{
    public int Id { get; set; }
    public required string Author { get; set; }
    public required string Content { get; set; }
}
