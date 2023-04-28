﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicUnit.Logic.GamePageLogic;
using Objects;
using Objects.Enums;
using Point = Objects.Point;
using Size = Objects.Size;

namespace LogicUnit
{
    public abstract partial class Game
    {
        private GameInformation m_GameInformation = GameInformation.Instance;
        private Player m_Player = Player.Instance;
        protected ScreenMapping m_ScreenMapping = new ScreenMapping();
        protected eTypeOfGameButtons m_TypeOfGameButtons;
        protected int m_AmountOfLivesTheClientHas;
        protected Size m_BoardSize = new Size();
        protected int[,] m_Board;
        protected int m_AmountOfActivePlayers;
        protected Buttons m_Buttons = new Buttons();
        Point PlayerObject = new Point();
        protected List<GameObject> m_PlayerGameObjects = new List<GameObject>();
        protected List<GameObject> m_gameObjects = new List<GameObject>();
        public eGameStatus m_GameStatus;
        protected Random m_randomPosition = new Random();

        public abstract void RunGame();



        public void InitializeGame()
        {
            m_BoardSize = m_ScreenMapping.m_TotalScreenSize;
            m_Board = new int[m_BoardSize.m_Width, m_BoardSize.m_Height];
            SetGameScreen();
        }

        protected abstract void setGameBoardAndGameObjects();

        public bool SetAmountOfPlayers(int i_amountOfPlayers)
        {
            if(i_amountOfPlayers >= 2 && i_amountOfPlayers <= 4)
            {
                m_GameInformation.AmountOfPlayers = i_amountOfPlayers;
                m_AmountOfActivePlayers = i_amountOfPlayers;
                return true;
            }
            else
            {
                return false; //If false is returned tell user that the number of players isn't right
            }
        }

        protected eGameStatus PlayerLostALife(string i_NameOfClientThatLostALife)
        {
            eGameStatus returnStatus = eGameStatus.Running;

            m_AmountOfLivesTheClientHas--;

            if (i_NameOfClientThatLostALife == m_Player.Name)
            {
                // Add a function here that tells the UI to take a heart away 
            }

            if (m_AmountOfLivesTheClientHas == 0)
            {
                returnStatus = eGameStatus.Lost;
            }

            return returnStatus;
        }

        private bool checkThatPointIsOnBoardGrided(Point i_Point)
        {
            bool isPointOnTheBoard = true;

            if(i_Point.m_Row < 0 || i_Point.m_Column < 0 || i_Point.m_Column > m_BoardSize.m_Width
               || i_Point.m_Row > m_BoardSize.m_Height)
            {
                isPointOnTheBoard = false;
            }
            else if(m_GameInformation.AmountOfPlayers == 3)
            {
                //check special boards
            }


            return isPointOnTheBoard;
        }

        protected bool isPointOnBoard(Point i_Point)
        {
            bool isPointOnTheBoard = true;

            if(i_Point.m_Row < 0 || i_Point.m_Row >= m_BoardSize.m_Height || i_Point.m_Column < 0
               || i_Point.m_Column >= m_BoardSize.m_Width)
            {
                isPointOnTheBoard = false;
            }

            return isPointOnTheBoard;
        }

        //protected virtual int whatObjectWillHit(Point i_Point)
        //{
        //    int res;

        //    if(isPointOnBoard(i_Point))
        //    {
        //        res= m_Board[i_Point.m_Column, i_Point.m_Row];
        //    }
        //    else
        //    {
        //        res = -1;
        //    }

        //    return res;
        //}

        protected eGameStatus ClientLostGame(string i_NameOfClientThatLost)
        {
            eGameStatus returnStatus = eGameStatus.Running;

            m_AmountOfActivePlayers--;

            if (i_NameOfClientThatLost == m_Player.Name)
            {
                // Add a function here that tells the UI what to show to the client in case of 
            }

            if(m_AmountOfActivePlayers == 0)
            {
                returnStatus = eGameStatus.Ended;
            }

            return returnStatus;
        }

        protected void OnUpdateScreenObject(List<ScreenObjectUpdate> i_Update)
        {
            GameObjectUpdate.Invoke(this,i_Update);
        }

        public void OnButtonClicked(object sender, EventArgs e)
        {
            Button button = sender as Button;
            List<ScreenObjectUpdate> l = new List<ScreenObjectUpdate>();
            l.Add(m_PlayerGameObjects[0].GetObjectUpdate());
            int movementDistance = m_ScreenMapping.m_GameBoardGridSize;
            GameObjectUpdate.Invoke(this, l);
            //if (m_GameInformation.m_ClientScreenDimension.Position.Row == eRowPosition.LowerRow)
            //{
            //    if (button.ClassId == eButton.Up.ToString())
            //    {
            //        PlayerObject.m_Row -= movementDistance;
            //        GameObjectUpdate.Invoke(this, PlayerObject);
            //    }
            //    else if (button.ClassId == eButton.Down.ToString())
            //    {
            //        PlayerObject.m_Row += movementDistance;
            //        GameObjectUpdate.Invoke(this, PlayerObject);
            //    }
            //    else if (button.ClassId == eButton.Right.ToString())
            //    {
            //        PlayerObject.m_Column += movementDistance;
            //        GameObjectUpdate.Invoke(this, PlayerObject);
            //    }
            //    else
            //    {
            //        PlayerObject.m_Column -= movementDistance;
            //        GameObjectUpdate.Invoke(this, PlayerObject);
            //    }
            //}
            //else
            //{
            //    if (button.ClassId == eButton.Up.ToString())
            //    {
            //        PlayerObject.m_Row += movementDistance;
            //        GameObjectUpdate.Invoke(this, PlayerObject);
            //    }
            //    else if (button.ClassId == eButton.Down.ToString())
            //    {
            //        PlayerObject.m_Row -= movementDistance;
            //        GameObjectUpdate.Invoke(this, PlayerObject);
            //    }
            //    else if (button.ClassId == eButton.Right.ToString())
            //    {
            //        PlayerObject.m_Column -= movementDistance;
            //        GameObjectUpdate.Invoke(this, PlayerObject);
            //    }
            //    else
            //    {
            //        PlayerObject.m_Column += movementDistance;
            //        GameObjectUpdate.Invoke(this, PlayerObject);
            //    }
            //}
        }
    }
}
