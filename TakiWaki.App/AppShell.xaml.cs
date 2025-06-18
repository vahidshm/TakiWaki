namespace TakiWaki.App;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		Routing.RegisterRoute("Client", typeof(Pages.ClientPage));
		Routing.RegisterRoute("Server", typeof(Pages.ServerPage));
		Routing.RegisterRoute("Settings", typeof(Pages.SettingsPage));
	}
}
