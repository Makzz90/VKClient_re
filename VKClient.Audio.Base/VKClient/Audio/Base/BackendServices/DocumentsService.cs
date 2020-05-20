using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;

namespace VKClient.Audio.Base.BackendServices
{
  public class DocumentsService
  {
    private static DocumentsService _current;

    public static DocumentsService Current
    {
      get
      {
        if (DocumentsService._current == null)
          DocumentsService._current = new DocumentsService();
        return DocumentsService._current;
      }
    }

    public void GetDocuments(Action<BackendResult<DocumentsInfo, ResultCode>> callback, int offset, int count, long ownerId = 0, long type = 0)
    {
      if (ownerId == 0L)
        ownerId = AppGlobalStateManager.Current.LoggedInUserId;
      Dictionary<string, string> dictionary = new Dictionary<string, string>()
      {
        {
          "offset",
          offset.ToString()
        },
        {
          "count",
          count.ToString()
        },
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "type",
          type.ToString()
        }
      };
      string methodName = "execute.getDocuments";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<DocumentsInfo, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<DocumentsInfo>(methodName, parameters, callback1, (Func<string, DocumentsInfo>) (jsonString => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<DocumentsInfo>>(jsonString).response), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void UploadVoiceMessageDocument(Stream stream, List<int> waveform, Action<BackendResult<Doc, ResultCode>> callback, Action<double> progressCallback = null, Cancellation cancellation = null)
    {
      DocumentsService.GetDocumentUploadServer(0, "audio_message", (Action<BackendResult<VKClient.Audio.Base.UploadServerAddress, ResultCode>>) (uploadUrlResponse =>
      {
        if (uploadUrlResponse.ResultCode == ResultCode.Succeeded)
          JsonWebRequest.UploadVoiceMessage(uploadUrlResponse.ResultData.upload_url, stream, "file", "file", waveform, (Action<JsonResponseData>) (jsonResult =>
          {
            if (jsonResult.IsSucceeded)
            {
              UploadDocResponseData uploadData = JsonConvert.DeserializeObject<UploadDocResponseData>(jsonResult.JsonString);
              if (string.IsNullOrEmpty(uploadData.error))
                DocumentsService.SaveDocument(uploadData, callback);
              else
                callback(new BackendResult<Doc, ResultCode>(ResultCode.UnknownError));
            }
            else
              callback(new BackendResult<Doc, ResultCode>(ResultCode.UnknownError));
          }), "Voice Message", progressCallback, cancellation);
        else
          callback(new BackendResult<Doc, ResultCode>(uploadUrlResponse.ResultCode));
      }));
    }

    public void UploadGraffitiDocument(string fileName, string fileExtension, Stream stream, Action<BackendResult<Doc, ResultCode>> callback, Action<double> progressCallback = null, Cancellation cancellation = null)
    {
      DocumentsService.GetDocumentUploadServer(0, "graffiti", (Action<BackendResult<VKClient.Audio.Base.UploadServerAddress, ResultCode>>) (uploadUrlResponse =>
      {
        if (uploadUrlResponse.ResultCode == ResultCode.Succeeded)
        {
          string uploadUrl = uploadUrlResponse.ResultData.upload_url;
          string str = fileName.EndsWith(fileExtension) ? fileName : fileName + fileExtension;
          Stream data = stream;
          string paramName = "file";
          string uploadContentType = "file";
          string fileName1 = str;
          Action<double> progressCallback1 = progressCallback;
          Cancellation c = cancellation;
          JsonWebRequest.Upload(uploadUrl, data, paramName, uploadContentType, (Action<JsonResponseData>) (jsonResult =>
          {
            if (jsonResult.IsSucceeded)
            {
              UploadDocResponseData uploadData = JsonConvert.DeserializeObject<UploadDocResponseData>(jsonResult.JsonString);
              if (string.IsNullOrEmpty(uploadData.error))
                DocumentsService.SaveDocument(uploadData, callback);
              else
                callback(new BackendResult<Doc, ResultCode>(ResultCode.UnknownError));
            }
            else
              callback(new BackendResult<Doc, ResultCode>(ResultCode.UnknownError));
          }), fileName1, progressCallback1, c);
        }
        else
          callback(new BackendResult<Doc, ResultCode>(uploadUrlResponse.ResultCode));
      }));
    }

    public void UploadDocument(long groupId, string fileName, string fileExtension, Stream stream, Action<BackendResult<Doc, ResultCode>> callback, Action<double> progressCallback = null, Cancellation cancellation = null)
    {
      DocumentsService.GetDocumentWallUploadServer(groupId, (Action<BackendResult<VKClient.Audio.Base.UploadServerAddress, ResultCode>>) (uploadUrlResponse =>
      {
        if (uploadUrlResponse.ResultCode == ResultCode.Succeeded)
        {
          string uploadUrl = uploadUrlResponse.ResultData.upload_url;
          string str = fileName.EndsWith(fileExtension) ? fileName : fileName + fileExtension;
          Stream data = stream;
          string paramName = "file";
          string uploadContentType = "file";
          string fileName1 = str;
          Action<double> progressCallback1 = progressCallback;
          Cancellation c = cancellation;
          JsonWebRequest.Upload(uploadUrl, data, paramName, uploadContentType, (Action<JsonResponseData>) (jsonResult =>
          {
            if (jsonResult.IsSucceeded)
            {
              UploadDocResponseData uploadData = JsonConvert.DeserializeObject<UploadDocResponseData>(jsonResult.JsonString);
              if (string.IsNullOrEmpty(uploadData.error))
                DocumentsService.SaveDocument(uploadData, callback);
              else
                callback(new BackendResult<Doc, ResultCode>(ResultCode.UnknownError));
            }
            else
              callback(new BackendResult<Doc, ResultCode>(ResultCode.UnknownError));
          }), fileName1, progressCallback1, c);
        }
        else
          callback(new BackendResult<Doc, ResultCode>(uploadUrlResponse.ResultCode));
      }));
    }

    private static void GetDocumentUploadServer(long optionalGroupId, string type, Action<BackendResult<VKClient.Audio.Base.UploadServerAddress, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      if (optionalGroupId != 0L)
        parameters["group_id"] = optionalGroupId.ToString();
      if (!string.IsNullOrWhiteSpace(type))
        parameters["type"] = type;
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Audio.Base.UploadServerAddress>("docs.getUploadServer", parameters, callback, (Func<string, VKClient.Audio.Base.UploadServerAddress>) null, false, true, new CancellationToken?(),  null);
    }

    private static void GetDocumentWallUploadServer(long optionalGroupId, Action<BackendResult<VKClient.Audio.Base.UploadServerAddress, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      if (optionalGroupId != 0L)
        parameters["group_id"] = optionalGroupId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Audio.Base.UploadServerAddress>("docs.getWallUploadServer", parameters, callback, (Func<string, VKClient.Audio.Base.UploadServerAddress>) null, false, true, new CancellationToken?(),  null);
    }

    private static void SaveDocument(UploadDocResponseData uploadData, Action<BackendResult<Doc, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary1 = new Dictionary<string, string>();
      string index = "file";
      string file = uploadData.file;
      dictionary1[index] = file;
      Dictionary<string, string> dictionary2 = dictionary1;
      string methodName = "docs.save";
      Dictionary<string, string> parameters = dictionary2;
      Action<BackendResult<Doc, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<Doc>(methodName, parameters, callback1, (Func<string, Doc>) (jsonString => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<List<Doc>>>(jsonString).response.First<Doc>()), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void Search(string q, int offset, int count, Action<BackendResult<VKList<Doc>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["q"] = q;
      parameters["offset"] = offset.ToString();
      parameters["count"] = count.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<VKList<Doc>>("docs.search", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void Delete(long ownerId, long documentId, Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = ownerId.ToString();
      parameters["doc_id"] = documentId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Audio.Base.ResponseWithId>("docs.delete", parameters, callback, (Func<string, VKClient.Audio.Base.ResponseWithId>) (s => new VKClient.Audio.Base.ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void Add(long ownerId, long docId, string accessKey, Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = ownerId.ToString();
      parameters["doc_id"] = docId.ToString();
      if (!string.IsNullOrWhiteSpace(accessKey))
        parameters["access_key"] = accessKey;
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Audio.Base.ResponseWithId>("docs.add", parameters, callback, (Func<string, VKClient.Audio.Base.ResponseWithId>) (s => new VKClient.Audio.Base.ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void Edit(long ownerId, long id, string title, string tags, Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string index1 = "owner_id";
      string str1 = ownerId.ToString();
      parameters[index1] = str1;
      string index2 = "doc_id";
      string str2 = id.ToString();
      parameters[index2] = str2;
      string index3 = "title";
      string str3 = title;
      parameters[index3] = str3;
      string index4 = "tags";
      string str4 = tags;
      parameters[index4] = str4;
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Audio.Base.ResponseWithId>("docs.edit", parameters, callback, (Func<string, VKClient.Audio.Base.ResponseWithId>) (s => new VKClient.Audio.Base.ResponseWithId()), false, true, new CancellationToken?(),  null);
    }
  }
}
