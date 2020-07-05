using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using XurCollector.Services;

namespace XurCollector
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MongoService.InitializeMongoDatabase();
            ActivityCacheService.FillActivityCache();
            BungieService.Start();
            Console.ReadKey();
        }
    }
}