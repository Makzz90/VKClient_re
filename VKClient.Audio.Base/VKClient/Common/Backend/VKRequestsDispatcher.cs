using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;
using VKClient.Common.Utils;
using VKMessenger.Library;

namespace VKClient.Common.Backend
{
    public static class VKRequestsDispatcher
    {
        private static readonly bool USE_HTTP = false;
        public static readonly string _requestBaseUriFrm = "api.vk.com/method/{0}";
        public static readonly string _loginUri = "api.vk.com/oauth/token";
        private static readonly string _errorPrefixGeneral = "{\"error\":{\"error_code\":";
        private static readonly string _errorPrefixFailed = "{\"failed";
        private static readonly string _errorPrefixCaptchaNeeded = "{\"error\":\"need_captcha\"";
        private static readonly string _errorPrefixInvalid = "{\"error\":\"invalid_client\"";
        private static readonly string _errorPrefixNeedValidationAuth = "{\"error\":\"need_validation\"";
        private static readonly string _errorPrefixInvalidRequest = "{\"error\":\"invalid_request\"";
        private static AutorizationData _autorizationData;
        private const string SCOPE = "notify, friends, photos, audio, video, docs, notes, pages, status, wall, groups, messages, notifications, stats, market";

        public static string RequestUriFrm
        {
            get
            {
                if (!string.IsNullOrEmpty(AppGlobalStateManager.Current.GlobalState.BaseDomain))
                    return AppGlobalStateManager.Current.GlobalState.BaseDomain + "/method/{0}";
                return VKRequestsDispatcher._requestBaseUriFrm;
            }
        }

        public static string LoginUri
        {
            get
            {
                if (!string.IsNullOrEmpty(AppGlobalStateManager.Current.GlobalState.BaseLoginDomain))
                    return AppGlobalStateManager.Current.GlobalState.BaseLoginDomain + "/oauth/token";
                return VKRequestsDispatcher._loginUri;
            }
        }

        public static bool HaveToken
        {
            get
            {
                return !string.IsNullOrEmpty(VKRequestsDispatcher._autorizationData != null ? VKRequestsDispatcher._autorizationData.access_token : null);
            }
        }

        public static AutorizationData AuthData
        {
            get
            {
                return VKRequestsDispatcher._autorizationData;
            }
        }

        public static string Token
        {
            get
            {
                return VKRequestsDispatcher._autorizationData == null ? "" : VKRequestsDispatcher._autorizationData.access_token;
            }
        }

        public static void SetAuthorizationData(AutorizationData autorizationData, bool startUpdatesManager = true)
        {
            VKRequestsDispatcher._autorizationData = autorizationData;
            if (!(VKRequestsDispatcher.HaveToken & startUpdatesManager))
                return;
            InstantUpdatesManager.Current.Restart();
        }

        public static void DispatchLoginRequest(string login, string password, Action<BackendResult<AutorizationData, ResultCode>> callback, CancellationToken? cancellationToken = null)
        {
            VKRequestsDispatcher.DispatchLoginRequest(login, password, "", false, callback, cancellationToken);
        }

        public static void DispatchLoginRequest(string login, string password, string code, bool forceSms, Action<BackendResult<AutorizationData, ResultCode>> callback, CancellationToken? cancellationToken = null)
        {
            Dictionary<string, string> dictionary1 = new Dictionary<string, string>();
            dictionary1["grant_type"] = "password";
            dictionary1["client_id"] = VKConstants.ApplicationID;
            dictionary1["client_secret"] = VKConstants.ApplicationSecretKey;
            dictionary1["username"] = login;
            dictionary1["password"] = password;
            dictionary1["2fa_supported"] = "1";
            dictionary1["device_id"] = AppGlobalStateManager.Current.GlobalState.DeviceId;
            
            if (!string.IsNullOrEmpty(code))
                dictionary1["code"] = code;
            else if (forceSms)
                dictionary1["force_sms"] = "1";
            dictionary1["scope"] = "notify, friends, photos, audio, video, docs, notes, pages, status, wall, groups, messages, notifications, stats, market";
            if (VKRequestsDispatcher.USE_HTTP)
            {
                dictionary1["scope"] = dictionary1["scope"] + ", nohttps";
            }
            VKRequestsDispatcher.DoDispatch<AutorizationData>(VKRequestsDispatcher.LoginUri, "", dictionary1, callback, new Func<string, AutorizationData>(JsonConvert.DeserializeObject<AutorizationData>), false, true, cancellationToken, null);
        }

