using Microsoft.AspNetCore.Http;
namespace LTSBackend.Services.ProfileService
{
    public interface IFileService
    {







        Task<string> SaveFileAsync(IFormFile file, string folderName);




        void DeleteFile(string? relativePath);









        Task<byte[]> ReadSecureFileAsync(string relativePath);





        void DeleteSecureFile(string? relativePath);










        Task<string> SaveCaseDocumentAsync(IFormFile file, int firmId, long caseId);











        Task<byte[]> ReadCaseDocumentAsync(string relativePath, int firmId, long caseId);





        void DeleteCaseDocument(string? relativePath, int firmId, long caseId);
    }
}