using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WebApplication1.Controllers;

namespace WebApplication1.Tests.Controllers;

[TestFixture]
public class WeatherForecastControllerTests
{
    private WeatherForecastController _controller;
    private Mock<ILogger<WeatherForecastController>> _loggerMock;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<WeatherForecastController>>();
        _controller = new WeatherForecastController(_loggerMock.Object);
    }

    [Test]
    public void Get_ReturnsCorrectNumberOfForecasts()
    {
        // Act
        var result = _controller.Get();

        // Assert
        Assert.That(result.Count(), Is.EqualTo(5));
    }

    [Test]
    public void Get_ReturnsCorrectDateRange()
    {
        // Act
        var result = _controller.Get();

        // Assert
        var today = DateOnly.FromDateTime(DateTime.Now);
        Assert.That(result.All(f => f.Date >= today && f.Date <= today.AddDays(5)));
    }

    [Test]
    public void Get_ReturnsValidTemperatures()
    {
        // Act
        var result = _controller.Get();

        // Assert
        Assert.That(result.All(f => f.TemperatureC >= -20 && f.TemperatureC <= 55));
    }

    [Test]
    public void Get_ReturnsValidSummaries()
    {
        // Arrange
        var validSummaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        // Act
        var result = _controller.Get();

        // Assert
        Assert.That(result.All(f => validSummaries.Contains(f.Summary)));
    }
}