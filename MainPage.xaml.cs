using CodebaseAnalysisLib;
using Microsoft.Maui.Controls;
using System;
using System.IO;

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
            }
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_solutionPath))
            {
                await DisplayAlert("Error", "Please select a .sln file.", "OK");
                return;
            }

            var instruction = InputEditor.Text;

            var codebaseInfo = await CodebaseAnalyzer.GetCodebaseInfo(_solutionPath, instruction);
            var codeBasestring = codebaseInfo.ToString();

            var codebaseInfoString = $"{codeBasestring}";
            var ns = System.Reflection.Assembly.GetExecutingAssembly().EntryPoint.DeclaringType.Namespace;
            string prompt = $"Given the following codebase information: \n\n Namespace: {ns} \n\n {codebaseInfoString} \n\n {instruction}";

            CodebaseInfo.Text = prompt;
        }
    }
}
