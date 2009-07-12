﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.Parsers;

namespace Carmageddon
{
    class Race
    {

        DatFile _models;
        ActFile _actors;
        ResourceCache _resourceCache;

        public Race(string filename)
        {
            RaceFile race = new RaceFile(filename);

            _resourceCache = new ResourceCache();
            
            foreach (string pixFileName in race.PixFiles)
            {
                PixFile pixFile = new PixFile(@"C:\Games\carma1\data\pixelmap\" + pixFileName);
                _resourceCache.Add(pixFile);
            }

            foreach (string matFileName in race.MaterialFiles)
            {
                MatFile matFile = new MatFile(@"C:\Games\carma1\data\material\" + matFileName);
                _resourceCache.Add(matFile);
            }
            
            _models = new DatFile(@"C:\Games\carma1\data\models\" + race.ModelFile);

            _actors = new ActFile(@"C:\Games\carma1\data\actors\" + race.ActorFile);
            _actors.ResolveMaterials(_resourceCache);
            _models.Resolve(_resourceCache);
        }

        public void Update(GameTime gameTime)
        {
        }

        public void Render()
        {
            _actors.Render(Matrix.CreateScale(30), _models);
        }
    }
}