        public static void Execute<T>(string code, Action<BackendResult<T, ResultCode>> callback, Func<string, T> customDesFunc = null, bool lowPriority = false, bool pageDataRequest = true, CancellationToken? cancellationToken = null) where T : class
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["code"] = code;
            VKRequestsDispatcher.DispatchRequestToVK<T>("execute", parameters, callback, customDesFunc, lowPriority, pageDataRequest, cancellationToken, null);
        }
        ////////

        public static void DispatchRequestToVK2(string methodName, Dictionary<string, string> parameters)
        {
            VKRequestsDispatcher.DoDispatch2(string.Format(VKRequestsDispatcher.RequestUriFrm, methodName), methodName, parameters);
        }

        private static void DoDispatch2(string baseUrl, string methodName, Dictionary<string, string> parameters)
        {
            parameters["v"] = VKConstants.API_VERSION;
            AutorizationData autorizationData1 = VKRequestsDispatcher._autorizationData;
            if (!string.IsNullOrEmpty(autorizationData1 != null ? autorizationData1.access_token : null))
                parameters["access_token"] = VKRequestsDispatcher._autorizationData.access_token;
            VKRequestsDispatcher.AddLangParameter(parameters);
            AutorizationData autorizationData2 = VKRequestsDispatcher._autorizationData;
            if (!string.IsNullOrEmpty(autorizationData2 != null ? autorizationData2.secret : null))
            {
                if (parameters.ContainsKey("sig"))
                    parameters.Remove("sig");
                string str1 = JsonWebRequest.ConvertDictionaryToQueryString(parameters, false);
                if (str1 != string.Empty)
                    str1 = "?" + str1;
                string str2 = VKRequestsDispatcher.HashString("/method/" + methodName + str1 + VKRequestsDispatcher._autorizationData.secret);
                parameters["sig"] = str2.ToLower();
            }
            JsonWebRequest.SendHTTPRequestAsync(!VKRequestsDispatcher.USE_HTTP ? "https://" + baseUrl : "http://" + baseUrl, parameters, (Action<JsonResponseData>)(jsonResp =>
            {
                if (jsonResp.IsSucceeded)
                {

                }
            }));
        }



        //////////
        public static void DispatchRequestToVK<R>(string methodName, Dictionary<string, string> parameters, Action<BackendResult<R, ResultCode>> callback, Func<string, R> customDeserializationFunc = null, bool lowPriority = false, bool pageDataRequest = true, CancellationToken? cancellationToken = null, Action confirmationRequiredHandler = null)
        {
            VKRequestsDispatcher.DoDispatch<R>(string.Format(VKRequestsDispatcher.RequestUriFrm, methodName), methodName, parameters, callback, customDeserializationFunc, lowPriority, pageDataRequest, cancellationToken, confirmationRequiredHandler);
        }

