﻿
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
//using ABI.Windows.Security.EnterpriseData;
using LogicUnit.Logic.GamePageLogic;
using LogicUnit.Logic.GamePageLogic.Games.Pacman;
using Microsoft.AspNetCore.SignalR.Client;
using Objects;
using Objects.Enums;
using Objects.Enums.BoardEnum;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Point = Objects.Point;

namespace LogicUnit
{
    public abstract partial class Game
    {
        private List<string> m_PlayerMovementsLogs = new List<string>();

        //Events
        public event EventHandler<List<GameObject>> AddGameObjectList;
        public event EventHandler<GameObject> GameObjectUpdate;
        public Notify GameStart;
        public Notify GameRestart;
        public Notify GameExit;
        public Notify ShowWinner;

        //basic game info
        protected readonly GameInformation r_GameInformation = GameInformation.Instance;
        protected Player m_Player;
        protected PlayerData[] m_PlayersDataArray = new PlayerData[4];
        protected PlayerData m_CurrentPlayerData;

        //Screen info 
        protected ScreenMapping m_ScreenMapping = new ScreenMapping();
        protected SizeD m_BoardSizeByGrid = new SizeD();

        //Need to initialize each different game
        protected string m_GameName;
        protected ScoreBoard m_ScoreBoard;
        protected Hearts m_Hearts = new Hearts();
        protected PauseMenu m_PauseMenu = new PauseMenu();

        //Things that might change while playing 
        protected int[,] m_Board;
        public eGameStatus m_GameStatus = eGameStatus.Running;
        protected List<string> m_LoseOrder = new List<string>();

        //Don't mind this
        protected Buttons m_Buttons = new Buttons();
        protected Random m_randomPosition = new Random();
       // protected List<List<Direction>> m_DirectionsBuffer = new List<List<Direction>>();
        protected Dictionary<int, Direction> m_PlayersDirectionsFromServer = new Dictionary<int, Direction>();

        //List for Ui changes
        protected List<GameObject> m_GameObjectsToAdd = new List<GameObject>();
        protected int m_AmountOfPlayers;

        //Game loop variables
        private Stopwatch m_LoopStopwatch = new Stopwatch();
        protected Stopwatch m_GameStopwatch = new Stopwatch();
        private int m_LastElapsedTime;
        protected eButton m_LastClickedButton = 0;
        public bool m_NewButtonPressed = false;
        public int m_LoopNumber = 0;
        public GameObject[] m_PlayerObjects;
        private const double k_DesiredFrameTime = 0.067;
        protected readonly CollisionManager r_CollisionManager = new CollisionManager();
        private bool m_ConnectedToServer = true;    //TODO
        static readonly object m_lock = new object();
        static readonly object m_lockxy = new object();
        protected Queue<SpecialUpdate> m_SpecialEventQueue;
        protected Queue<SpecialUpdate> m_SpecialEventWithPointQueue;
        private int[] m_ServerUpdates = new int[12];
        protected eMoveType m_MoveType;
        protected string m_EndGameText = string.Empty;
        private readonly InGameConnectionManager r_InGameConnectionManager;

        //Server Error
        public Action<string> ServerError;
        public Notify DisposeEvents;

        public Game(InGameConnectionManager i_InGameConnectionManager)
        {
            try
            {
                r_GameInformation.RealWorldStopwatch = new Stopwatch();
                m_Player = r_GameInformation.Player;
                for (int i = 0; i < 4; i++)
                {
                    m_PlayersDataArray[i] = new(i);
                }

                m_PlayerObjects = new GameObject[r_GameInformation.AmountOfPlayers];// new GameObject[2];//

                r_InGameConnectionManager = i_InGameConnectionManager;
                m_SpecialEventQueue = r_InGameConnectionManager.SpecialEventQueue;
                m_SpecialEventWithPointQueue = r_InGameConnectionManager.SpecialEventWithPointQueue;
                m_GameStatus = r_InGameConnectionManager.GameStatus;

            
                if (r_InGameConnectionManager.r_ConnectionToServer.State != HubConnectionState.Connected)
                {
                    r_InGameConnectionManager.r_ConnectionToServer.StartAsync();
                }

                if(r_GameInformation.PlayerNumber == 1)
                {
                    r_InGameConnectionManager.r_ConnectionToServer.SendAsync("ResetHub");
                }

                m_ConnectedToServer = true;
            }
            catch (Exception ex)
            {
                //throw;
                ServerError?.Invoke($"{ex.Message}{Environment.NewLine}error on StartAsync or SendAsync(\"ResetHub\")");
            }
        }

