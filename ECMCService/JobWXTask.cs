using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMCService
{
    class JobWXTask : IJob
    {
        //private static readonly ILog logger = LogManager.GetLogger(typeof(TestJob1));

        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                ILog logger = (ILog)context.JobDetail.JobDataMap.Get("logger");
                string url = context.JobDetail.JobDataMap.GetString("url");
                Service1.HttpGet(url);
                logger.InfoFormat("JobWXTask访问发送微信待办任务成功 {0}", DateTime.Now);
            }
            catch (Exception ex)
            {
                ILog logger = (ILog)context.JobDetail.JobDataMap.Get("logger");
                logger.ErrorFormat("JobWXTask访问发送微信待办任务出错，错误信息：{0}", ex);
            }
            return Task.FromResult(0);
        }
    }
}
