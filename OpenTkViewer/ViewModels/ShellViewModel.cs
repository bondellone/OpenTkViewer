using Caliburn.Micro;
using OpenTkViewer.Controls;
using OpenTkViewer.Models;

namespace OpenTkViewer.ViewModels
{
    public class ShellViewModel : Screen
    {
        private readonly Camera camera;
        private readonly OpenTkControl openTkControl;
        private readonly ViewerScene viewerScene;
        private readonly FileManager fileManager;

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

        public ShellViewModel(
            Camera camera,
            OpenTkControl openTkControl,
            ViewerScene viewerScene,
            FileManager fileManager,
            SceneViewModel sceneViewModel)
        {
            this.camera = camera;
            this.openTkControl = openTkControl;
            this.viewerScene = viewerScene;
            this.fileManager = fileManager;
            SceneViewModel = sceneViewModel;
        }

        public void OpenFile()
        {
            fileManager.OpenFile();
            var sceneBoundingBox = viewerScene.GetSceneBoundingBox();
            if (sceneBoundingBox.HasValue)
                camera.Update(sceneBoundingBox.Value);
            else
                camera.Reset();

            openTkControl.Update();
        }
    }
}