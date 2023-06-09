using CodebaseAnalysisLib;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace CodebaseAnalysisMauiApp
{
    public partial class MainPage : ContentPage
    {

        private readonly CodebaseAnalyzer _analyzer;
        private string _solutionPath;

        public MainPage()
        {
            InitializeComponent();
            _analyzer = new CodebaseAnalyzer();
            _solutionPath = CodebaseAnalyzer.GetSolutionPath();
            SolutionPathLabel.Text = $"Solution Path: {_solutionPath}";
        }

        // Add this method to handle the file picker functionality
        private async void OnFilePickerClicked(object sender, EventArgs e)
        {
            var filePickerOptions = new PickOptions
            {
                FileTypes = new FilePickerFileType
                    (new Dictionary<DevicePlatform, IEnumerable<string>>
                        {{
                        DevicePlatform.WinUI, new[] { ".sln" }
                        },}
                    )
            };

            var fileResult = await FilePicker.PickAsync(filePickerOptions);
            if (fileResult != null)
            {
                _solutionPath = fileResult.FullPath;
                SolutionPathLabel.Text = $"Solution Path: {_solutionPath}";
            }
        }




        private async void OnApplySuggestedChangesClicked(object sender, EventArgs e)
        {
            string json = SuggestedChangesEditor.Text;

            try
            {
                string solutionPath = CodebaseAnalyzer.GetSolutionPath();
                string solutionDirectory = Path.GetDirectoryName(solutionPath);
                var changes = JsonSerializer.Deserialize<List<SuggestedChange>>(json);
                foreach (var change in changes)
                {
                    string filePath = Directory.GetFiles(solutionDirectory, change.FileName.TrimStart('\\'), SearchOption.AllDirectories).FirstOrDefault();

                    if (string.IsNullOrEmpty(filePath))
                    {
                        throw new FileNotFoundException($"File '{change.FileName}' not found.");
                    }
                    change.FileName = filePath;
                }
                await SuggestedChange.ApplySuggestedChanges(changes);
                await DisplayAlert("Success", "Suggested changes have been applied.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to apply suggested changes: {ex.Message}", "OK");
            }
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            var instruction = InputEditor.Text;

            var codebaseInfo = await CodebaseAnalyzer.GetCodebaseInfo(_solutionPath, instruction);
            var codeBasestring = codebaseInfo.ToString();

            var codebaseInfoString = $"{codeBasestring}";
            var ns = System.Reflection.Assembly.GetExecutingAssembly().EntryPoint.DeclaringType.Namespace;
            string prompt = $"Given the following codebase information: \n\n Namespace: {ns} \n\n {codebaseInfoString} \n\n {instruction}";
            prompt += $"\n\n output all parts 1 codeblock at a time.\r\nIf you cannot finish a codeblock before running out of characters/tokens, omit the codeblock and tell me to type continue";
            CodebaseInfo.Text = prompt;
        }
    }
}
