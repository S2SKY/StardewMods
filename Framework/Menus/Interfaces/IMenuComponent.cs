﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Entoarox.Framework.Menus
{
    public interface IMenuComponent
    {
        void Update(GameTime t, IComponentCollection collection, FrameworkMenu menu);
        void Draw(SpriteBatch b, Point offset);
        void Attach(IComponentCollection collection);
        void Detach(IComponentCollection collection);
        Point GetPosition();
        Rectangle GetRegion();
        bool Visible { get; set; }
    }
}