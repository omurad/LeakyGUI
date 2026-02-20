using Terminal.Gui.App;
using Terminal.Gui.Views;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Input;

// --- LeakyGUI: Minimal reproducer for OutputBase._lastOutputStringBuilder memory leak ---
//
// Bug: Terminal.Gui/Drivers/Output/OutputBase.cs line 52 + 194-197
//   StringBuilder _lastOutputStringBuilder = new();   // never cleared
//   protected virtual void Write(StringBuilder output)
//   {
//       _lastOutputStringBuilder.Append(output);      // grows forever
//   }
//
// All three output drivers (AnsiOutput, WindowsOutput, NetOutput) call base.Write(),
// feeding this unbounded accumulator every render frame.
//
// Run this app and watch the "Heap" label climb continuously with no plateau.
// Ctrl+Q to quit.

IApplication app = Application.Create ().Init ();

var window = new Window
{
    Title = "LeakyGUI — OutputBase._lastOutputStringBuilder leak reproducer",
};

var bugInfo = new Label
{
    Text = "Bug: OutputBase.cs:52 — _lastOutputStringBuilder.Append() grows forever (never cleared)",
    X = 1,
    Y = 1,
    Width = Dim.Fill (1),
};

var heapLabel = new Label
{
    Text = "Heap: ...",
    X = 1,
    Y = 3,
    Width = Dim.Fill (1),
};

var frameLabel = new Label
{
    Text = "Frame: 0",
    X = 1,
    Y = 4,
    Width = Dim.Fill (1),
};

var instrLabel = new Label
{
    Text = "Press Ctrl+Q to quit. Watch Heap grow — it never plateaus.",
    X = 1,
    Y = 6,
    Width = Dim.Fill (1),
};

window.Add (bugInfo, heapLabel, frameLabel, instrLabel);

int frameCount = 0;

app.AddTimeout (TimeSpan.FromMilliseconds (50), () =>
{
    frameCount++;
    long heapBytes = GC.GetTotalMemory (false);
    double heapMB = heapBytes / (1024.0 * 1024.0);

    heapLabel.Text = $"Heap: {heapMB:F2} MB  ({heapBytes:N0} bytes)";
    frameLabel.Text = $"Frame: {frameCount}";

    return true; // reschedule
});

app.Run (window);
app.Dispose ();
