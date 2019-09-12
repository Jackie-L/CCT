using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMCService
{
    class JobNodePlan : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                ILog logger = (ILog)context.JobDetail.JobDataMap.Get("logger");
                string url = context.JobDetail.JobDataMap.GetString("url");
                Service1.HttpGet(url);
                logger.InfoFormat("JobNodePlan访问发送节点超期提醒链接成功 {0}", DateTime.Now);
            }
            catch (Exception ex)
            {
                ILog logger = (ILog)context.JobDetail.JobDataMap.Get("logger");
                logger.ErrorFormat("JobNodePlan访问发送节点超期提醒链接成功出错，错误信息：{0}", ex);
            }
            return Task.FromResult(0);
        }
    }
}
