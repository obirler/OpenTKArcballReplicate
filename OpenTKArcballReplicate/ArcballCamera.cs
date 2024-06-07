using System;
using System.Diagnostics;

using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenTKArcballReplicate
{
    /* A simple arcball camera that moves around the camera's focal point.
    * The mouse inputs to the camera should be in normalized device coordinates,
    * where the top-left of the screen corresponds to [-1, 1], and the bottom
    * right is [1, -1].
    */
    public class ArcballCamera
    {
        /* Create an arcball camera focused on some center point
     * screen: [win_width, win_height]
     */
        public ArcballCamera(Vector3 eye, Vector3 center, Vector3 up, int width, int height)
        {
            _width = width + 16;
            _height = height + 39;

            Logger.WriteLine($"constructing: width={_width}, height={_height}");

            Vector3 origin = Vector3.Zero;

            Vector3 dir = origin - eye;
            Vector3 z_axis = Vector3.Normalize(dir);
            Vector3 x_axis = Vector3.Normalize(Vector3.Cross(z_axis, up));
            Vector3 y_axis = Vector3.Normalize(Vector3.Cross(x_axis, z_axis));
            x_axis = Vector3.Normalize(Vector3.Cross(z_axis, y_axis));

            center_translation = Matrix4.CreateTranslation(origin).Inverted();
            Logger.WriteLine("center translation matrix");
            Logger.WriteLine(center_translation.ToString());

            translation = Matrix4.Transpose(Matrix4.CreateTranslation(0, 0, -dir.Length));
            Logger.WriteLine("translation matrix");
            Logger.WriteLine(translation.ToString());

            // Create the initial rotation quaternion from the axis vectors
            rotation = Quaternion.FromMatrix(new Matrix3(
                x_axis.X, y_axis.X, -z_axis.X,
                x_axis.Y, y_axis.Y, -z_axis.Y,
                x_axis.Z, y_axis.Z, -z_axis.Z
            ));
            

            float aspect = (float)_width / ((float)_height);

            set_projection(fov, aspect, near, far);

            update_camera();

            move(-center);
        }

        private int _width;

        private int _height;

        // We store the unmodified look at matrix along with
        // decomposed translation and rotation components
        private Matrix4 center_translation;
        private Matrix4 translation;
        private Quaternion rotation = Quaternion.Identity;

        // camera is the full camera transform,
        // inv_camera is stored as well to easily compute
        // eye position and world space rotation axes

        private Matrix4 camera;
        private Matrix4 inv_camera;

        private Matrix4 proj;

        private Matrix4 proj_inv;

        private float fov = 65f;

        private float near = 0.1f;

        private float far = 400.0f;

        private ProjectionMode _projectionMode = ProjectionMode.Perspective;

        private float _ortho_scale = 5.0f;

        public void set_projection(float fov, float aspect, float near, float far)
        {
            Logger.WriteLine($"projection set fov={fov}, aspect={aspect}, near={near}, far={far})");

            switch (_projectionMode)
            {
                case ProjectionMode.Perspective:
                    proj = Matrix4.Transpose(Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fov), aspect, near, far));
                    break;

                case ProjectionMode.Orthogonal:
                    float ortho_width = _ortho_scale * aspect;
                    proj = Matrix4.Transpose(Matrix4.CreateOrthographic(ortho_width, _ortho_scale, near, far));
                    break;
            }
            
            proj_inv = proj.Inverted();
        }

        public void resize(int width, int height)
        {
            _width = width;
            _height = height;
            Logger.WriteLine($"Camera size set to ({_width}, {_height})");
            float aspect = (float)_width / ((float)_height);
            set_projection(fov, aspect, near, far);
        }

        public void set_projection_mode(ProjectionMode mode)
        {
            if (_projectionMode == mode)
            {
                // No change, so no need to update
                return;
            }
                
            _projectionMode = mode;
            float aspect = ((float)_width) / ((float)_height);

            switch (_projectionMode)
            {
                case ProjectionMode.Perspective:
                    // When switching to perspective, we need to adjust the FOV
                    // to maintain the same zoom level as the current orthographic scale
                    float newFov = MathHelper.RadiansToDegrees(2.0f * MathF.Atan(1.0f / _ortho_scale));
                    fov = newFov;
                    set_projection(fov, aspect, near, far);
                    break;

                case ProjectionMode.Orthogonal:
                    // When switching to orthogonal, we need to adjust the orthographic
                    // scale to maintain the same zoom level as the current FOV
                    float currentFov = MathHelper.DegreesToRadians(fov);
                    _ortho_scale = 1.0f / MathF.Tan(0.5f * currentFov);
                    set_projection(fov, aspect, near, far);
                    break;
            }
        }

        public void rotate(Vector2 prev_mouse, Vector2 cur_mouse)
        {
            // Clamp mouse positions to stay in NDC
            Logger.WriteLine($"rotate called: prev_mouse:({prev_mouse.X}, {prev_mouse.Y}), cur_mouse:({cur_mouse.X}, {cur_mouse.Y})");
            cur_mouse = Vector2.Clamp(cur_mouse, new Vector2(-1, -1), new Vector2(1, 1));
            prev_mouse = Vector2.Clamp(prev_mouse, new Vector2(-1, -1), new Vector2(1, 1));

            // Convert screen positions to arcball positions
            Quaternion mouse_cur_ball = screen_to_arcball(cur_mouse);
            Quaternion mouse_prev_ball = screen_to_arcball(prev_mouse);

            Logger.WriteLine("mouse_cur_ball");
            Logger.WriteLine(mouse_cur_ball.ToString());

            Logger.WriteLine("mouse_prev_ball");
            Logger.WriteLine(mouse_prev_ball.ToString());

            // Update rotation
            rotation = mouse_cur_ball * mouse_prev_ball * rotation;

            Logger.WriteLine("rotation");
            Logger.WriteLine(rotation.ToString());

            update_camera();
        }

        // Moves the object using mouse move in 2d viewport
        public void pan(Vector2 mouse_delta)
        {
            Logger.WriteLine($"panning: ({mouse_delta.X}, {mouse_delta.Y})");
            Logger.WriteLine("translation matrix");
            Logger.WriteLine(translation.ToString());

            //Unlike the original, we take m34 probably because of it is transposed
            float zoom_amount = Math.Abs(translation.M34);
            Logger.WriteLine($"zoom amount= {zoom_amount}");
            Vector4 motion = new Vector4(mouse_delta.X * zoom_amount, mouse_delta.Y * zoom_amount, 0f, 0f);

            // Find the panning amount in the world space
            motion = inv_camera * motion;
            Logger.WriteLine("motion vec4");
            Logger.WriteLine(motion.ToString());

            Matrix4 motion_trans = Matrix4.Transpose(Matrix4.CreateTranslation(motion.Xyz));
            Logger.WriteLine("motion_trans matrix");
            Logger.WriteLine(motion_trans.ToString());

            center_translation = motion_trans * center_translation;

            Logger.WriteLine("center_translation matrix");
            Logger.WriteLine(center_translation.ToString());

            update_camera();
        }

        // Moves the object in the 3d space using translation vector
        public void move(Vector3 delta)
        {
            Matrix4 motion_trans = Matrix4.Transpose(Matrix4.CreateTranslation(delta));
            Logger.WriteLine("motion_trans matrix");
            Logger.WriteLine(motion_trans.ToString());

            center_translation = motion_trans * center_translation;

            Logger.WriteLine("center_translation matrix");
            Logger.WriteLine(center_translation.ToString());

            update_camera();
        }

        public void zoom(float zoom_amount)
        {
            Logger.WriteLine($"zooming: {zoom_amount}");
            switch (_projectionMode)
            {
                case ProjectionMode.Perspective:
                    Vector3 motion = new Vector3(0f, 0f, zoom_amount);
                    Matrix4 motion_trans = Matrix4.Transpose(Matrix4.CreateTranslation(motion));
                    Logger.WriteLine("motion_trans matrix");
                    Logger.WriteLine(motion_trans.ToString());

                    translation = motion_trans * translation;
                    Logger.WriteLine("translation");
                    Logger.WriteLine(translation.ToString());
                    break;

                case ProjectionMode.Orthogonal:
                    _ortho_scale -= zoom_amount;
                    set_projection(fov, ((float)_width) / ((float)_height), near, far);
                    break;
            }


            update_camera();
        }

        public Matrix4 transform()
        {
            return camera;
        }

        public Matrix4 inv_transform()
        {
            return inv_camera;
        }

        public Matrix4 projection()
        {
            return proj;
        }

        public Matrix4 inv_projection()
        {
            return proj_inv;
        }

        public Vector3 eye()
        {
            Vector4 eyePos = inv_camera * new Vector4(0, 0, 0, 1);
            return new Vector3(eyePos.X, eyePos.Y, eyePos.Z);
        }

        public Vector3 dir()
        {
            Vector4 dirPos = inv_camera * new Vector4(0, 0, -1, 0);
            return new Vector3(dirPos.X, dirPos.Y, dirPos.Z).Normalized();
        }

        public Vector3 up()
        {
            Vector4 upPos = inv_camera * new Vector4(0, 1, 0, 0);
            return new Vector3(upPos.X, upPos.Y, upPos.Z).Normalized();
        }

        private void update_camera()
        {
            Logger.WriteLine("uptate_camera called");
            Matrix4 rotation_mat = Matrix4.Transpose(Matrix4.CreateFromQuaternion(rotation));

            Logger.WriteLine("translation matrix");
            Logger.WriteLine(translation.ToString());

            Logger.WriteLine("rotation_mat matrix");
            Logger.WriteLine(rotation_mat.ToString());

            Logger.WriteLine("center_translation matrix");
            Logger.WriteLine(center_translation.ToString());

            //Matrix4 mat1 = translation * rotation_mat;
            //Logger.WriteLine("mat1 matrix");
            //Logger.WriteLine(mat1.ToString());

            camera = translation * rotation_mat * center_translation;

            Logger.WriteLine("camera matrix");
            Logger.WriteLine(camera.ToString()); 

            inv_camera = camera.Inverted();

            Logger.WriteLine("inv_camera matrix");
            Logger.WriteLine(inv_camera.ToString());
        }

        private Quaternion screen_to_arcball(Vector2 point)
        {
            float dist = Vector2.Dot(point, point);

            // If we're on/in the sphere return the point on it
            if (dist <= 1.0f)
            {
                return new Quaternion(point.X, point.Y, MathF.Sqrt(1.0f - dist), 0.0f);
            }
            else
            {
                // otherwise we project the point onto the sphere
                Vector2 proj = Vector2.Normalize(point);
                return new Quaternion(proj.X, proj.Y, 0.0f, 0.0f);
            }
        }
    }
}
