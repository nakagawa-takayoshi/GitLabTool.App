using System.Windows;
using System.Windows.Controls;

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
        AppIconImage.Visibility = Visibility.Collapsed;
        TitleTextBlock.Visibility = Visibility.Visible;
        CloseButton.Visibility = Visibility.Collapsed;
        MinimizeButton.Visibility = Visibility.Collapsed;
        RestoreButton.Visibility = Visibility.Visible;
        _windowRect.X = Left;
        _windowRect.Y = Top;
        _windowRect.Width = ActualWidth;
        _windowRect.Height = ActualHeight;
        Left = SystemParameters.PrimaryScreenWidth - ActualWidth;
        Top = 0;
        Width = 32.0d;
        Height = 32.0d;
    }

    private void RestoreButton_OnClick(object sender, RoutedEventArgs e)
    {
        AppIconImage.Visibility = Visibility.Visible;
        TitleTextBlock.Visibility = Visibility.Visible;
        CloseButton.Visibility = Visibility.Visible;
        MinimizeButton.Visibility = Visibility.Visible;
        RestoreButton.Visibility = Visibility.Collapsed;
        Left = _windowRect.X;
        Top = _windowRect.Y;
        Width = _windowRect.Width;
        Height = _windowRect.Height;
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
        KeywordComboBox.SelectedIndex = 0;

        BranchNamePrefixTextBlock.Text = $"user/nakagawa/{DateTime.Now.ToString("yyyyMMdd")}_A231_";
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
}