using Microsoft.Phone.Scheduler;
using System;

namespace VKClient.Common.Library
{
    public class TileScheduledUpdate
    {
        private string _periodicTaskName = "LiveTileScheduledTaskAgent";
        private static TileScheduledUpdate _instance;
        private PeriodicTask _periodicTask;

        public static TileScheduledUpdate Instance
        {
            get
            {
                if (TileScheduledUpdate._instance == null)
                    TileScheduledUpdate._instance = new TileScheduledUpdate();
                return TileScheduledUpdate._instance;
            }
        }

        public void Initialize()
        {
            this.EnsureScheduledUpdate();
        }

        private void EnsureScheduledUpdate()
        {
            this._periodicTask = ScheduledActionService.Find(this._periodicTaskName) as PeriodicTask;
            if (this._periodicTask != null)
            {
                try
                {
                    ScheduledActionService.Remove(this._periodicTaskName);
                }
                catch
                {
                }
            }
            this._periodicTask = new PeriodicTask(this._periodicTaskName);
            this._periodicTask.Description = "VK LiveTile update agent.";
            this._periodicTask.ExpirationTime = DateTime.Now.AddDays(14.0);
            try
            {
                ScheduledActionService.Add((ScheduledAction)this._periodicTask);
            }
            catch (InvalidOperationException ex)
            {
                ex.Message.Contains("BNS Error: The action is disabled");
                ex.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added.");
            }
            catch
            {
            }
        }
    }
}
