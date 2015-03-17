using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BitHoursApp.MI.WebApi
{
    public interface IBitHoursApi
    {
        /// <summary>
        /// Get login data
        /// </summary>
        /// <param name="email">email</param>
        /// <param name="password">password</param>
        /// <returns>API Response object</returns>
        Task<BitHoursApiResponse<BitHoursLoginObject>> LoginAsync(string email, string password);

        /// <summary>
        /// Get contracts
        /// </summary>
        /// <returns></returns>
        Task<BitHoursApiResponse<BitHoursContractsObject>> GetContractsAsync(int contractorId);

    }

    public class BitHoursApi : IBitHoursApi
    {
        private BitHoursApi()
        {
        }

        #region Singleton

        private static IBitHoursApi instance;

        public static IBitHoursApi Instance
        {
            get
            {
                return instance ?? (instance = new BitHoursApi());
            }
        }

        #endregion

        /// <summary>
        /// Get login data
        /// </summary>
        /// <param name="email">email</param>
        /// <param name="password">password</param>
        /// <returns>API Response object</returns>
        public async Task<BitHoursApiResponse<BitHoursLoginObject>> LoginAsync(string email, string password)
        {
            var requestUri = BitHoursUrlHelper.GetLoginUrl();

            var postContent = new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string>(BitHoursReqParams.EmailParameter, email),
                new KeyValuePair<string, string>(BitHoursReqParams.PasswordParameter, password)
            });

            HttpClient httpClient = null;

            var bitHoursApiResponse = new BitHoursApiResponse<BitHoursLoginObject>();

            try
            {
                httpClient = new HttpClient();
                var response = await httpClient.PostAsync(requestUri, postContent);
                var content = await response.Content.ReadAsStringAsync();
                bitHoursApiResponse.Result = JsonConvert.DeserializeObject<BitHoursApiJsonResponse<BitHoursLoginObject>>(content);

                //if couldn't connect 
                if (!bitHoursApiResponse.Result.success)
                    bitHoursApiResponse.ErrorCode = BitHoursApiErrors.LoginError;
            }
            catch (Exception ex)
            {
                bitHoursApiResponse.ErrorCode = BitHoursApiErrors.LoginError;

#if DEBUG

                bitHoursApiResponse.Error = ex.Message;

#endif
            }
            finally
            {
                if (httpClient != null)
                    httpClient.Dispose();
            }

            return bitHoursApiResponse;
        }

        /// <summary>
        /// Get contracts
        /// </summary>
        /// <param name="contractorId"></param>
        /// <returns></returns>
        public async Task<BitHoursApiResponse<BitHoursContractsObject>> GetContractsAsync(int contractorId)
        {
            throw new NotImplementedException();
        }
    }
}