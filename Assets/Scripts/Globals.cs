/// <summary>
/// Global things. Does not include things that only exist based on context. Everything here is always valid as soon as the application gets past Awake.
/// </summary>
public static class Globals
{
	static Registry Registry = new Registry();

	public static Database Database => Get<Database>();
	public static Notifications Notifications => Get<Notifications>();
	public static World World => Get<World>();
	public static Game Game => Get<Game>();
	public static GameUISystem UI => Get<GameUISystem>();
	public static QuestSystem Quests => Get<QuestSystem>();
	public static MiniGames MiniGames => Get<MiniGames>();

	// TODO: Needs a review. Should turn this into a reusable audio system soon
	public static MenuSwitcherSounds audioManager => Get<MenuSwitcherSounds>();


	public static void Register<T>(T obj)
	{
		Registry.Register(obj);
	}

	public static void Unregister<T>(T obj)
	{
		Registry.Unregister(obj);
	}

	public static bool Has<T>() => Registry.Has<T>();
	public static T Get<T>()
	{
		return Registry.Get<T>();
	}
}
