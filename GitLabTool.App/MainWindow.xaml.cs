using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using R3;

namespace GitLabTool.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// ウィンドウサイズ
    /// </summary>
    private Rect _windowRect = new Rect(0, 0, 0, 0);

    /// <summary>
    /// 最小化されているかどうか
    /// </summary>
    private bool minimized;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
    {
        MinimizeWindow();
    }

    private void MinimizeWindow()
    {
        if (minimized) return;

        IconBackgroundBorder.Visibility = Visibility.Visible;
        CloseButton.Visibility = Visibility.Collapsed;
        MinimizeButton.Visibility = Visibility.Collapsed;
        RestoreButton.Visibility = Visibility.Visible;
        Left = SystemParameters.PrimaryScreenWidth - ActualWidth;
        Top = 0;
        Width = 32.0d;
        Height = 32.0d;
        PanelBorder.BorderBrush = Brushes.Olive;
        minimized = true;
    }

    private void RestoreButton_OnClick(object sender, RoutedEventArgs e)
    {
        IconBackgroundBorder.Visibility = Visibility.Hidden;
        CloseButton.Visibility = Visibility.Visible;
        MinimizeButton.Visibility = Visibility.Visible;
        RestoreButton.Visibility = Visibility.Collapsed;
        Left = _windowRect.X;
        Top = _windowRect.Y;
        Width = _windowRect.Width;
        Height = _windowRect.Height;
        PanelBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
        minimized = false;
    }


    private void CopyButton_OnClick(object sender, RoutedEventArgs e)
    {
        IReadOnlyDictionary<int, Action> tabActions = new Dictionary<int, Action>()
        {
            { 0, CopyCommitMessage },
            { 1, () => Clipboard.SetText($"{BranchNamePrefixTextBlock.Text}{BranchNameTextBox.Text}") }
        };

        tabActions[GitTabControl.SelectedIndex]();
        MinimizeWindow();
    }

    private void CopyCommitMessage()
    {
        if (KeywordComboBox.SelectedItem is not ComboBoxItem selectedValue) return;

        Clipboard.SetText($"{selectedValue.Content}: {CommitMessageTextBox.Text}");
        ClearCommitMessage();
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Left = SystemParameters.PrimaryScreenWidth - ActualWidth;
        Top = 0;
        _windowRect.X = Left;
        _windowRect.Y = Top;
        _windowRect.Width = ActualWidth;
        _windowRect.Height = ActualHeight;
        KeywordComboBox.SelectedIndex = 0;

        BranchNamePrefixTextBlock.Text = $"user/nakagawa/{DateTime.Now.ToString("yyyyMMdd")}_";
        MinimizeWindow();
    }

    private void KeywordComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        if (KeywordComboBox.SelectedItem is not ComboBoxItem selectedValue) return;

        FixTextComboBox.SelectedIndex = -1;
        FixTextComboBox.Items.Clear();
        switch (selectedValue.Content)
        {
            case "Task":
                FixTextComboBox.Items.Add(new ComboBoxItem() { Content = "クラスを修正しました。" });
                FixTextComboBox.Items.Add(new ComboBoxItem() { Content = "クラスを追加しました。" });
                break;
            case "LowRisk":
                FixTextComboBox.Items.Add(new ComboBoxItem() { Content = "コメントを付与しました。" });
                FixTextComboBox.Items.Add(new ComboBoxItem() { Content = "コメントを修正しました。" });
                FixTextComboBox.Items.Add(new ComboBoxItem() { Content = "未使用のusingを削除しました。" });
                FixTextComboBox.Items.Add(new ComboBoxItem() { Content = "未使用のフィールドを削除しました。" });
                FixTextComboBox.Items.Add(new ComboBoxItem() { Content = "未使用のメソッドを削除しました。" });
                FixTextComboBox.Items.Add(new ComboBoxItem() { Content = "未使用のクラスを削除しました。" });
                break;
            case "ReSharper":
                FixTextComboBox.Items.Add(new ComboBoxItem() { Content = "クラス名を修正しました。" });
                FixTextComboBox.Items.Add(new ComboBoxItem() { Content = "プロパティ名を抽出しました。" });
                FixTextComboBox.Items.Add(new ComboBoxItem() { Content = "フィールド名を抽出しました。" });
                FixTextComboBox.Items.Add(new ComboBoxItem() { Content = "メソッドを抽出しました。" });
                break;
            case "ConflictMerge":
                FixTextComboBox.Items.Add(new ComboBoxItem() { Content = "競合解決の為、最新のmainとマージしました。" });
                FixTextComboBox.Items.Add(new ComboBoxItem() { Content = "競合解決防止の為、最新のmainとマージしました。" });
                FixTextComboBox.SelectedIndex = 0;
                break;
        }
    }

    private void FixTextComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        if (FixTextComboBox.SelectedItem is not ComboBoxItem selectedValue) return;

        CommitMessageTextBox.Text = selectedValue.Content.ToString() ?? string.Empty;
    }

    private void PastButton_OnClick(object sender, RoutedEventArgs e)
    {
        string copyText = GitTabControl.SelectedIndex switch
        {
            0 => GetCommitMessage(),
            1 => GetBrunchName(),
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(copyText)) return;
        Clipboard.SetText(copyText);

        UIElement targetElement = GitTabControl.SelectedIndex switch
        {
            0 => CommitMessageTextBox,
            1 => BranchNameTextBox,
            _ => this
        };

        // バルーンチップ表示
        ShowBalloonTip("コピーしました", targetElement);

    }

    private string GetBrunchName()
    {
        Match match = Regex.Match(Clipboard.GetText(), @"https?://[a-zA-Z0-9.-]+\.atlassian\.net/browse/([^/?#]+)");
        if (!match.Success) return string.Empty;

        BranchNameTextBox.Text = match.Groups[1].Value;
        return $"{BranchNamePrefixTextBlock.Text}{BranchNameTextBox.Text}";
    }

    private string GetCommitMessage()
    {
        if (KeywordComboBox.SelectedItem is not ComboBoxItem selectedValue) return string.Empty;

        CommitMessageTextBox.Text = Clipboard.GetText();
        return $"{selectedValue.Content}: {Clipboard.GetText()}";
    }

    private void MainWindow_OnDeactivated(object? sender, EventArgs e)
    {
        ClearCommitMessage();
        MinimizeWindow();
    }

    private void ClearCommitMessage()
    {
        CommitMessageTextBox.Text = string.Empty;
        FixTextComboBox.SelectedIndex = -1;
    }

    private static void ShowBalloonTip(string message, UIElement target)
    {
        ToolTip toolTip = new()
        {
            Content = message,
            Placement = PlacementMode.Bottom,
            StaysOpen = false,
            IsOpen = true,
            PlacementTarget = target,
            Background = Brushes.Yellow,
            Foreground = Brushes.Black,
            HorizontalOffset = -((FrameworkElement)target).ActualWidth + 100.0d
        };

        // 一定時間後に自動で閉じる
        DispatcherTimer timer = new()
            { Interval = TimeSpan.FromSeconds(2) };

        timer.TickAsObservable().Take(1).Subscribe(_ =>
        {
            toolTip.IsOpen = false;
            timer.Stop();
        });
        timer.Start();
    }
}