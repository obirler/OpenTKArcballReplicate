using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.AccessControl;

namespace OpenTKArcballReplicate
{
    public class Game : GameWindow
    {
        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            VSync = VSyncMode.On;
            prev_mouse = new Vector2(-2f, -2f);
            _width = nativeWindowSettings.ClientSize.X;
            _height = nativeWindowSettings.ClientSize.Y;
            read_model();
        }

        private static ArcballCamera _camera;

        private static int _width;

        private static int _height;

        private int _shaderProgram;

        private int _vertexArrayObject;

        private int _vertexBufferObject;

        private int _projviewLocation;

        private int _fractionLocation;

        private int _draw_size;

        private bool mouse_left = false;

        private bool mouse_right = false;

        private Vector2 prev_mouse;

        private static DebugProc DebugMessageDelegate = OpenGLDebugMessage;

        private float[] _vertices = {
            // Upper (+z) face    // Color red
             0.0f, -5.0f, 10.0f,  1.0f, 0.0f, 0.0f,
            20.0f, -5.0f, 10.0f,  1.0f, 0.0f, 0.0f,
            20.0f,  5.0f, 10.0f,  1.0f, 0.0f, 0.0f,
             0.0f, -5.0f, 10.0f,  1.0f, 0.0f, 0.0f,
            20.0f,  5.0f, 10.0f,  1.0f, 0.0f, 0.0f,
             0.0f,  5.0f, 10.0f,  1.0f, 0.0f, 0.0f,
            // Front (+y) face    // Color green
             0.0f,  5.0f,  0.0f,  0.0f, 1.0f, 0.0f,
             0.0f,  5.0f, 10.0f,  0.0f, 1.0f, 0.0f,
            20.0f,  5.0f, 10.0f,  0.0f, 1.0f, 0.0f,
             0.0f,  5.0f,  0.0f,  0.0f, 1.0f, 0.0f,
            20.0f,  5.0f, 10.0f,  0.0f, 1.0f, 0.0f,
            20.0f,  5.0f,  0.0f,  0.0f, 1.0f, 0.0f,
            // Left (+x) face     // Color blue
            20.0f, -5.0f,  0.0f,  0.0f, 0.0f, 1.0f,
            20.0f,  5.0f,  0.0f,  0.0f, 0.0f, 1.0f,
            20.0f, -5.0f, 10.0f,  0.0f, 0.0f, 1.0f,
            20.0f,  5.0f,  0.0f,  0.0f, 0.0f, 1.0f,
            20.0f,  5.0f, 10.0f,  0.0f, 0.0f, 1.0f,
            20.0f, -5.0f, 10.0f,  0.0f, 0.0f, 1.0f,
            // Lower (-z) face    // Color yellow
             0.0f, -5.0f,  0.0f,  1.0f, 1.0f, 0.0f,
             0.0f,  5.0f,  0.0f,  1.0f, 1.0f, 0.0f,
            20.0f,  5.0f,  0.0f,  1.0f, 1.0f, 0.0f,
            20.0f,  5.0f,  0.0f,  1.0f, 1.0f, 0.0f,
            20.0f, -5.0f,  0.0f,  1.0f, 1.0f, 0.0f,
             0.0f, -5.0f,  0.0f,  1.0f, 1.0f, 0.0f,
            // Back (-y) face    // Color aqua
             0.0f, -5.0f,  0.0f,  0.0f, 1.0f, 1.0f,
            20.0f, -5.0f,  0.0f,  0.0f, 1.0f, 1.0f,
            20.0f, -5.0f, 10.0f,  0.0f, 1.0f, 1.0f,
            20.0f, -5.0f, 10.0f,  0.0f, 1.0f, 1.0f,
             0.0f, -5.0f, 10.0f,  0.0f, 1.0f, 1.0f,
             0.0f, -5.0f,  0.0f,  0.0f, 1.0f, 1.0f,
            // Right (-x) face    // Color maroon
             0.0f, -5.0f,  0.0f,  1.0f, 0.0f, 1.0f,
             0.0f, -5.0f, 10.0f,  1.0f, 0.0f, 1.0f,
             0.0f,  5.0f, 10.0f,  1.0f, 0.0f, 1.0f,
             0.0f,  5.0f, 10.0f,  1.0f, 0.0f, 1.0f,
             0.0f,  5.0f,  0.0f,  1.0f, 0.0f, 1.0f,
             0.0f, -5.0f,  0.0f,  1.0f, 0.0f, 1.0f,
        };

