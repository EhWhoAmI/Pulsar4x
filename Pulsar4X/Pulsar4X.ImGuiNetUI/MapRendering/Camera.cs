﻿using System;
using System.Numerics;
using ImGuiNET;
using ImGuiSDL2CS;
using Pulsar4X.ECSLib;
using SDL2;
using Point = SDL2.SDL.SDL_Point;
//using Vector2 = ImGuiNET.Vector2;

namespace Pulsar4X.SDL2UI
{


    public class Camera
    {
        internal bool IsGrabbingMap = false;
        internal int MouseFrameIncrementX;
        internal int MouseFrameIncrementY;

        internal bool IsPinnedToEntity { get; private set; }
        internal Guid PinnedEntityGuid;
        PositionDB _entityPosDB;
        internal ECSLib.Vector3 _camWorldPos_m = new ECSLib.Vector3();
        public ECSLib.Vector3 CameraWorldPosition_AU
        {
            get
            {
                if (IsPinnedToEntity && _entityPosDB != null)
                    return new ECSLib.Vector3
                    {
                        X = Distance.MToAU(_camWorldPos_m.X) + _entityPosDB.AbsolutePosition_AU.X,
                        Y = Distance.MToAU(_camWorldPos_m.Y) + _entityPosDB.AbsolutePosition_AU.Y
                    };
                else
                    return Distance.MToAU(_camWorldPos_m);
            }

        }
        
        public ECSLib.Vector3 CameraWorldPosition_m
        {
            get
            {
                if (IsPinnedToEntity && _entityPosDB != null)
                    return new ECSLib.Vector3
                    {
                        X = _camWorldPos_m.X + _entityPosDB.AbsolutePosition_m.X,
                        Y = _camWorldPos_m.Y + _entityPosDB.AbsolutePosition_m.Y
                    };
                else
                    return _camWorldPos_m;
            }
            set
            {
                if (IsPinnedToEntity)
                {
                    IsPinnedToEntity = false;
                }
                _camWorldPos_m = value;
            }
        }

        //public ImVec2 WorldPosition { get { return _cameraWorldPosition; } }

        public Vector2 ViewPortCenter { get { return new Vector2(_viewPort.Size.X * 0.5f, _viewPort.Size.Y * 0.5f); } }

        public Vector2 ViewPortSize { get { return _viewPort.Size; } }
        public float ZoomLevel { get; set; } = 200;
        public float zoomSpeed { get; set; } = 1.25f;

        public ImGuiSDL2CSWindow _viewPort;

        double MAX_ZOOMLEVEL = 1.496e+11;

        /// <summary>
        /// Construct a new Camera class within the Graphic Control Viewport. 
        /// </summary>
        public Camera(ImGuiSDL2CSWindow viewPort)
        {
            _viewPort = viewPort;
            //_viewPort.SizeChanged += _viewPort_SizeChanged;

        }

        public void PinToEntity(Entity entity)
        {
            if (entity.HasDataBlob<PositionDB>())
            {
                _entityPosDB = entity.GetDataBlob<PositionDB>();
                _camWorldPos_m = new ECSLib.Vector3(); //zero on it. 
                IsPinnedToEntity = true;
                PinnedEntityGuid = entity.Guid;
            }
        }

        public void CenterOnEntity(Entity entity)
        {
            if (entity.HasDataBlob<PositionDB>())
            {
                _camWorldPos_m = entity.GetDataBlob<PositionDB>().AbsolutePosition_m;
            }
        }

        public Point ViewCoordinate_m(ECSLib.Vector3 worldCoord_m)
        {
            int x = (int)((Distance.MToAU( worldCoord_m.X) - CameraWorldPosition_m.X) * ZoomLevel + ViewPortCenter.X);
            int y = -(int)((Distance.MToAU(worldCoord_m.Y) - CameraWorldPosition_m.Y) * ZoomLevel - ViewPortCenter.Y);
            Point viewCoord = new Point() { x = x, y = y };

            return viewCoord;
        }
        
