using Caliburn.Micro;
using Microsoft.Win32;

namespace OpenTkViewer.ViewModels
{
    public class ShellViewModel : Screen
    {
        private readonly ViewerScene viewerScene;

        private string displayName;

        public override string DisplayName
        {
            get => displayName;
            set
            {
                displayName = value;
                NotifyOfPropertyChange(nameof(DisplayName));
            }
        }

        public SceneViewModel SceneViewModel { get; set; }
        

        public ShellViewModel(ViewerScene viewerScene, SceneViewModel sceneViewModel)
        {
            this.viewerScene = viewerScene;
            SceneViewModel = sceneViewModel;
        }

        public void OpenFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                DefaultExt = ".stl",
                Filter = "(.stl)|*.stl"
            };

            var dialogResult = openFileDialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                foreach (var fileName in openFileDialog.FileNames)
                {
                    var model = StlReader.Load(fileName);
                    viewerScene.Add(model);
                }
            }
        }
    }
}