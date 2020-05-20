using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.BackendServices
{
    public class InternalService
    {
        public static InternalService Instance = new InternalService();

        public void HideUserNotification(long notificationId, NewsFeedNotificationHideReason reason, Action<BackendResult<bool, ResultCode>> callback)
        {
            Dictionary<string, string> dictionary1 = new Dictionary<string, string>();
            string index1 = "notification_id";
            string str1 = notificationId.ToString();
            dictionary1[index1] = str1;
            string index2 = "reason";
            string str2 = reason.ToString();
            dictionary1[index2] = str2;
            Dictionary<string, string> dictionary2 = dictionary1;
            string methodName = "internal.hideUserNotification";
            Dictionary<string, string> parameters = dictionary2;
            Action<BackendResult<bool, ResultCode>> callback1 = callback;
            int num1 = 0;
            int num2 = 1;
            CancellationToken? cancellationToken = new CancellationToken?();
            VKRequestsDispatcher.DispatchRequestToVK<bool>(methodName, parameters, callback1, (Func<string, bool>)(jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<int>>(jsonStr).response == 1), num1 != 0, num2 != 0, cancellationToken, null);
        }
    }
}
