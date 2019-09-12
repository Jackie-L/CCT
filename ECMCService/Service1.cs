using log4net;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ECMCService
{
    public partial class Service1 : ServiceBase
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(Service1));
        private static readonly string SendWXTaskUrl = ConfigurationManager.AppSettings["SendWXTaskUrl"].ToString();
        private static readonly int SendWXTaskTime = Convert.ToInt32(ConfigurationManager.AppSettings["SendWXTaskTime"].ToString());
        private static readonly string SendWXMsgUrl = ConfigurationManager.AppSettings["SendWXMsgUrl"].ToString();
        private static readonly int SendWXMsgTime = Convert.ToInt32(ConfigurationManager.AppSettings["SendWXMsgTime"].ToString());
        private static readonly string NodePlanMsgUrl = ConfigurationManager.AppSettings["NodePlanMsgUrl"].ToString();
        private static readonly string NextTrackMsgUrl = ConfigurationManager.AppSettings["NextTrackMsgUrl"].ToString();
        private static readonly string SynchronousOrganizationUrl = ConfigurationManager.AppSettings["SynchronousOrganizationUrl"].ToString();
        private static readonly int SynchronousOrganizationTime = Convert.ToInt32(ConfigurationManager.AppSettings["SynchronousOrganizationTime"].ToString());
        private bool Flag = false;
        public Service1()
        {
            InitializeComponent();

            logger.Info("程序启动");
            Quartz.Logging.LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());
            //System.Timers.Timer timer = new System.Timers.Timer();
            //timer.Elapsed += new System.Timers.ElapsedEventHandler(TimedEvent);
            //timer.Interval = SendWXMsgTime * 1000;//执行时间间隔
            //timer.Enabled = true;

            RunProgramRunExample().GetAwaiter().GetResult();
        }
        private static async Task RunProgramRunExample()
        {
            try
            {
                System.Collections.Specialized.NameValueCollection props = new System.Collections.Specialized.NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
                StdSchedulerFactory factory = new StdSchedulerFactory(props);
                IScheduler scheduler = await factory.GetScheduler();
                await scheduler.Start();

                if (!string.IsNullOrEmpty(SendWXTaskUrl) && SendWXTaskTime > 0)
                {
                    IJobDetail jobWXTask = JobBuilder.Create<JobWXTask>()
                        .WithIdentity(nameof(jobWXTask), "group1").Build();
                    jobWXTask.JobDataMap.Put("url", SendWXTaskUrl);
                    jobWXTask.JobDataMap.Put("logger", logger);
                    ITrigger triggerWXTask = TriggerBuilder.Create()
                        .WithIdentity(nameof(triggerWXTask), "group1")
                        .StartNow()
                        .WithSimpleSchedule(x => x.WithIntervalInSeconds(SendWXTaskTime).RepeatForever()).Build();
                    await scheduler.ScheduleJob(jobWXTask, triggerWXTask);
                }

                if (!string.IsNullOrEmpty(SendWXMsgUrl) && SendWXMsgTime > 0)
                {
                    IJobDetail jobWXMsg = JobBuilder.Create<JobWXMsg>()
                        .WithIdentity(nameof(jobWXMsg), "group1").Build();
                    jobWXMsg.JobDataMap.Put("url", SendWXMsgUrl);
                    jobWXMsg.JobDataMap.Put("logger", logger);
                    ITrigger triggerWXMsg = TriggerBuilder.Create()
                        .WithIdentity(nameof(triggerWXMsg), "group1")
                        .StartAt(new DateTimeOffset(DateTime.Now.AddSeconds(33)))
                        .WithSimpleSchedule(x => x.WithIntervalInSeconds(SendWXMsgTime).RepeatForever()).Build();
                    await scheduler.ScheduleJob(jobWXMsg, triggerWXMsg);
                }

                if (!string.IsNullOrEmpty(NodePlanMsgUrl))
                {
                    IJobDetail jobNodePlan = JobBuilder.Create<JobNodePlan>().WithIdentity(nameof(jobNodePlan), "group1").Build();
                    jobNodePlan.JobDataMap.Put("url", NodePlanMsgUrl);
                    jobNodePlan.JobDataMap.Put("logger", logger);
                    ITrigger triggerNodePlan = TriggerBuilder.Create()
                        .WithIdentity(nameof(triggerNodePlan), "group1")
                        .StartAt(DateBuilder.DateOf(10, 0, 0))
                        .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever()).Build();
                    await scheduler.ScheduleJob(jobNodePlan, triggerNodePlan);
                }

                if (!string.IsNullOrEmpty(NextTrackMsgUrl))
                {
                    IJobDetail jobNextTrack = JobBuilder.Create<JobNextTrack>().WithIdentity(nameof(jobNextTrack), "group1").Build();
                    jobNextTrack.JobDataMap.Put("url", NextTrackMsgUrl);
                    jobNextTrack.JobDataMap.Put("logger", logger);
                    ITrigger triggerNextTrack = TriggerBuilder.Create()
                        .WithIdentity(nameof(triggerNextTrack), "group1")
                        .StartAt(DateBuilder.DateOf(9, 30, 0))
                        .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever()).Build();
                    await scheduler.ScheduleJob(jobNextTrack, triggerNextTrack);
                }

                if (!string.IsNullOrEmpty(SynchronousOrganizationUrl) && SynchronousOrganizationTime > 0)
                {
                    IJobDetail jobSO = JobBuilder.Create<JobSO>()
                        .WithIdentity(nameof(jobSO), "group1").Build();
                    jobSO.JobDataMap.Put("url", SynchronousOrganizationUrl);
                    jobSO.JobDataMap.Put("logger", logger);
                    ITrigger triggerSO = TriggerBuilder.Create()
                        .WithIdentity(nameof(triggerSO), "group1")
                        .StartAt(new DateTimeOffset(DateTime.Now.AddSeconds(55)))
                        .WithSimpleSchedule(x => x.WithIntervalInSeconds(SynchronousOrganizationTime).RepeatForever()).Build();
                    await scheduler.ScheduleJob(jobSO, triggerSO);
                }
                //await Task.Delay(-1);
                //await scheduler.Shutdown();
            }
            catch (SchedulerException se)
            {
                logger.FatalFormat("RunProgramRunExample方法异常，异常信息： {0}", se);
            }
        }
        //定时执行事件
        //private void TimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    int intHour = e.SignalTime.Hour;
        //    int intMinute = e.SignalTime.Minute;
        //    int intSecond = e.SignalTime.Second;
        //    //WriteLog();
        //    //10点发任务超期提醒
        //    if (intHour == 10 && Flag)
        //    {
        //        Flag = false;
        //        if (!string.IsNullOrEmpty(NodePlanMsgUrl))
        //        {
        //            HttpGet(NodePlanMsgUrl);
        //        }
        //    }
        //    if (intHour == 00)
        //    {
        //        Flag = true;
        //    }

        //    HttpGet(SendWXMsgUrl); //发送微信待办消息
        //}
        private void WriteLog()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\log.txt";
            using (StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8))
            {
                sw.WriteLine(DateTime.Now.ToString() + " 写入");
            }
        }
        public static string HttpGet(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";//设置请求的方法
            request.Accept = "*/*";//设置Accept标头的值
            request.Timeout = 10000;
            string responseStr = "";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())//获取响应
            {
                if (response != null)
                {
                    using (StreamReader reader =
        new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        responseStr = reader.ReadToEnd();
                    }
                }
            }
            return responseStr;
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}
