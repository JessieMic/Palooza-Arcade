﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Objects.Enums;

namespace Objects
{
    public class GameInformation
    {
        public bool isInitialized = false;
        private static GameInformation m_Instance = null;
        public eGames m_NameOfGame;
        private int m_AmountOfPlayers;
        public Player Player { get; set; }
        public ScreenDimension m_ClientScreenDimension = new ScreenDimension();
        private List<ScreenDimension> m_ScreenInfoOfAllPlayers = new List<ScreenDimension>();
        public Point PointValuesToAddToScreen { get; set; } = new Point();
        public string[] m_NamesOfAllPlayers;
        public SizeD GameBoardSizeByPixel { get; set; }
        public SizeD GameBoardSizeByGrid { get; set; }
        private static readonly object s_InstanceLock = new object();
        public Rect BackgroundRect { get; set; }
        public double ImageDensity { get; set; }
        public double ImageXValues { get;  set; }
        public double ScreenDensity { get; set; } = 1;
        public bool ServerReset { get; set; } = false;
        public Stopwatch RealWorldStopwatch { get; set; }
        public int Counter { get; set; } = 0;

        public void Reset()
        {
            Player.IsInitialized = false;
            Player.DidPlayerPickAPlacement = false;
            Player.PlayerNumber = 0;
            m_AmountOfPlayers = 0;
            PointValuesToAddToScreen = new Point();
            Counter = 0;
            m_ScreenInfoOfAllPlayers= new List<ScreenDimension>();
        }

        public static GameInformation Instance
        {
            get
            {
                lock (s_InstanceLock)
                {
                    if (m_Instance == null)
                    {
                        m_Instance = new GameInformation();
                    }
                }
                return m_Instance;
            }
        }

       

        public void init()
        {
            Player = new Player();
        }

        public string GetNameOfPlayer(int i_PlayerNumber)
        {
            return m_NamesOfAllPlayers[i_PlayerNumber];
        }

        public bool IsPointIsOnBoardPixels(Point i_Point)
        {
            bool isPointOnTheBoard = false;

            if (i_Point.Row >= 0 && i_Point.Column >= 0 && i_Point.Column < m_ClientScreenDimension.ScreenSizeInPixels.Width
                && i_Point.Row < m_ClientScreenDimension.ScreenSizeInPixels.Height)
            {
                isPointOnTheBoard = true;
            }

            return isPointOnTheBoard;
        }

        public bool IsPointIsOnBoardGrided(Point i_Point)
        {
            i_Point.Row *= GameSettings.GameGridSize + PointValuesToAddToScreen.Row;
            i_Point.Column *= GameSettings.GameGridSize + PointValuesToAddToScreen.Column;

            return IsPointIsOnBoardPixels(i_Point);
        }


        public void SetScreenInfo(string[] i_NamesOfPlayers, int[] i_ScreenSizeWidth, int[] i_ScreenSizeHeight, double[] i_Density)
        {
            m_NamesOfAllPlayers = i_NamesOfPlayers;

            for (int i = 0; i < m_AmountOfPlayers; i++)
            {
                m_ScreenInfoOfAllPlayers.Add(new ScreenDimension(i_ScreenSizeWidth[i], i_ScreenSizeHeight[i], new Position(m_AmountOfPlayers, i + 1), i_Density[i]));
            }

            m_ClientScreenDimension.m_Position = m_ScreenInfoOfAllPlayers[Player.PlayerNumber - 1].Position;
        }

        public List<ScreenDimension> ScreenInfoOfAllPlayers
        {
            get { return m_ScreenInfoOfAllPlayers; }
        }

        public int PlayerNumber
        {
            get { return Player.PlayerNumber; }
        }

        public int AmountOfPlayers
        {
            get { return m_AmountOfPlayers; }
            set { m_AmountOfPlayers = value; }
        }

        public eGames NameOfGame
        {
            get => m_NameOfGame;
            set => m_NameOfGame = value;
        }
    }
}