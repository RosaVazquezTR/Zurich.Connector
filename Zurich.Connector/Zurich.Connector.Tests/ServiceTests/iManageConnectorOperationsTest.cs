﻿using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Zurich.Connector.App.Services.DataSources;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Zurich.Connector.Tests.Common;
using Zurich.Connector.Data;
using System;

namespace Zurich.Connector.Tests.ServiceTests
{
    //TODO: Create an app tests project and move this there

    [TestClass]
    public class iManageConnectorOperationsTest
    {
        private Mock<ILogger<IManageConnectorOperations>> _mockLogger;

        [TestInitialize]
        public void Init()
        {
            _mockLogger = new Mock<ILogger<IManageConnectorOperations>>();
        }

        [TestMethod]
        public void SetItemLinkTest_Should_SetWebUrl()
        {
            //Arrange
            var mockDocuments = MockConnectorData.SetupDocumentsModel();
            var hostName = "my.cookieapp.com";
            var expectedUrl = $"https://{hostName}/work/link/d/1";
            //Act
            var service = new IManageConnectorOperations(_mockLogger.Object);
            var result = (service.SetItemLink(Data.Model.EntityType.Document, mockDocuments, hostName) as JArray);
            //Assert
            result.Should().NotBeNullOrEmpty();
            var doc = result[0] as JObject;
            doc.ContainsKey(StructuredCDMProperties.WebLink).Should().BeTrue();
            doc[StructuredCDMProperties.WebLink].Value<string>().Should().Be(expectedUrl);
        }

        [TestMethod]
        public void SetItemLinkTest_Should_Log_Error_And_Return_Original_Entities_If_Hostname_Is_Invalid()
        {
            //Arrange
            var mockDocuments = MockConnectorData.SetupDocumentsModel();
            //Act
            var service = new IManageConnectorOperations(_mockLogger.Object);
            var result = (service.SetItemLink(Data.Model.EntityType.Document, mockDocuments, null) as JArray);
            //Assert
            _mockLogger.Verify(ml => ml.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, _) => v.ToString().StartsWith("Unable to parse")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));
            result.Should().NotBeNullOrEmpty();
            var doc = result[0] as JObject;
            doc.ContainsKey(StructuredCDMProperties.WebLink).Should().BeFalse();
        }
    }
}