        private BoundingBox _bbox;

        private bool _animation_active = false;

        private bool _reverse;

        private float _fraction_left;

        private float _fraction = 0.01f;

        private float _fraction_time_left = 2f;

        private float _delta_time;

        private float _fraction_speed;

        private float _step_fraction;

        private float _half_cycle_time = 1f;

        protected override void OnLoad()
        {
            GL.ClearColor(0.6f, 0.6f, 0.6f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            //GL.Enable(EnableCap.CullFace);

            GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
            GL.Enable(EnableCap.DebugOutput);

            _shaderProgram = CreateShaderProgram(@"Shaders/main.vert", @"Shaders/main.frag");
            GL.UseProgram(_shaderProgram);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            // position attribute
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0);

            //displacement attribute
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 3 * sizeof(float));

            //color attribute
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 6 * sizeof(float));

            _projviewLocation = GL.GetUniformLocation(_shaderProgram, "proj_view");
            _fractionLocation = GL.GetUniformLocation(_shaderProgram, "frameFrac");

            /*
            var eye = new Vector3(24.1421356f, -14.1421356f, 19.1421356f);
            var center = new Vector3(10f, 0f, 5f);
            var up_vec = Vector3.UnitY;
            _camera = new ArcballCamera(eye, center, up_vec, _width, _height);*/
            var center = new Vector3(0f, 0f, 0f);
            //var center = new Vector3(0f, 0f, 0f);

            var diag = _bbox.DiagonalLength();

            var eye_pos = new Vector3(diag/1000f, 0f, diag);
            //var eye_pos = new Vector3(new Vector3(24.1421356f, -14.1421356f, 19.1421356f));

            var up_vec = new Vector3(0f, 0f, 1f);

            _camera = new ArcballCamera(eye_pos, center, up_vec, _width, _height);

            _draw_size = (int)(_vertices.Length / 3.0);

            Logger.WriteLine("projection matrix");
            Logger.WriteLine(_camera.projection().ToString());

            //test();
            //test3();
            _animation_active = true;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            if (!_animation_active || _camera == null)
            {
                return;
            }

            _delta_time = Convert.ToSingle(e.Time);

            //test();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(_shaderProgram);

            // Use shaders and draw objects here...

            var projection = _camera.projection();
            //var projection = _camera.projection();
            //Logger.WriteLine("projection matrix");
            //Logger.WriteLine(projection.ToString());

            var transform = _camera.transform();
            //var transform = _camera.transform();
            //Logger.WriteLine("transform matrix");
            //Logger.WriteLine(transform.ToString());

            var projview = projection * transform;

            //Logger.WriteLine("final matrix");
            //Logger.WriteLine(projview.ToString());
            // Pass transform matrix to shader here...

            GL.UniformMatrix4(_projviewLocation, false, ref projview);
            GL.Uniform1(_fractionLocation, _fraction * 5.0f);

            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _draw_size);

            if (_animation_active)
            {
                bool reverse = this._reverse;
                if (reverse)
                {
                    this._fraction_left = this._fraction;
                    ref float ptr = ref this._fraction_time_left;
                    this._fraction_time_left = ptr - this._delta_time;
                    this._fraction_speed = this._fraction_left / this._fraction_time_left;
                    this._step_fraction = this._delta_time * this._fraction_speed;
                    ptr = ref this._fraction;
                    this._fraction = ptr - this._step_fraction;
                    bool flag2 = this._fraction <= 0f || this._fraction_time_left <= 0f;
                    bool flag5 = flag2;
                    if (flag5)
                    {
                        this._fraction = 0f;
                        this._reverse = false;
                        this._fraction_time_left = this._half_cycle_time;
                    }
                }
                else
                {
                    this._fraction_left = 1f - this._fraction;
                    ref float ptr = ref this._fraction_time_left;
                    this._fraction_time_left = ptr - this._delta_time;
                    this._fraction_speed = this._fraction_left / this._fraction_time_left;
                    this._step_fraction = this._delta_time * this._fraction_speed;
                    ptr = ref this._fraction;
                    this._fraction = ptr + this._step_fraction;
                    bool flag3 = this._fraction >= 1f || this._fraction_time_left <= 0f;
                    bool flag6 = flag3;
                    if (flag6)
                    {
                        this._fraction = 1f;
                        this._reverse = true;
                        this._fraction_time_left = this._half_cycle_time;
                    }
                }
            }

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            _width = Size.X;
            _height = Size.Y;
            Logger.WriteLine($"Size set to ({_width}, {_height})");
            GL.Viewport(0, 0, _width, _height);
            _camera.resize(_width, _height);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
           
            Vector2 cur_mouse = transform_mouse(e.X, e.Y);

            if (prev_mouse.X != -2f)
            {
                if (mouse_left)
                {
                    Logger.WriteLine($"Mouse move for rotate: ({e.X}, {e.Y})");
                    Logger.WriteLine($"Transformed pos: ({cur_mouse.X}, {cur_mouse.Y})");
                    _camera.rotate(prev_mouse, cur_mouse);
                }
                else if(mouse_right)
                {
                    Vector2 dxy = cur_mouse - prev_mouse;
                    Vector4 dxy4 = _camera.inv_projection() * new Vector4(dxy.X, dxy.Y, 0f, 1f);

                    _camera.pan(new Vector2(dxy4.X, dxy4.Y));
                }
            }
            prev_mouse = cur_mouse;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButton.Left)
            {
                mouse_left = true;
            }
            else if (e.Button == MouseButton.Right)
            {
                mouse_right = true;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs args)
        {
            if (args.Button == MouseButton.Left)
            {
                mouse_left = false;
            }
            else if (args.Button == MouseButton.Right)
            {
                mouse_right = false;
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            _camera.zoom(e.OffsetY * 0.1f);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (e.Key == Keys.O)
            {
                _camera.set_projection_mode(ProjectionMode.Orthogonal);
            }
            else if(e.Key == Keys.P)
            {
                _camera.set_projection_mode(ProjectionMode.Perspective);
            }
        }

        private int CreateShaderProgram(string vertexPath, string fragmentPath)
        {
            int vertexShader = CompileShader(ShaderType.VertexShader, File.ReadAllText(vertexPath));
            int fragmentShader = CompileShader(ShaderType.FragmentShader, File.ReadAllText(fragmentPath));

            int shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);

            GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out int status);
            if (status == 0)
            {
                throw new Exception(GL.GetProgramInfoLog(shaderProgram));
            }

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return shaderProgram;
        }

        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
            if (status == 0)
            {
                throw new Exception(GL.GetShaderInfoLog(shader));
            }

            return shader;
        }

        private Vector2 transform_mouse(float xposIn, float yposIn)
        {
            return new Vector2(xposIn * 2f / Size.X - 1f, 1f - 2f * yposIn / Size.X);
        }

        private void test()
        {
            Logger.WriteLine("test method called");

            var test_prev_mouse = new Vector2(-0.29f, 0.635f);
            var test_cur_mouse = new Vector2(-0.2875f, 0.635f);
            _camera.rotate(test_prev_mouse, test_cur_mouse);

            var projection = _camera.projection();
            //var projection = _camera.projection();
            Logger.WriteLine("projection matrix");
            Logger.WriteLine(projection.ToString());

            var transform = _camera.transform();
            //var transform = _camera.transform();
            Logger.WriteLine("transform matrix");
            Logger.WriteLine(transform.ToString());

            var final_mat = projection * transform;
            Logger.WriteLine("final matrix");
            Logger.WriteLine(final_mat.ToString());
        }

        private void test3()
        {
            Logger.WriteLine("test3 method called");

            var test_prev_mouse = new Vector2(-0.29f, 0.635f);
            var test_cur_mouse = new Vector2(-0.2875f, 0.635f);

            Logger.WriteLine("test3 rotation1");
            _camera.rotate(test_prev_mouse, test_cur_mouse);
            test_prev_mouse = test_cur_mouse;
            test_cur_mouse = new Vector2(-0.3f, 0.7f);

            Logger.WriteLine("test3 rotation2");
            _camera.rotate(test_prev_mouse, test_cur_mouse);
            test_prev_mouse = test_cur_mouse;
            test_cur_mouse = new Vector2(-0.4f, 0.71f);

            Logger.WriteLine("test3 rotation3");
            _camera.rotate(test_prev_mouse, test_cur_mouse);

            Logger.WriteLine("test3 zoom1");
            _camera.zoom(-1.0f);

            Logger.WriteLine("test3 pan1");
            Vector2 mouse_delta = new Vector2(0.05f, 0.1f);
            _camera.pan(mouse_delta);


            var projection = _camera.projection();
            //var projection = _camera.projection();
            Logger.WriteLine("projection matrix");
            Logger.WriteLine(projection.ToString());

            var transform = _camera.transform();
            //var transform = _camera.transform();
            Logger.WriteLine("transform matrix");
            Logger.WriteLine(transform.ToString());

            var final_mat = projection * transform;
            Logger.WriteLine("final matrix");
            Logger.WriteLine(final_mat.ToString());
        }

        private static void OpenGLDebugMessage(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr pMessage, IntPtr pUserParam)
        {
            var message = Marshal.PtrToStringAnsi(pMessage, length);
            Logger.WriteLine($"[{severity} source={source} type={type} id={id}] {message}");
        }

        private void read_model()
        {
            var vert_list = new List<float>();

            _bbox = new BoundingBox();

            using (var str = new StreamReader("geometry.txt"))
            {
                while(str.Peek() >= 0)
                {
                    var xval = Convert.ToSingle(str.ReadLine()) * 1f;
                    var yval = Convert.ToSingle(str.ReadLine()) * 1f;
                    var zval = Convert.ToSingle(str.ReadLine()) * 1f;
                    var dxval = Convert.ToSingle(str.ReadLine());
                    var dyval = Convert.ToSingle(str.ReadLine());
                    var dzval = Convert.ToSingle(str.ReadLine());
                    var rval = Convert.ToSingle(str.ReadLine());
                    var gval = Convert.ToSingle(str.ReadLine());
                    var bval = Convert.ToSingle(str.ReadLine());

                    _bbox.Expand(xval, yval, zval);

                    vert_list.Add(xval);
                    vert_list.Add(yval);
                    vert_list.Add(zval);
                    vert_list.Add(dxval);
                    vert_list.Add(dyval);
                    vert_list.Add(dzval);
                    vert_list.Add(rval);
                    vert_list.Add(gval);
                    vert_list.Add(bval);
                }
                
            }

            var middle = _bbox.Middle;
            var xoffset = middle.X;
            var yoffset = middle.Y;
            var zoffset = middle.Z;

            _vertices = new float[vert_list.Count];
            int counter = 0;
            for (int i = 0; i < vert_list.Count / 9; i++)
            {
                _vertices[counter] = vert_list[counter] - xoffset;
                counter++;
                _vertices[counter] = vert_list[counter] - yoffset;
                counter++;
                _vertices[counter] = vert_list[counter] - zoffset;
                counter++;
                _vertices[counter] = vert_list[counter];
                counter++;
                _vertices[counter] = vert_list[counter];
                counter++;
                _vertices[counter] = vert_list[counter];
                counter++;
                _vertices[counter] = vert_list[counter];
                counter++;
                _vertices[counter] = vert_list[counter];
                counter++;
                _vertices[counter] = vert_list[counter];
                counter++;
            }
            //_vertices = vert_list.ToArray();
        }

        [STAThread]
        public static void Main(string[] args)
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(1525 -16, 893 - 39),
                Title = "LearnOpenTK - Creating a Window",
                // This is needed to run on macos
                Flags = ContextFlags.ForwardCompatible,
            };

            using (Game game = new Game(GameWindowSettings.Default, nativeWindowSettings))
            {
                game.Run();
            }
        }
    } 
}
