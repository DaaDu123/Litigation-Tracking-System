using FluentValidation;
using LTSBackend.Comman.Security;

namespace LTSBackend.Features.Documents.Commands.UploadDocument;

// Requires a valid CaseID/DocumentTypeID, a non-empty document name, and a
// file that is present, non-empty, under 15MB, has an allowed extension,
// AND whose actual byte signature matches that extension (blocks a
// renamed/mislabeled file, e.g. a .exe saved as .pdf).
public class UploadDocumentValidator : AbstractValidator<UploadDocumentCommand>
{
    // Defines the validation rules for an upload request (case, type, name, file).
    public UploadDocumentValidator()
    {
        RuleFor(x => x.CaseID)
            .GreaterThan(0)
            .WithMessage("Valid Case ID is required");

        RuleFor(x => x.DocumentTypeID)
            .GreaterThan(0)
            .WithMessage("Document type is required");

        RuleFor(x => x.DocumentName)
            .NotEmpty()
            .WithMessage("Document name is required")
            .MaximumLength(255)
            .WithMessage("Document name cannot exceed 255 characters");

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required")
            .Must(f => f?.Length > 0)
            .WithMessage("File cannot be empty")
            .Must(f => f?.Length <= 15 * 1024 * 1024)
            .WithMessage("File size is too large. Please reduce the document size to 15MB or less.")
            .Must(IsValidFileType)
            .WithMessage("Invalid file type. Allowed types: PDF, DOC, DOCX, XLS, XLSX, JPG, JPEG, PNG, TXT")
            .Must(HasValidContentSignature)
            .WithMessage("File content does not match its file extension. The file may be corrupted, mislabeled, or of a disallowed type.");

        RuleFor(x => x.Remarks)
            .MaximumLength(500)
            .WithMessage("Remarks cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Remarks));
    }

    // Checks the file's extension against the allowed list.
    private static bool IsValidFileType(IFormFile? file)
    {
        if (file == null)
            return false;

        var allowedExtensions = new[]
        {
            ".pdf",
            ".doc", ".docx",
            ".xls", ".xlsx",
            ".jpg", ".jpeg", ".png",
            ".txt"
        };

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowedExtensions.Contains(fileExtension);
    }

    // Verifies the file's actual bytes (magic number) match its claimed extension.
    private static bool HasValidContentSignature(IFormFile? file)
    {
        if (file == null)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        using var stream = file.OpenReadStream();
        return FileSignatureValidator.HasValidSignature(stream, extension);
    }
}
