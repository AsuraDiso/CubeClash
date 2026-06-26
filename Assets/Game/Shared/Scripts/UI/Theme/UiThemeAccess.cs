namespace Game.Shared.Scripts.UI.Theme
{
    public static class UiThemeAccess
    {
        public static UiTheme Current { get; private set; }

        public static void Set(UiTheme theme) => Current = theme;
    }
}
