using FireCast.Client.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FireCast.Client
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private readonly INetworkReceiver _receiver;
        private Texture2D activeTexture;
        private readonly Queue<Texture2D> textureQueue;
        private FrameBuffer FrameBuffer;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            FrameBuffer = new FrameBuffer();
            _receiver = new UdpReceiver("127.0.0.1", 1488);
            textureQueue = new Queue<Texture2D>();
        }

        protected override void Initialize()
        {
            new Task(async () =>
            {
                while (true)
                {
                    var bytes = await _receiver.ReceiveNextPackage();
                    FrameBuffer.AddToBufferQueue(bytes);
                }
            }).Start();
            new Task(() =>
            {
                while (true)
                {
                    var frame = FrameBuffer.ProcessBuffer();
                    if(frame != null)
                    {
                        var texture = CreateTexture(frame);
                        this.textureQueue.Enqueue(texture);
                    }
                }
            }).Start();
            this.Window.AllowUserResizing = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            this.Window.Title = $"{FrameBuffer.GetBufferLength()} {textureQueue.Count}";

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (textureQueue.Count > 0)
            {
                activeTexture = textureQueue.Dequeue();
            }

            if (activeTexture != null)
            {
                

                _spriteBatch.Begin();

                _spriteBatch.Draw(activeTexture,
                    new Rectangle(0, 0, _graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height),
                    new Rectangle(0, 0, activeTexture.Width, activeTexture.Height),
                    Color.White);

                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        private Texture2D CreateTexture(Frame frame)
        {
            var chunks = frame.PackageChunks.OrderBy(x => x.Order);
            byte[] image = new byte[chunks.Sum(x => x.Data.Length)];
            int lastIndex = 0;
            foreach(var chunk in chunks)
            {
                Array.Copy(chunk.Data, 0, image, lastIndex, chunk.Data.Length);
                lastIndex += chunk.Data.Length;
            }
            using (MemoryStream ms = new MemoryStream(image))
            {
                return Texture2D.FromStream(this.GraphicsDevice, ms);
            }
        }
    }
}
