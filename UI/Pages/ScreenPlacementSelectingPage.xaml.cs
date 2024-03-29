//using Objects;
//namespace UI.Pages;
//using LogicUnit;
//using Microsoft.AspNetCore.SignalR.Client;
//using Microsoft.Maui.Controls;
//using LogicUnit;
using CommunityToolkit.Mvvm.ComponentModel;
using LogicUnit;
using Objects;
using Objects.Enums;
using Point = Objects.Point;
using Image = Microsoft.Maui.Controls.Image;
namespace UI.Pages;

using CommunityToolkit.Maui.Views;
using System.Xml.Linq;
using UI.Pages.LobbyPages;

public partial class ScreenPlacementSelectingPage : ContentPage
{
    private ScreenPlacementSelectingLogic m_pageLogic;
    private  List<Button> m_PlacementButton = new List<Button>();
    private List<Image> m_Images = new List<Image>();
    private GameInformation m_GameInformation = GameInformation.Instance;
    private static List<ButtonImage> m_PlacementButtons = new List<ButtonImage>();

    public ScreenPlacementSelectingPage()
	{
        InitializeComponent();
        initializePage();
    }

    protected override bool OnBackButtonPressed()
    {
        return true;
    }

    public static void visualButtonUpdate(object sender, VisualUpdateSelectButtons i_VisualUpdate)
    {
        m_PlacementButtons[i_VisualUpdate.spot].Text = i_VisualUpdate.textOnButton;

        if (i_VisualUpdate.didPlayerSelect)
        {
            m_PlacementButtons[i_VisualUpdate.spot].IsButtonPressed(true);
            m_PlacementButtons[i_VisualUpdate.spot].FontSize = m_PlacementButtons[i_VisualUpdate.spot].FontSize*0.3;
        }
        else
        {
            m_PlacementButtons[i_VisualUpdate.spot].IsButtonPressed(false);
        }
    }

    async Task initializePage()
    {
        try
        {
            m_pageLogic = new ScreenPlacementSelectingLogic();
            m_PlacementButton = new List<Button>();
            m_Images = new List<Image>();
            m_PlacementButtons = new List<ButtonImage>();
            UIbackground.TranslationY = UIbackground.HeightRequest = GameSettings.UIBackgroundSize.Height;
            UIbackground.WidthRequest = m_GameInformation.m_ClientScreenDimension.ScreenSizeInPixels.Width;
            m_pageLogic.UpdateSelectButton += visualButtonUpdate;
            m_pageLogic.ReceivedPlayerAmount += initializeButtons;
            m_pageLogic.GameIsStarting += startGame;
            m_pageLogic.ServerError += serverError;
            initializeButtons();
        }
        catch(Exception e)
        {
            serverError("An error has occurred.");
        }
    }

    async Task getScreenUpdate()
    {
        m_pageLogic.GetScreenUpdate();
    }

    private void startGame()
    {
        m_pageLogic.StopConnection();
        Shell.Current.GoToAsync("GamePage");
    }

    private void initializeButtons()
    {
        if (!m_GameInformation.Player.IsInitialized)
        {
            Application.Current.Dispatcher.Dispatch(async () =>
                {
                    for (int i = 0; i < m_pageLogic.AmountOfPlayers; i++)
                    {
                        int a = ((m_GameInformation.m_ClientScreenDimension.SizeInPixelsD.Height) / 12) / 3;

                        ButtonImage buttonImage = new ButtonImage();
                        Position position = new Position(m_pageLogic.AmountOfPlayers, i + 1);
                        buttonImage.WidthRequest = 19 * a;
                        buttonImage.HeightRequest = 12 * a;
                        buttonImage.Source = "placementbutton.png";
                        buttonImage.Text = (i + 1).ToString();
                        m_PlacementButtons.Add(buttonImage);
                        gridLayout.Add(buttonImage.GetImage(), (int)position.Column, (int)position.Row);
                        gridLayout.Add(buttonImage.GetButton(), (int)position.Column, (int)position.Row);
                        buttonImage.GetButton().Clicked += m_pageLogic.OnButtonClicked;
                    }
                    m_GameInformation.Player.IsInitialized = true;
                    getScreenUpdate();
                });
        }
    }

    private void serverError(string i_Message)
    {
        this.Dispatcher.Dispatch(async () =>
        {
            //MessagePopUp messagePopUp = new MessagePopUp(goToLobby, i_Message);
            //this.ShowPopup(messagePopUp);
            //Shell.Current.GoToAsync($"{nameof(Lobby)}?Error=True");
            await Shell.Current.GoToAsync("///MainPage");
        });
    }

    private void goToLobby()
    {
        Shell.Current.GoToAsync(nameof(Lobby));
    }
}