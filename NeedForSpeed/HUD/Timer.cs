﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;
using Microsoft.Xna.Framework;

namespace Carmageddon.HUD
{
    class Timer : BaseHUDItem
    {
        SpriteFont _font;
        int x, y;
        public Timer()
        {
            _font = Engine.Instance.ContentManager.Load<SpriteFont>("content/timer-font");
        }

        public override void Update()
        {
        }

        public override void Render()
        {
            Rectangle rect = CenterRectX(0, 0.182f, 0.075f);
            Engine.Instance.SpriteBatch.Draw(_shadow, rect, Color.White);

            TimeSpan ts = TimeSpan.FromSeconds(Race.Current.RaceTime.TimeRemaining);
            float nudge = ts.Minutes < 10 ? 13 * FontScale : 0;
            Engine.Instance.SpriteBatch.DrawString(_font,
                String.Format("{0}:{1}", (int)ts.Minutes, ts.Seconds.ToString("00")), new Vector2(rect.X + 5 + nudge, rect.Y + 3), Color.White, 0, Vector2.Zero, FontScale, SpriteEffects.None, 0);
        }
    }
}