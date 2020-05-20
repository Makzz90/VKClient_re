using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using VKClient.Audio.Base.Core;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.BackendServices
{
    public class AppsService
    {
        private static AppsService _instance;
        private const string Platform = "winphone";
        private const string Fields = "photo_max,sex";

        public static AppsService Instance
        {
            get
            {
                return AppsService._instance ?? (AppsService._instance = new AppsService());
            }
        }

        private AppsService()
        {
        }

        public void GetCatalog(int offset, int count, Action<BackendResult<VKList<Game>, ResultCode>> callback)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary["platform"] = "winphone";
            dictionary["offset"] = offset.ToString();
            dictionary["count"] = count.ToString();
            string methodName = "apps.getCatalog";
            Dictionary<string, string> parameters = dictionary;
            CancellationToken? cancellationToken = new CancellationToken?();
            VKRequestsDispatcher.DispatchRequestToVK<VKList<Game>>(methodName, parameters, callback, (Func<string, VKList<Game>>)(jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<VKList<Game>>>(jsonStr).response), false, true, cancellationToken, null);
        }

        public void GetMyGames(int offset, int count, Action<BackendResult<VKList<Game>, ResultCode>> callback)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary["platform"] = "winphone";
            dictionary["filter"] = "installed";
            dictionary["offset"] = offset.ToString();
            dictionary["count"] = count.ToString();
            string methodName = "apps.getCatalog";
            Dictionary<string, string> parameters = dictionary;

            CancellationToken? cancellationToken = new CancellationToken?();
            VKRequestsDispatcher.DispatchRequestToVK<VKList<Game>>(methodName, parameters, callback, (Func<string, VKList<Game>>)(jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<VKList<Game>>>(jsonStr).response), false, true, cancellationToken, null);
        }

        public void GetActivity(long gameId, int count, string start_from, Action<BackendResult<GamesFriendsActivityResponse, ResultCode>> callback)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary["platform"] = "winphone";
            dictionary["fields"] = "photo_max,sex";
            dictionary["count"] = count.ToString();
            if (gameId > 0L)
                dictionary["filter_app_id"] = gameId.ToString();
            if (!string.IsNullOrEmpty(start_from))
                dictionary["start_from"] = start_from;
            string methodName = "apps.getActivity";
            Dictionary<string, string> parameters = dictionary;
            int num1 = 0;
            int num2 = 1;
            CancellationToken? cancellationToken = new CancellationToken?();
            // ISSUE: variable of the null type

            VKRequestsDispatcher.DispatchRequestToVK<GamesFriendsActivityResponse>(methodName, parameters, callback, (Func<string, GamesFriendsActivityResponse>)(jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<GamesFriendsActivityResponse>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
        }

        public void GetInviteRequests(int offset, int count, Action<BackendResult<GamesRequestsResponse, ResultCode>> callback)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary["platform"] = "winphone";
            dictionary["filter_type"] = "invite";
            dictionary["fields"] = "photo_max,sex";
            dictionary["count"] = count.ToString();
            dictionary["offset"] = offset.ToString();
            string methodName = "apps.getRequests";
            Dictionary<string, string> parameters = dictionary;
            Action<BackendResult<GamesRequestsResponse, ResultCode>> callback1 = callback;
            int num1 = 0;
            int num2 = 1;
            CancellationToken? cancellationToken = new CancellationToken?();
            // ISSUE: variable of the null type

            VKRequestsDispatcher.DispatchRequestToVK<GamesRequestsResponse>(methodName, parameters, callback1, (Func<string, GamesRequestsResponse>)(jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<GamesRequestsResponse>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
        }

        public void DeleteRequest(long requestId, Action<BackendResult<OwnCounters, ResultCode>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<OwnCounters>("execute", new Dictionary<string, string>()
      {
        {
          "code",
          string.Format("var requestId = {0};\r\n                                           API.apps.deleteRequest({{\"request_ids\": requestId}});\r\n                                           return API.getCounters();", requestId)
        }
      }, callback, new Func<string, OwnCounters>(CountersDeserializerHelper.Deserialize), false, true, new CancellationToken?(), null);
        }

        public void GetDashboard(int offset, int count, Action<BackendResult<GamesDashboardResponse, ResultCode>> callback)
        {
            string str = string.Format("var platform = \"{0}\";\r\n                                            var fields = \"{1}\";\r\n                                            var count = {2};\r\n                                            var offset = {3};\r\n\r\n                                            var requests = {{}};\r\n                                            var myGames = {{}};\r\n                                            var activity = {{}};\r\n                                            var banners = {{}};\r\n\r\n                                            if (offset > 0) {{\r\n                                            \r\n                                            requests = null;\r\n                                            myGames = null;\r\n                                            activity = null;\r\n                                            banners = null;\r\n\r\n                                            }} else {{\r\n\r\n                                            requests = API.apps.getRequests({{\"platform\": platform, \"fields\": fields}});\r\n                                            myGames = API.apps.getCatalog({{\"platform\": platform, \"count\": 12, \"filter\": \"installed\"}});\r\n                                            activity = API.apps.getActivity({{\"platform\": platform, \"count\": 4, \"fields\": fields}});\r\n                                            banners = API.apps.getCatalog({{\"platform\": platform, \"filter\": \"featured\"}});\r\n\r\n                                            }}\r\n\r\n                                            var catalog = API.apps.getCatalog({{\"platform\": platform, \"count\": count, \"offset\": offset}});\r\n                                            return {{\"requests\": requests, \"myGames\": myGames, \"activity\": activity, \"banners\": banners, \"catalog\": catalog}};", "winphone", "photo_max,sex", count, offset);
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["code"] = str;
            VKRequestsDispatcher.DispatchRequestToVK<GamesDashboardResponse>("execute", parameters, callback, null, false, true, new CancellationToken?(), null);
        }

        public void GetGameDashboard(long appId, Action<BackendResult<GameDashboardResponse, ResultCode>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["fields"] = "photo_max,sex";
            parameters["appId"] = appId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<GameDashboardResponse>("execute.getGameDashboard", parameters, callback, (Func<string, GameDashboardResponse>)(jsonStr =>
            {
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "requests", true);
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "activity", true);
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "leaderboard", false);
                return JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<GameDashboardResponse>>(jsonStr).response;
            }), false, true, new CancellationToken?(), null);
        }

        public void GetApp(long id, Action<BackendResult<VKList<Game>, ResultCode>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["app_id"] = id.ToString();
            parameters["extended"] = "1";
            parameters["platform"] = "winphone";
            VKRequestsDispatcher.DispatchRequestToVK<VKList<Game>>("apps.get", parameters, callback, null, false, true, new CancellationToken?(), null);
        }

        public void ToggleRequests(long id, bool isEnabled, Action<BackendResult<int, ResultCode>> callback)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary["id"] = id.ToString();
            dictionary["enabled"] = isEnabled ? "1" : "0";
            string methodName = "apps.toggleRequests";
            Dictionary<string, string> parameters = dictionary;
            Action<BackendResult<int, ResultCode>> callback1 = callback;
            int num1 = 0;
            int num2 = 1;
            CancellationToken? cancellationToken = new CancellationToken?();
            // ISSUE: variable of the null type

            VKRequestsDispatcher.DispatchRequestToVK<int>(methodName, parameters, callback1, (Func<string, int>)(jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<int>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
        }

        public void MarkRequestAsRead(IEnumerable<long> requestIds, Action<BackendResult<OwnCounters, ResultCode>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<OwnCounters>("execute", new Dictionary<string, string>()
      {
        {
          "code",
          string.Format("API.apps.markRequestAsRead({{\"request_ids\": \"{0}\"}});\r\n                                           return API.getCounters();", string.Join<long>(",", requestIds))
        }
      }, callback, new Func<string, OwnCounters>(CountersDeserializerHelper.Deserialize), false, true, new CancellationToken?(), null);
        }

        public void Remove(long id, Action<BackendResult<OwnCounters, ResultCode>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<OwnCounters>("execute", new Dictionary<string, string>()
      {
        {
          "code",
          string.Format("var id = {0};\r\n                                           API.apps.remove({{\"id\": id}});\r\n                                           return API.getCounters();", id)
        }
      }, callback, new Func<string, OwnCounters>(CountersDeserializerHelper.Deserialize), false, true, new CancellationToken?(), null);
        }

        public void GetEmbeddedUrl(long appId, long ownerId, Action<BackendResult<EmbeddedUrlResponse, ResultCode>> callback)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string index = "app_id";
            string str = appId.ToString();
            dictionary[index] = str;
            Dictionary<string, string> parameters = dictionary;
            if (ownerId != 0L)
                parameters["owner_id"] = ownerId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<EmbeddedUrlResponse>("apps.getEmbeddedUrl", parameters, callback, null, false, true, new CancellationToken?(), null);
        }

        public void Report(long appId, ReportAppReason reason, long ownerId = 0, string comment = "", Action<BackendResult<int, ResultCode>> callback = null)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string index1 = "app_id";
            string str1 = appId.ToString();
            dictionary[index1] = str1;
            string index2 = "reason";
            string str2 = ((int)reason).ToString();
            dictionary[index2] = str2;
            Dictionary<string, string> parameters = dictionary;
            if (ownerId != 0L)
                parameters["owner_id"] = ownerId.ToString();
            if (!string.IsNullOrWhiteSpace(comment))
                parameters["comment"] = comment;
            VKRequestsDispatcher.DispatchRequestToVK<int>("apps.report", parameters, callback, null, false, true, new CancellationToken?(), null);
        }

        public void SendLog(string log, Action<BackendResult<int, ResultCode>> callback = null)
        {
            if (string.IsNullOrEmpty(log))
                return;
            string str1 = "";
            try
            {
                Encoding utF8 = Encoding.UTF8;
                str1 = Convert.ToBase64String(ServiceLocator.Resolve<IGZipEncoder>().Compress(log, utF8));
            }
            catch
            {
            }
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string index = "log";
            parameters[index] = str1;
            VKRequestsDispatcher.DispatchRequestToVK<int>("apps.sendLog", parameters, callback, null, false, true, new CancellationToken?(), null);
        }
    }
}
