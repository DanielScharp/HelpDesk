using HelpDesk.Domain;
using HelpDesk.Web.Services;

namespace HelpDesk.Web.Api
{
    public class ApiSender
    {
        private readonly IApiService _apiService;

        public ApiSender(IApiService apiService)
        {
            _apiService = apiService;
        }

        //-----------------------------------------Usuarios-----------------------------------------
        public async Task<ApiResponse> EfetuarLoginCliente(Login login)
        {

            try
            {
                var parameters = new Dictionary<string, string>
                {
                    {"apelido", login.Apelido},
                    {"password", login.Password},
                    {"ipOrigem", login.IpOrigem},
                    {"appOrigem", login.AppOrigem},
                };

                var request = new ApiRequest
                {
                    RouteValue = "Login/in",
                    Content = parameters,
                };

                return await _apiService.PostAsync(request);

            }
            catch
            {
                throw;
            }

        }

        public async Task<ApiResponse> RecuperaSenhaCliente(Login login)
        {

            try
            {
                var parameters = new Dictionary<string, string>
                {
                    {"email", login.Email},
                    {"apelido", login.Apelido},
                    {"ipOrigem", login.IpOrigem},
                    {"appOrigem", login.AppOrigem},
                };

                var leilaoRequest = new ApiRequest
                {
                    RouteValue = "Login/reenviar-senha",
                    Content = parameters,
                };

                return await _apiService.PostAsync(leilaoRequest);

            }
            catch
            {
                throw;
            }

        }
    }
}
