﻿using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.Parsers;
using PlatformEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NFSEngine;
using StillDesign.PhysX;
using Carmageddon.HUD;
using System.Diagnostics;

namespace Carmageddon.CameraViews
{
    class CockpitView : BaseHUDItem, ICameraView
    {
        CockpitFile _cockpitFile;
        //FPSCamera _camera;
        SimpleCamera _camera;
        ActFile _actorFile;
        DatFile _modelsFile;
        VehicleModel _vehicle;

        public CockpitView(VehicleModel vehicle, string cockpitFile)
        {
            _vehicle = vehicle;
            _cockpitFile = new CockpitFile(cockpitFile);
            _camera = new SimpleCamera();
            _camera.FieldOfView = MathHelper.ToRadians(55.55f);
            _camera.AspectRatio = Engine.AspectRatio;

            _modelsFile = new DatFile(GameVariables.BasePath + "data\\models\\" + vehicle.Config.BonnetModelFile);
            _actorFile = new ActFile(GameVariables.BasePath + "data\\actors\\" + vehicle.Config.BonnetActorFile, _modelsFile);

            _actorFile.ResolveHierarchy(false, null);
            _actorFile.ResolveMaterials();
            _modelsFile.Resolve();

            //move head back
            _vehicle.Config.DriverHeadPosition.Z += 0.11f;

            foreach (var x in _cockpitFile.LeftHands)
            {
                x.Position1 += new Vector2(-20, 0);
                x.Position2 += new Vector2(-20, 0);
                x.Position1 /= new Vector2(640, 480);
                x.Position2 /= new Vector2(640, 480);
            }
            foreach (var x in _cockpitFile.RightHands)
            {
                x.Position1 += new Vector2(-20, 0);
                x.Position2 += new Vector2(-20, 0);
                x.Position1 /= new Vector2(640, 480);
                x.Position2 /= new Vector2(640, 480);
            }
            _cockpitFile.CenterHands.Position1 += new Vector2(-20, 0);
            _cockpitFile.CenterHands.Position2 += new Vector2(-20, 0);
            _cockpitFile.CenterHands.Position1 /= new Vector2(640, 480);
            _cockpitFile.CenterHands.Position2 /= new Vector2(640, 480);

        }

        #region ICameraView Members

        public bool Selectable
        { 
            get { return true; }
        }

        public override void Update()
        {
            Matrix m = Matrix.CreateRotationX(-0.08f) * _vehicle.Chassis.Body.GlobalOrientation;
            Vector3 forward = m.Forward;
            _camera.Orientation = forward;

            _camera.Up = m.Up;
            Vector3 vehicleBottom = new Vector3(_vehicle.Chassis.Body.GlobalPosition.X, -53.4348f, _vehicle.Chassis.Body.GlobalPosition.Z);
            vehicleBottom = GetBodyBottom();
            
            _camera.Position = vehicleBottom + Vector3.Transform(_vehicle.Config.DriverHeadPosition, _vehicle.Chassis.Body.GlobalOrientation) + new Vector3(0, 0.018f, 0);
        }

        public override void Render()
        {
            Rectangle src = new Rectangle(32, 20, 640, 480);
            Rectangle rect = new Rectangle(0, 0, 800, 600);
            Engine.SpriteBatch.Draw(_cockpitFile.Forward, rect, src, Color.White);

            float steerRatio = _vehicle.Chassis.SteerRatio;
            
            CockpitHandFrame frame = null;
            if (steerRatio < -0.2)
            {
                if (steerRatio < -0.8f)
                {
                    int hands = Math.Min(2, _cockpitFile.RightHands.Count - 1);
                    frame = _cockpitFile.RightHands[hands];
                }
                else if (steerRatio < -0.5f)
                    frame = _cockpitFile.RightHands[1];
                else if (steerRatio < -0.2f)
                    frame = _cockpitFile.RightHands[0];
                
            }
            else if (steerRatio > 0.2f)
            {
                if (steerRatio > 0.8f)
                {
                    int hands = Math.Min(2, _cockpitFile.LeftHands.Count - 1);
                    frame = _cockpitFile.LeftHands[hands];
                }
                else if (steerRatio > 0.5f)
                    frame = _cockpitFile.LeftHands[1];
                else if (steerRatio > 0.2)
                    frame = _cockpitFile.LeftHands[0];
            }
            else
            {
                frame = _cockpitFile.CenterHands;
            }

            if (frame.Texture1 != null)
                Engine.SpriteBatch.Draw(frame.Texture1, ScaleVec2(frame.Position1), Color.White);
            if (frame.Texture2 != null)
                Engine.SpriteBatch.Draw(frame.Texture2, ScaleVec2(frame.Position2), Color.White);
            
            Vector3 vehicleBottom = new Vector3(_vehicle.Chassis.Body.GlobalPosition.X, -53.4348f, _vehicle.Chassis.Body.GlobalPosition.Z);
            vehicleBottom = GetBodyBottom();
            
            _modelsFile.SetupRender();
            _actorFile.Render(_modelsFile, Matrix.CreateFromQuaternion(_vehicle.Chassis.Body.GlobalOrientationQuat) * Matrix.CreateTranslation(vehicleBottom));
        }

        public void Activate()
        {
            Engine.Camera = _camera;

            Vector3 vehicleBottom = GetBodyBottom();
            
            _camera.Position = vehicleBottom + Vector3.Transform(_vehicle.Config.DriverHeadPosition, _vehicle.Chassis.Body.GlobalOrientation);
            _camera.Update(); 
        }

        public void Deactivate()
        {
            
        }

        #endregion

        private Vector3 GetBodyBottom()
        {
            Vector3 pos = _vehicle.Chassis.Body.GlobalPosition;
            pos.Y = pos.Y - _vehicle.Config.WheelActors[0].Position.Y - _vehicle.Config.DrivenWheelRadius;
            return pos;
        }
    }
}