        private void checkForSpecialUpdates()
        {
            if (m_SpecialEventQueue.Count != 0 || m_SpecialEventWithPointQueue.Count != 0)
            {
                lock (m_lock)
                {
                    if (m_SpecialEventQueue.Count != 0)
                    {
                        SpecialUpdate specialUpdate = m_SpecialEventQueue.Dequeue();
                        SpecialUpdateReceived(specialUpdate);
                    }
                    if (m_SpecialEventWithPointQueue.Count != 0)
                    {
                        SpecialUpdate specialUpdate = m_SpecialEventWithPointQueue.Dequeue();
                        SpecialUpdateWithPointReceived(specialUpdate);
                    }
                }
            }
        }

        public GameObject InitializeGame()
        {
            m_AmountOfPlayers = r_GameInformation.AmountOfPlayers;
            m_BoardSizeByGrid = m_ScreenMapping.m_TotalScreenGridSize;
            m_Board = new int[m_BoardSizeByGrid.Width, m_BoardSizeByGrid.Height];
            m_CurrentPlayerData = new PlayerData(m_Player.PlayerNumber);
            m_CurrentPlayerData.Button = -1;
            SetGameScreen();
            m_ScoreBoard.Label.UpdateGameObject += OnUpdateScreenObject;

            return m_ScoreBoard.Label;
        }

        protected virtual void gameLoop()
        {
            r_GameInformation.RealWorldStopwatch.Start();
            m_GameStopwatch.Start();
            while (m_GameStatus != eGameStatus.Restarted && m_GameStatus != eGameStatus.Exited)
            {
                m_GameStopwatch.Restart();
                updateGame();
                Draw();

                Thread.Sleep((int)((k_DesiredFrameTime - m_LoopStopwatch.Elapsed.Seconds) * 1000));
                m_LastElapsedTime = (int)m_GameStopwatch.Elapsed.TotalMilliseconds;
            }

            m_ConnectedToServer = false;
        }

        private void updateGame()
        {
            SendServerUpdate();
            getUpdatedPosition();
            getButtonUpdate();
            if (m_GameStatus == eGameStatus.Running)
            {
                UpdatePosition(m_LastElapsedTime);
            }
            else
            {
                checkForSpecialUpdates();
            }
            m_LoopNumber = m_LastElapsedTime;
        }



        protected void stopMovement(int i_Player)
        {
            if (i_Player == m_Player.PlayerNumber)
            {
                m_NewButtonPressed = true;
                m_CurrentPlayerData.Button = 0;
            }
        }

        public GameObject GetLabel()
        {
            return m_Buttons.GameObjectFitForLabel;
        }

        protected void addBoarderFor3Players()
        {
            if (m_AmountOfPlayers == 3)
            {
                int y = m_ScreenMapping.m_Boundaries.Height;
                int x = m_ScreenMapping.m_Boundaries.Width;

                for (int i = y; i < m_BoardSizeByGrid.Height; i++)
                {
                    addBoarder(new Point(x, i));
                }
                for (int i = x; i < m_BoardSizeByGrid.Width; i++)
                {
                    addBoarder(new Point(i, y));
                }
            }
        }

        protected virtual void addBoarder(Point i_Point)
        {
            m_GameObjectsToAdd.Add(new Boarder(new Point(i_Point.Column, i_Point.Row), string.Empty));
        }

        protected virtual void Draw()
        {
            foreach (var player in m_PlayerObjects)
            {
                player.OnDraw();
            }
        }

        private async void SendServerUpdate()
        {
            if (m_NewButtonPressed)
            {
                Point playerPosition = m_PlayerObjects[m_Player.PlayerNumber - 1]
                    .GetCurrentPointOnScreen();
                try
                {
                    r_InGameConnectionManager.r_ConnectionToServer.SendAsync(
                      "UpdatePlayerSelection",
                      m_Player.PlayerNumber - 1,
                      m_CurrentPlayerData.Button,
                      0, 0);
                }
                catch (Exception e)
                {
                    ServerError.Invoke($"{e.Message}{Environment.NewLine}error on SendAsync(\"UpdatePlayerSelection\") in function SendServerUpdate");
                }

                m_NewButtonPressed = false;
            }
        }

