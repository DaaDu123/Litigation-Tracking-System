using System.Net;
using LTSFrontend.Core.Auth;
using LTSFrontend.Core.Http;
using LTSFrontend.Features.Auth.Services;
using LTSFrontend.Features.AuditLogs.Services;
using LTSFrontend.Features.CaseAssignments.Services;
using LTSFrontend.Features.CaseNotes.Services;
using LTSFrontend.Features.CaseParties.Services;
using LTSFrontend.Features.Cases.Services;
using LTSFrontend.Features.Deadlines.Services;
using LTSFrontend.Features.Documents.Services;
using LTSFrontend.Features.Firms.Services;
using LTSFrontend.Features.FirmAdminRequests.Services;
using LTSFrontend.Features.UserJoinRequests.Services;
using LTSFrontend.Features.Hearings.Services;
using LTSFrontend.Features.LoginHistory.Services;
using LTSFrontend.Features.Marketing.Services;
using LTSFrontend.Features.Milestones.Services;
using LTSFrontend.Features.Notifications.Services;
using LTSFrontend.Features.Dashboard.Services;
using LTSFrontend.Features.Masters.Services;
using LTSFrontend.Features.Permissions.Services;
using LTSFrontend.Features.Profile.Services;
using LTSFrontend.Features.Roles.Services;
using LTSFrontend.Features.Users.Services;
using LTSFrontend.State;
using Microsoft.AspNetCore.Components.Authorization;

namespace LTSFrontend.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLtsFrontendServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Blazor auth plumbing
            services.AddAuthorizationCore();
            services.AddCascadingAuthenticationState();

            // Session / token storage
            services.AddScoped<UserSessionState>();
            services.AddScoped<ITokenStorageService, TokenStorageService>();
            services.AddScoped<CustomAuthStateProvider>();
            services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());

            services.AddSingleton<TokenRefreshGate>();

            // SECURITY FIX: previously registered via services.AddHttpClient<ApiClient>(...),
            // which pools ONE HttpMessageHandler (and its CookieContainer,
            // since UseCookies=true) across EVERY user's circuit on this
            // Blazor Server instance, recycled every ~2 minutes. That let
            // one user's refreshToken cookie be overwritten by another
            // concurrent user's, and silently wiped everyone's cookie on
            // each recycle - which is why Logout / silent refresh started
            // failing ("Refresh token not found in cookie") for anyone
            // active more than a couple of minutes, and LoginHistory's
            // LogoutTime was never being saved. Scoped = one instance (and
            // one private CookieContainer) per circuit = per logged-in
            // user, disposed with that circuit. See ApiClient.Dispose().
            services.AddScoped(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var env = sp.GetRequiredService<IHostEnvironment>();
                var baseUrl = config["ApiSettings:BaseUrl"] ?? "https://localhost:7167";

                var socketHandler = new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = new CookieContainer()
                };

                if (env.IsDevelopment())
                {
                    // Local dev SSL cert validation override
                    socketHandler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }

                var httpClient = new HttpClient(socketHandler)
                {
                    BaseAddress = new Uri(baseUrl),
                    Timeout = TimeSpan.FromSeconds(100)
                };

                return new ApiClient(
                    httpClient,
                    sp.GetRequiredService<UserSessionState>(),
                    sp.GetRequiredService<ITokenStorageService>(),
                    sp.GetRequiredService<TokenRefreshGate>());
            });

            // Feature services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICourtService, CourtService>();
            services.AddScoped<ICaseCategoryService, CaseCategoryService>();
            services.AddScoped<ICaseStageService, CaseStageService>();
            services.AddScoped<ICaseStatusService, CaseStatusService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IDocumentTypeService, DocumentTypeService>();
            services.AddScoped<IMasterDataService, MasterDataService>();
            services.AddScoped<ICaseService, CaseService>();
            services.AddScoped<ICaseAssignmentService, CaseAssignmentService>();
            services.AddScoped<ICaseNoteService, CaseNoteService>();
            services.AddScoped<ICasePartyService, CasePartyService>();
            services.AddScoped<IHearingService, HearingService>();
            services.AddScoped<IDeadlineService, DeadlineService>();
            services.AddScoped<IMilestoneService, MilestoneService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<ILoginHistoryService, LoginHistoryService>();
            services.AddScoped<IFirmService, FirmService>();
            services.AddScoped<IFirmAdminRequestService, FirmAdminRequestService>();
            services.AddScoped<IContactMessageService, ContactMessageService>();
            services.AddScoped<IUserJoinRequestService, UserJoinRequestService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<ProfileCompletionState>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<INotificationService, NotificationService>();

            // App-wide UI services
            services.AddScoped<ToastService>();
            services.AddScoped<AppState>();
            services.AddScoped<CaseFilterState>();

            return services;
        }
    }
}