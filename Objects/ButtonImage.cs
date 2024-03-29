﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Objects
{
    public class ButtonImage : Image
    {
        public Button m_Button = new Button();
        private string m_SourcePressed;

        public ButtonImage()
        {
            Application.Current.Dispatcher.Dispatch(() =>
            {
                m_Button.BorderColor = Colors.Transparent;
                m_Button.Background = Brush.Transparent;
                m_Button.ZIndex = 0;
                m_Image.ZIndex = -1;
            });
        }

        public Button GetButton()
        {
            m_SourcePressed = "pressed" + m_Source;
            return m_Button;
        }

        public void SetButtonImage(GameObject i_GameObject)
        {
            SetImage(i_GameObject);
            //m_Button.ZIndex = 1;
            m_Image.ZIndex = 0;
            m_Button.TranslationX = i_GameObject.PointOnScreen.Column;
            m_Button.TranslationY =i_GameObject.PointOnScreen.Row;
            m_Button.ClassId = i_GameObject.ButtonType.ToString();
            m_Button.ZIndex = 1;
            IsVisible = i_GameObject.IsVisable;
            m_Button.Rotation = i_GameObject.Rotatation;
            setButtonSize(i_GameObject.Size);

            if (m_Button.WidthRequest != GameSettings.GameGridSize)
            {
                Text = i_GameObject.Text;
            }
        }



        public void IsButtonPressed(bool i_IsButtonPressed)
        {
            if (i_IsButtonPressed)
            {
                m_Image.Source = m_SourcePressed;
            }
            else
            {
                m_Image.Source = m_Source;
            }
        }

        public string Text
        {
            get
            {
                return m_Button.Text;
            }
            set
            {
                m_Button.Text = value;
                m_Button.FontSize = m_Button.HeightRequest * 0.3;
                m_Button.FontAutoScalingEnabled = true;
            }
        }

        public bool IsEnabled
        {
            set
            {
                m_Button.IsEnabled = value;
            }
        }

        public double FontSize
        {
            get
            {
                return m_Button.FontSize;
            }
            set
            {
                m_Button.FontSize = value;
            }
        }

        public void SetDefualtFontSize()
        {
            m_Button.FontSize = m_Button.HeightRequest * 0.3;
        }

        override public double HeightRequest
        {
            set
            {
                m_Button.HeightRequest = value;
                m_Image.HeightRequest = value;
            }
        }
        override public bool IsVisible
        {
            set
            {
                m_Button.IsVisible = value;
                m_Image.IsVisible = value;
            }
        }

        override public string ClassId
        {
            set
            {
                m_Button.ClassId = value;
                m_Image.ClassId = value;
            }
        }

        public string Source
        {
            set
            {
                if (value == null)
                {
                    IsVisible = false;
                }

                try
                {
                    m_Source = value;
                    m_Image.Source = value;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            }
        }

        override public double WidthRequest
        {
            set
            {
                m_Button.WidthRequest = value;
                m_Image.WidthRequest = value;
            }
        }

        override public double TranslationX
        {
            set
            {
                m_Button.TranslationX = value;
                m_Image.TranslationX = value;
            }
        }

        override public double TranslationY
        {
            set
            {
                m_Button.TranslationY = value;
                m_Image.TranslationY = value;
            }
        }

        override public int ZIndex
        {
            set
            {
                m_Button.ZIndex = value;
                m_Image.ZIndex = value - 1;
            }
        }

        override public double Rotation
        {
            set
            {
                m_Button.Rotation = value;
                m_Image.Rotation = value;
            }
        }

        override public Microsoft.Maui.Controls.LayoutOptions HorizontalOptions
        {
            set
            {
                m_Button.HorizontalOptions = value;
                m_Image.HorizontalOptions = value;
            }
        }

        private void setButtonSize(SizeD i_Size)
        {
            //m_Button.WidthRequest = (int)(i_Size.Width / m_Density);
            //m_Button.HeightRequest = (int)(i_Size.Height / m_Density);
            //if (i_Size.Width <= GameSettings.GameGridSize)
            //{
            //    m_Button.WidthRequest = (int)(m_Button.WidthRequest * 1.2);
            //    m_Button.HeightRequest = (int)(m_Button.WidthRequest * 1.25);
            //    m_Button.TranslationX = (int)(m_Button.TranslationX - 4 / m_Density);
            //    m_Button.TranslationY = (int)(m_Button.TranslationY - 4 / m_Density);
            //}
            m_Button.WidthRequest = i_Size.Width;
            m_Button.HeightRequest = i_Size.Height;
            if (i_Size.Width <= GameSettings.GameGridSize)
            {
                m_Button.WidthRequest = m_Button.WidthRequest * 1.2;
                m_Button.HeightRequest =m_Button.WidthRequest * 1.25;
                m_Button.TranslationX = m_Button.TranslationX - 4 / m_Density;
                m_Button.TranslationY = m_Button.TranslationY - 4 / m_Density;
            }
        }

        override public Microsoft.Maui.Controls.LayoutOptions VerticalOptions
        {
            set
            {
                m_Button.VerticalOptions = value;
                m_Image.VerticalOptions = value;
            }
        }
    }
}

