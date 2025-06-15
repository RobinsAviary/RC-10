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
        double time = 0;

        Color renderBg = new Color(198, 216, 182);
        Color bg = new Color(167, 193, 145);
        Color fg = new Color(13, 15, 11);
        Color border = new Color(241, 244, 235);
        Color pen = fg;

        VideoMode mode = new VideoMode();

        mode.Width = (uint)(windowWidth);
        mode.Height = (uint)(windowHeight);

        Clock deltaTimer = new();

        RenderWindow window = new RenderWindow(mode, "RC-10", Styles.Close);
        RenderTexture render = new RenderTexture(screenWidth, screenHeight);
        Texture borderTexture = new Texture("resources/border.png");
        Texture fontTexture = new Texture("resources/font.png");
        string fontChars = " ./0123456789:<=>?ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^abcdefghijklmnopqrstuvwxyz{}!\"\'()*+,-";
        const uint fontWidth = 6;

        void DrawFrame(RenderTarget target, Texture texture, uint width, uint index, Vector2f position, Color tint)
        {
            Sprite frame = new();
            frame.Texture = texture;
            frame.TextureRect = new(new((int)(index * width), 0), new((int)width, (int)texture.Size.Y));
            frame.Position = position;
            frame.Color = tint;

            target.Draw(frame);
        }

        render.Clear(bg);

        // Lua function set

        void DrawLine(int x1, int y1, int x2, int y2)
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

        void DrawChar(int x, int y, char letter)
        {
            if (fontChars.Contains(letter))
            {
                uint index = (uint)fontChars.IndexOf(letter);
                DrawFrame(render, fontTexture, fontWidth, index, new(x * fontWidth, y * fontTexture.Size.Y), pen);
            }
        }

        void Text(int x, int y, string text)
        {
            Vector2u pos = new(0, 0);

            foreach (char character in text)
            {
                DrawChar((int)(x + pos.X), (int)(y + pos.Y), character);
                pos.X++;
            }
        }

        void Rectangle(int x, int y, int sizex, int sizey, bool fill = false)
        {
            RectangleShape shape = new();
            shape.Position = new(x, y);
            shape.Size = new(sizex, sizey);
            if (fill)
            {
                shape.FillColor = pen;
            } else
            {
                shape.FillColor = Color.Transparent;
                shape.OutlineColor = pen;
                shape.OutlineThickness = 1;
            }

            render.Draw(shape);
        }

        void Circle(int x, int y, uint radius, bool fill = false)
        {
            CircleShape shape = new();
            shape.Position = new(x, y);
            shape.Radius = radius;

            if (fill)
            {
                shape.FillColor = pen;
            } else
            {
                shape.FillColor = Color.Transparent;
                shape.OutlineColor = pen;
                shape.OutlineThickness = 1;
            }

            render.Draw(shape);
        }

        void Horizontal(int y)
        {
            DrawLine(0, y, (int)render.Size.X, y);
        }

        void Vertical(int x)
        {
            DrawLine(x, 0, x, (int)render.Size.Y);
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

        double Time()
        {
            return time;
        }

        bool InputDown(uint input)
        {
            switch(input)
            {
                case 0:
                    return Keyboard.IsKeyPressed(Keyboard.Key.Left) || Keyboard.IsKeyPressed(Keyboard.Key.A);
                case 1:
                    return Keyboard.IsKeyPressed(Keyboard.Key.Right) || Keyboard.IsKeyPressed(Keyboard.Key.D);
                case 2:
                    return Keyboard.IsKeyPressed(Keyboard.Key.Up) || Keyboard.IsKeyPressed(Keyboard.Key.W);
                case 3:
                    return Keyboard.IsKeyPressed(Keyboard.Key.Down) || Keyboard.IsKeyPressed(Keyboard.Key.S);
            }

            return false;
        }

        Script scr = new Script();
        scr.Options.DebugPrint = s => { Console.WriteLine(s); };
        scr.DoFile("lib/rc10.lua");

        //Define in-built functions
        scr.Globals["Horizontal"] = (Action<int>)Horizontal;
        scr.Globals["Vertical"] = (Action<int>)Vertical;
        scr.Globals["Clear"] = (Action)Clear;
        scr.Globals["PenColor"] = (Action<bool>)PenColor;
        scr.Globals["Rectangle"] = (Action<int, int, int, int, bool>)Rectangle;
        scr.Globals["Circle"] = (Action<int, int, uint, bool>)Circle;
        scr.Globals["Time"] = (Func<double>)Time;
        scr.Globals["InputDown"] = (Func<uint, bool>)InputDown;
        scr.Globals["Text"] = (Action<int, int, string>)Text;
        scr.Globals["Line"] = (Action<int, int, int, int>)DrawLine;

        scr.DoFile("main.lua");

        // Set up event handler
        void Window_Closed(object sender, EventArgs e)
        {
            var window = (SFML.Window.Window)sender;
            window.Close();
        }

        window.Closed += Window_Closed;

        object updateFunc = scr.Globals["update"];

        deltaTimer.Restart();

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

            RectangleShape renderBgShape = new();
            renderBgShape.Position = new((screenWidth * scale) / 8, (screenHeight * scale) / 8);
            renderBgShape.Size = new(screenWidth * scale, screenHeight * scale);
            renderBgShape.FillColor = renderBg;

            window.Draw(renderBgShape);

            window.Draw(offsetSprite);
            window.Draw(sprite);
            window.Draw(borderSprite);

            window.Display();

            time = deltaTimer.Restart().AsSeconds();
        }
    }
}