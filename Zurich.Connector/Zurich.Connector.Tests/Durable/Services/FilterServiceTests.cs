using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Connector.App;
using Zurich.Connector.App.Model;
using Zurich.Connector.App.Services;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Durable;
using Zurich.Connector.Durable.Model;
using Zurich.Connector.Durable.Service;

namespace Zurich.Connector.Tests.Durable.Services
{
    [TestClass]
    public class FilterServiceTests
    {
        private Mock<ICosmosService> _cosmosServiceMock;
        private Mock<ILogger<FilterService>> _loggerMock;
        private IMapper _mapper;

        [TestInitialize]
        public void Init()
        {
            _cosmosServiceMock = new Mock<ICosmosService>();
            _loggerMock = new Mock<ILogger<FilterService>>();


            var mapConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingRegistrar());
                cfg.AddProfile(new ServiceMappingRegistrar());
            });
            _mapper = mapConfig.CreateMapper();
        }

        [TestMethod]
        public async Task FilterService_Should_Not_Update_Connector_If_No_Filers()
        {
            var service = new FilterService(_cosmosServiceMock.Object, _mapper, _loggerMock.Object);

            var mockConnector = new ConnectorModel
            {
                Id = "1",
                Alias = "cookie.connector"
            };

            await service.UpdateDynamicFilter(mockConnector, null);

            _cosmosServiceMock.Verify(cs => cs.StoreConnector(It.IsAny<ConnectorDocument>()), Times.Never);
        }

        [TestMethod]
        public async Task FilterService_Should_Not_Update_FilterList_Of_First_Filter_If_It_Is_Empty()
        {
            var service = new FilterService(_cosmosServiceMock.Object, _mapper, _loggerMock.Object);

            var mockConnector = new ConnectorModel
            {
                Id = "1",
                Alias = "cookie.connector",
                Filters = new List<ConnectorsFiltersModel>()
                {
                    new ConnectorsFiltersModel
                    {
                        Name = "Flavour",
                        FilterList = new List<FilterListModel>()
                        {
                            new FilterListModel
                            {
                                Name = "Chocolate",
                                Id = "1"
                            },
                            new FilterListModel
                            {
                                Name = "Raisin",
                                Id = "2"
                            }
                        }
                    },
                    new ConnectorsFiltersModel
                    {
                        Name = "Dough"
                    }
                }
            };

            await service.UpdateDynamicFilter(mockConnector, null);

            _cosmosServiceMock.Verify(cs => cs.StoreConnector(It.IsAny<ConnectorDocument>()), Times.Never);
        }

        [TestMethod]
        public async Task FilterService_Should_Update_FilterList_Of_First_Filter_If_Filters_Are_Available()
        {
            var service = new FilterService(_cosmosServiceMock.Object, _mapper, _loggerMock.Object);

            var mockConnector = new ConnectorModel
            {
                Id = "1",
                Alias = "cookie.connector",
                Filters = new List<ConnectorsFiltersModel>()
                {
                    new ConnectorsFiltersModel
                    {
                        Name = "Flavour"
                    },
                    new ConnectorsFiltersModel
                    {
                        Name = "Dough"
                    }
                }
            };

            var mockFilterLIst = new List<TaxonomyOptions>()
            {
               new TaxonomyOptions()
               {
                   Name = "Chocolate",
                   Id = "1"
               }
            };

            await service.UpdateDynamicFilter(mockConnector, mockFilterLIst);

            _cosmosServiceMock.Verify(cs => cs.StoreConnector(It.IsAny<ConnectorDocument>()), Times.Once);
        }
    }
}
