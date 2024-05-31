using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.InteropServices;
using System.Diagnostics;

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
        }

        private static ArcballCamera _camera;

        private static int _width;
        private static int _height;

        private int _shaderProgram;
        private int _vertexArray;
        private int _vertexBuffer;
        private int _indexBuffer;

        private int _projviewLocation;

        private int _draw_size;

        private bool mouse_left = false;

        private bool mouse_right = false;

        private Vector2 prev_mouse;

        private static DebugProc DebugMessageDelegate = OpenGLDebugMessage;

        private readonly float[] _vertices = {
            // Front face
            -1.0f, -1.0f,  1.0f,
             1.0f, -1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
            -1.0f,  1.0f,  1.0f,
            // Back face
            -1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,
        };

        private readonly uint[] _indices = {
            // Front face
            0, 1, 2, 2, 3, 0,
            // Top face
            3, 2, 6, 6, 7, 3,
            // Back face
            7, 6, 5, 5, 4, 7,
            // Bottom face
            4, 5, 1, 1, 0, 4,
            // Left face
            4, 0, 3, 3, 7, 4,
            // Right face
            1, 5, 6, 6, 2, 1,
        };

        protected override void OnLoad()
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
            GL.Enable(EnableCap.DebugOutput);

            _shaderProgram = CreateShaderProgram("main.vert", "main.frag");
            GL.UseProgram(_shaderProgram);

            _vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArray);
            
            _vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _indexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            _projviewLocation = GL.GetUniformLocation(_shaderProgram, "proj_view");

            _camera = new ArcballCamera(new Vector3(0, 0, 5), Vector3.Zero, Vector3.UnitY, _width, _height);

            _draw_size = (int)(_vertices.Length / 3.0);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(_shaderProgram);

            // Use shaders and draw objects here...

            var projection = _camera.projection();
            var transform = _camera.transform();

            var projview = projection * transform;
            // Pass transform matrix to shader here...

            GL.UniformMatrix4(_projviewLocation, false, ref projview);

            GL.BindVertexArray(_vertexArray);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            _width = Size.X;
            _height = Size.Y;
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

        private static void OpenGLDebugMessage(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr pMessage, IntPtr pUserParam)
        {
            var message = Marshal.PtrToStringAnsi(pMessage, length);
            Console.WriteLine($"[{severity} source={source} type={type} id={id}] {message}");
        }

        [STAThread]
        public static void Main(string[] args)
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(800, 600),
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
