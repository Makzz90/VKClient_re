using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Utils;

namespace VKClient.Common.Backend
{
  public class PollService
  {
    private static PollService _instance;

    public static PollService Current
    {
      get
      {
        if (PollService._instance == null)
          PollService._instance = new PollService();
        return PollService._instance;
      }
    }

    public void GetById(long owner_id, long poll_id, bool is_board, Action<BackendResult<Poll, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = owner_id.ToString();
      parameters["poll_id"] = poll_id.ToString();
      parameters["is_board"] = is_board.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<Poll>("polls.getById", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void AddRemoveVote(bool add, long owner_id, long poll_id, long answer_id, Action<BackendResult<long, ResultCode>> callback, long topicId = 0)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = owner_id.ToString();
      parameters["poll_id"] = poll_id.ToString();
      parameters["answer_id"] = answer_id.ToString();
      if (topicId != 0L)
        parameters["board"] = topicId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<long>(add ? "polls.addVote" : "polls.deleteVote", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void CreatePoll(string question, bool isAnonymous, long ownerId, List<string> answers, Action<BackendResult<Poll, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["question"] = question;
      parameters["is_anonymous"] = isAnonymous ? "1" : "0";
      if (ownerId != 0L)
        parameters["owner_id"] = ownerId.ToString();
      parameters["add_answers"] = "[" + answers.Select<string, string>((Func<string, string>) (a => "\"" + a + "\"")).ToList<string>().GetCommaSeparated(",") + "]";
      VKRequestsDispatcher.DispatchRequestToVK<Poll>("polls.create", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void EditPoll(long ownerId, long pollId, string question, List<string> addAnswers, Dictionary<string, string> editAnswers, List<long> deleteAnswers, Action<BackendResult<Poll, ResultCode>> callback)
    {
      VKRequestsDispatcher.Execute<Poll>(string.Format("API.polls.edit({{\"owner_id\":{0}, \"poll_id\":{1}, \"question\":\"{2}\" {3} {4} {5}}});\r\nreturn API.polls.getById({{\"owner_id\":{0}, \"poll_id\":{1}}});", ownerId, pollId, question, (addAnswers.Any<string>() ? ", \"add_answers\":\"[" + addAnswers.Select<string, string>((Func<string, string>) (a => "\\\"" + a + "\\\"")).ToList<string>().GetCommaSeparated(",") + "]\"" : ""), (editAnswers.Any<KeyValuePair<string, string>>() ? ", \"edit_answers\":\"" + JsonConvert.SerializeObject(editAnswers).Replace("\"", "\\\"") + "\"" : ""), (deleteAnswers.Any<long>() ? ", \"delete_answers\":\"[" + deleteAnswers.GetCommaSeparated() + "]\"" : "")), callback,  null, false, true, new CancellationToken?());
    }

    public void GetVoters(long ownerId, long pollId, long answerId, int offset, int count, Action<BackendResult<UsersListWithCount, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = ownerId.ToString();
      parameters["poll_id"] = pollId.ToString();
      parameters["answer_ids"] = answerId.ToString();
      parameters["fields"] = "online,online_mobile,photo_max";
      parameters["count"] = count.ToString();
      parameters["offset"] = offset.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<UsersListWithCount>("polls.getVoters", parameters, callback, (Func<string, UsersListWithCount>) (jsonStr =>
      {
        List<PollVotersResponse> response = JsonConvert.DeserializeObject<GenericRootList<PollVotersResponse>>(jsonStr).response;
        return new UsersListWithCount()
        {
          count = response[0].users.count,
          users = response[0].users.items
        };
      }), false, true, new CancellationToken?(),  null);
    }
  }
}
