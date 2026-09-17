using System;
using snake2d;
using util.text;

namespace view.keyboard
{
    public class KeyPageMain : KeyPage
    {
        public KeyPageMain() : base("MAIN")
        {
            D.gInit(this);
        }

        public readonly Key ASSIGN_HOTKEY = new Key("ASSIGN_HOTKEY", D.g("Assign-hotkey"), D.g("AssignD", "When hovering specific buttons, you can press this to assign a new hotkey for the button function."), this, KEYCODES.KEY_LEFT_CONTROL, KEYCODES.KEY_H);
        public readonly Key SCROLL_LEFT = new Key("SCROLL_LEFT", D.g("Scroll-Left"), D.g("Scroll-LeftD", "Moves the map."), this, KEYCODES.KEY_A);
        public readonly Key SCROLL_RIGHT = new Key("SCROLL_RIGHT", D.g("Scroll-Right"), D.g("Scroll-RightD", "Moves the map."), this, KEYCODES.KEY_D);
        public readonly Key SCROLL_UP = new Key("SCROLL_UP", D.g("Scroll-Up"), D.g("Scroll-UpD", "Moves the map."), this, KEYCODES.KEY_W);
        public readonly Key SCROLL_DOWN = new Key("SCROLL_DOWN", D.g("Scroll-Down"), D.g("Scroll-DownD", "Moves the map."), this, KEYCODES.KEY_S);
        public readonly Key ZOOM_IN = new Key("ZOOM_IN", D.g("Zoom-In"), D.g("Zoom-InD", "Zoom in (Mousewheel)."), this, KEYCODES.KEY_N);
        public readonly Key ZOOM_OUT = new Key("ZOOM_OUT", D.g("Zoom-Out"), D.g("Zoom-OutD", "Zoom out (Mousewheel)."), this, KEYCODES.KEY_B);
        public readonly Key MINIMAP = new Key("MINIMAP", D.g("Minimap"), D.g("MinimapD", "Toggles the minimap."), this, KEYCODES.KEY_M);
        public readonly Key THRONE = new Key("THRONE", D.g("Center"), D.g("ThroneD", "Center the map on the throne or the capital."), this, KEYCODES.KEY_LEFT_CONTROL, KEYCODES.KEY_A);

        public readonly Key ROTATE = new Key("ROTATE", D.g("Rotate"), D.g("RotateD", "Rotates things."), this, KEYCODES.KEY_R);
        public readonly Key GROW = new Key("GROW", D.g("Increase"), D.g("IncreaseD", "Increases size/cycle options."), this, KEYCODES.KEY_E);
        public readonly Key SHRINK = new Key("SHRINK", D.g("Decrease"), D.g("DecreaseD", "Increases size/cycle options."), this, KEYCODES.KEY_Q);

        public readonly Key QUICKSAVE = new Key("QUICKSAVE", D.g("Quicksave"), D.g("QuicksaveD", "Saves the game under the name 'Quicksave'."), this, KEYCODES.KEY_LEFT_CONTROL, KEYCODES.KEY_S);
        public readonly Key QUICKLOAD = new Key("QUICKLOAD", D.g("Quick-Load"), D.g("QuickLoadD", "Loads the latest save."), this);
        public readonly Key PAUSE = new Key("PAUSE", D.g("Pause"), D.g("PauseD", "Toggles game pause."), this, KEYCODES.KEY_1);
        //public readonly Key SPEED0 = new Key("SPEED0", D.g("speed-paused"), D.g("speed-pausedD", "Sets 0 speed, thus pausing the game."), this, KEYCODES.KEY_1);
        public readonly Key SPEED1 = new Key("SPEED1", D.g("speed-normal"), D.g("speed-normalD", "Sets 1x speed. Double press for 1/4th speed."), this, KEYCODES.KEY_2);
        public readonly Key SPEED2 = new Key("SPEED2", D.g("speed-fast"), D.g("speed-fastD", "Sets 5x speed."), this, KEYCODES.KEY_3);
        public readonly Key SPEED3 = new Key("SPEED3", D.g("speed-fastest"), D.g("speed-fastestD", "Sets 25x speed. Double press for x250 speed."), this, KEYCODES.KEY_4);

        public readonly Key MUP = new Key("MAP_UP", D.g("Slow-Up"), D.g("Slow-UpD", "Slowly pans the map."), this, KEYCODES.KEY_LEFT_ALT, KEYCODES.KEY_UP);
        public readonly Key MDOWN = new Key("MAP_DOWN", D.g("Slow-Down"), D.g("Slow-DownD", "Slowly pans the map."), this, KEYCODES.KEY_LEFT_ALT, KEYCODES.KEY_DOWN);
        public readonly Key MLEFT = new Key("MAP_LEFT", D.g("Slow-Left"), D.g("Slow-LeftD", "Slowly pans the map."), this, KEYCODES.KEY_LEFT_ALT, KEYCODES.KEY_LEFT);
        public readonly Key MRIGHT = new Key("MAP_RIGHT", D.g("Slow-Right"), D.g("Slow-RightD", "Slowly pans the map."), this, KEYCODES.KEY_LEFT_ALT, KEYCODES.KEY_RIGHT);

        public readonly Key SCREENSHOT = new Key("SCREENSHOT", D.g("Screenshot"), D.g("ScreenshotD", "Creates a screenshot, saved in your local files, reachable through the launcher -> info."), this, -1, KEYCODES.KEY_PRINT_SCREEN, false);

        public readonly Key DEBUGGER = new Key("STATS", D.g("Stats"), D.g("StatsD", "Toggles stats."), this, KEYCODES.KEY_F11);

        public readonly Key ENTER = new Key("ENTER", D.g("Enter"), D.g("EnterD", "The enter key."), this, -1, KEYCODES.KEY_ENTER, false);
        public readonly Key BACKSPACE = new Key("BACKSPACE", D.g("Backspace"), D.g("BackspaceD", "The backspace key."), this, -1, KEYCODES.KEY_BACKSPACE, false);
        public readonly Key ESCAPE = new Key("ESCAPE", D.g("Escape"), D.g("EscapeD", "Toggles the menu, or closes panels."), this, -1, KEYCODES.KEY_ESCAPE, false);

        public readonly Key MOD = new Key("MOD", D.g("mod"), D.g("modD", "When pressed, modulates certain functions, such as the mouse wheel."), this, KEYCODES.KEY_LEFT_CONTROL);
        public readonly Key UNDO = new Key("UNDO", D.g("place-undo"), D.g("place-undoD", "When pressed, lets you use alternative tools when using a tool."), this, KEYCODES.KEY_LEFT_SHIFT);

        public readonly Key SWAP = new Key("SWAP", D.g("toggle", "Toggle view"), D.g("toggleD", "Toggle between common views."), this, KEYCODES.KEY_TAB);

        private readonly string name = D.g("General");

        public override string Name()
        {
            return name;
        }
    }
}