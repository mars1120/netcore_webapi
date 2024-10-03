using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using WebApplication1.Controllers;
using WebApplication1.Models;
using WebApplication1.Models.Dto;
using WebApplication1.Tests.Interface;

namespace WebApplication1.Tests.Controllers
{
    [TestFixture]
    public class CurrentLangControllerTests
    {
        private Mock<CurrencyDbContext> _mockContext;
        private CurrentLangCurrencyController _controller;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<CurrencyDbContext>();
            _controller = new CurrentLangCurrencyController(_mockContext.Object);
        }

        [Test]
        public async Task GetCurrentLangCurrencies_ReturnsAllCurrentLangCurrencies()
        {
            // Arrange
            var currentLangCurrenciesList = new List<CurrentLangCurrency>
            {
                new CurrentLangCurrency
                {
                    Id = 1, CurrencyId = 1, LangId = 1, CurrentLang = "en-us", LangTitle = "US Dollar",
                    UpdatedAt = DateTime.UtcNow
                },
                new CurrentLangCurrency
                {
                    Id = 2, CurrencyId = 2, LangId = 1, CurrentLang = "en-us", LangTitle = "Euro",
                    UpdatedAt = DateTime.UtcNow
                }
            };

            var mockDbSet = MockDbSet(currentLangCurrenciesList);
            _mockContext.Setup(c => c.CurrentLangCurrencies).Returns(mockDbSet.Object);

            // Act
            var result = await _controller.GetCurrentLangCurrencies();

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var returnedCurrencies = okResult.Value as IEnumerable<CurrentLangCurrencyDto>;
            Assert.That(returnedCurrencies, Is.Not.Null);
            Assert.That(returnedCurrencies.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetCurrentLangCurrency_WithValidId_ReturnsCurrentLangCurrency()
        {
            // Arrange
            var currentLangCurrenciesList = new List<CurrentLangCurrency>
            {
                new CurrentLangCurrency
                {
                    Id = 1, CurrencyId = 1, LangId = 1, CurrentLang = "en-us", LangTitle = "US Dollar",
                    UpdatedAt = DateTime.UtcNow
                }
            };

            var mockDbSet = MockDbSet(currentLangCurrenciesList);
            _mockContext.Setup(c => c.CurrentLangCurrencies).Returns(mockDbSet.Object);

            // Act
            var result = await _controller.GetCurrentLangCurrency(1);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var returnedCurrencies = okResult.Value as IEnumerable<CurrentLangCurrencyDto>;
            Assert.That(returnedCurrencies, Is.Not.Null);
            Assert.That(returnedCurrencies.Count(), Is.EqualTo(1));
            Assert.That(returnedCurrencies.First().LangTitle, Is.EqualTo("US Dollar"));
        }

        [Test]
        public async Task GetCurrentLangCurrency_WithValidCidAndLanid_ReturnsCurrentLangCurrency()
        {
            // Arrange
            var currentLangCurrenciesList = new List<CurrentLangCurrency>
            {
                new CurrentLangCurrency
                {
                    Id = 1, CurrencyId = 1, LangId = 1, CurrentLang = "en-us", LangTitle = "US Dollar",
                    UpdatedAt = DateTime.UtcNow
                }
            };

            var mockDbSet = MockDbSet(currentLangCurrenciesList);
            _mockContext.Setup(c => c.CurrentLangCurrencies).Returns(mockDbSet.Object);

            // Act
            var result = await _controller.GetCurrentLangCurrency(1, 1);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var returnedCurrency = okResult.Value as CurrentLangCurrencyDto;
            Assert.That(returnedCurrency, Is.Not.Null);
            Assert.That(returnedCurrency.CurrencyId, Is.EqualTo(1));
            Assert.That(returnedCurrency.LangId, Is.EqualTo(1));
            Assert.That(returnedCurrency.LangTitle, Is.EqualTo("US Dollar"));
        }

        [Test]
        public async Task PostCurrentLangCurrency_WithValidData_CreatesNewEntry()
        {
            // Arrange
            var currency = new Currency { Id = 1, Code = "USD" };
            var language = new Language { Id = 1, LangCode = "en-us" };
            var currentLangCurrenciesList = new List<CurrentLangCurrency>();

            // Mock DbSet for Currencies, Languages, and CurrentLangCurrencies
            var mockCurrencyDbSet = MockDbSet(new List<Currency> { currency });
            var mockLanguageDbSet = MockDbSet(new List<Language> { language });
            var mockCurrentLangCurrencyDbSet = MockDbSet(currentLangCurrenciesList);

            // Setup the mock context to return the mock DbSets
            _mockContext.Setup(c => c.Currencies).Returns(mockCurrencyDbSet.Object);
            _mockContext.Setup(c => c.Languages).Returns(mockLanguageDbSet.Object);
            _mockContext.Setup(c => c.CurrentLangCurrencies).Returns(mockCurrentLangCurrencyDbSet.Object);

            // Mock FindAsync for Currency and Language
            _mockContext.Setup(c => c.Currencies.FindAsync(1)).ReturnsAsync(currency);
            _mockContext.Setup(c => c.Languages.FindAsync(1)).ReturnsAsync(language);

            // Mock SaveChangesAsync to return success
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _controller.PostCurrentLangCurrency(1, 1, "US Dollar");

            // Assert
            // Check if the result is OkObjectResult
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            // Check if the returned object is CurrentLangCurrencyDto
            var createdCurrency = okResult.Value as CurrentLangCurrencyDto;
            Assert.That(createdCurrency, Is.Not.Null);

            // Verify the properties of the created CurrentLangCurrencyDto
            Assert.That(createdCurrency.CurrencyId, Is.EqualTo(1));
            Assert.That(createdCurrency.LangId, Is.EqualTo(1));
            Assert.That(createdCurrency.LangTitle, Is.EqualTo("US Dollar"));
            Assert.That(createdCurrency.CurrentLang, Is.EqualTo("en-us"));
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