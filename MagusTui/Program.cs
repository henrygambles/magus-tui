
using System.Text;
using Terminal.Gui;

namespace MagusTui;

public static class Program
{
    public static void Main()
    {
        Application.Init();

        var app = new MagusApp();
        app.Run();

        Application.Shutdown();
    }
}
