﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs;
using Microsoft.Maui.Controls.Shapes;
using Objects;
using Objects.Enums;
using Point = Objects.Point;

namespace Objects
{
    public class GameObject : ICollidable
    {
        public event EventHandler<EventArgs> UpdateGameObject;
        public event EventHandler<EventArgs> Disposed;
        public event EventHandler<int> PlayerGotHit;
        public bool Turn { get; set; }
        public Point PointOnScreen { get; set; }
        public bool IsVisable { get; set; } = true;
        public int Rotatation { get; set; } = 0;
        public int ScaleX { get; set; } = 1;
        private Point m_StartupPoint;
        public bool IsCollisionDetectionEnabled { get; set; }
        public int ScaleY { get; set; } = 1;
        public string ImageSource { get; set; }
        public int ObjectNumber { get; set; }
        public eScreenObjectType ScreenObjectType { get; set; }
        public int GameBoardGridSize { get; set; } = GameSettings.GameGridSize;
        protected GameInformation m_GameInformation = GameInformation.Instance;
        private Point m_ValuesToAdd;
        public bool IsObjectMoving { get; set; } = true;
        public Direction Direction { get; set; } = Direction.Stop;
        public Direction RequestedDirection { get; set; } = Direction.Stop;
        public eButton ButtonType { get; set; }
        public string Text { get; set; }
        public SizeDTO m_Size = GameSettings.m_MovementButtonOurSize;
        public int ID { get; set; }
        public int Velocity { get; set; } = 105;
        public bool Fade { get; set; } = false;
        protected int[,] m_Board;
        protected bool m_WantToTurn = false;

        public void Initialize(eScreenObjectType i_ScreenObjectType, int i_ObjectNumber, string i_Png, Point i_Point, bool i_IsGrid, Point i_ValuesToAdd)
        {
            ObjectNumber = i_ObjectNumber;
            ScreenObjectType = i_ScreenObjectType;
            m_ValuesToAdd = i_ValuesToAdd;
            Point point = getScreenPoint(i_Point, i_IsGrid);
            PointOnScreen =m_StartupPoint= point;
            ImageSource=i_Png;
            ID=GameSettings.getID();
        }

        public void SetImageDirection(Direction i_Direction)
        {
            if (i_Direction == Direction.Up)
            {
                Rotatation = 270;
            }
            else if (i_Direction == Direction.Down)
            {
                Rotatation = 90;
            }
            else if (i_Direction == Direction.Left)
            {
                Rotatation = 180;
            }
            else
            {
                Rotatation = 0;
            }
        }

        public void FlipImage(int i_Index, eImageScale i_Scale)
        {
            if (i_Scale == eImageScale.FlipX)
            {
                ScaleX = -1;
            }
            else if (i_Scale == eImageScale.FlipY)
            {
                ScaleY = -1;
            }
            if (i_Scale == eImageScale.OriginalX)
            {
                ScaleX = 1;
            }
            else
            {
                ScaleY= 1;
            }
        }

        public void InitializeButton(eButton i_ButtonType, string i_Png, Point i_Point, bool i_IsGrided, SizeDTO i_Size, Point i_ValuesToAdd)
        {
            ButtonType = i_ButtonType;
            ScreenObjectType = eScreenObjectType.Button;
            m_ValuesToAdd = i_ValuesToAdd;
            Point point = getScreenPoint(i_Point,i_IsGrided);
            PointOnScreen=point;
            ImageSource=i_Png;
            ID=GameSettings.getID();
            m_Size = i_Size;
        }

        public void Draw()
        {
            UpdateGameObject.Invoke(this, null);
        }

        public void RequestDirection(Direction i_Direction)
        {
            if(IsObjectMoving)
            {
                if (Direction == Direction.Stop)
                {
                    Direction = i_Direction;
                }
                else if (Direction != i_Direction)
                {
                    RequestedDirection = i_Direction;
                    if (checkIfCanChangeDirection(i_Direction))
                    {
                        checkIfWantToTurn(i_Direction);
                        Direction = i_Direction;
                    }
                }
            }
        }

        protected void OnGotHit()
        {
            PlayerGotHit.Invoke(this,ObjectNumber);
        }

        void checkIfWantToTurn(Direction i_Direction)
        {
            int x = Direction.ColumnOffset + i_Direction.ColumnOffset;
            int y = Direction.RowOffset + i_Direction.RowOffset;

            if(x != 0 && y != 0)
            {
                m_WantToTurn = true;
            }
        }

        protected void resetToStartupPoint()
        {
            IsObjectMoving = false;
            PointOnScreen = m_StartupPoint;
            Direction= RequestedDirection = Direction.Stop;
        }

        protected virtual void OnDisposed()
        {
            IsCollisionDetectionEnabled = false;
            Disposed.Invoke(this,null);
            IsVisable = false;
            UpdateGameObject.Invoke(this,null);
        }

        bool checkIfCanChangeDirection(Direction i_Direction)
        {
            bool canChange = false;
            Point point = getPointOnGrid();

            if(point.Row + i_Direction.RowOffset >= 0 && point.Column + i_Direction.ColumnOffset >= 0)
            {
                if (m_Board[(int)point.Column+i_Direction.ColumnOffset, (int)point.Row + i_Direction.RowOffset] != 1)
                {
                    canChange = true;
                }
            }

            return canChange;
        }

