using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        Task<BitHoursApiResponse<BitHoursContractsObject>> GetContractsAsync(int contractorId);

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
                var response = await httpClient.PostAsync(requestUrl, postContent);
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

                if (postContent != null)
                    postContent.Dispose();
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
            var requestUrl = BitHoursUrlHelper.GetContractsListUrl();

            var postContent = new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string>(BitHoursReqParams.ContractorId, contractorId.ToString())
            });

            HttpClient httpClient = null;

            var bitHoursApiResponse = new BitHoursApiResponse<BitHoursContractsObject>();

            try
            {
                httpClient = new HttpClient();
                var response = await httpClient.PostAsync(requestUrl, postContent);
                var content = await response.Content.ReadAsStringAsync();
                bitHoursApiResponse.Result = JsonConvert.DeserializeObject<BitHoursApiJsonResponse<BitHoursContractsObject>>(content);

                if (!bitHoursApiResponse.Result.success)
                    bitHoursApiResponse.ErrorCode = BitHoursApiErrors.GetContractsError;
            }
            catch (Exception ex)
            {
                bitHoursApiResponse.ErrorCode = BitHoursApiErrors.GetContractsError;

#if DEBUG

                bitHoursApiResponse.Error = ex.Message;

#endif
            }
            finally
            {
                if (httpClient != null)
                    httpClient.Dispose();

                if (postContent != null)
                    postContent.Dispose();
            }

            return bitHoursApiResponse;
        }

        public async Task<BitHoursApiResponse<object>> UploadSnapshotAsync(BitHoursUploadRequest uploadRequest, CancellationToken cancellationToken)
        {
            var requestUrl = BitHoursUrlHelper.GetTrackTimeUrl();

            var postContent = new MultipartFormDataContent();

            var values = new[]
            {
                new KeyValuePair<string, string>(BitHoursReqParams.ContractorId, uploadRequest.UserId.ToString()),
                new KeyValuePair<string, string>(BitHoursReqParams.ContractId, uploadRequest.ContractId.ToString()),
                new KeyValuePair<string, string>(BitHoursReqParams.Minutes, uploadRequest.ElapsedMinutes.ToString()),
                new KeyValuePair<string, string>(BitHoursReqParams.Memo, uploadRequest.Memo),
                new KeyValuePair<string, string>(BitHoursReqParams.Status, uploadRequest.Status.ToString())                
            };

            foreach (var keyValuePair in values)
            {
                postContent.Add(new StringContent(keyValuePair.Value),
                    String.Format("\"{0}\"", keyValuePair.Key));
            }

            MemoryStream memoryStream = new MemoryStream();
            uploadRequest.Snapshot.Save(memoryStream, ImageFormat.Jpeg);

            var bytes = memoryStream.ToArray();

            var streamContent = new ByteArrayContent(bytes);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = BitHoursReqParams.Screenshot,
                FileName = String.Format("{0}_{1}.jpg", BitHoursReqParams.Screenshot, Guid.NewGuid())
            };

            postContent.Add(streamContent);
            
            HttpClient httpClient = null;

            var bitHoursApiResponse = new BitHoursApiResponse<object>();

            try
            {
                httpClient = new HttpClient();
                var response = await httpClient.PostAsync(requestUrl, postContent, cancellationToken);
                var content = await response.Content.ReadAsStringAsync();
                bitHoursApiResponse.Result = JsonConvert.DeserializeObject<BitHoursApiJsonResponse<object>>(content);

                if (!bitHoursApiResponse.Result.success)
                    bitHoursApiResponse.ErrorCode = BitHoursApiErrors.GetContractsError;
            }
            catch (Exception ex)
            {
                bitHoursApiResponse.ErrorCode = BitHoursApiErrors.GetContractsError;

#if DEBUG

                bitHoursApiResponse.Error = ex.Message;

#endif
            }
            finally
            {
                if (httpClient != null)
                    httpClient.Dispose();

                if (postContent != null)
                    postContent.Dispose();

                if (streamContent != null)
                    streamContent.Dispose();

                if (memoryStream != null)
                    memoryStream.Dispose();
            }

            return bitHoursApiResponse;
        }
    }
}