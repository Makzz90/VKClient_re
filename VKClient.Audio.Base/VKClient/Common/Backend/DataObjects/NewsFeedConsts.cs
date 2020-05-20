namespace VKClient.Common.Backend.DataObjects
{
  public class NewsFeedConsts
  {
    public int fresh_news_check_interval { get; set; }

    public int fresh_news_expiration_timeout { get; set; }

    public int fresh_news_load_count { get; set; }

    public int notifications_sync_interval { get; set; }

    public int newsfeed_auto_reload_interval { get; set; }

    public int newsfeed_top_auto_reload_interval { get; set; }
  }
}