        Point getPointOnGrid()
        {
            double temp;
            Point point = GetCurrentPointOnScreen();

            temp = point.Row / GameSettings.GameGridSize;
            point.Row = (int)Math.Round(temp);
            temp = point.Column / GameSettings.GameGridSize;
            point.Column = (int)Math.Round(temp);

            return point;
        }

        protected void isPointOnBoard(ref Point i_Point)
        {
            bool collided = true;
            if(i_Point.Column < m_ValuesToAdd.Column)
            {
                i_Point.Column = m_ValuesToAdd.Column;
            }
            else if(i_Point.Row < m_ValuesToAdd.Row)
            {
                i_Point.Row  = m_ValuesToAdd.Row;
            }
            else if(i_Point.Row > m_GameInformation.GameBoardSizeByPixel.Height + m_ValuesToAdd.Row - m_Size.Width)
            {
                i_Point.Row = m_GameInformation.GameBoardSizeByPixel.Height + m_ValuesToAdd.Row - m_Size.Width;
            }
            else if(i_Point.Column > m_GameInformation.GameBoardSizeByPixel.Width + m_ValuesToAdd.Column - m_Size.Height)
            {
                i_Point.Column = m_GameInformation.GameBoardSizeByPixel.Width + m_ValuesToAdd.Column - m_Size.Height;
            }
            else
            {
                collided = false;
            }

            if(collided)
            {
                Turn = true;
                Direction = RequestedDirection;
            }
        }

        protected void collidedWithSolid(ICollidable i_Solid)//(Point i_PointOfSolid,SizeDTO i_SizeOfSolid)
        {
            Point newPoint = PointOnScreen;
            if (Direction == Direction.Left)//solid on the left
            {
                newPoint.Column = i_Solid.PointOnScreen.Column + i_Solid.Bounds.Width; 
            }
            else if (Direction == Direction.Right) //solid on the right
            {
                newPoint.Column = i_Solid.PointOnScreen.Column - m_Size.Width;
            }
            else if(Direction == Direction.Up)
            {
                newPoint.Row = i_Solid.PointOnScreen.Row + i_Solid.Bounds.Height;
            }
            else //direction is down
            {
                newPoint.Row = i_Solid.PointOnScreen.Row - m_Size.Height;
            }
            PointOnScreen = newPoint;
        }

        protected Point getScreenPoint(Point i_Point,bool i_IsGrided)
        {
            Point point = new Point();
            
            if(!i_IsGrided)
            {
                GameBoardGridSize = 1;
            }

            point = i_Point;
            point.Column = point.Column * GameBoardGridSize + m_ValuesToAdd.Column;
            point.Row = point.Row * GameBoardGridSize + m_ValuesToAdd.Row;

            return point;
        }

        public virtual void Update(double i_TimeElapsed)
        {
            updatePosition(i_TimeElapsed);
        }

        public virtual bool CheckCollision(ICollidable i_Source)
        {
            bool collided = false;
            if(i_Source != null)
            {
                collided = i_Source.Bounds.IntersectsWith(this.Bounds);
            }

            return collided;
        }

        public virtual void Collided(ICollidable i_Collidable)
        {

        }

        protected void centerObjectInGrid()
        {
            Point newPoint = PointOnScreen;
            double valueToAdd = (GameSettings.GameGridSize / 2) - m_Size.Height / 2;

            newPoint.Row += valueToAdd;
            newPoint.Column += valueToAdd;
            PointOnScreen = newPoint;
        }

        private void updatePosition(double i_TimeElapsed)
        {
            if(m_WantToTurn)
            {
                PointOnScreen= getScreenPoint(getPointOnGrid(), true);
                m_WantToTurn = false;
            }
            else
            {
                Point newPoint = PointOnScreen;
                newPoint.Column += ((Direction.ColumnOffset * Velocity) * i_TimeElapsed / 1000);
                newPoint.Row += ((Direction.RowOffset * Velocity) * i_TimeElapsed / 1000);

                isPointOnBoard(ref newPoint);
                PointOnScreen = newPoint;
            }
        }

        public Point GetCurrentPointOnScreen()//without add values
        {
            Point value = PointOnScreen;

            value.Row -= m_ValuesToAdd.Row;
            value.Column -= m_ValuesToAdd.Column;
          
            return value;
        }

        public void UpdatePointOnScreen(Point i_Point)
        {
            i_Point.Row += m_ValuesToAdd.Row;
            i_Point.Column += m_ValuesToAdd.Column;
            PointOnScreen = i_Point;
            //updatePosition(85);
        }

        public Rect Bounds
        {
            get
            {
                return new Rect(PointOnScreen.Column, PointOnScreen.Row,
                m_Size.Width,m_Size.Height);
            }
        }
    

        public void MoveToPointInGrided(Point i_Point)
        {
            Point point = getScreenPoint(i_Point,true);
            PointOnScreen = point;
        }
        public void FadeWhenObjectIsRemoved()
        {
            Fade = true;
        }
    }
}