        public Point ViewCoordinate_AU(ECSLib.Vector3 worldCoord_AU)
        {
            int x = (int)((worldCoord_AU.X - CameraWorldPosition_AU.X) * ZoomLevel + ViewPortCenter.X);
            int y = -(int)((worldCoord_AU.Y - CameraWorldPosition_AU.Y) * ZoomLevel - ViewPortCenter.Y);
            Point viewCoord = new Point() { x = x, y = y };

            return viewCoord;
        }
        
        
        public ECSLib.Vector3 MouseWorldCoordinate_AU()
        {
            Vector2 mouseCoord = ImGui.GetMousePos();
            double x = ((mouseCoord.X - ViewPortCenter.X) / ZoomLevel) + CameraWorldPosition_AU.X;
            double y = -(((mouseCoord.Y - ViewPortCenter.Y) / ZoomLevel) - CameraWorldPosition_AU.Y);
            return new ECSLib.Vector3(x, y, 0);

        }
        public ECSLib.Vector3 MouseWorldCoordinate_m()
        {
            //Vector2 mouseCoord = ImGui.GetMousePos();
            //double x = ((mouseCoord.X - ViewPortCenter.X) / (ZoomLevel / GameConstants.Units.MetersPerAu) + CameraWorldPosition_m.X);
            //double y = -(((mouseCoord.Y - ViewPortCenter.Y) / (ZoomLevel/ GameConstants.Units.MetersPerAu) - CameraWorldPosition_m.Y));

            var mwcau = MouseWorldCoordinate_AU();
            double x = Distance.AuToMt(mwcau.X);
            double y = Distance.AuToMt(mwcau.Y);
            
            return new ECSLib.Vector3(x, y, 0);

        }
        
        /// <summary>
        /// returns the worldCoordinate of a given View Coordinate 
        /// </summary>
        /// <param name="viewCoordinate"></param>
        /// <returns></returns>
        public ECSLib.Vector3 WorldCoordinate_m(int viewCoordinateX, int viewCoordinateY)
        {
            double x = ((viewCoordinateX - ViewPortCenter.X) / ZoomLevel * GameConstants.Units.MetersPerAu) + CameraWorldPosition_m.X;
            double y = -(((viewCoordinateY - ViewPortCenter.Y) / ZoomLevel * GameConstants.Units.MetersPerAu) - CameraWorldPosition_m.Y);
            return new ECSLib.Vector3(x, y, 0);
        }

        /// <summary>
        /// Returns the size of an object in view-Coordinates
        /// </summary>
        /// <param name="worldSize"></param>
        /// <returns></returns>
        public Vector2 ViewSize(Vector2 worldSize)
        {
            Vector2 viewSize = new Vector2(worldSize.X * ZoomLevel, worldSize.Y * ZoomLevel);
            return viewSize;
        }

        /// <summary>
        /// Returns the Distance in view-Coordinates
        /// </summary>
        /// <param name="dist"></param>
        /// <returns></returns>
        public float ViewDistance(double dist)
        {
            return (float)(dist * ZoomLevel);
        }

        /// <summary>
        /// Returns the Distance in World-Coordinates
        /// </summary>
        /// <param name="dist"></param>
        /// <returns></returns>
        public double WorldDistance(float dist)
        {
            return dist / ZoomLevel;
        }

        /// <summary>
        /// Returns the size of an object in world-Coordinates
        /// </summary>
        /// <param name="viewSize"></param>
        /// <returns></returns>
        public Vector2 WorldSize(Vector2 viewSize)
        {
            return new Vector2(viewSize.X / ZoomLevel, viewSize.Y / ZoomLevel);
        }


