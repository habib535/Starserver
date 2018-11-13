﻿using Microsoft.Owin.Hosting;
using System;
using System.Net.Http;

namespace StarServer
{
    internal class Program
    {
        internal static string AuthorizationKey = string.Empty;
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                AuthorizationKey = args[0];
            }
            else
            {
                AuthorizationKey = "551691c9-43da-477d-a358-94bd01be0d73";//Guid.NewGuid().ToString();
            }
            StartOptions options = new StartOptions();
            options.Urls.Add("http://localhost:5050");
            options.Urls.Add("http://127.0.0.1:5050");
            options.Urls.Add($"http://{Environment.MachineName}:5050");

            // Start OWIN host 
            using (WebApp.Start<Startup>(options))
            {
                Console.WriteLine($"Server started at localhost:5050 with authorization key: {AuthorizationKey}");
                Console.WriteLine($"Enter 'q' to exit the server");
                var input = Console.ReadLine();
                while (input != "q")
                {
                    input = Console.ReadLine();
                }
            }
        }
    }
}