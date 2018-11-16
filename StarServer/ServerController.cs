﻿using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using System;
using System.Threading.Tasks;

namespace StarServer
{
    public class StarServerController : ApiController
    {
        protected const int MaxKeyValue = 9999;
        StarServerHttpClient client = new StarServerHttpClient();
        ServicesManager servicesManager = new ServicesManager();

        [HttpGet]
        public HttpResponseMessage Health()
        {
            Console.WriteLine($"Request received for Health check");
            return Request.CreateResponse(HttpStatusCode.OK, "Server is up and running");
        }

        //todo: come with a better name
        [HttpGet]
        public HttpResponseMessage Start()
        {
            Console.WriteLine($"Request received for Starting the server");
            if (IsAuthorizedRequest())
            {
                var scriptPath = ConfigurationManager.AppSettings["RunAllPath"];
                Processor.ExecuteCommand("CMD.exe", $"/c {@scriptPath}");

                scriptPath = ConfigurationManager.AppSettings["LoadConfigPath"];
                Processor.ExecuteCommand("CMD.exe", $"/c {@scriptPath}");

                return Request.CreateResponse(HttpStatusCode.Accepted, "Success");
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Authorization failed!");
            }
        }

        //todo: come with a better name
        [HttpGet]
        public HttpResponseMessage Deploy()
        {
            Console.WriteLine($"Request received for Deployment");
            if (IsAuthorizedRequest())
            {
                Task.Run(() => { UpdateChannel("deployment started"); });
                servicesManager.StopService("nginx");
                //execute script order by their `Key` value i.e. 1, 2, 3...
                foreach (int key in ConfigurationManager.AppSettings.AllKeys.Select(x => int.TryParse(x, out int val) ? val : MaxKeyValue).OrderBy(x => x))
                {
                    if (key != MaxKeyValue)
                    {
                        var scriptPath = ConfigurationManager.AppSettings[key.ToString()];
                        Console.WriteLine($"Executing {scriptPath}");
                        if (scriptPath.EndsWith(".ps1"))
                        {
                            Processor.ExecuteCommand("powershell.exe", $"-noprofile -executionpolicy bypass -file {@scriptPath}");
                        }
                        else
                        {
                            Processor.ExecuteCommand("CMD.exe", $"/c {@scriptPath}");
                        }
                    }
                }
                servicesManager.StartService("nginx");
                Task.Run(() => { UpdateChannel("deployment Finished"); });
                return Request.CreateResponse(HttpStatusCode.Accepted, "Success");
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Authorization failed!");
            }
        }

        private void UpdateChannel(string message)
        {
            try
            {
                var environmentName = ConfigurationManager.AppSettings["Environment"];
                var result = client.PostAsync($"*{environmentName}*: {message}").Result;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.ToString());
            }
        }

        [HttpGet]
        public HttpResponseMessage Create(string dbName)
        {
            Console.WriteLine($"Request received for creating new database");
            if (IsAuthorizedRequest())
            {
                if (string.IsNullOrWhiteSpace(dbName))
                {
                    dbName = "default";
                }
                Processor.ExecuteCommand("CMD.exe", "/C staradmin start server");
                Processor.ExecuteCommand("CMD.exe", $"/C staradmin new db {dbName}");
                return Request.CreateResponse(HttpStatusCode.Accepted, "Success");
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Authorization failed!");
            }
        }

        [HttpGet]
        public HttpResponseMessage Kill()
        {
            Console.WriteLine($"Request received for to kill starcounter host");
            if (IsAuthorizedRequest())
            {
                Processor.ExecuteCommand("CMD.exe", "/C staradmin kill all");
                return Request.CreateResponse(HttpStatusCode.Accepted, "Success");
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Authorization failed!");
            }
        }

        [HttpGet]
        public HttpResponseMessage Delete(string dbName)
        {
            Console.WriteLine($"Request received for Deleting the database");
            if (IsAuthorizedRequest())
            {
                if (string.IsNullOrWhiteSpace(dbName))
                {
                    dbName = "default";
                }
                Processor.ExecuteCommand("CMD.exe", "/C staradmin kill all");
                Processor.ExecuteCommand("CMD.exe", "/C staradmin start server");
                Processor.ExecuteCommand("CMD.exe", $"/C staradmin -d={dbName} delete db --force");

                return Request.CreateResponse(HttpStatusCode.Accepted, "Success");
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Authorization failed!");
            }
        }

        private bool IsAuthorizedRequest()
        {
            var headers = this.Request.Headers;
            var key = headers.GetValues("AuthorizationKey").FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(key) && key != Program.AuthorizationKey)
            {
                return false;
            }
            return true;
        }
    }
}
