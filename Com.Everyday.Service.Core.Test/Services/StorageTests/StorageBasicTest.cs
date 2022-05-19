﻿using Com.Danliris.Service.Core.Test.Helpers;
using Com.DanLiris.Service.Core.Lib;
using Com.DanLiris.Service.Core.Lib.Helpers.IdentityService;
using Com.DanLiris.Service.Core.Lib.Helpers.ValidateService;
using Com.DanLiris.Service.Core.Lib.Models;
using Com.DanLiris.Service.Core.Lib.Models.Module;
using Com.DanLiris.Service.Core.Lib.Services;
using Com.DanLiris.Service.Core.Lib.ViewModels;
using Com.DanLiris.Service.Core.Test.DataUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Xunit;

namespace Com.DanLiris.Service.Core.Test.Services.StorageTests
{
    public class StorageBasicTest
    {
        private const string ENTITY = "Storages";

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCurrentMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return string.Concat(sf.GetMethod().Name, "_", ENTITY);
        }

        private CoreDbContext _dbContext(string testName)
        {
            DbContextOptionsBuilder<CoreDbContext> optionsBuilder = new DbContextOptionsBuilder<CoreDbContext>();
            optionsBuilder
                .UseInMemoryDatabase(testName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            CoreDbContext dbContext = new CoreDbContext(optionsBuilder.Options);

            return dbContext;
        }


        private StorageDataUtil _dataUtil(StorageService service)
        {
            CoreDbContext dbContext = _dbContext(GetCurrentMethod());
            return new StorageDataUtil(dbContext, service);
        }

        Mock<IServiceProvider> GetServiceProvider()
        {
            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
              .Setup(s => s.GetService(typeof(IIdentityService)))
              .Returns(new IdentityService() { TimezoneOffset = 1, Token = "token", Username = "username" });

            var validateService = new Mock<IValidateService>();
            serviceProvider
              .Setup(s => s.GetService(typeof(IValidateService)))
              .Returns(validateService.Object);
            return serviceProvider;
        }

        protected string GetCurrentAsyncMethod([CallerMemberName] string methodName = "")
        {
            var method = new StackTrace()
                .GetFrames()
                .Select(frame => frame.GetMethod())
                .FirstOrDefault(item => item.Name == methodName);

            return method.Name;

        }

        [Fact]
        public async void Should_Success_Create_Data()
        {

            CoreDbContext dbContext = _dbContext(GetCurrentAsyncMethod());
            Mock<IServiceProvider> serviceProvider = GetServiceProvider();

            StorageService service = new StorageService(serviceProvider.Object);

            serviceProvider.Setup(s => s.GetService(typeof(StorageService))).Returns(service);
            serviceProvider.Setup(s => s.GetService(typeof(CoreDbContext))).Returns(dbContext);

            // var service = new StorageService(GetServiceProvider().Object);
            var data = _dataUtil(service).GetNewData();

            var Response = await service.CreateAsync(data);
            Assert.NotEqual(0, Response);
        }

        [Fact]
        public async void Should_Success_Get_Data()
        {
            CoreDbContext dbContext = _dbContext(GetCurrentAsyncMethod());
            Mock<IServiceProvider> serviceProvider = GetServiceProvider();

            StorageService service = new StorageService(serviceProvider.Object);

            serviceProvider.Setup(s => s.GetService(typeof(StorageService))).Returns(service);
            serviceProvider.Setup(s => s.GetService(typeof(CoreDbContext))).Returns(dbContext);

            var data = await _dataUtil(service).GetTestDataAsync();

            var Response = service.ReadModel(1, 25, "{}", null, data.Name, "{}");
            Assert.NotEmpty(Response.Item1);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            CoreDbContext dbContext = _dbContext(GetCurrentAsyncMethod());
            Mock<IServiceProvider> serviceProvider = GetServiceProvider();

            StorageService service = new StorageService(serviceProvider.Object);

            serviceProvider.Setup(s => s.GetService(typeof(StorageService))).Returns(service);
            serviceProvider.Setup(s => s.GetService(typeof(CoreDbContext))).Returns(dbContext);
            var data = await _dataUtil(service).GetTestDataAsync();

            var Response = await service.ReadModelById(data.Id);
            Assert.NotNull(Response);
        }

        [Fact]
        public async void Should_Success_Update_Data()
        {
            CoreDbContext dbContext = _dbContext(GetCurrentAsyncMethod());
            Mock<IServiceProvider> serviceProvider = GetServiceProvider();

            StorageService service = new StorageService(serviceProvider.Object);

            serviceProvider.Setup(s => s.GetService(typeof(StorageService))).Returns(service);
            serviceProvider.Setup(s => s.GetService(typeof(CoreDbContext))).Returns(dbContext);
            var data = await _dataUtil(service).GetTestDataAsync();
            var newData = await service.ReadModelById(data.Id);

            var Response = await service.UpdateAsync(newData.Id, newData);
            Assert.NotEqual(0, Response);
        }

        [Fact]
        public async void Should_Success_Delete_Data()
        {
            CoreDbContext dbContext = _dbContext(GetCurrentAsyncMethod());
            Mock<IServiceProvider> serviceProvider = GetServiceProvider();

            StorageService service = new StorageService(serviceProvider.Object);

            serviceProvider.Setup(s => s.GetService(typeof(StorageService))).Returns(service);
            serviceProvider.Setup(s => s.GetService(typeof(CoreDbContext))).Returns(dbContext);
            var data = await _dataUtil(service).GetTestDataAsync();

            var Response = await service.DeleteAsync(data.Id);
            Assert.NotEqual(0, Response);
        }

        [Fact]
        public void Validate_Model()
        {
            CoreDbContext dbContext = _dbContext(GetCurrentAsyncMethod());
            var serviceProvider = GetServiceProvider();
            StorageService service = new StorageService(serviceProvider.Object);

            serviceProvider.Setup(s => s.GetService(typeof(StorageService))).Returns(service);
            serviceProvider.Setup(s => s.GetService(typeof(CoreDbContext))).Returns(dbContext);
            var model = new Storage();
            var validationContext = new ValidationContext(model, serviceProvider.Object, null);

            var result = model.Validate(validationContext);

            Assert.NotEmpty(result.ToList());
        }

        [Fact]
        public async void Validate_ViewModel_Duplicate()
        {
            CoreDbContext dbContext = _dbContext(GetCurrentAsyncMethod());
            Mock<IServiceProvider> serviceProvider = GetServiceProvider();

            StorageService service = new StorageService(serviceProvider.Object);

            serviceProvider.Setup(s => s.GetService(typeof(StorageService))).Returns(service);
            serviceProvider.Setup(s => s.GetService(typeof(CoreDbContext))).Returns(dbContext);

            var data = await _dataUtil(service).GetTestDataAsync();
            var viewModel = new StorageViewModel
            {
                code="code",
                moduleSources= new System.Collections.Generic.List<ModuleSourceViewModel>()
                {
                    new ModuleSourceViewModel
                    {
                        moduleSource=new Lib.ViewModels.Module.ModuleViewModel()
                        {
                            Name="name",
                            _id=1,
                            Code="code"
                        },
                        
                    }
                },
                moduleDestinations= new System.Collections.Generic.List<ModuleDestinationViewModel>()
                {
                    new ModuleDestinationViewModel()
                    {
                        moduleDestination= new Lib.ViewModels.Module.ModuleViewModel()
                        {
                            Name="name",
                            _id=1,
                            Code="code"
                        }
                    }
                }
            };

            var model = service.MapToModel(viewModel);
            var validationContext = new ValidationContext(model, serviceProvider.Object, null);
            var result = model.Validate(validationContext);

            Assert.NotEmpty(result.ToList());
        }
    }
}
