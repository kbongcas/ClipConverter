namespace ClipConverter.Dtos;

public class GenerateHtmlRequestDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string From { get; set; }
    public string ConvertedFile { get; set; }
}
