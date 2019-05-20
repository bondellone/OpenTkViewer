using System;
using Caliburn.Micro;
using Microsoft.Win32;
using OpenTkViewer.Controls;
using OpenTK;

namespace OpenTkViewer.ViewModels
{
    public class ShellViewModel : Screen
    {
        private readonly Camera camera;
        private readonly OpenTkControl openTkControl;
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


        public ShellViewModel(
            Camera camera,
            OpenTkControl openTkControl,
            ViewerScene viewerScene,
            SceneViewModel sceneViewModel)
        {
            this.camera = camera;
            this.openTkControl = openTkControl;
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

            var sceneBoundingBox = viewerScene.GetSceneBoundingBox();
            if (sceneBoundingBox.HasValue)
                camera.Update(sceneBoundingBox.Value);
            else
                camera.Reset();

            openTkControl.Update();
        }
    }
}