using System;
using OpenTK;

namespace OpenTkViewer
{
    public class Camera
    {
        private double width;
        private double height;
        private double zNear = 1;
        private double zFar = 25000;

        private Matrix4d rotationMatrix;
        private Matrix4d translationMatrix;
        private Matrix4d rotationCenterMatrix;

        public Matrix4d ProjectionMatrix { get; private set; }

        public Matrix4d InverseProjectionMatrix { get; private set; }

        public Matrix4d ModelViewMatrix { get; private set; }

        public Matrix4d InverseModelViewMatrix { get; set; }

        public Matrix4d RotationMatrix
        {
            get => rotationMatrix;
            set
            {
                rotationMatrix = value;
                OnTransformChanged();
            }
        }

        public Matrix4d TranslationMatrix
        {
            get => translationMatrix;
            set
            {
                translationMatrix = value;
                OnTransformChanged();
            }
        }

        public Matrix4d RotationCenterMatrix
        {
            get => rotationCenterMatrix;
            set
            {
                var prev = GetTransform4X4();
                rotationCenterMatrix = value;
                var cur = GetTransform4X4();
                var offset = new Vector3d(
                    prev.M41 - cur.M41,
                    prev.M42 - cur.M42,
                    prev.M43 - cur.M43);

                Translate(offset);
            }
        }

        public Vector2d ScreenCenter { get; set; }

        public Camera(double width, double height)
        {
            this.width = width;
            this.height = height;

            rotationMatrix = Matrix4d.Identity;
            translationMatrix = Matrix4d.Identity;
            rotationCenterMatrix = Matrix4d.Identity;
            ProjectionMatrix = Matrix4d.Identity;
            //Scale = 0.03;
            CalculateProjectionMatrix(width, height);
            CalculateModelViewMatrix();
        }

        public void OnTransformChanged()
        {
            CalculateModelViewMatrix();
        }

        public void CalculateProjectionMatrix(double newWidth, double newHeight)
        {
            if (!(newWidth > 0) || !(newHeight > 0))
                return;

            width = newWidth;
            height = newHeight;

            ScreenCenter = new Vector2d(width / 2, height / 2);
            Matrix4d.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45), width / height, zNear, zFar, out var projectionMatrix);

            ProjectionMatrix = projectionMatrix;
            InverseProjectionMatrix = Matrix4d.Invert(projectionMatrix);
        }
        
        private void CalculateModelViewMatrix()
        {
            ModelViewMatrix = GetTransform4X4();
            InverseModelViewMatrix = Matrix4d.Invert(ModelViewMatrix);
        }

        private Matrix4d GetTransform4X4()
        {
            return RotationCenterMatrix * rotationMatrix * translationMatrix;
        }

        public void Translate(Vector3d deltaPosition)
        {
            var maxFar = zFar / 1.8;
            translationMatrix = Matrix4d.CreateTranslation(deltaPosition) * translationMatrix;
            if (Math.Abs(translationMatrix.M41) > maxFar)
                translationMatrix.M41 = translationMatrix.M41 > 0 ? maxFar : -maxFar;

            if (Math.Abs(translationMatrix.M42) > maxFar)
                translationMatrix.M42 = translationMatrix.M42 > 0 ? maxFar : -maxFar;

            if (Math.Abs(translationMatrix.M43) > maxFar)
                translationMatrix.M43 = translationMatrix.M43 > 0 ? maxFar : -maxFar;

            OnTransformChanged();
        }

        public void Rotate(Quaterniond rotation)
        {
            rotationMatrix = rotationMatrix * ViewerMath.CreateRotation(rotation);
            OnTransformChanged();
        }

        //public void Reset()
        //{
        //    rotationCenterMatrix = Matrix4d.Identity;
        //    rotationMatrix = Matrix4d.Identity;
        //    translationMatrix = Matrix4d.Identity;
        //}

        //public double GetWorldUnitsPerScreenPixelAtPosition(Vector3 worldPosition, double maxRatio = 5)
        //{
        //    var screenPosition = GetScreenPosition(worldPosition);

        //    var rayFromScreen = GetRayForLocalBounds(screenPosition);
        //    var distanceFromOriginToWorldPos = (worldPosition - rayFromScreen.origin).Length;

        //    var rightOnePixelRay = GetRayForLocalBounds(new Vector2(screenPosition.X + 1, screenPosition.Y));
        //    var rightOnePixel = rightOnePixelRay.origin +
        //                        rightOnePixelRay.directionNormal * distanceFromOriginToWorldPos;
        //    var distBetweenPixelsWorldSpace = (rightOnePixel - worldPosition).Length;
        //    if (distBetweenPixelsWorldSpace > maxRatio)
        //    {
        //        return maxRatio;
        //    }
        //    return distBetweenPixelsWorldSpace;
        //}

        //public Ray GetRayForLocalBounds(Vector2 localPosition)
        //{
        //    var rayClip = new Vector4();
        //    rayClip.X = (2.0 * localPosition.X) / width - 1.0;
        //    rayClip.Y = (2.0 * localPosition.Y) / height - 1.0;
        //    rayClip.Z = -1.0;
        //    rayClip.W = 1.0;

        //    var rayEye = Vector4.Transform(rayClip, InverseProjectionMatrix);
        //    rayEye.Z = -1;
        //    rayEye.W = 0;

        //    var rayWorld = Vector4.Transform(rayEye, InverseModelViewMatrix);

        //    var finalRayWorld = new Vector3(rayWorld).GetNormal();

        //    var origin = Vector3.Transform(Vector3.Zero, InverseModelViewMatrix);

        //    return new Ray(origin, finalRayWorld);
        //}

        //public Vector3 GetWorldPosition(Vector2 screenPosition)
        //{
        //    var homoginizedScreenSpace = new Vector4((2.0f * (screenPosition.X / width)) - 1,
        //        1 - (2 * (screenPosition.Y / height)),
        //        1,
        //        1);

        //    var viewProjection = ModelViewMatrix * ProjectionMatrix;
        //    var viewProjectionInverse = Matrix4d.Invert(viewProjection);
        //    var woldSpace = Vector4.Transform(homoginizedScreenSpace, viewProjectionInverse);

        //    var perspectiveDivide = 1 / woldSpace.W;

        //    woldSpace.X *= perspectiveDivide;
        //    woldSpace.Y *= perspectiveDivide;
        //    woldSpace.Z *= perspectiveDivide;

        //    return new Vector3(woldSpace);
        //}

        //public Vector2d GetScreenPosition(Vector3 worldPosition)
        //{
        //    var homoginizedViewPosition = Vector3d.Transform(worldPosition, ModelViewMatrix);
        //    var homoginizedScreenPosition = Vector3d.TransformPerspective(homoginizedViewPosition, ProjectionMatrix);

        //    // Screen position
        //    return new Vector2d(homoginizedScreenPosition.X * width / 2 + width / 2,
        //        homoginizedScreenPosition.Y * height / 2 + height / 2);
        //}
    }
}