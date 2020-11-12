using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PlayFourRowGame
{
    /// <summary>
    /// Interaction logic for EmojIWindow.xaml
    /// </summary>
    public partial class EmojIWindow : Window
    {
        public EmojIWindow()
        {
            InitializeComponent();
        }

        string emojiName;

        public string EmojiName
        {
            get => emojiName;
            set => emojiName = value;
        }

        private void Image_MouseLeftButtonUp0(object sender, MouseButtonEventArgs e)
        {
            ResetBorders();
            EmojiName = "emoji-1.png";
            Border00.BorderThickness = new Thickness(1,1,1,1);
        }
        private void Image_MouseLeftButtonUp1(object sender, MouseButtonEventArgs e)
        {
            ResetBorders();
            EmojiName = "emoji-2.png";
            Border01.BorderThickness = new Thickness(1, 1, 1, 1);
        }
        private void Image_MouseLeftButtonUp2(object sender, MouseButtonEventArgs e)
        {
            ResetBorders();
            EmojiName = "emoji-3.png";
            Border02.BorderThickness = new Thickness(1, 1, 1, 1);
        }
        private void Image_MouseLeftButtonUp3(object sender, MouseButtonEventArgs e)
        {
            ResetBorders();
            EmojiName = "emoji-4.png";
            Border03.BorderThickness = new Thickness(1, 1, 1, 1);
        }
        private void Image_MouseLeftButtonUp4(object sender, MouseButtonEventArgs e)
        {
            ResetBorders();
            EmojiName = "emoji-5.png";
            Border10.BorderThickness = new Thickness(1, 1, 1, 1);
        }
        private void Image_MouseLeftButtonUp5(object sender, MouseButtonEventArgs e)
        {
            ResetBorders();
            EmojiName = "emoji-6.png";
            Border11.BorderThickness = new Thickness(1, 1, 1, 1);
        }
        private void Image_MouseLeftButtonUp6(object sender, MouseButtonEventArgs e)
        {
            ResetBorders();
            EmojiName = "emoji-7.png";
            Border12.BorderThickness = new Thickness(1, 1, 1, 1);
        }
        private void Image_MouseLeftButtonUp7(object sender, MouseButtonEventArgs e)
        {
            ResetBorders();
            EmojiName = "emoji-8.png";
            Border13.BorderThickness = new Thickness(1, 1, 1, 1);
        }
        private void Image_MouseLeftButtonUp8(object sender, MouseButtonEventArgs e)
        {
            ResetBorders();
            EmojiName = "emoji-9.png";
            Border20.BorderThickness = new Thickness(1, 1, 1, 1);
        }
        private void Image_MouseLeftButtonUp9(object sender, MouseButtonEventArgs e)
        {
            ResetBorders();
            EmojiName = "emoji-10.png";
            Border21.BorderThickness = new Thickness(1, 1, 1, 1);
        }
        private void Image_MouseLeftButtonUp10(object sender, MouseButtonEventArgs e)
        {
            ResetBorders();
            EmojiName = "emoji-11.png";
            Border22.BorderThickness = new Thickness(1, 1, 1, 1);
        }
        private void Image_MouseLeftButtonUp11(object sender, MouseButtonEventArgs e)
        {
            ResetBorders();
            EmojiName = "emoji-12.png";
            Border23.BorderThickness = new Thickness(1, 1, 1, 1);
        }

        private void SelectEmojiButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ResetBorders()
        {
            Border00.BorderThickness = new Thickness(0, 0, 0, 0);
            Border01.BorderThickness = new Thickness(0, 0, 0, 0);
            Border02.BorderThickness = new Thickness(0, 0, 0, 0);
            Border03.BorderThickness = new Thickness(0, 0, 0, 0);
            Border10.BorderThickness = new Thickness(0, 0, 0, 0);
            Border11.BorderThickness = new Thickness(0, 0, 0, 0);
            Border12.BorderThickness = new Thickness(0, 0, 0, 0);
            Border13.BorderThickness = new Thickness(0, 0, 0, 0);
            Border20.BorderThickness = new Thickness(0, 0, 0, 0);
            Border21.BorderThickness = new Thickness(0, 0, 0, 0);
            Border22.BorderThickness = new Thickness(0, 0, 0, 0);
            Border23.BorderThickness = new Thickness(0, 0, 0, 0);
            

        }
    }
}