        private async void GetServerUpdate()
        {
            while (m_ConnectedToServer)
            {
                try
                {
                    m_ServerUpdates = await r_InGameConnectionManager.r_ConnectionToServer.InvokeAsync<int[]>("GetPlayersData");
                }
                catch (TaskCanceledException ex) //TODO : Continue only when we restarted
                {
                    stopConnection();
                    r_GameInformation.Reset();
                    m_ConnectedToServer = false;
                    continue;
                }
                catch (Exception e)
                {
                    stopConnection();
                    r_GameInformation.Reset();
                    m_ConnectedToServer = false;
                    ServerError.Invoke($"{e.Message}{Environment.NewLine}error on InvokeAsync(\"GetPlayersData\") in function GetServerUpdate");
                }

                for (int i = 0; i < 4; i++)
                {
                    m_PlayersDataArray[i].Button = m_ServerUpdates[i];
                }
            }

            if (m_GameStatus == eGameStatus.Exited)
            {
                stopConnection();
                r_GameInformation.Reset();
                GameExit.Invoke();
            }
        }

        protected virtual void specialEventInvoked(object i_Sender, int i_eventNumber)
        {
            SendSpecialServerUpdate(i_Sender, i_eventNumber);
        }

        protected async void SendSpecialServerUpdate(object? sender, int i_eventNumber)
        {
            int number = r_GameInformation.Player.PlayerNumber;
            if (sender != null)
            {
                GameObject gameObject = sender as GameObject;
                number = gameObject.ObjectNumber;
            }
            try
            {
                r_InGameConnectionManager.r_ConnectionToServer.SendAsync(
                    "SpecialUpdate",
                    i_eventNumber, number
                    );
            }
            catch (Exception e)
            {
                ServerError.Invoke($"{e.Message}{Environment.NewLine}error on SendAsync(\"SpecialUpdate\") in function SendSpecialServerUpdate");
            }
        }

        protected virtual void PlayerLostALife(object sender, int i_Player)
        {
            m_Hearts.setPlayerLifeAndCheckIfDead(i_Player);
        }

        protected virtual void UpdatePosition(double i_TimeElapsed)
        {
            foreach (var gameObject in m_PlayerObjects)
            {
                gameObject.Update(i_TimeElapsed);
            }

            checkForSpecialUpdates();
            r_CollisionManager.CheckAllCollidablesForCollision();
            checkForGameStatusUpdate();
        }

        public void UpdateClientsAboutPosition(object sender, Point i_Point)
        {
            GameObject a = sender as GameObject;
            SendServerPositionUpdate(a.ObjectNumber, i_Point);
        }

        private async void SendServerPositionUpdate(int i_Player, Point i_Point)
        {
            try
            {
                if (i_Point.Row == 0)
                {
                    i_Point.Row = -100;
                }
                if (i_Point.Column == 0)
                {
                    i_Point.Column = -100;
                }
                await r_InGameConnectionManager.r_ConnectionToServer.SendAsync(
                  "UpdatePlayerSelection", i_Player - 1
                  ,
                  -1,
                  (int)i_Point.Column, (int)i_Point.Row);
            }
            catch (Exception e)
            {
                ServerError.Invoke($"{e.Message}{Environment.NewLine}error on SendAsync(\"UpdatePlayerSelection\") in function SendServerPositionUpdate");
            }
        }

        virtual public void OnButtonClicked(object sender, EventArgs e)
        {
            Button button = sender as Button;

            m_NewButtonPressed = m_CurrentPlayerData.Button != (int)m_Buttons.StringToButton(button!.ClassId);
            //m_CurrentPlayerData.Button = (int)m_Buttons.StringToButton(button!.ClassId);

            //r_ConnectionToServer.StopAsync();

            if ((int)m_Buttons.StringToButton(button!.ClassId) > 6)
            {
                SendSpecialServerUpdate(null, (int)m_Buttons.StringToButton(button!.ClassId));
            }
            else
            {
                m_CurrentPlayerData.Button = (int)m_Buttons.StringToButton(button!.ClassId);
            }
        }

        public void OnButtonRelesed(object sender, EventArgs e)
        {
            Button button = sender as Button;

            if (m_CurrentPlayerData.Button == (int)m_Buttons.StringToButton(button!.ClassId))
            {
                m_CurrentPlayerData.Button = (int)eButton.Stop;
                m_NewButtonPressed = true;
            }
        }

