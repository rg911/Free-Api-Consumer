﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Common.Infrastructure.Api;
using Common.Services.Interfaces;
using Common.ViewModel;
using log4net;

namespace Common.Services.Implementations
{
    public class EstablishmentService : IEstablishmentService
    {
        #region Properties

        private IApi<EstablishmentsViewModel> Api { get; }
        private ILog Log { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Taking a new instance of HttpClient and set it up in base class
        /// </summary>
        /// <param name="log">Log4Net instance</param>
        /// <param name="api">Api instance</param>
        public EstablishmentService(ILog log, IApi<EstablishmentsViewModel> api)
        {
            Api = api;
            Log = log;
        }

        #endregion

        #region Public Method
        /// <summary>
        /// Get establishments data and build Rating model
        /// </summary>
        /// <param name="uri">API uri</param>
        /// <param name="queryString">Query string</param>
        /// <returns>List of eatablishment ratings</returns>
        public async Task<IEnumerable<RatingViewModel>> GetRating(string uri, Dictionary<string, string> queryString)
        {
            var model = await Api.GetAsync(GetUri(uri, queryString));
            if (model != null)
            {
                try
                {
                    var restult = model.Establishments.GroupBy(x => x.RatingValue).Select(group => new { RateValue = group.Key, Count = group.Count() }).OrderBy(x => x.RateValue).ToList();

                    return restult.Select(x => new RatingViewModel() { RatingName = x.RateValue, Percentage = (decimal)x.Count / model.Establishments.Count() })
                        .ToList();
                }
                catch (Exception ex)
                {
                    Log.Error("Get establishment rating throw unhandled Exception: /r/n", ex);
                    throw;
                }
                
            }
            Log.Info("Failed to get establishments from remote Api");
            return null;
        }

        #endregion

        #region Private Method

        /// <summary>
        /// Build uri for API calls. If query string dictionary provided, append all query string
        /// </summary>
        /// <param name="uri">Api Rui</param>
        /// <param name="queryString">Query string dictionary</param>
        /// <returns>Api uri and all query string</returns>
        private static string GetUri(string uri, Dictionary<string, string> queryString)
        {
            //if no query string provided, just return base uri
            if (queryString.Count <= 0) return uri;
            var array = queryString.Select(x => $"{HttpUtility.UrlEncode(x.Key)}={HttpUtility.UrlEncode(x.Value)}").ToArray();
            return $"{uri}?{string.Join("&", array)}";
        }
        #endregion
    }
}