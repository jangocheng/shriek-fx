﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shriek.Samples.WebApiProxy.Contracts;
using Shriek.ServiceProxy.Http;
using Shriek.ServiceProxy.Http.Server;
using System;
using Shriek.Samples.WebApiProxy.Models;

namespace Shriek.Samples.WebApiProxy
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://*:8080", "http://*:8081")
                .ConfigureServices(services =>
                {
                    services.AddMvcCore()
                        .AddJsonFormatters()
                        .AddWebApiProxyServer(opt =>
                            {
                                opt.AddWebApiProxy<SampleApiProxy>("http://localhost:8081");
                                opt.AddWebApiProxy<Samples.Services.SampleApiProxy>("http://localhost:8080");
                            });

                    //服务里注册代理客户端
                    services.AddWebApiProxy(opt =>
                    {
                        opt.AddWebApiProxy<SampleApiProxy>("http://localhost:8081");
                    });
                })
                .Configure(app => app.UseMvc())
                .Build()
                .Start();

            var provider = new ServiceCollection()
                .AddWebApiProxy(opt =>
                {
                    opt.AddWebApiProxy<SampleApiProxy>("http://localhost:8081");
                    opt.AddWebApiProxy<Samples.Services.SampleApiProxy>("http://localhost:8080");
                })
                .BuildServiceProvider();

            var todoService = provider.GetService<ITodoService>();
            var testService = provider.GetService<ITestService>();
            var sampleTestService = provider.GetService<Samples.Services.ITestService>();
            var tcpService = provider.GetService<ITcpTestService>();

            Console.ReadKey();

            var createRsult = todoService.Create(new Todo() { Name = "james" }).Result;
            Console.WriteLine(JsonConvert.SerializeObject(createRsult));

            var result = todoService.Get(1).Result;
            Console.WriteLine(JsonConvert.SerializeObject(result));

            result = todoService.Get(2).Result;
            Console.WriteLine(JsonConvert.SerializeObject(result));

            var typeResult = todoService.GetTypes(new[] { Contracts.Type.起床, Contracts.Type.睡觉 }, "james", 10);
            Console.WriteLine(JsonConvert.SerializeObject(typeResult));

            //这个调用服务，服务内注入了一个代理客户端调用另一个服务
            var result2 = testService.Test(11);
            Console.WriteLine(JsonConvert.SerializeObject(result2));

            //var result3 = sampleTestService.Test("elderjames").Result;
            //Console.WriteLine(JsonConvert.SerializeObject(result3));

            Console.WriteLine("press any key to tcp testing...");
            Console.ReadKey();

            //var result4 = tcpService.Test("hahaha").Result;
            //Console.WriteLine(JsonConvert.SerializeObject(result4));

            Console.ReadKey();
        }
    }
}