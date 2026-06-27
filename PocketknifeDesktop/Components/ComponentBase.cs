using Vellum;
using Vellum.Web;

namespace PocketKnifeDesktop;

public abstract class ComponentBase
{
	public abstract void Draw(Ui host, AppState state);
}