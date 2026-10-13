namespace LTSBackend.Features.DocumentTypes.DTOs;

public class DocumentTypeDTO
{
    public int DocumentTypeID { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }


    public bool IsGlobal { get; set; }
}
