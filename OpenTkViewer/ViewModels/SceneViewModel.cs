using System;
using System.Windows.Forms;
using OpenTkViewer.Controls;
using OpenTkViewer.Models;
using OpenTK;
using Screen = Caliburn.Micro.Screen;

namespace OpenTkViewer.ViewModels
{
    public class SceneViewModel : Screen
    {
        private readonly CameraManipulator cameraManipulator;
        private readonly ViewerScene viewerScene;

        public OpenTkControl OpenTkControl { get; set; }

        private GLControl GlControl => OpenTkControl.GlControl;

        public SceneViewModel(
            CameraManipulator cameraManipulator, 
            ViewerScene viewerScene,
            OpenTkControl openTkControl)
        {
            this.cameraManipulator = cameraManipulator;
            this.viewerScene = viewerScene;
            OpenTkControl = openTkControl;
            SubscribeOnGlControl();
        }

        private void SubscribeOnGlControl()
        {
            if(GlControl == null)
                return;

            GlControl.Paint += GlControl_Paint;
            GlControl.Resize += GlControl_Resize;
            GlControl.MouseWheel += GlControl_MouseWheel;
            GlControl.MouseDown += GlControl_MouseDown;
            GlControl.MouseMove += GlControl_MouseMove;
            GlControl.MouseUp += GlControl_MouseUp;
        }

        private void GlControl_Resize(object sender, EventArgs e)
        {
            cameraManipulator.ChangeSize(GlControl.Width, GlControl.Height);
            DrawHelper.InitializeViewport(GlControl.Width, GlControl.Height);
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            DrawHelper.InitializeScene(cameraManipulator.Camera);
            viewerScene.Draw();
            GlControl.SwapBuffers();
            GlControl.Invalidate();
        }

        private void GlControl_MouseDown(object sender, MouseEventArgs e)
        {
            cameraManipulator.MouseDown(e);
            GlControl.Invalidate();
        }

        private void GlControl_MouseMove(object sender, MouseEventArgs e)
        {
            cameraManipulator.MouseMove(e);
            GlControl.Invalidate();
        }
        
        private void GlControl_MouseUp(object sender, MouseEventArgs e)
        {
            cameraManipulator.MouseUp(e);
            GlControl.Invalidate();
        }

        private void GlControl_MouseWheel(object sender, MouseEventArgs e)
        {
            cameraManipulator.MouseWheel(e);
            GlControl.Invalidate();
        }
    }
}