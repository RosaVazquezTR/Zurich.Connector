using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Common;
using Zurich.Common.Services.Security;
using Zurich.Connector.Durable.Constants;
using Zurich.Connector.Durable.Model;
using Zurich.Connector.Durable.Repository;

namespace Zurich.Connector.Durable
{
    /// <summary>
    /// Practical Law Service
    /// </summary>
    public interface IPLService
    {
        /// <summary>
        /// Get Practical Law filters by locale
        /// </summary>
        /// <param name="locale"></param>
        /// <returns></returns>
        public Task<IEnumerable<TaxonomyOptions>> GetPLDynamicFilterList(string locale);
    }

    public class PLService: IPLService
    {
        private ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IOAuthService _oAuthService;
        private readonly IPracticalLawRepository _practicalLawRepository;
        public PLService(IOAuthService oAuthService, IMapper mapper, IPracticalLawRepository practicalLawRepository, ILogger<PLService> logger)
        {
            _logger = logger;
            _mapper = mapper;
            _oAuthService = oAuthService;
            _practicalLawRepository = practicalLawRepository;
        }

        public async Task<IEnumerable<TaxonomyOptions>> GetPLDynamicFilterList(string locale)
        {
            try
            {
                var result = await GetPlFacetFromPlService(locale);
                return result.ServicePracticeAreaList.Categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching filters.", ex.Message);
                throw;
            }
        }

        private async Task<PlFacetCommonModel> GetPlFacetFromPlService(string locale)
        {
            PlFacetCommonModel response = new PlFacetCommonModel();
            // Get Practice Area
            ServicePracticeAreaList taxonomyList = await GetPlTaxonomy(locale);
            PlTaxonomyResponse taxonomyView = new PlTaxonomyResponse();
            if (locale.ToUpper() == "US")
            {
                var practiceAreaList = taxonomyList.Service.FirstOrDefault(x => x.Name == "US").PracticeAreaList;
                taxonomyView.Categories = _mapper.Map<TaxonomyOptions[]>(practiceAreaList).OrderBy(x => x.Name);
            }
            else
            {
                taxonomyView.Categories = _mapper.Map<TaxonomyOptions[]>(taxonomyList.Service).OrderBy(x => x.Name);
            }
            var practiceArea = JsonConvert.SerializeObject(taxonomyView, new StringEnumConverter());
            response.ServicePracticeAreaList = taxonomyView;

            return response;
        }

        private async Task<ServicePracticeAreaList> GetPlTaxonomy(string locale)
        {
            ServicePracticeAreaList taxonomyList = new ServicePracticeAreaList();

            var token = await GetToken(locale);

            if (token != null)
            {
                taxonomyList = await _practicalLawRepository.GetTaxonomyList(token.ToString(), locale, PracticalLawFacet.Taxonomy);

            }
            return taxonomyList;
        }

        private async Task<string> GetToken(string locale)
        {
            try
            {
                OAuthApplicationType appType = OAuthApplicationType.ServiceApp;
                var token = await _oAuthService.RequestNewToken(ServiceAppCodes.PracticalLaw, OAuthConstants.GrantTypes.ClientCredentials, appType, locale: locale);
                return token.access_token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not get access token.", ex.Message);
                throw;
            }
        }
    }
}