        private static void DoDispatch<R>(string baseUrl, string methodName, Dictionary<string, string> parameters, Action<BackendResult<R, ResultCode>> callback, Func<string, R> customDeserializationFunc = null, bool lowPriority = false, bool pageDataRequest = true, CancellationToken? cancellationToken = null, Action confirmationRequiredHandler = null)
        {
            parameters["v"] = VKConstants.API_VERSION;
            if (!string.IsNullOrEmpty(VKRequestsDispatcher._autorizationData != null ? VKRequestsDispatcher._autorizationData.access_token : null))
                parameters["access_token"] = VKRequestsDispatcher._autorizationData.access_token;
            VKRequestsDispatcher.AddLangParameter(parameters);
            if (!string.IsNullOrEmpty(VKRequestsDispatcher._autorizationData != null ? VKRequestsDispatcher._autorizationData.secret : null))
            {
                if (parameters.ContainsKey("sig"))
                    parameters.Remove("sig");
                string str1 = JsonWebRequest.ConvertDictionaryToQueryString(parameters, false);
                if (str1 != string.Empty)
                    str1 = "?" + str1;
                string str2 = VKRequestsDispatcher.HashString("/method/" + methodName + str1 + VKRequestsDispatcher._autorizationData.secret);
                parameters["sig"] = str2.ToLower();
            }
            JsonWebRequest.SendHTTPRequestAsync(!VKRequestsDispatcher.USE_HTTP ? "https://" + baseUrl : "http://" + baseUrl, parameters, (Action<JsonResponseData>)(jsonResp =>
            {
                BackendResult<R, ResultCode> backendResult = new BackendResult<R, ResultCode>(ResultCode.CommunicationFailed);
                if (jsonResp.IsSucceeded)
                {
                    VKRequestsDispatcher.ResultData resultFromJson = VKRequestsDispatcher.GetResultFromJson(JObject.Parse(jsonResp.JsonString));
                    backendResult.ResultCode = resultFromJson.ResultCode;
                    backendResult.Error = resultFromJson.error;
                    if (backendResult.ResultCode == ResultCode.UserAuthorizationFailed)
                    {
                        if (!string.IsNullOrEmpty(VKRequestsDispatcher._autorizationData != null ? VKRequestsDispatcher._autorizationData.access_token : null))
                        {
                            Logger.Instance.Error("RECEIVED AUTHORIZATION FAILURE!!! JSON STR = " + jsonResp.JsonString ?? "");
                            LogoutRequestHandler.InvokeLogoutRequest();
                        }
                    }
                    if (backendResult.ResultCode == ResultCode.CaptchaRequired)
                        CaptchaUserRequestHandler.InvokeCaptchaRequest(new CaptchaUserRequest()
                        {
                            CaptchaSid = resultFromJson.captcha_sid,
                            Url = resultFromJson.captcha_img
                        }, (Action<CaptchaUserResponse>)(captchaResp =>
                        {
                            if (!captchaResp.IsCancelled)
                            {
                                Dictionary<string, string> parameters1 = parameters;
                                parameters1["captcha_sid"] = captchaResp.Request.CaptchaSid;
                                parameters1["captcha_key"] = captchaResp.EnteredString;
                                VKRequestsDispatcher.DoDispatch<R>(baseUrl, methodName, parameters1, callback, customDeserializationFunc, lowPriority, pageDataRequest, cancellationToken, null);
                            }
                            else
                                VKRequestsDispatcher.InvokeCallback((Action)(() => callback(new BackendResult<R, ResultCode>(ResultCode.CaptchaControlCancelled))), cancellationToken);
                        }));
                    else if (backendResult.ResultCode == ResultCode.ValidationRequired)
                    {
                        if (resultFromJson.validation_type == "2fa_app" || resultFromJson.validation_type == "2fa_sms")
                        {
                            if (parameters.ContainsKey("force_sms") || parameters.ContainsKey("code"))
                            {
                                R r = customDeserializationFunc != null ? customDeserializationFunc("") : default(R);
                                VKRequestsDispatcher.InvokeCallback((Action)(() => callback(new BackendResult<R, ResultCode>(ResultCode.Succeeded, r))), cancellationToken);
                            }
                            else
                                CaptchaUserRequestHandler.InvokeValidation2FARequest(new Validation2FAUserRequest()
                                {
                                    username = parameters.ContainsKey("username") ? parameters["username"] : "",
                                    password = parameters.ContainsKey("password") ? parameters["password"] : "",
                                    phoneMask = resultFromJson.phone_mask,
                                    validationType = resultFromJson.validation_type,
                                    validationSid = resultFromJson.validation_sid
                                }, (Action<ValidationUserResponse>)(valResp => VKRequestsDispatcher.ProcessValidationResponse<R>(valResp, baseUrl, methodName, parameters, callback, customDeserializationFunc, lowPriority, pageDataRequest, cancellationToken)));
                        }
                        else
                            CaptchaUserRequestHandler.InvokeValidationRequest(new ValidationUserRequest()
                            {
                                ValidationUri = resultFromJson.redirect_uri
                            }, (Action<ValidationUserResponse>)(valResp => VKRequestsDispatcher.ProcessValidationResponse<R>(valResp, baseUrl, methodName, parameters, callback, customDeserializationFunc, lowPriority, pageDataRequest, cancellationToken)));
                    }
                    else if (backendResult.ResultCode == ResultCode.ConfirmationRequired)
                    {
                        if (!VKRequestsDispatcher.GetIsResponseCancelled(cancellationToken))
                        {
                            Action action = confirmationRequiredHandler;
                            if (action != null)
                                action();
                            IBackendConfirmationHandler confirmationHandler = ServiceLocator.Resolve<IBackendConfirmationHandler>();
                            if (confirmationHandler != null)
                            {
                                confirmationHandler.Confirm(resultFromJson.confirmation_text, (Action<bool>)(confirmed =>
                                {
                                    if (confirmed)
                                    {
                                        parameters["confirm"] = "1";
                                        VKRequestsDispatcher.DoDispatch<R>(baseUrl, methodName, parameters, callback, customDeserializationFunc, lowPriority, pageDataRequest, cancellationToken, null);
                                    }
                                    else
                                        VKRequestsDispatcher.InvokeCallback((Action)(() => callback(new BackendResult<R, ResultCode>(ResultCode.ConfirmationCancelled))), cancellationToken);
                                }));
                            }
                        }
                    }
                    else if (backendResult.ResultCode == ResultCode.NotEnoughMoney)
                    {
                        if (!VKRequestsDispatcher.GetIsResponseCancelled(cancellationToken))
                        {
                            IBackendNotEnoughMoneyHandler enoughMoneyHandler = ServiceLocator.Resolve<IBackendNotEnoughMoneyHandler>();
                            Action action1 = (() => VKRequestsDispatcher.DoDispatch<R>(baseUrl, methodName, parameters, callback, customDeserializationFunc, lowPriority, pageDataRequest, cancellationToken, null));
                            Action action2 = (Action)(() => VKRequestsDispatcher.InvokeCallback((Action)(() => callback(new BackendResult<R, ResultCode>(ResultCode.BalanceRefillCancelled))), cancellationToken));
                            if (enoughMoneyHandler != null)
                            {
                                Action refilledCallback = action1;
                                Action cancelledCallback = action2;
                                enoughMoneyHandler.RequestBalanceRefill(refilledCallback, cancelledCallback);
                            }
                        }
                    }
                    else if (backendResult.ResultCode == ResultCode.Succeeded)
                    {
                        try
                        {
                            List<ExecuteError> executeErrorList = null;
                            R r;
                            if (customDeserializationFunc != null)
                                r = customDeserializationFunc(jsonResp.JsonString);
                            else if (typeof(R) == typeof(VKClient.Common.Backend.DataObjects.ResponseWithId))
                            {
                                r = JsonConvert.DeserializeObject<R>(jsonResp.JsonString);
                            }
                            else
                            {
                                VKRequestsDispatcher.GenericRoot<R> genericRoot = JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<R>>(jsonResp.JsonString);
                                r = genericRoot.response;
                                executeErrorList = genericRoot.execute_errors;
                            }
                            backendResult.ResultData = r;
                            backendResult.ExecuteErrors = executeErrorList;
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Error("Error during deserialization", ex);
                            backendResult.ResultCode = ResultCode.DeserializationError;
                        }
                    }
                }
                if (backendResult.ResultCode == ResultCode.CaptchaRequired || backendResult.ResultCode == ResultCode.ValidationRequired || (backendResult.ResultCode == ResultCode.ConfirmationRequired || backendResult.ResultCode == ResultCode.NotEnoughMoney))
                    return;
                VKRequestsDispatcher.InvokeCallback((Action)(() =>
                {
                    Action<BackendResult<R, ResultCode>> action = callback;
                    if (action == null)
                        return;
                    BackendResult<R, ResultCode> backendResult1 = backendResult;
                    action(backendResult1);
                }), cancellationToken);
            }), true, lowPriority, pageDataRequest);
        }

