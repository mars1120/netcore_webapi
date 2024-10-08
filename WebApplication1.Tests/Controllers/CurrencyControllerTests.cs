using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using WebApplication1.Controllers;
using WebApplication1.Models;
using Newtonsoft.Json;
using WebApplication1.Models.Dto;
using WebApplication1.Tests.Interface;

namespace WebApplication1.Tests.Controllers
{
    [TestFixture]
    public class CurrencyControllerTests
    {
        private Mock<CurrencyDbContext> _mockContext;
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private CurrencyController _controller;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<CurrencyDbContext>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _controller = new CurrencyController(_mockContext.Object, _mockHttpClientFactory.Object);
        }

        [Test]
        public async Task GetCombinedCurrencies_ReturnsSortedCombinedData()
        {
            // Arrange
            var currenciesList = new List<Currency>
            {
                new Currency
                {
                    Id = 1, Code = "USD", Symbol = "$", Description = "US Dollar", Rate = 1m, RateFloat = 1m,
                    UpdatedAt = DateTime.UtcNow
                },
                new Currency
                {
                    Id = 2, Code = "EUR", Symbol = "€", Description = "Euro", Rate = 0.85m, RateFloat = 0.85m,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            var currentLangCurrenciesList = new List<CurrentLangCurrency>
            {
                new CurrentLangCurrency { Id = 1, CurrencyId = 1, CurrentLang = "zh-tw", LangTitle = "美元" },
                new CurrentLangCurrency { Id = 2, CurrencyId = 2, CurrentLang = "zh-tw", LangTitle = "歐元" }
            };

            var mockCurrencyDbSet = MockDbSet(currenciesList);
            var mockCurrentLangCurrencyDbSet = MockDbSet(currentLangCurrenciesList);

            _mockContext.Setup(c => c.Currencies).Returns(mockCurrencyDbSet.Object);
            _mockContext.Setup(c => c.CurrentLangCurrencies).Returns(mockCurrentLangCurrencyDbSet.Object);

            // Act
            var result = await _controller.GetCombinedCurrencies();

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var combinedCurrencies = okResult.Value as IEnumerable<CurrencyInfoDto>;
            Assert.That(combinedCurrencies, Is.Not.Null);

            var currencyList = combinedCurrencies.ToList();
            Assert.That(currencyList.Count, Is.EqualTo(2));

            // Check if sorted by Code
            Assert.That(currencyList[0].Code, Is.EqualTo("EUR"));
            Assert.That(currencyList[1].Code, Is.EqualTo("USD"));

            // Check combined data
            Assert.That(currencyList[0].LangTitle, Is.EqualTo("歐元"));
            Assert.That(currencyList[1].LangTitle, Is.EqualTo("美元"));

            // Check other properties
            Assert.That(currencyList[0].Symbol, Is.EqualTo("€"));
            Assert.That(currencyList[0].Description, Is.EqualTo("Euro"));
            Assert.That(currencyList[0].Rate, Is.EqualTo(0.85m));
            Assert.That(currencyList[0].RateFloat, Is.EqualTo(0.85m));

            Assert.That(currencyList[1].Symbol, Is.EqualTo("$"));
            Assert.That(currencyList[1].Description, Is.EqualTo("US Dollar"));
            Assert.That(currencyList[1].Rate, Is.EqualTo(1m));
            Assert.That(currencyList[1].RateFloat, Is.EqualTo(1m));
        }

        [Test]
        public async Task GetCurrency_ReturnsCorrectCurrency()
        {
            // Arrange
            var currency = new Currency { Id = 1, Code = "USD", Rate = 1 };
            _mockContext.Setup(c => c.Currencies.FindAsync(1)).ReturnsAsync(currency);
            var currentLangList = new List<CurrentLangCurrency>
            {
                new CurrentLangCurrency { }
            };
            var mockCurrentLangListDbSet = MockDbSet(currentLangList);
            _mockContext.Setup(c => c.CurrentLangCurrencies).Returns(mockCurrentLangListDbSet.Object);
            // Act
            var result = await _controller.GetCurrency(1);

            // Assert
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var actionResult = result.Result as OkObjectResult;
            Assert.That(actionResult, Is.Not.Null);
            var returnedCurrency = actionResult.Value as CurrencyInfoDto;
            Assert.That(returnedCurrency.Code, Is.EqualTo("USD"));
        }

        [Test]
        public async Task PostCurrency_CreatesNewCurrency()
        {
            // Arrange
            var newCurrency = new Currency { Code = "GBP", Rate = 0.72m };
            var mockSet = new Mock<DbSet<Currency>>();

            _mockContext.Setup(c => c.Currencies).Returns(mockSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1)
                .Verifiable();

            // Act
            var result = await _controller.PostCurrency(newCurrency);

            // Assert
            Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
            var createdAtActionResult = result.Result as CreatedAtActionResult;
            Assert.That(createdAtActionResult, Is.Not.Null);
            var returnedCurrency = createdAtActionResult.Value as Currency;
            Assert.That(returnedCurrency, Is.Not.Null);
            Assert.That(returnedCurrency.Code, Is.EqualTo("GBP"));
            Assert.That(returnedCurrency.Rate, Is.EqualTo(0.72m));

            mockSet.Verify(m => m.Add(It.Is<Currency>(c => c.Code == "GBP" && c.Rate == 0.72m)), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task UpdateCurrenciesFromCoinDesk_UpdatesExistingCurrencies()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(new CoinDeskResponse
                    {
                        Time = new TimeInfo
                        {
                            Updated = "May 9, 2023 00:00:00 UTC",
                            UpdatedISO = "2023-05-09T00:00:00+00:00",
                            UpdatedUK = "May 9, 2023 at 00:00 BST"
                        },
                        Disclaimer = "This data was produced from the CoinDesk Bitcoin Price Index (USD).",
                        ChartName = "Bitcoin",
                        Bpi = new Dictionary<string, CurrencyInfo>
                        {
                            { "USD", new CurrencyInfo { Code = "USD", Rate = 1, RateFloat = 1m } },
                            { "EUR", new CurrencyInfo { Code = "EUR", Rate = 0.85m, RateFloat = 0.85m } }
                        }
                    }), System.Text.Encoding.UTF8, "application/json")
                });
            var client = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            var existingCurrencies = new List<Currency>
            {
                new Currency { Id = 1, Code = "USD", Rate = 1 }
            };
            var mockDbSet = MockDbSet(existingCurrencies);
            _mockContext.Setup(c => c.Currencies).Returns(mockDbSet.Object);

            // Act
            var result = await _controller.UpdateCurrenciesFromCoinDesk();

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo("Successfully updated 2 currencies."));
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        private static Mock<DbSet<T>> MockDbSet<T>(List<T> list) where T : class
        {
            var queryable = list.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider)
                .Returns(new MockAsyncQueryProvider<T>(queryable.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new MockAsyncEnumerator<T>(queryable.GetEnumerator()));

            return mockSet;
        }
    }
}