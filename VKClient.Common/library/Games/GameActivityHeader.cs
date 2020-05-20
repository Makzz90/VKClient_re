using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.Games
{
  public class GameActivityHeader : INotifyPropertyChanged
  {
    private static readonly FontFamily _semilightFont = new FontFamily("Segoe WP Semilight");
    private static readonly FontFamily _semiboldFont = new FontFamily("Segoe WP Semibold");
    private static readonly Brush _subtleBrush = (Brush) Application.Current.Resources["PhoneVKSubtleBrush"];
    private bool _isSeparatorVisible;

    public GameActivity GameActivity { get; private set; }

    public Game Game { get; private set; }

    public User User { get; private set; }

    public bool IsSeparatorVisible
    {
      get
      {
        return this._isSeparatorVisible;
      }
      set
      {
        this._isSeparatorVisible = value;
        this.OnPropertyChanged("IsSeparatorVisible");
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public GameActivityHeader(GameActivity gameActivity, Game game, User user)
    {
      this.GameActivity = gameActivity;
      this.Game = game;
      this.User = user;
      this.IsSeparatorVisible = true;
    }

    public List<Inline> ComposeActivityText(bool includeInTheGame)
    {
      CultureName current = CultureHelper.GetCurrent();
      List<Inline> inlinesList = new List<Inline>();
      if (this.GameActivity.type == "install")
        inlinesList.Add((Inline) this.GetRunInstalledTheGame());
      else if (this.GameActivity.type == "level")
      {
        if (current != CultureName.KZ)
          GameActivityHeader.AddInlines(inlinesList, (Inline) this.GetRunReached(), (Inline) this.GetRunLevelValue(), (Inline) this.GetRunLevel(), (Inline) this.GetRunInTheGame(includeInTheGame));
        else
          GameActivityHeader.AddInlines(inlinesList, (Inline) this.GetRunInTheGame(includeInTheGame), (Inline) this.GetRunLevelValue(), (Inline) this.GetRunLevel(), (Inline) this.GetRunReached());
      }
      else if (this.GameActivity.type == "activity")
      {
        if (current != CultureName.KZ)
          GameActivityHeader.AddInlines(inlinesList, (Inline) this.GetRunActivity(), (Inline) this.GetRunInTheGame(includeInTheGame));
        else
          GameActivityHeader.AddInlines(inlinesList, (Inline) this.GetRunInTheGame(includeInTheGame), (Inline) this.GetRunActivity());
      }
      else if (this.GameActivity.type == "score")
      {
        if (current != CultureName.KZ)
          GameActivityHeader.AddInlines(inlinesList, (Inline) this.GetRunScored(), (Inline) this.GetRunScoreValue(), (Inline) this.GetRunPoints(), (Inline) this.GetRunInTheGame(includeInTheGame));
        else
          GameActivityHeader.AddInlines(inlinesList, (Inline) this.GetRunInTheGame(includeInTheGame), (Inline) this.GetRunScoreValue(), (Inline) this.GetRunPoints(), (Inline) this.GetRunScored());
      }
      else if (this.GameActivity.type == "achievement")
      {
        if (current != CultureName.KZ)
          GameActivityHeader.AddInlines(inlinesList, (Inline) this.GetRunAchievement(), (Inline) this.GetRunInTheGame(includeInTheGame));
        else
          GameActivityHeader.AddInlines(inlinesList, (Inline) this.GetRunInTheGame(includeInTheGame), (Inline) this.GetRunAchievement());
      }
      if (includeInTheGame)
      {
        Run runGameTitle = this.GetRunGameTitle();
        if (current == CultureName.KZ)
          inlinesList.Insert(0, (Inline) runGameTitle);
        else
          inlinesList.Add((Inline) runGameTitle);
      }
      inlinesList.Insert(0, (Inline) this.GetRunName());
      return inlinesList;
    }

    private static void AddInlines(List<Inline> inlinesList, params Inline[] inlines)
    {
        inlinesList.AddRange((IEnumerable<Inline>)Enumerable.Where<Inline>(inlines, (Func<Inline, bool>)(inline => inline != null)));
    }

    private Run GetRunName()
    {
      Run run = new Run();
      string firstName = this.User.first_name;
      run.Text = firstName;
      return run;
    }

    private Run GetRunGameTitle()
    {
      Run run = new Run();
      string title = this.Game.title;
      run.Text = title;
      FontFamily semilightFont = GameActivityHeader._semilightFont;
      ((TextElement) run).FontFamily = semilightFont;
      return run;
    }

    private Run GetRunInstalledTheGame()
    {
      string str1 = this.User.IsFemale ? CommonResources.Games_FriendsActivity_InstalledGameFemale : CommonResources.Games_FriendsActivity_InstalledGameMale;
      Run run = new Run();
      string str2 = str1;
      run.Text = str2;
      Brush subtleBrush = GameActivityHeader._subtleBrush;
      ((TextElement) run).Foreground = subtleBrush;
      return run;
    }

    private Run GetRunReached()
    {
      Run run = new Run();
      string str = this.User.IsFemale ? CommonResources.Games_FriendsActivity_ReachedFemale : CommonResources.Games_FriendsActivity_ReachedMale;
      run.Text = str;
      FontFamily semilightFont = GameActivityHeader._semilightFont;
      ((TextElement) run).FontFamily = semilightFont;
      Brush subtleBrush = GameActivityHeader._subtleBrush;
      ((TextElement) run).Foreground = subtleBrush;
      return run;
    }

    private Run GetRunLevelValue()
    {
      Run run = new Run();
      string str = this.GameActivity.level.ToString();
      run.Text = str;
      FontFamily semiboldFont = GameActivityHeader._semiboldFont;
      ((TextElement) run).FontFamily = semiboldFont;
      Brush subtleBrush = GameActivityHeader._subtleBrush;
      ((TextElement) run).Foreground = subtleBrush;
      return run;
    }

    private Run GetRunLevel()
    {
      Run run = new Run();
      string friendsActivityLevel = CommonResources.Games_FriendsActivity_Level;
      run.Text = friendsActivityLevel;
      FontFamily semilightFont = GameActivityHeader._semilightFont;
      ((TextElement) run).FontFamily = semilightFont;
      Brush subtleBrush = GameActivityHeader._subtleBrush;
      ((TextElement) run).Foreground = subtleBrush;
      return run;
    }

    private Run GetRunInTheGame(bool includeInTheGame)
    {
      if (!includeInTheGame)
        return  null;
      Run run = new Run();
      string activityInTheGame = CommonResources.Games_FriendsActivity_InTheGame;
      run.Text = activityInTheGame;
      FontFamily semilightFont = GameActivityHeader._semilightFont;
      ((TextElement) run).FontFamily = semilightFont;
      Brush subtleBrush = GameActivityHeader._subtleBrush;
      ((TextElement) run).Foreground = subtleBrush;
      return run;
    }

    private Run GetRunActivity()
    {
      Run run = new Run();
      string activity = this.GameActivity.activity;
      run.Text = activity;
      FontFamily semilightFont = GameActivityHeader._semilightFont;
      ((TextElement) run).FontFamily = semilightFont;
      Brush subtleBrush = GameActivityHeader._subtleBrush;
      ((TextElement) run).Foreground = subtleBrush;
      return run;
    }

    private Run GetRunScored()
    {
      Run run = new Run();
      string str = this.User.IsFemale ? CommonResources.Games_FriendsActivity_ScoredFemale : CommonResources.Games_FriendsActivity_ScoredMale;
      run.Text = str;
      FontFamily semilightFont = GameActivityHeader._semilightFont;
      ((TextElement) run).FontFamily = semilightFont;
      Brush subtleBrush = GameActivityHeader._subtleBrush;
      ((TextElement) run).Foreground = subtleBrush;
      return run;
    }

    private Run GetRunScoreValue()
    {
      Run run = new Run();
      string str = this.GameActivity.score.ToString();
      run.Text = str;
      FontFamily semiboldFont = GameActivityHeader._semiboldFont;
      ((TextElement) run).FontFamily = semiboldFont;
      Brush subtleBrush = GameActivityHeader._subtleBrush;
      ((TextElement) run).Foreground = subtleBrush;
      return run;
    }

    private Run GetRunPoints()
    {
      Run run = new Run();
      string str = UIStringFormatterHelper.FormatNumberOfSomething(this.GameActivity.score, CommonResources.Games_OnePoint, CommonResources.Games_TwoFourPoints, CommonResources.Games_FivePoints, true,  null, false);
      run.Text = str;
      FontFamily semilightFont = GameActivityHeader._semilightFont;
      ((TextElement) run).FontFamily = semilightFont;
      Brush subtleBrush = GameActivityHeader._subtleBrush;
      ((TextElement) run).Foreground = subtleBrush;
      return run;
    }

    private Run GetRunAchievement()
    {
      Run run = new Run();
      string text = this.GameActivity.text;
      run.Text = text;
      FontFamily semilightFont = GameActivityHeader._semilightFont;
      ((TextElement) run).FontFamily = semilightFont;
      Brush subtleBrush = GameActivityHeader._subtleBrush;
      ((TextElement) run).Foreground = subtleBrush;
      return run;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      // ISSUE: reference to a compiler-generated field
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
      propertyChanged(this, e);
    }
  }
}
