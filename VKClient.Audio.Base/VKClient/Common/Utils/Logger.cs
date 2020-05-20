using Microsoft.Phone.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Utils
{
    public class Logger
    {
        private object lockObj = new object();
        private string LOGNAME = "VKLog.txt";
        //private static bool IsLoggingToIsolatedStorageEnabled=false;
        private static Logger _logger;

        private List<string> _logItems /*{ get; }*/ = new List<string>();

        public static Logger Instance
        {
            get
            {
                return Logger._logger ?? (Logger._logger = new Logger());
            }
        }

        public void LogMemoryUsage()
        {
            this.Info("Memory usage: {0}", (long)DeviceExtendedProperties.GetValue("ApplicationCurrentMemoryUsage"));
        }

        private void Log(string message)
        {
            this.AddLogItem(message);
        }

        private void AddLogItem(string message)
        {
            this._logItems.Add(message);
            if (this._logItems.Count <= 200)
                return;
            while (this._logItems.Count > 200)
                this._logItems.RemoveAt(0);
        }

        public string GetLog()
        {
            return string.Join("\n", this._logItems);
        }

        public void Assert(bool assertion, string commentOnFailure)
        {
            if (assertion)
                return;
            this.Info("ASSERTION FAILED, {0}", commentOnFailure);
        }

        public void Info(string info, params object[] formatParameters)
        {
            string logMsg = info;
            if (formatParameters != null && formatParameters.Length != 0)
                logMsg = string.Format(info, formatParameters);
            //
            //System.Diagnostics.Debug.WriteLine(logMsg);
            //
            string str = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + ": " + logMsg;
            this.Log(str.Substring(0, Math.Min(500, str.Length)));
            if (!VKClient.Common.Library.AppGlobalStateManager.Current.GlobalState.IsLogsEnabled)//if (!Logger.IsLoggingToIsolatedStorageEnabled)
                return;
            this.WriteLogToStorage(logMsg);
        }

        public void Error(string error, ResultCode code)
        {
            string str = "ERROR: " + error + " ErrorCode: " + code;
            this.Log(str);
            
            if (!VKClient.Common.Library.AppGlobalStateManager.Current.GlobalState.IsLogsEnabled)//if (!Logger.IsLoggingToIsolatedStorageEnabled)
                return;
            //
            System.Diagnostics.Debug.WriteLine(str);
            //
            this.WriteLogToStorage(str);
        }

        public void Error(string error)
        {
            string str = "ERROR: " + error;
            this.Log(str);
            
            if (!VKClient.Common.Library.AppGlobalStateManager.Current.GlobalState.IsLogsEnabled)//if (!Logger.IsLoggingToIsolatedStorageEnabled)
                return;
            //
            System.Diagnostics.Debug.WriteLine(str);
            //
            this.WriteLogToStorage(str);
        }

        public void Error(string error, Exception e)
        {
            string exceptionData = this.GetExceptionData(e);
            string str = "ERROR: " + error + Environment.NewLine + exceptionData;
            this.Log(str);
            
            if (!VKClient.Common.Library.AppGlobalStateManager.Current.GlobalState.IsLogsEnabled)//if (!Logger.IsLoggingToIsolatedStorageEnabled)
                return;
            //
            System.Diagnostics.Debug.WriteLine(str);
            //
            this.WriteLogToStorage(str);
        }

        public void ErrorAndSaveToIso(string error, Exception e)
        {
            string exceptionData = this.GetExceptionData(e);
            string str = "ERROR: " + error + Environment.NewLine + exceptionData;
            this.Log(str);
            this.WriteLogToStorage(str);
        }

        private string GetExceptionData(Exception e)
        {
            string str = "e.Message = " + e.Message + Environment.NewLine + "e.Stack = " + e.StackTrace;
            if (e.InnerException != null)
                return str + Environment.NewLine + this.GetExceptionData(e.InnerException);
            return str;
        }

        private void WriteLogToStorage(string logMsg)
        {
            try
            {
                logMsg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + logMsg;
                lock (this.lockObj)
                {
                    using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        Stream stream;
                        if (!storeForApplication.FileExists(this.LOGNAME))
                        {
                            stream = (Stream)storeForApplication.CreateFile(this.LOGNAME);
                        }
                        else
                        {
                            stream = (Stream)storeForApplication.OpenFile(this.LOGNAME, FileMode.OpenOrCreate);
                            stream.Seek(0, SeekOrigin.End);
                        }
                        using (StreamWriter streamWriter = new StreamWriter(stream))
                        {
                            streamWriter.WriteLine(logMsg);
                            streamWriter.Flush();
                            streamWriter.Close();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private string SanitizeLog(string msg)
        {
            string str = msg.ToLowerInvariant();
            if (str.Contains("body"))
                str = str.Substring(0, str.IndexOf("body"));
            if (str.Contains("password"))
                str = str.Substring(0, str.IndexOf("password"));
            return str;
        }

        public void DeleteLogFromIsolatedStorage()
        {
            using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storeForApplication.FileExists(this.LOGNAME))
                    return;
                storeForApplication.DeleteFile(this.LOGNAME);
            }
        }
    }
}
