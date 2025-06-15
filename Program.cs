using MoonSharp.Interpreter;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Numerics;
using System.Runtime.InteropServices;

namespace HelloWorld;

class Program
{
    public const uint screenWidth = 96;
    public const uint screenHeight = 64;
    public const uint scale = 4;

    [STAThread]
    public static void Main()
    {
        Color bg = new Color(167, 193, 145);
        Color fg = new Color(13, 15, 11);

        VideoMode mode = new VideoMode();

        mode.Width = screenWidth * scale;
        mode.Height = screenHeight * scale;

        RenderWindow window = new RenderWindow(mode, "RC-01", Styles.Close);
        RenderTexture render = new RenderTexture(screenWidth, screenHeight);
        render.Clear(bg);

        void DrawLine(uint x1, uint y1, uint x2, uint y2)
        {
            VertexArray array = new VertexArray();
            array.PrimitiveType = PrimitiveType.Lines;
            Vertex v1 = new Vertex();
            Vertex v2 = new Vertex();
            v1.Position = new Vector2f((float)x1, (float)y1);
            v2.Position = new Vector2f((float)x2, (float)y2);
            v1.Color = fg;
            v2.Color = fg;
            
            array.Append(v1);
            array.Append(v2);

            render.Draw(array);
        }

        void Horizontal(uint y)
        {
            DrawLine(0, y, render.Size.X, y);
        }

        void Vertical(uint x)
        {
            DrawLine(x, 0, x, render.Size.Y);
        }

        void Clear(bool fill = false)
        {
            Color clearColor;

            if (fill)
            {
                clearColor = fg;
            } else
            {
                clearColor = bg;
            }

            render.Clear(clearColor);
        }

        Script scr = new Script();
        scr.Options.DebugPrint = s => { Console.WriteLine(s); };

        //Define in-built functions
        scr.Globals["Horizontal"] = (Action<uint>)Horizontal;
        scr.Globals["Vertical"] = (Action<uint>)Vertical;
        scr.Globals["Clear"] = (Action<bool>)Clear;

        scr.DoFile("main.lua");

        // Set up event handler
        void Window_Closed(object sender, EventArgs e)
        {
            var window = (SFML.Window.Window)sender;
            window.Close();
        }

        window.Closed += Window_Closed;

        while (window.IsOpen)
        {
            window.DispatchEvents();

            scr.Call(scr.Globals["update"]);

            RectangleShape shape = new RectangleShape();

            shape.Position = new Vector2f(2, 8);
            shape.Size = new Vector2f(5, 5);
            shape.FillColor = fg;

            render.Draw(shape);

            render.Display();
            Sprite sprite = new Sprite();
            sprite.Texture = render.Texture;
            sprite.Scale = new SFML.System.Vector2f(scale, scale);

            window.Draw(sprite);

            window.Display();
        }
    }
}