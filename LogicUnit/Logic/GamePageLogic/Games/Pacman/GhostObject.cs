﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Objects;
using Objects.Enums;
using Point = Objects.Point;

namespace LogicUnit.Logic.GamePageLogic.Games.Pacman
{
    public class GhostObject : GameObject, IPacmanGamePlayer
    {
        public event EventHandler<int> PlayerGotHit ;
        public int AmountOfLives { get; set; } = 2;
        private bool m_IsDyingAnimationOn = false;
        private bool m_IsCherryTime = false;
        public double m_CherryTimeStart;
        public double m_DeathAnimationStart;
        private int m_Blink;
        public bool IsHunting { get; set; } = true;

        public GhostObject(int i_playerNumber, int i_X, int i_Y, int[,] i_Board)
        {
            m_CanRotateToAllDirections = false;
            m_FlipsWhenMoved = true;
            IsCollisionDetectionEnabled = true;
            m_Board = i_Board;
            this.Initialize(eScreenObjectType.Player, i_playerNumber, "pacman_ghost_"+i_playerNumber+".png", getPointOnGrid(i_X,  i_Y), true,
                m_GameInformation.PointValuesToAddToScreen);
        }

        Point getPointOnGrid(int i_X, int i_Y)
        {
            Point point = new Point(0, i_Y-1);

            return point;
        }

        public override void Update(double i_TimeElapsed)
        {
            if(m_IsDyingAnimationOn)
            {
                double timePassed = m_GameInformation.RealWorldStopwatch.Elapsed.TotalMilliseconds - m_DeathAnimationStart;

                if(timePassed < 1600)
                {
                    IsVisable = !IsVisable;
                }
                else
                {
                    IsVisable = true;
                    m_IsDyingAnimationOn = false;
                    IsObjectMoving = true;
                }
            }

            if(m_IsCherryTime)
            {
                double timePassed = m_GameInformation.RealWorldStopwatch.Elapsed.TotalMilliseconds - m_CherryTimeStart;

                if (timePassed > 4500 && timePassed <7000)
                {
                    if(m_Blink % 5 == 0)
                    {
                        if(ImageSource == "pacman_ghost_2c.png")
                        {
                            ImageSource = "pacman_ghost_2.png";
                        }
                        else
                        {
                            ImageSource = "pacman_ghost_2c.png";
                        }
                    }
                    m_Blink++;
                }
                else if(timePassed>7000)
                {
                    ImageSource = "pacman_ghost_2.png";
                    m_IsCherryTime = false;
                    IsHunting = true;
                }
            }
            
            base.Update(i_TimeElapsed);
        }

        public override void Collided(ICollidable i_Collidable)
        {
            if (i_Collidable is PacmanObject)
            {
                if(IsHunting) //Ate pacman
                {
                    //Maybe gets points but in general, we wait for pacman to reset pos
                }
                else//got eaten
                {
                    ImageSource = "pacman_ghost_2.png";
                    m_IsCherryTime = false;
                    IsVisable = false;
                    IsHunting = true;
                    m_IsDyingAnimationOn = true;
                    IsObjectMoving = false;
                    AmountOfLives--;
                    if(AmountOfLives == 0 && IsCollisionDetectionEnabled)
                    {
                        m_IsDyingAnimationOn = false;
                        OnDisposed();
                    }
                    OnGotHit();
                }
            }
            else if(i_Collidable is Boarder)
            {
                collidedWithSolid(i_Collidable);
            }
        }
        public void ResetPosition(double i_DeathStartTime)
        {
            m_IsDyingAnimationOn = true;
            m_DeathAnimationStart = i_DeathStartTime;
            resetToStartupPoint();
        }

        public void InitiateCherryTime(double i_BerryStartTime)
        {
            IsHunting = false;
            ImageSource = "pacman_ghost_2c.png";
            m_IsCherryTime = true;
            m_CherryTimeStart= i_BerryStartTime;
        }
    }
}
