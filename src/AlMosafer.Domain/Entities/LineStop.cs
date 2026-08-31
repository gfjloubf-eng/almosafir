namespace AlMosafer.Domain.Entities;

public class LineStop
{
    public int Id { get; set; }
    public int LineId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int OrderIndex { get; set; }

    public RouteLine Line { get; set; } = null!;
}
