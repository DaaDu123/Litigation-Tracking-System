using MediatR;
using LTSBackend.Features.CaseNotes.DTOs;

namespace LTSBackend.Features.CaseNotes.Queries.GetCaseNotes
{
    public class GetCaseNotesQuery : IRequest<List<CaseNoteDetailDTO>>
    {
        public long CaseID { get; set; }
    }
}
