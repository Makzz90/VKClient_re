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

    public GameActivity GameActivity { get; set; }

    public Game Game { get; set; }

    public User User { get; set; }

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
      inlinesList.AddRange(((IEnumerable<Inline>) inlines).Where<Inline>((Func<Inline, bool>) (inline => inline != null)));
    }

    private Run GetRunName()
    {
      return new Run()
      {
        Text = this.User.first_name
      };
    }

    private Run GetRunGameTitle()
    {
      Run run = new Run();
      run.Text = this.Game.title;
      FontFamily fontFamily = GameActivityHeader._semilightFont;
      run.FontFamily = fontFamily;
      return run;
    }

    private Run GetRunInstalledTheGame()
    {
      string str = this.User.IsFemale ? CommonResources.Games_FriendsActivity_InstalledGameFemale : CommonResources.Games_FriendsActivity_InstalledGameMale;
      Run run = new Run();
      run.Text = str;
      Brush brush = GameActivityHeader._subtleBrush;
      run.Foreground = brush;
      return run;
    }

    private Run GetRunReached()
    {
      Run run = new Run();
      run.Text = this.User.IsFemale ? CommonResources.Games_FriendsActivity_ReachedFemale : CommonResources.Games_FriendsActivity_ReachedMale;
      FontFamily fontFamily = GameActivityHeader._semilightFont;
      run.FontFamily = fontFamily;
      Brush brush = GameActivityHeader._subtleBrush;
      run.Foreground = brush;
      return run;
    }

    private Run GetRunLevelValue()
    {
      Run run = new Run();
      run.Text = this.GameActivity.level.ToString();
      FontFamily fontFamily = GameActivityHeader._semiboldFont;
      run.FontFamily = fontFamily;
      Brush brush = GameActivityHeader._subtleBrush;
      run.Foreground = brush;
      return run;
    }

    private Run GetRunLevel()
    {
      Run run = new Run();
      run.Text = CommonResources.Games_FriendsActivity_Level;
      FontFamily fontFamily = GameActivityHeader._semilightFont;
      run.FontFamily = fontFamily;
      Brush brush = GameActivityHeader._subtleBrush;
      run.Foreground = brush;
      return run;
    }

    private Run GetRunInTheGame(bool includeInTheGame)
    {
      if (!includeInTheGame)
        return (Run) null;
      Run run = new Run();
      run.Text = CommonResources.Games_FriendsActivity_InTheGame;
      FontFamily fontFamily = GameActivityHeader._semilightFont;
      run.FontFamily = fontFamily;
      Brush brush = GameActivityHeader._subtleBrush;
      run.Foreground = brush;
      return run;
    }

    private Run GetRunActivity()
    {
      Run run = new Run();
      run.Text = this.GameActivity.activity;
      FontFamily fontFamily = GameActivityHeader._semilightFont;
      run.FontFamily = fontFamily;
      Brush brush = GameActivityHeader._subtleBrush;
      run.Foreground = brush;
      return run;
    }

    private Run GetRunScored()
    {
      Run run = new Run();
      run.Text = this.User.IsFemale ? CommonResources.Games_FriendsActivity_ScoredFemale : CommonResources.Games_FriendsActivity_ScoredMale;
      FontFamily fontFamily = GameActivityHeader._semilightFont;
      run.FontFamily = fontFamily;
      Brush brush = GameActivityHeader._subtleBrush;
      run.Foreground = brush;
      return run;
    }

    private Run GetRunScoreValue()
    {
      Run run = new Run();
      run.Text = this.GameActivity.score.ToString();
      FontFamily fontFamily = GameActivityHeader._semiboldFont;
      run.FontFamily = fontFamily;
      Brush brush = GameActivityHeader._subtleBrush;
      run.Foreground = brush;
      return run;
    }

    private Run GetRunPoints()
    {
      Run run = new Run();
      run.Text = UIStringFormatterHelper.FormatNumberOfSomething(this.GameActivity.score, CommonResources.Games_OnePoint, CommonResources.Games_TwoFourPoints, CommonResources.Games_FivePoints, true, null, false);
      FontFamily fontFamily = GameActivityHeader._semilightFont;
      run.FontFamily = fontFamily;
      Brush brush = GameActivityHeader._subtleBrush;
      run.Foreground = brush;
      return run;
    }

    private Run GetRunAchievement()
    {
      Run run = new Run();
      run.Text = this.GameActivity.text;
      FontFamily fontFamily = GameActivityHeader._semilightFont;
      run.FontFamily = fontFamily;
      Brush brush = GameActivityHeader._subtleBrush;
      run.Foreground = brush;
      return run;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler changedEventHandler = this.PropertyChanged;
      if (changedEventHandler == null)
        return;
      PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
      changedEventHandler((object) this, e);
    }
  }
}
