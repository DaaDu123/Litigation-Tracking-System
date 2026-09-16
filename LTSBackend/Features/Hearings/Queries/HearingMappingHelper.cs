using LTSBackend.Features.Hearings.DTOs;
using LTSBackend.Models.Cases;

namespace LTSBackend.Features.Hearings.Queries
{
    // ROOT-CAUSE FIX (duplication): this exact mapping block - priority
    // calculation, creator-name lookup wiring, full HearingDetailDTO
    // construction - was copy-pasted identically in both
    // GetUpcomingHearingsQueryHandler and GetCaseHearingsQueryHandler.
    // Any future field change had to be made in two places or the two
    // endpoints would silently drift apart. Centralized here so both
    // handlers call the same code.
    public static class HearingMappingHelper
    {
        public static List<HearingDetailDTO> MapToDetailDtos(IEnumerable<Hearing> hearings,IReadOnlyDictionary<int, string> creatorNamesByUserId)
        {
            var now = DateTime.UtcNow;

            return hearings.Select(h =>
            {
                int daysRemaining = (int)(h.HearingDate - now).TotalDays;
                string priority = daysRemaining <= 1 ? "Critical" :
                                daysRemaining <= 7 ? "High" :
                                daysRemaining <= 15 ? "Medium" : "Normal";

                return new HearingDetailDTO
                {
                    HearingId = h.HearingID,
                    CaseId = h.CaseID,
                    CaseNumber = h.Case?.CaseNumber,
                    CaseTitle = h.Case?.CaseTitle,
                    CourtId = h.CourtID,
                    CourtName = h.Court?.CourtName,
                    HearingDate = h.HearingDate,
                    CourtRoom = h.CourtRoom,
                    JudgeName = h.JudgeName,
                    HearingPurpose = h.Purpose,
                    HearingOutcome = h.Outcome,
                    NextHearingDate = h.NextHearingDate,
                    Remarks = h.Remarks,
                    CreatedByUser = creatorNamesByUserId.TryGetValue(h.CreatedBy, out var name) ? name : null,
                    CreatedDate = h.CreatedDate,
                    DaysRemaining = daysRemaining,
                    HearingPriority = priority
                };
            }).ToList();
        }
    }
}