using HelpDesk.Web.Api;

namespace HelpDesk.Web.Services
{
    public interface IApiService
    {
        Task<ApiResponse> GetAsync(ApiRequest request);

        Task<ApiResponse> PostAsync(ApiRequest request);

        Task<ApiResponse> PostFileAsync(ApiRequest request);
    }
}
