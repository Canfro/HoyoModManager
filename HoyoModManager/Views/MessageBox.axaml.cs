using Avalonia.Controls;
using Avalonia.Interactivity;

namespace HoyoModManager.Views;

public partial class MessageBox : Window
{
    public MessageBox(string title, string message)
    {
        InitializeComponent();
        Title = title;
        this.FindControl<TextBlock>("Message").Text = message;
        DataContext = this;
    }
}

