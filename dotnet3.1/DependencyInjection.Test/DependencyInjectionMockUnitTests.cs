﻿using DependencyInjection.Dependencies;
using DependencyInjection.Dependencies.Dependency1.Options;
using DependencyInjection.Dependencies.Dependency2.Options;
using DependencyInjection.Test.Mocks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace DependencyInjection.Test
{
    [TestClass]
    public class DependencyInjectionMockUnitTests
    {
        private readonly ServiceCollection _services;
        private readonly ServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public DependencyInjectionMockUnitTests()
        {
            // setup test settings
            var appSettings = new Dictionary<string, string>
            {
                { "dependency1:option1", "MockedOption1.1" },
                { "dependency1:option2", "MockedOption1.2" },
                { "dependency2:setting1", "MockedSetting2.1" },
                { "dependency2:setting2", "MockedSetting2.2" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(appSettings)
                .Build();

            // build the service collection
            _services = new ServiceCollection();

            _services.AddLogging(logging =>
            {
                logging.AddConsole();
            });

            // dependency1
            _services.Configure<Dependency1Options>(_configuration.GetSection("dependency1"));
            _services.AddOptions<IOptions<Dependency1Options>>()
                .Configure<IConfiguration>((settings, _configuration) =>
                {
                    _configuration.GetSection("dependency1").Bind(settings);
                });
            _services.AddScoped<IDependency1, MockDependency1>();

            // dependency2
            _services.Configure<Dependency2Options>(_configuration.GetSection("dependency2"));
            _services.AddOptions<IOptions<Dependency2Options>>()
                .Configure<IConfiguration>((settings, _configuration) =>
                {
                    _configuration.GetSection("dependency2").Bind(settings);
                });
            _services.AddScoped<IDependency2, Dependency_2>();

            _serviceProvider = _services.BuildServiceProvider();
        }

        [TestInitialize]
        public void Setup()
        {
        }

        [TestMethod]
        public void VerifyMockedSettingsGetInjected()
        {
            var dependency1 = _serviceProvider.GetRequiredService<IDependency1>();
            var jsonString1 = dependency1.GetPrettyJsonOfOptions();

            var dependency2 = _serviceProvider.GetRequiredService<IDependency2>();
            var jsonString2 = dependency2.GetPrettyJsonOfOptions();

            Assert.IsTrue(jsonString1.Contains("mockedOutput"));
            Assert.IsTrue(jsonString1.Contains("mockedOutput"));
            Assert.IsTrue(jsonString2.Contains("MockedSetting2.1"));
            Assert.IsTrue(jsonString2.Contains("MockedSetting2.2"));
        }
    }
}
