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
    public const uint windowWidth = 480;
    public const uint windowHeight = 320;
    public const uint scale = 4;

    [STAThread]
    public static void Main()
    {
        Color bg = new Color(167, 193, 145);
        Color fg = new Color(13, 15, 11);
        Color border = new Color(241, 244, 235);
        Color pen = fg;

        VideoMode mode = new VideoMode();

        mode.Width = (uint)(windowWidth);
        mode.Height = (uint)(windowHeight);

        RenderWindow window = new RenderWindow(mode, "RC-01", Styles.Close);
        RenderTexture render = new RenderTexture(screenWidth, screenHeight);
        Texture borderTexture = new Texture("resources/border.png");
        render.Clear(bg);

        void DrawLine(uint x1, uint y1, uint x2, uint y2)
        {
            VertexArray array = new VertexArray();
            array.PrimitiveType = PrimitiveType.Lines;
            Vertex v1 = new Vertex();
            Vertex v2 = new Vertex();
            v1.Position = new Vector2f((float)x1 + .5f, (float)y1 + .5f);
            v2.Position = new Vector2f((float)x2 + .5f, (float)y2 + .5f);
            v1.Color = pen;
            v2.Color = pen;
            
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

        void PenColor(bool on)
        {
            if (on) pen = fg;
            else pen = bg;
        }

        void Clear()
        {
            render.Clear(pen);
        }

        Script scr = new Script();
        scr.Options.DebugPrint = s => { Console.WriteLine(s); };

        //Define in-built functions
        scr.Globals["Horizontal"] = (Action<uint>)Horizontal;
        scr.Globals["Vertical"] = (Action<uint>)Vertical;
        scr.Globals["Clear"] = (Action)Clear;
        scr.Globals["PenColor"] = (Action<bool>)PenColor;

        scr.DoFile("main.lua");

        // Set up event handler
        void Window_Closed(object sender, EventArgs e)
        {
            var window = (SFML.Window.Window)sender;
            window.Close();
        }

        window.Closed += Window_Closed;

        object updateFunc = scr.Globals["update"];

        while (window.IsOpen)
        {
            window.DispatchEvents();

            // Make sure we have an update function
            if (updateFunc != null)
            {
                scr.Call(scr.Globals["update"]);
            }

            // Turn the screen into a texture/sprite
            render.Display();
            Sprite sprite = new Sprite();
            Image image = render.Texture.CopyToImage();
            image.CreateMaskFromColor(bg);
            sprite.Texture = new Texture(image);
            sprite.Origin = new Vector2f(screenWidth / 2, screenHeight / 2);
            sprite.Position = new Vector2f(windowWidth / 2, windowHeight / 2);
            
            sprite.Scale = new SFML.System.Vector2f(scale, scale);

            uint offset = scale / 2;
            Sprite offsetSprite = new Sprite(sprite);
            offsetSprite.Position += new Vector2f((float)offset, (float)offset);
            offsetSprite.Color = new Color(255, 255, 255, 255 / 4);

            Sprite borderSprite = new Sprite();
            borderSprite.Texture = borderTexture;
            borderSprite.Position = new(0, 0);
            borderSprite.Color = border;

            window.Clear(bg);

            window.Draw(offsetSprite);
            window.Draw(sprite);
            window.Draw(borderSprite);

            window.Display();
        }
    }
}