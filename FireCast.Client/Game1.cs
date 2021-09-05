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
        private Frame activeFrame;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _receiver = new UdpReceiver("127.0.0.1", 1488);
        }

        protected override void Initialize()
        {
            new Task(async () => await _receiver.StartReceiverAsync()).Start();
            _receiver.OnFrameComposed += (sender, args) =>
            {
                activeFrame = args;
            };
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

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (activeFrame != null)
            {
                var texture = CreateTexture(activeFrame);

                _spriteBatch.Begin();

                _spriteBatch.Draw(texture,
                    new Rectangle(0, 0, _graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height),
                    new Rectangle(0, 0, texture.Width, texture.Height),
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
            Bitmap bitmap;
            MemoryStream memoryStream = new MemoryStream(image);
            try
            {
                bitmap = (Bitmap)Bitmap.FromStream(memoryStream);
                memoryStream = new MemoryStream();
                bitmap.Save(memoryStream, ImageFormat.Png);
                return Texture2D.FromStream(_graphics.GraphicsDevice, memoryStream);
            }
            finally
            {
                memoryStream.Dispose();
            }
        }
    }
}
