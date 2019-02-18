using System;
using System.ServiceProcess;

namespace StarServer
{
    public class ServicesManager
    {
        ILogger logger = new Logger();
        public void StartService(string serviceName)
        {
            try
            {
                logger.Log($"Starting windows service: {serviceName}");
                ServiceController service = new ServiceController(serviceName);
                service.Start();
                var timeout = new TimeSpan(0, 0, 10); // 10 seconds
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch (Exception exp)
            {
                logger.Log(exp.ToString());
            }
        }

        public void StopService(string serviceName)
        {
            try
            {
                logger.Log($"Stopping windows service: {serviceName}");
                ServiceController service = new ServiceController(serviceName);
                service.Stop();
                var timeout = new TimeSpan(0, 0, 10); // 10 seconds
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch (Exception exp)
            {
                logger.Log(exp.ToString());
            }
        }
    }
}