        private static void ProcessValidationResponse<R>(ValidationUserResponse valResp, string baseUrl, string methodName, Dictionary<string, string> parameters, Action<BackendResult<R, ResultCode>> callback, Func<string, R> customDeserializationFunc = null, bool lowPriority = false, bool pageDataRequest = true, CancellationToken? cancellationToken = null)
        {
            if (valResp.IsSucceeded)
            {
                int num = !string.IsNullOrEmpty(AppGlobalStateManager.Current.GlobalState.AccessToken) ? 1 : 0;
                if (!string.IsNullOrEmpty(valResp.access_token))
                {
                    VKRequestsDispatcher._autorizationData.access_token = valResp.access_token;
                    AppGlobalStateManager.Current.GlobalState.AccessToken = valResp.access_token;
                }
                if (num != 0)
                {
                    VKRequestsDispatcher.DoDispatch<R>(baseUrl, methodName, parameters, callback, customDeserializationFunc, lowPriority, pageDataRequest, cancellationToken, null);
                }
                else
                {
                    string str = string.Format("{{\"access_token\":\"{0}\",\"user_id\":{1} }} ", valResp.access_token, valResp.user_id);
                    R r = customDeserializationFunc(str);
                    VKRequestsDispatcher.InvokeCallback((Action)(() => callback(new BackendResult<R, ResultCode>(ResultCode.Succeeded, r))), cancellationToken);
                }
            }
            else
                VKRequestsDispatcher.InvokeCallback((Action)(() => callback(new BackendResult<R, ResultCode>(ResultCode.ValidationCancelledOrFailed))), cancellationToken);
        }