        protected virtual void SpecialUpdateReceived(SpecialUpdate i_SpecialUpdate)
        {
            if (i_SpecialUpdate.Update == 8)
            {
                if (m_GameStatus == eGameStatus.Running)
                {
                    m_GameStatus = eGameStatus.Paused;
                    if (m_Player.PlayerNumber == i_SpecialUpdate.Player_ID)
                    {
                        m_PauseMenu.ShowPauseMenu();
                    }
                    r_GameInformation.RealWorldStopwatch.Stop();
                }
            }
            else
            {
                if (m_GameStatus == eGameStatus.Paused || m_GameStatus == eGameStatus.Ended)
                {

                    m_PauseMenu.HidePauseMenu();
                    if (i_SpecialUpdate.Update == 7)
                    {
                        m_GameStatus = eGameStatus.Running;
                    }
                    else if (i_SpecialUpdate.Update == 9)
                    {
                        //stopConnection();
                        m_GameStatus = eGameStatus.Restarted;
                        GameRestart.Invoke();
                    }
                    else if (i_SpecialUpdate.Update == 10)
                    {
                        m_GameStatus = eGameStatus.Exited;
                        m_ConnectedToServer = false;
                    }
                    r_GameInformation.RealWorldStopwatch.Start();
                }
            }
        }

        public void stopConnection()
        {
            r_InGameConnectionManager.r_ConnectionToServer.StopAsync();
            //r_ConnectionToServer = null;
        }

        private void getButtonUpdate()
        {
            for (int i = 0; i < m_AmountOfPlayers; i++)
            {
                m_PlayerObjects[i].RequestDirection(Direction.getDirection(m_PlayersDataArray[i].Button));
            }
        }

        private void OnUpdateScreenObject(object sender, EventArgs e)
        {
            GameObject i = sender as GameObject;
            GameObjectUpdate.Invoke(this, i);
        }

        protected async void SendServerSpecialPointUpdate(Point i_Point, int i_Player)
        {
            try
            {
                r_InGameConnectionManager.r_ConnectionToServer.SendAsync(
                    "SpecialUpdateWithPoint",
                    (int)i_Point.Column, (int)i_Point.Row, i_Player
                );
            }
            catch (Exception e)
            {
                ServerError.Invoke($"{e.Message}{Environment.NewLine}error on SendAsync(\"SpecialUpdateWithPoint\") in function SendServerSpecialPointUpdate");
            }
        }

        public void RunGame()
        {
            Thread serverUpdateThread = new(GetServerUpdate);//(serverUpdateLoop);
            Thread newThread = new(gameLoop);
            serverUpdateThread.Start();
            newThread.Start();
        }

        protected virtual void OnAddScreenObjects()
        {
            foreach (var newObject in m_GameObjectsToAdd)
            {
                if (newObject.MonitorForCollision)
                {
                    r_CollisionManager.AddObjectToMonitor(newObject);
                    if (newObject.ScreenObjectType == eScreenObjectType.Player)
                    {
                        m_PlayerObjects[newObject.ObjectNumber - 1] = newObject;
                        newObject.SpecialEvent += specialEventInvoked;
                        newObject.UpdatePosition += UpdateClientsAboutPosition;
                    }
                    else
                    {
                        newObject.SpecialEvent += specialEventInvoked;
                    }
                }

                newObject.UpdateGameObject += OnUpdateScreenObject;
            }
            AddGameObjectList.Invoke(this, m_GameObjectsToAdd); //..Invoke(this, i_ScreenObject));
        }

        protected virtual void OnGameStart()
        {
            GameStart.Invoke();
        }

        public bool DoesGameNeedToKnowIfButtonReleased()
        {
            bool result = false;

            if (m_MoveType == eMoveType.ClickAndRelease)
            {
                result = true;
            }

            return result;
        }

        void getUpdatedPosition()
        {
            for (int i = 0; i < m_AmountOfPlayers; i++)
            {
                Point pointRecived = new Point(m_ServerUpdates[i + 4], m_ServerUpdates[i + 8]);
                Point point = new Point(
                    pointRecived.Column,
                    pointRecived.Row);

                if (point.Row == -100)
                {
                    point.Row = 0;
                }
                if (point.Column == -100)
                {
                    point.Column = 0;
                }

                if (pointRecived.Row != 0 && pointRecived.Column != 0 && m_PlayersDataArray[i].PlayerPointData != pointRecived)
                {
                    if (point.Row < 0)
                    {
                        point.Row = -pointRecived.Row;
                    }
                    m_PlayerObjects[i].UpdatePointOnScreenByGrid(point);
                    m_PlayersDataArray[i].PlayerPointData = pointRecived;
                }
                m_PlayersDataArray[i].Button = m_ServerUpdates[i];
            }
        }

        protected virtual void SpecialUpdateWithPointReceived(SpecialUpdate i_SpecialUpdate)
        {

        }

        protected virtual void checkForGameStatusUpdate()
        {

        }
    }
}