        /// <summary>
        /// Offset the position of the camare i.e. Pan in world units.
        /// <param name="xOffset">Pans the camera horizontaly relative to offset</param>
        /// <param name="yOffset">Pans the camera verticaly relative to offset</param>
        /// </summary>
        public void WorldOffset_m(double xOffset, double yOffset)
        {
            
            _camWorldPos_m.X += (float)(xOffset * GameConstants.Units.MetersPerAu / ZoomLevel);
            _camWorldPos_m.Y += (float)(-yOffset * GameConstants.Units.MetersPerAu / ZoomLevel);
        }


        /// <summary>
        /// Zoom in and keep try to keep the given pixel under the mouse.
        /// </summary>
        /// <param name="zoomCoords">The coordinates of the panel to zoom in</param>
        public void ZoomIn(int mouseX, int mouseY)
        {
            var worldCoord = WorldCoordinate_m(mouseX, mouseY);
            if (ZoomLevel < MAX_ZOOMLEVEL)
            {
                ZoomLevel *= zoomSpeed;
                double xOffset = mouseX - ViewPortCenter.X - (mouseX - ViewPortCenter.X) * zoomSpeed;
                double yOffset = mouseY - ViewPortCenter.Y - (mouseY - ViewPortCenter.Y) * zoomSpeed;
                WorldOffset_m(-xOffset, -yOffset);
            }
        }


        /// <summary>
        /// Zoom out and keep try to keep the given pixel under the mouse.
        /// </summary>
        /// <param name="zoomCoords">The coordinates of the panel to soom out from</param>
        public void ZoomOut(int mouseX, int mouseY)
        {
            var worldCoord = WorldCoordinate_m(mouseX, mouseY);

            if (ZoomLevel > 0)
            {
                ZoomLevel /= zoomSpeed;
                double xOffset = mouseX - ViewPortCenter.X - (mouseX - ViewPortCenter.X) / zoomSpeed;
                double yOffset = mouseY - ViewPortCenter.Y - (mouseY - ViewPortCenter.Y) / zoomSpeed;
                WorldOffset_m(-xOffset, -yOffset);
            }
        }

        /// <summary>
        /// returns a matrix scaled to zoom. is not translated to camera position. 
        /// </summary>
        /// <returns>The zoom matrix.</returns>
        public Matrix GetZoomMatrix()
        {
            var mirrorMatrix = Matrix.NewMirrorMatrix(true, false);
            var scaleMtx = Matrix.NewScaleMatrix(ZoomLevel, ZoomLevel);
            return mirrorMatrix * scaleMtx;
        }

        public Matrix GetPanMatrix()
        {
            int x = (int)((0 - CameraWorldPosition_AU.X) * ZoomLevel + ViewPortCenter.X);
            int y = -(int)((0 - CameraWorldPosition_AU.Y) * ZoomLevel - ViewPortCenter.Y);
            return Matrix.NewTranslateMatrix(x, y);
        }


    }

    /// <summary>
    /// Cursor crosshair.
    /// Primarily made to debug a problem with getting the world coordinate of the mouse cursor. 
    /// </summary>
    class CursorCrosshair : Icon
    {
        public CursorCrosshair(ECSLib.Vector3 positionM) : base(positionM)
        {
            var colour = new SDL.SDL_Color() { r = 0, g = 255, b = 0, a = 255 };

            PointD point0 = new PointD() { X = -5, Y = 0 };
            PointD point1 = new PointD() { X = +5, Y = 0 };
            Shape shape0 = new Shape() { Points = new PointD[2] { point0, point1 }, Color = colour };

            PointD point2 = new PointD() { X = 0, Y = -5 };
            PointD point3 = new PointD() { X = 0, Y = +5 };
            Shape shape1 = new Shape() { Points = new PointD[2] { point2, point3 }, Color = colour };

            this.Shapes = new System.Collections.Generic.List<Shape>() { shape0, shape1 };
        }

        public override void OnFrameUpdate(Matrix matrix, Camera camera)
        {
            WorldPosition_m = camera.MouseWorldCoordinate_m();
            base.OnFrameUpdate(matrix, camera);
        }

    }
}