        private static void InvokeCallback(Action callback, CancellationToken? cancellationToken = null)
        {
            if (VKRequestsDispatcher.GetIsResponseCancelled(cancellationToken) || callback == null)
                return;
            callback();
        }

        private static bool GetIsResponseCancelled(CancellationToken? cancellationToken)
        {
            if (cancellationToken.HasValue)
                return cancellationToken.Value.IsCancellationRequested;
            return false;
        }

        private static void AddLangParameter(Dictionary<string, string> parameters)
        {
            string lang = LangHelper.GetLang();
            if (!(lang != ""))
                return;
            parameters["lang"] = lang;
        }

        private static string HashString(string strToHash)
        {
            return MD5Core.GetHashString(strToHash);
        }

        private static VKRequestsDispatcher.ResultData GetResultFromJson(string jsonStr)
        {
            VKRequestsDispatcher.ResultData resultData = new VKRequestsDispatcher.ResultData()
            {
                ResultCode = ResultCode.Succeeded
            };
            try
            {
                if (jsonStr.StartsWith(VKRequestsDispatcher._errorPrefixGeneral))
                {
                    VKRequestsDispatcher.RootObjectError rootObjectError = JsonConvert.DeserializeObject<VKRequestsDispatcher.RootObjectError>(jsonStr);
                    resultData.error = rootObjectError.error;
                    resultData.ResultCode = (ResultCode)rootObjectError.error.error_code;
                    resultData.captcha_sid = rootObjectError.error.captcha_sid;
                    resultData.captcha_img = rootObjectError.error.captcha_img;
                    resultData.redirect_uri = rootObjectError.error.redirect_uri;
                }
                else if (jsonStr.StartsWith(VKRequestsDispatcher._errorPrefixFailed))
                    resultData.ResultCode = ResultCode.NewLongPollServerRequested;
                else if (jsonStr.StartsWith(VKRequestsDispatcher._errorPrefixInvalid))
                    resultData.ResultCode = ResultCode.WrongUsernameOrPassword;
                else if (jsonStr.StartsWith(VKRequestsDispatcher._errorPrefixNeedValidationAuth))
                {
                    resultData.ResultCode = ResultCode.ValidationRequired;
                    VKRequestsDispatcher.ErrorAuthentification authentification = JsonConvert.DeserializeObject<VKRequestsDispatcher.ErrorAuthentification>(jsonStr);
                    resultData.redirect_uri = authentification.redirect_uri;
                    resultData.validation_type = authentification.validation_type;
                    resultData.validation_sid = authentification.validation_sid;
                    resultData.phone_mask = authentification.phone_mask;
                }
                else if (jsonStr.StartsWith(VKRequestsDispatcher._errorPrefixInvalidRequest))
                    resultData.ResultCode = ResultCode.Unauthorized;
                else if (jsonStr.StartsWith(VKRequestsDispatcher._errorPrefixCaptchaNeeded))
                {
                    VKRequestsDispatcher.ErrorCaptcha errorCaptcha = JsonConvert.DeserializeObject<VKRequestsDispatcher.ErrorCaptcha>(jsonStr);
                    resultData.ResultCode = ResultCode.CaptchaRequired;
                    resultData.captcha_img = errorCaptcha.captcha_img;
                    resultData.captcha_sid = errorCaptcha.captcha_sid;
                }
                else if (jsonStr.Contains("execute_errors"))
                {
                    JToken jtoken = JObject.Parse(jsonStr)["execute_errors"];
                    if (jtoken != null)
                    {
                        List<VKRequestsDispatcher.Error> errorList = JsonConvert.DeserializeObject<List<VKRequestsDispatcher.Error>>(jtoken.ToString());
                        if (errorList != null)
                        {
                            if (errorList.Count > 0)
                                return VKRequestsDispatcher.GetResultFromJson(string.Format("{{\"error\":{0}}}", JsonConvert.SerializeObject(errorList[0])));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resultData.ResultCode = ResultCode.UnknownError;
                Logger.Instance.Error("Failed to parse error code, jsonStr =" + jsonStr, ex);
            }
            return resultData;
        }

        private static VKRequestsDispatcher.ResultData GetResultFromJson(JObject jObject)
        {
            VKRequestsDispatcher.ResultData resultData = new VKRequestsDispatcher.ResultData()
            {
                ResultCode = ResultCode.Succeeded
            };
            try
            {
                JToken jtoken1 = jObject["error"];
                if (jtoken1 != null)
                {
                    VKRequestsDispatcher.Error error;
                    if (jtoken1.HasValues)
                    {
                        JToken jtoken2 = jtoken1["error_code"];
                        if (jtoken2 != null)
                        {
                            int num = (int)jtoken2;
                            resultData.ResultCode = (ResultCode)num;
                        }
                        error = jtoken1.ToObject<VKRequestsDispatcher.Error>();
                    }
                    else
                    {
                        string str = jtoken1.ToString();
                        if (!(str == "need_validation"))
                        {
                            if (!(str == "need_captcha"))
                            {
                                if (!(str == "invalid_client"))
                                {
                                    if (!(str == "invalid_request"))
                                    {
                                        if (str == "need_confirmation")
                                            resultData.ResultCode = ResultCode.ConfirmationRequired;
                                    }
                                    else
                                        resultData.ResultCode = ResultCode.Unauthorized;
                                }
                                else
                                    resultData.ResultCode = ResultCode.WrongUsernameOrPassword;
                            }
                            else
                                resultData.ResultCode = ResultCode.CaptchaRequired;
                        }
                        else
                            resultData.ResultCode = ResultCode.ValidationRequired;
                        error = jObject.ToObject<VKRequestsDispatcher.Error>();
                    }
                    resultData.error = error;
                    resultData.captcha_sid = error.captcha_sid;
                    resultData.captcha_img = error.captcha_img;
                    resultData.redirect_uri = error.redirect_uri;
                    resultData.validation_type = error.validation_type;
                    resultData.validation_sid = error.validation_sid;
                    resultData.phone_mask = error.phone_mask;
                    resultData.confirmation_text = error.confirmation_text;
                    return resultData;
                }
                if (jObject["failed"] != null)
                {
                    resultData.ResultCode = ResultCode.NewLongPollServerRequested;
                    return resultData;
                }
                JToken jtoken3 = jObject["execute_errors"];
                if (jtoken3 != null)
                {
                    List<VKRequestsDispatcher.Error> errorList = jtoken3.ToObject<List<VKRequestsDispatcher.Error>>();
                    if (errorList != null)
                    {
                        if (errorList.Count > 0)
                        {
                            VKRequestsDispatcher.Error error = errorList[0];
                            switch ((ResultCode)error.error_code)
                            {
                                case ResultCode.CaptchaRequired:
                                case ResultCode.ValidationRequired:
                                    return VKRequestsDispatcher.GetResultFromJson(JObject.FromObject(new VKRequestsDispatcher.RootObjectError()
                                    {
                                        error = error
                                    }));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resultData.ResultCode = ResultCode.UnknownError;
                Logger.Instance.Error("Failed to parse error code, jsonStr =" + jObject, ex);
            }
            return resultData;
        }

        public static string GetArrayCountsAndRemove(string jsonStr, string arrayName, out List<int> resultCounts)
        {
            int startInd = 0;
            resultCounts = new List<int>();
            while (true)
            {
                int resultCount = 0;
                int foundInd;
                jsonStr = VKRequestsDispatcher.GetArrayCountAndRemoveExt(jsonStr, arrayName, startInd, out resultCount, out foundInd);
                if (foundInd >= 0)
                {
                    resultCounts.Add(resultCount);
                    startInd = foundInd + 1;
                }
                else
                    break;
            }
            return jsonStr;
        }

        public static string GetArrayCountAndRemoveExt(string jsonStr, string arrayName, int startInd, out int resultCount, out int foundInd)
        {
            resultCount = 0;
            int startIndex1 = startInd < jsonStr.Length ? jsonStr.IndexOf("\"" + arrayName + "\":", startInd) : -1;
            foundInd = startIndex1;
            if (startIndex1 < 0)
                return jsonStr;
            int startIndex2 = jsonStr.IndexOf("[", startIndex1);
            if (startIndex2 < 0)
                return jsonStr;
            int val1 = jsonStr.IndexOf(",", startIndex2);
            int num = jsonStr.IndexOf("]", startIndex2);
            if (val1 < 0 && num < 0)
                return jsonStr;
            int val2 = num < 0 ? val1 : num;
            if (val1 > 0)
                val2 = Math.Min(val1, val2);
            if (val2 - startIndex2 <= 1 || !int.TryParse(jsonStr.Substring(startIndex2 + 1, val2 - startIndex2 - 1).Replace("\"", ""), out resultCount))
                return jsonStr;
            if (resultCount < 0)
                resultCount = 0;
            if (val1 > num || val1 == -1)
                return jsonStr.Remove(startIndex2 + 1, val2 - startIndex2 - 1);
            return jsonStr.Remove(startIndex2 + 1, val2 - startIndex2);
        }

        public static string GetArrayCountAndRemove(string jsonStr, string arrayName, out int resultCount)
        {
            int foundInd;
            return VKRequestsDispatcher.GetArrayCountAndRemoveExt(jsonStr, arrayName, 0, out resultCount, out foundInd);
        }

        public static string FixFalseArray(string jsonStr, string arrayName, bool substituteWithCurlyBrackets = false)
        {
            if (!substituteWithCurlyBrackets)
                return jsonStr.Replace(arrayName + "\":false", arrayName + "\":[]");
            return jsonStr.Replace(arrayName + "\":false", arrayName + "\":{}");
        }

        public static string FixArrayToObject(string jsonStr, string arrayName)
        {
            return jsonStr.Replace(arrayName + "\":[]", arrayName + "\":{}");
        }

        private class ResultData
        {
            public ResultCode ResultCode { get; set; }

            public string captcha_sid { get; set; }

            public string captcha_img { get; set; }

            public string redirect_uri { get; set; }

            public VKRequestsDispatcher.Error error { get; set; }

            public string validation_type { get; set; }

            public string validation_sid { get; set; }

            public string phone_mask { get; set; }

            public string confirmation_text { get; set; }
        }

        public class ErrorCaptcha
        {
            public string error { get; set; }

            public string captcha_sid { get; set; }

            public string captcha_img { get; set; }
        }

        public class GenericRoot<T>
        {
            public T response { get; set; }

            public List<ExecuteError> execute_errors { get; set; }
        }

        public class RequestParam
        {
            public string key { get; set; }

            public string value { get; set; }
        }

        public class Error
        {
            public int error_code { get; set; }

            public string error_msg { get; set; }

            public string error_text { get; set; }

            public List<VKRequestsDispatcher.RequestParam> request_params { get; set; }

            public string captcha_sid { get; set; }

            public string captcha_img { get; set; }

            public string redirect_uri { get; set; }

            public string validation_type { get; set; }

            public string validation_sid { get; set; }

            public string phone_mask { get; set; }

            public string confirmation_text { get; set; }
        }

        public class RootObjectError
        {
            public VKRequestsDispatcher.Error error { get; set; }
        }

        public class ErrorAuthentification
        {
            public string error { get; set; }

            public string validation_type { get; set; }

            public string validation_sid { get; set; }

            public string phone_mask { get; set; }

            public string error_description { get; set; }

            public string redirect_uri { get; set; }
        }
    }
}
