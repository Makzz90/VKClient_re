namespace VKClient.Audio.Base.DataObjects
{
  public interface IProfile
  {
    string name { get; }

    string photo_max { get; }

    int can_see_gifts { get; }

    string first_name { get; }

    string first_name_gen { get; }
  }
}
