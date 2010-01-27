﻿using System;
using System.Collections.Generic;

using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework;
using StillDesign.PhysX;
using NFSEngine;
using Particle3DSample;
using Carmageddon.Gfx;

namespace Carmageddon.Physics
{
    class VehicleWheel
    {
        public WheelShape Shape { get; private set; }
        public CWheelActor CActor { get; private set; }
        private Matrix _renderMatrix = Matrix.Identity;
        private Matrix _rotationMatrix = Matrix.Identity;
        private VehicleChassis _chassis;
        public bool IsRear;
        private float _axleOffset;
        private bool _handbrakeOn;
        ParticleEmitter _smokeEmitter;
        public bool IsSkiddingLat, IsSkiddingLng;
        public Vector3 ContactPoint;
        public int MaterialIndex;
        public int Index;

        public bool InAir
        {
            get { return Shape.GetContactData().ContactForce == 0; }
        }

        public VehicleWheel(VehicleChassis chassis, CWheelActor cactor, WheelShape wheel, float axleOffset)
        {
            Shape = wheel;
            _chassis = chassis;
            _axleOffset = axleOffset;
            CActor = cactor;

            if (_smokeEmitter == null)
            {
                if (GameVariables.TyreSmokeSystem == null) GameVariables.TyreSmokeSystem = new TyreSmokeParticleSystem();
                _smokeEmitter = new ParticleEmitter(GameVariables.TyreSmokeSystem, 15, Vector3.Zero);
            }

            IsRear = !CActor.IsFront;
        }

        public Vector3 GlobalPosition
        {
            get { return Shape.GlobalPosition; }
        }

        public void Update()
        {
            WheelContactData wcd = Shape.GetContactData();

            UpdateMatrices(wcd);

            ContactPoint = wcd.ContactPoint;
            
            if (wcd.ContactForce != 0)
            {
                CMaterialModifier materialModifier = Race.Current.RaceFile.MaterialModifiers[(int)wcd.OtherShapeMaterialIndex];
                materialModifier.UpdateWheelShape(_chassis, this);
            }

            if (_chassis.Speed > 10 && (_handbrakeOn || Math.Abs(wcd.LateralSlip) > 0.23f))
            {
                _smokeEmitter.Enabled = true;
                IsSkiddingLat = true;
            }
            else if (_chassis.Speed > 3 && CActor.IsDriven && wcd.LongitudalSlip > 0.04f)
            {
                _smokeEmitter.Enabled = true;
                IsSkiddingLng = true;
            }
            else
            {
                IsSkiddingLat = IsSkiddingLng = false;
                _smokeEmitter.Enabled = false;
            }

            _smokeEmitter.Update(wcd.ContactPoint);
        }

        public void ApplyHandbrake(bool apply)
        {
            _handbrakeOn = apply;
            if (apply)
            {
                Shape.MotorTorque = 0;
                Shape.BrakeTorque = 800;
            }
        }


        private void UpdateMatrices(WheelContactData wcd)
        {
            float suspensionLength = 0;

            if (wcd.ContactShape == null)
                suspensionLength = Shape.SuspensionTravel;
            else
                suspensionLength = wcd.ContactPosition - Shape.Radius;

            if (!_handbrakeOn) _rotationMatrix *= Matrix.CreateRotationX(MathHelper.ToRadians(Shape.AxleSpeed));
            
            Matrix translation = Matrix.CreateTranslation(_axleOffset, -suspensionLength, 0.0f);
            _renderMatrix = _rotationMatrix * Matrix.CreateRotationY(Shape.SteeringAngle) * translation;
        }

        public Matrix GetRenderMatrix()
        {
            Matrix pose = Shape.GlobalPose;
            Vector3 trans = pose.Translation;
            return _renderMatrix * pose;
        }
    }
}