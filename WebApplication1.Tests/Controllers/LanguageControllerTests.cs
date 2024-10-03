using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
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
    public class LanguageControllerTests
    {
        private Mock<CurrencyDbContext> _mockContext;
        private LanguageController _controller;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<CurrencyDbContext>();
            _controller = new LanguageController(_mockContext.Object);
        }

        [Test]
        public async Task PostLanguage_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            string langCode = "en";
            string langName = "English";

            var languagesList = new List<Language>
            {
                new Language
                {
                    // Id = 1, LangCode = "en", LangName = "English", CreatedAt = DateTime.UtcNow,
                    // UpdatedAt = DateTime.UtcNow
                }
            };
            var mockLanguagesListDbSet = MockDbSet(languagesList);


            _mockContext.Setup(c => c.Languages).Returns(mockLanguagesListDbSet.Object);
            // Act
            var result = await _controller.PostLanguage(langCode, langName);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo(201));
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task PostLanguage_WithExistingLangCode_ReturnsConflict()
        {
            // Arrange
            string langCode = "en";
            string langName = "English";
            var languagesList = new List<Language>
            {
                new Language
                {
                    Id = 1, LangCode = "en", LangName = "English", CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            var mockLanguagesListDbSet = MockDbSet(languagesList);
            _mockContext.Setup(c => c.Languages).Returns(mockLanguagesListDbSet.Object);
            // Act
            var result = await _controller.PostLanguage(langCode, langName);

            Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
            var objectResult = result as ConflictObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo(409));
        }

        [Test]
        public async Task PostLanguage_WithEmptyLangName_ReturnsBadRequest()
        {
            // Arrange
            string langCode = "en";
            string langName = "";

            // Act
            var result = await _controller.PostLanguage(langCode, langName);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var objectResult = result as BadRequestObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task GetLanguages_ReturnsAllLanguagesAsDto()
        {
            var mockLanguagesList = new List<Language>
            {
                new Language
                {
                    Id = 1,
                    LangCode = "en",
                    LangName = "English",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow
                },
                new Language
                {
                    Id = 2,
                    LangCode = "es",
                    LangName = "Spanish",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddHours(-1)
                }
            };

            var mockLanguagesListDbSet = MockDbSet(mockLanguagesList);
            _mockContext.Setup(c => c.Languages).Returns(mockLanguagesListDbSet.Object);
            // Act
            var result = await _controller.GetLanguages();

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult.Value, Is.InstanceOf<IEnumerable<LanguageDto>>());

            var languages = okResult.Value as IEnumerable<LanguageDto>;
            Assert.That(languages, Is.Not.Null);
            Assert.That(languages.Count(), Is.EqualTo(2));

            var languagesList = languages.ToList();
            Assert.That(languagesList[0].Id, Is.EqualTo(1));
            Assert.That(languagesList[0].LangCode, Is.EqualTo("en"));
            Assert.That(languagesList[0].LangName, Is.EqualTo("English"));

            Assert.That(languagesList[1].Id, Is.EqualTo(2));
            Assert.That(languagesList[1].LangCode, Is.EqualTo("es"));
            Assert.That(languagesList[1].LangName, Is.EqualTo("Spanish"));
        }

        [Test]
        public async Task PutLanguage_WithValidData_ReturnsNoContent()
        {
            // Arrange
            int id = 1;
            string newLangName = "New English Name";
            var existingLanguage = new Language
            {
                Id = id,
                LangCode = "en",
                LangName = "English",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var languagesList = new List<Language> { existingLanguage };
            var mockLanguagesDbSet = MockDbSet(languagesList);

            _mockContext.Setup(c => c.Languages).Returns(mockLanguagesDbSet.Object);

            mockLanguagesDbSet.Setup(m => m.FindAsync(id))
                .ReturnsAsync(existingLanguage);

            // Act
            var result = await _controller.PutLanguage(id, newLangName);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
            Assert.That(existingLanguage.LangName, Is.EqualTo(newLangName));
            Assert.That(existingLanguage.UpdatedAt, Is.GreaterThan(existingLanguage.CreatedAt));

            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DeleteLanguage_WithValidId_ReturnsNoContent()
        {
            // Arrange
            int id = 1;
            var languageToDelete = new Language
            {
                Id = id,
                LangCode = "en",
                LangName = "English",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var languagesList = new List<Language> { languageToDelete };
            var mockLanguagesDbSet = MockDbSet(languagesList);

            _mockContext.Setup(c => c.Languages).Returns(mockLanguagesDbSet.Object);

            mockLanguagesDbSet.Setup(m => m.FindAsync(id))
                .ReturnsAsync(languageToDelete);

            mockLanguagesDbSet.Setup(m => m.Remove(It.IsAny<Language>()))
                .Callback<Language>((entity) => languagesList.Remove(entity));

            // Act
            var result = await _controller.DeleteLanguage(id);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
            Assert.That(languagesList, Is.Empty);

            mockLanguagesDbSet.Verify(m => m.Remove(languageToDelete), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DeleteLanguage_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            int id = 999; // 不存在的 ID

            _mockContext.Setup(c => c.Languages.FindAsync(id))
                .ReturnsAsync((Language)null);

            // Act
            var result = await _controller.DeleteLanguage(id);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteLanguage_WithOtherDbUpdateException_ReturnsInternalServerError()
        {
            // Arrange
            int id = 1;
            var language = new Language { Id = id, LangCode = "en", LangName = "English" };

            var mockLanguagesDbSet = new Mock<DbSet<Language>>();
            mockLanguagesDbSet.Setup(m => m.FindAsync(id))
                .ReturnsAsync(language);

            _mockContext.Setup(c => c.Languages).Returns(mockLanguagesDbSet.Object);

            // Setup SaveChangesAsync to throw a generic DbUpdateException
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Some other database error"));

            // Act
            var result = await _controller.DeleteLanguage(id);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo(500));
            Assert.That(objectResult.Value, Is.EqualTo("An error occurred while deleting the language."));
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