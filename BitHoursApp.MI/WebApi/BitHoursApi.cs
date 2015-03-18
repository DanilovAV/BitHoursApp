using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BitHoursApp.Common;
using BitHoursApp.MI.Models;
using Newtonsoft.Json;

namespace BitHoursApp.MI.WebApi
{
    public interface IBitHoursApi
    {
        /// <summary>
        /// Get login data
        /// </summary>      
        Task<BitHoursApiResponse<BitHoursLoginObject>> LoginAsync(string email, string password);

        /// <summary>
        /// Get contracts
        /// </summary>       
        Task<BitHoursApiResponse<IEnumerable<BitHoursContractsObject>>> GetContractsAsync(UserInfo userInfo);

        /// <summary>
        /// Upload snapshot
        /// </summary>       
        Task<BitHoursApiResponse<object>> UploadSnapshotAsync(BitHoursUploadRequest uploadRequest, CancellationToken cancellationToken);
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
            var requestUrl = BitHoursUrlHelper.GetLoginUrl();

            var parameters = new[] 
            {
                new KeyValuePair<string, string>(BitHoursReqParams.EmailParameter, email),            
                new KeyValuePair<string, string>(BitHoursReqParams.PasswordParameter, password)
            };

            using (var postContent = new FormUrlEncodedContent(parameters))
            {
                var bitHoursApiResponse = await SendHttpPostRequestAsync<BitHoursLoginObject>(requestUrl, postContent, BitHoursApiErrors.LoginError);
                return bitHoursApiResponse;
            }
        }

        /// <summary>
        /// Get contracts
        /// </summary>
        /// <param name="contractorId"></param>
        /// <returns></returns>
        public async Task<BitHoursApiResponse<IEnumerable<BitHoursContractsObject>>> GetContractsAsync(UserInfo userInfo)
        {
            Check.Require(userInfo != null);

            var requestUrl = BitHoursUrlHelper.GetContractsListUrl();

            var parameters = new[] 
            {
                new KeyValuePair<string, string>(BitHoursReqParams.ContractorId, userInfo.UserId.ToString())                           
            };

            using (var postContent = new FormUrlEncodedContent(parameters))
            {
                var bitHoursApiResponse = await SendHttpPostRequestAsync<IEnumerable<BitHoursContractsObject>>(requestUrl, postContent, BitHoursApiErrors.GetContractsError, userInfo.SessionId);
                return bitHoursApiResponse;
            }
        }

        public async Task<BitHoursApiResponse<object>> UploadSnapshotAsync(BitHoursUploadRequest uploadRequest, CancellationToken cancellationToken)
        {
            Check.Require(uploadRequest != null);

            var requestUrl = BitHoursUrlHelper.GetTrackTimeUrl();

            using (var postContent = new MultipartFormDataContent())
            {
                var parameters = new[]
                {
                    new KeyValuePair<string, string>(BitHoursReqParams.ContractorId, uploadRequest.UserInfo.UserId.ToString()),
                    new KeyValuePair<string, string>(BitHoursReqParams.ContractId, uploadRequest.ContractId.ToString()),
                    new KeyValuePair<string, string>(BitHoursReqParams.StartTime, uploadRequest.StartTime.ToString(BitHoursReqParams.RequestTimeFormat)),
                    new KeyValuePair<string, string>(BitHoursReqParams.EndTime, uploadRequest.EndTime.ToString(BitHoursReqParams.RequestTimeFormat)),
                    new KeyValuePair<string, string>(BitHoursReqParams.Memo, uploadRequest.Memo)                    
                };

                foreach (var keyValuePair in parameters)
                {
                    postContent.Add(new StringContent(keyValuePair.Value),
                        String.Format("\"{0}\"", keyValuePair.Key));
                }

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    uploadRequest.Snapshot.Save(memoryStream, ImageFormat.Jpeg);

                    var bytes = memoryStream.ToArray();

                    using (var streamContent = new ByteArrayContent(bytes))
                    {
                        streamContent.Headers.ContentType = new MediaTypeHeaderValue(BitHoursReqParams.JpegContentType);
                        streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue(BitHoursReqParams.FormDataHeader)
                        {
                            Name = BitHoursReqParams.Screenshot,
                            FileName = String.Format(BitHoursReqParams.ScreenshotFormat, BitHoursReqParams.Screenshot, Guid.NewGuid(), BitHoursReqParams.ScreenshotExtension)
                        };

                        postContent.Add(streamContent);

                        var bitHoursApiResponse = await SendHttpPostRequestAsync<object>(requestUrl, postContent, BitHoursApiErrors.UploadSnapshotError, uploadRequest.UserInfo.SessionId);
                        return bitHoursApiResponse;
                    }
                }
            }
        }

        private async Task<BitHoursApiResponse<T>> SendHttpPostRequestAsync<T>(string requestUrl, HttpContent postContent, BitHoursApiErrors requestError, string sessionId = null)
            where T : class
        {
            HttpClient httpClient = null;

            var bitHoursApiResponse = new BitHoursApiResponse<T>();

            try
            {
                Uri uri = new Uri(requestUrl);

                CookieContainer cookies = new CookieContainer();

                if (sessionId != null)
                    cookies.Add(uri, new Cookie(BitHoursReqParams.SessionParameter, sessionId));

                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;

                httpClient = new HttpClient(handler);

                var response = await httpClient.PostAsync(requestUrl, postContent);
                var content = await response.Content.ReadAsStringAsync();

                IEnumerable<Cookie> responseCookies = cookies.GetCookies(uri).Cast<Cookie>();
                var sessionCookie = responseCookies.FirstOrDefault(x => x.Name == BitHoursReqParams.SessionParameter);

                if (sessionCookie != null)
                    bitHoursApiResponse.SessionId = sessionCookie.Value;

                bitHoursApiResponse.Result = JsonConvert.DeserializeObject<BitHoursApiJsonResponse<T>>(content);

                if (!bitHoursApiResponse.Result.success)
                    bitHoursApiResponse.ErrorCode = requestError;
            }
            catch (TaskCanceledException)
            {
                bitHoursApiResponse.ErrorCode = BitHoursApiErrors.Timeout;
            }
            catch (Exception ex)
            {
                bitHoursApiResponse.ErrorCode = requestError;

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
    }
}