using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;

namespace StarServer
{
    public class StarServerController : ApiController
    {
        protected const int MaxKeyValue = 9999;

        [HttpGet]
        public HttpResponseMessage Health()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Server is up and running");
        }

        [HttpGet]
        public HttpResponseMessage Start()
        {
            if (IsAuthorizedRequest())
            {
                //execute script order by their `Key` value i.e. 1, 2, 3...
                foreach (int key in ConfigurationManager.AppSettings.AllKeys.Select(x => int.TryParse(x, out int val) ? val : MaxKeyValue).OrderBy(x => x))
                {
                    if (key != MaxKeyValue)
                    {
                        var scriptPath = ConfigurationManager.AppSettings[key.ToString()];
                        if (scriptPath.EndsWith(".ps1"))
                        {
                            Processor.ExecuteCommand("powershell.exe", $"-noprofile -executionpolicy bypass -file {@scriptPath}");
                        }
                        else
                        {
                            Processor.ExecuteCommand("CMD.exe", "/c {@scriptPath}");
                        }
                    }
                }
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
