using LTSFrontend.Features.SuperAdminRequests.DTOs;

namespace LTSFrontend.Features.SuperAdminRequests.Services
{




    public interface ISuperAdminRequestService
    {





        Task<SuperAdminRequestDTO?> GetMineAsync();
    }
}
