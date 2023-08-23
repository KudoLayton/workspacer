using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace workspacer
{
    public class NativeMonitorContainer : IMonitorContainer
    {
        private Monitor[] _monitors;

        private Dictionary<IMonitor, int> _monitorMap;

        private bool _isFocusedMonitorOverrided = false;

        private IMonitor _OverridedFocusedMonitor;

        public NativeMonitorContainer()
        {
            var screens = Screen.AllScreens;
            _monitors = new Monitor[screens.Length];
            _monitorMap = new Dictionary<IMonitor, int>();

            var primaryMonitor = new Monitor(0, Screen.PrimaryScreen);
            _monitors[0] = primaryMonitor;
            _monitorMap[primaryMonitor] = 0;

            var index = 1;
            foreach (var screen in screens)
            {
                if (!screen.Primary)
                {
                    var monitor = new Monitor(index, screen);
                    _monitors[index] = monitor;
                    _monitorMap[monitor] = index;
                    index++;
                }
            }
            FocusedMonitor = _monitors[0];
        }

        public int NumMonitors => _monitors.Length;

        private IMonitor _FocusedMonitor;

        public IMonitor FocusedMonitor 
        { 
            get 
            {
                return _isFocusedMonitorOverrided ? _OverridedFocusedMonitor : _FocusedMonitor;
            }

            set 
            {
                _isFocusedMonitorOverrided = false;
                _FocusedMonitor = value;
            } 
        }

        public IMonitor[] GetAllMonitors()
        {
            return _monitors.ToArray();
        }
        public IMonitor GetMonitorAtIndex(int index)
        {
            return _monitors[index % _monitors.Length];
        }

        public IMonitor GetMonitorAtPoint(int x, int y)
        {
            var screen = Screen.FromPoint(new Point(x, y));
            return _monitors.FirstOrDefault(m => m.Screen.DeviceName == screen.DeviceName) ?? _monitors[0];
        }

        public IMonitor GetMonitorAtRect(int x, int y, int width, int height)
        {
            var screen = Screen.FromRectangle(new Rectangle(x, y, width, height));
            return _monitors.FirstOrDefault(m => m.Screen.DeviceName == screen.DeviceName) ?? _monitors[0];
        }

        public IMonitor GetNextMonitor()
        {
            var index = _monitorMap[FocusedMonitor];
            if (index >= _monitors.Length - 1)
                index = 0;
            else
                index = index + 1;

            return _monitors[index];
        }

        public IMonitor GetPreviousMonitor()
        {
            var index = _monitorMap[FocusedMonitor];
            if (index == 0)
                index = _monitors.Length - 1;
            else
                index = index - 1;

            return _monitors[index];
        }

        public void OverrideFocusedMonitor(IMonitor monitor) 
        { 
            _isFocusedMonitorOverrided = true;
            _OverridedFocusedMonitor = monitor;
        }

        public void DisableOverrideFocusedMonitor()
        {
            _isFocusedMonitorOverrided = false;
        }
    }
}
