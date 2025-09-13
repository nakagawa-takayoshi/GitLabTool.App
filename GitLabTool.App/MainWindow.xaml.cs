using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
        if (GitTabControl.SelectedIndex == 0)
        {
            if (KeywordComboBox.SelectedItem is not ComboBoxItem selectedValue) return;

            Clipboard.SetText($"{selectedValue.Content}: {CommitMessageTextBox.Text}");
            CommitMessageTextBox.Text = string.Empty;
            FixTextComboBox.SelectedIndex = -1;
        }
        else
        {
            Clipboard.SetText($"{BranchNamePrefixTextBlock.Text}{BranchNameTextBox.Text}");
        }

        MinimizeWindow();
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
        if (GitTabControl.SelectedIndex == 0)
        {
            if (KeywordComboBox.SelectedItem is not ComboBoxItem selectedValue) return;

            Clipboard.SetText($"{selectedValue.Content}: {Clipboard.GetText()}");
            CommitMessageTextBox.Text = string.Empty;
            FixTextComboBox.SelectedIndex = -1;
            MinimizeWindow();
            return;
        }

        Match match = Regex.Match(Clipboard.GetText(), @"https?://[a-zA-Z0-9.-]+\.atlassian\.net/browse/([^/?#]+)");
        if (!match.Success) return;

        BranchNameTextBox.Text = match.Groups[1].Value;
        Clipboard.SetText($"{BranchNamePrefixTextBlock.Text}{BranchNameTextBox.Text}");
    }

    private void MainWindow_OnDeactivated(object? sender, EventArgs e)
    {
        MinimizeWindow();
    }
}