﻿//using System.Threading;
//using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
//using static Java.Util.Jar.Attributes;
//using Microsoft.Maui.Controls;
//using static Java.Util.Jar.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Objects;
using Point = Objects.Point;
using Size = Objects.Size;

namespace LogicUnit
{
    public delegate void Notify();
    public class ScreenPlacementSelectingLogic
    {
        public event EventHandler<VisualUpdateSelectButtons> UpdateSelectButton;
        public event Notify ReceivedPlayerAmount;
        public event Notify GameIsStarting;
        private readonly HubConnection r_ConnectionToServer;
        Player m_Player = Player.Instance;
        private int m_AmountOfPlayerThatAreConnected;
        private GameInformation m_GameInformation = GameInformation.Instance;
        
        public ScreenPlacementSelectingLogic()
        {

            r_ConnectionToServer = new HubConnectionBuilder()
                .WithUrl(Utils.m_GameHubAddress)
                .Build();

            r_ConnectionToServer.On<int>
            ("GetAmountOfPlayers", (i_AmountOfPlayers) =>
                {
                    m_GameInformation.AmountOfPlayers = i_AmountOfPlayers;
                    OnReceivedAmountOfPlayers();
                });

            r_ConnectionToServer.On<string[]>("RecieveScreenUpdate", (i_ButtonsThatAreOccupied) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                        {
                            int buttonNumber = 0;
                            VisualUpdateSelectButtons visualUpdate = new();

                            foreach (string element in i_ButtonsThatAreOccupied)
                            {
                                if (i_ButtonsThatAreOccupied[buttonNumber] != String.Empty
                                    && i_ButtonsThatAreOccupied[buttonNumber] != null)
                                {
                                    visualUpdate.Set(buttonNumber, i_ButtonsThatAreOccupied[buttonNumber], true);
                                    OnUpdateButton(visualUpdate);
                                }
                                buttonNumber++;
                            }
                        });
                });

            r_ConnectionToServer.On<string, int>("DeSelectPlacementUpdateReceived", (i_NameOfPlayerThatDeselected, i_Spot) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    VisualUpdateSelectButtons visualUpdate = new(i_Spot, (1 + i_Spot).ToString(), false);

                    if (m_Player.Name == i_NameOfPlayerThatDeselected) //(Name.Equals(nameOfPlayerThatDeselected))
                    {
                        m_Player.ButtonThatPlayerPicked = 0;
                        m_Player.DidPlayerPickAPlacement = false;
                    }

                    OnUpdateButton(visualUpdate);
                    m_AmountOfPlayerThatAreConnected--;
                });
            });

            r_ConnectionToServer.On("StartGame", (string[] i_NamesOfPlayers, Size[] i_ScreenSizes) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    m_GameInformation.SetScreenInfo(i_NamesOfPlayers, i_ScreenSizes);
                    OnEnterGameRoom();
                });
            });

            r_ConnectionToServer.On<string, int>
                ("PlacementUpdateRecevied", (i_NameOfPlayerThatGotASpot, i_Spot) =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                            {
                                VisualUpdateSelectButtons visualUpdate = new(i_Spot, i_NameOfPlayerThatGotASpot, true);

                                OnUpdateButton(visualUpdate);
                                m_AmountOfPlayerThatAreConnected++;

                                if (m_Player.Name == i_NameOfPlayerThatGotASpot)
                                {
                                    m_Player.ButtonThatPlayerPicked = 1 + i_Spot;
                                    m_Player.DidPlayerPickAPlacement = true;
                                }

                                if (m_AmountOfPlayerThatAreConnected == m_GameInformation.AmountOfPlayers)
                                {
                                    r_ConnectionToServer.InvokeAsync("GameIsAboutToStart");
                                }
                            });
                    });

            Task.Run(() =>
            {
                Application.Current.Dispatcher.Dispatch(async () =>
                    {
                        await r_ConnectionToServer.StartAsync();
                        await r_ConnectionToServer.InvokeAsync("GetAmountOfPlayers");
                    });
            });

        }

        public void OnUpdateButton(VisualUpdateSelectButtons i_VisualUpdate)
        {
            UpdateSelectButton?.Invoke(this,i_VisualUpdate);
        }

        public void OnReceivedAmountOfPlayers()
        {
            ReceivedPlayerAmount?.Invoke();
        }

        protected virtual void OnEnterGameRoom()
        {
            //m_GameInformation.m_ClientScreenDimension.m_Position.SetPosition
            //    (m_GameInformation.AmountOfPlayers,m_Player.ButtonThatPlayerPicked);
            GameIsStarting?.Invoke();
        }

        public int AmountOfPlayers
        {
            get { return m_GameInformation.AmountOfPlayers; }
        }


        public int AmountOfPlayerThatAreConnected
        {
            get { return m_AmountOfPlayerThatAreConnected; }
            set { m_AmountOfPlayerThatAreConnected = value; }
        }

        public bool AreAllTheUsersReady()
        {
            bool result = false;

            if (m_AmountOfPlayerThatAreConnected == m_GameInformation.AmountOfPlayers && m_Player.ButtonThatPlayerPicked == 1)
            {
                result = true;
            }

            return result;
        }

        public void SetPlayerScreenSize(int i_Width, int i_Height)
        {
            m_GameInformation.m_ClientScreenDimension.Size = new Size(i_Width, i_Height);

        }

        public async void OnButtonClicked(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (m_Player.DidPlayerPickAPlacement)
            {
                TryToDeselectScreenSpot(button.Text);
            }
            else
            {
                TryPickAScreenSpot(button.Text);
            }
        }

        public async Task GetScreenUpdate()
        {
            await r_ConnectionToServer.InvokeCoreAsync("RequestScreenUpdate", args: new[]
                {r_ConnectionToServer.ConnectionId});
        }

        public async Task TryPickAScreenSpot(string i_TextOnButton)
        {
            await r_ConnectionToServer.SendAsync(
                "TryPickAScreenSpot",
                m_Player.Name,
                i_TextOnButton,
                m_GameInformation.m_ClientScreenDimension.Size);

        }

        public async Task TryToDeselectScreenSpot(string i_TextOnButton)
        {
            await r_ConnectionToServer.InvokeCoreAsync("TryToDeselectScreenSpot", args: new[]
                {m_Player.Name,m_Player.ButtonThatPlayerPicked.ToString(),i_TextOnButton});
        }
    }
}
