using FireCast.Client.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FireCast.Client
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private readonly INetworkReceiver _receiver;
        Queue<Frame> framesQuery = new Queue<Frame>();
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
            _receiver.OnFramesComposed += (sender, args) =>
            {
                foreach(var  frame in args)
                {
                    framesQuery.Enqueue(frame);
                }
            };
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

            if (framesQuery.Count > 0)
            {
                var renderFrame = this.framesQuery.Dequeue();

                var texture = CreateTexture(renderFrame);

                _spriteBatch.Begin();

                _spriteBatch.Draw(texture,
                    new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight),
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
            using (MemoryStream memoryStream = new MemoryStream(image))
            {
                return Texture2D.FromStream(this.GraphicsDevice, memoryStream);
            }
        }
    }
}
