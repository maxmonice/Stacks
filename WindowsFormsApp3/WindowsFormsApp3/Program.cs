using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;

#if NET5_0_OR_GREATER
[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif

static class StructureLimits
{
    public const int MaximumCapacity = 20;
}

class Stack
{
    private int[] _stackArray;
    private int _top;
    private int _maxSize;

    public Stack(int size)
    {
        if (size <= 0) throw new ArgumentOutOfRangeException("size");

        _maxSize = size;
        _stackArray = new int[_maxSize];
        _top = -1;
    }

    public bool IsEmpty() { return _top == -1; }

    public bool IsFull() { return _top == _maxSize - 1; }

    public bool Push(int item)
    {
        if (IsFull()) return false;
        _stackArray[++_top] = item;
        return true;
    }

    public int Pop()
    {
        if (IsEmpty()) throw new InvalidOperationException("The stack is empty.");
        return _stackArray[_top--];
    }

    public int Peek()
    {
        if (IsEmpty()) throw new InvalidOperationException("The stack is empty.");
        return _stackArray[_top];
    }

    public int GetMaxSize() { return _maxSize; }

    public int GetCount() { return _top + 1; }

    public int GetItem(int index)
    {
        if (index < 0 || index > _top) throw new ArgumentOutOfRangeException("index");
        return _stackArray[index];
    }

    public bool Resize(int newSize)
    {
        if (newSize <= 0 || newSize > StructureLimits.MaximumCapacity || newSize < GetCount()) return false;
        int[] resizedArray = new int[newSize];
        Array.Copy(_stackArray, resizedArray, GetCount());
        _stackArray = resizedArray;
        _maxSize = newSize;
        return true;
    }
}

class Queue
{
    private int[] _queueArray;
    private int _front;
    private int _rear;
    private int _count;
    private int _maxSize;

    public Queue(int size)
    {
        if (size <= 0) throw new ArgumentOutOfRangeException("size");
        _maxSize = size;
        _queueArray = new int[size];
        _front = 0;
        _rear = -1;
        _count = 0;
    }

    public bool Enqueue(int item)
    {
        if (IsFull()) return false;
        _rear = (_rear + 1) % _maxSize;
        _queueArray[_rear] = item;
        _count++;
        return true;
    }

    public int Dequeue()
    {
        if (IsEmpty()) throw new InvalidOperationException("The queue is empty.");
        int item = _queueArray[_front];
        _front = (_front + 1) % _maxSize;
        _count--;
        if (_count == 0)
        {
            _front = 0;
            _rear = -1;
        }
        return item;
    }

    public int Peek()
    {
        if (IsEmpty()) throw new InvalidOperationException("The queue is empty.");
        return _queueArray[_front];
    }

    public bool IsEmpty() { return _count == 0; }
    public bool IsFull() { return _count == _maxSize; }
    public int GetMaxSize() { return _maxSize; }
    public int GetCount() { return _count; }
    public int GetItem(int logicalIndex)
    {
        if (logicalIndex < 0 || logicalIndex >= _count) throw new ArgumentOutOfRangeException("logicalIndex");
        return _queueArray[(_front + logicalIndex) % _maxSize];
    }

    public bool Resize(int newSize)
    {
        if (newSize <= 0 || newSize > StructureLimits.MaximumCapacity || newSize < _count) return false;
        int[] resizedArray = new int[newSize];
        for (int i = 0; i < _count; i++)
            resizedArray[i] = GetItem(i);
        _queueArray = resizedArray;
        _maxSize = newSize;
        _front = 0;
        _rear = _count == 0 ? -1 : _count - 1;
        return true;
    }
}

// =====================================================================
//  Design language: a black instrument panel.
//  Near-black surfaces, light type, one electric blue. Blue marks the thing you are pointing at
//  (top, front, reading head); red is reserved for errors.
// =====================================================================

static class Ui
{
    public static readonly Color Paper = Color.FromArgb(12, 13, 16);
    public static readonly Color Paper2 = Color.FromArgb(25, 27, 34);
    public static readonly Color Side = Color.FromArgb(17, 18, 22);
    public static readonly Color Rule = Color.FromArgb(40, 43, 52);
    public static readonly Color GridLine = Color.FromArgb(44, 47, 56);
    public static readonly Color Line = Color.FromArgb(98, 103, 117);
    public static readonly Color Ink = Color.FromArgb(232, 234, 240);
    public static readonly Color Muted = Color.FromArgb(126, 131, 144);
    public static readonly Color Accent = Color.FromArgb(82, 142, 255);
    public static readonly Color AccentDk = Color.FromArgb(54, 108, 230);
    public static readonly Color Err = Color.FromArgb(255, 112, 112);

    private static Dictionary<string, Font> _fonts = new Dictionary<string, Font>();

    public static Font F(string family, float size, FontStyle style)
    {
        string key = family + "|" + size + "|" + (int)style;
        Font f;
        if (!_fonts.TryGetValue(key, out f)) { f = new Font(family, size, style); _fonts[key] = f; }
        return f;
    }
    public static Font Serif(float s, FontStyle st) { return (st & FontStyle.Bold) != 0 ? F("Segoe UI Semibold", s, FontStyle.Regular) : F("Segoe UI", s, FontStyle.Regular); }
    public static Font Mono(float s, FontStyle st) { return F("Consolas", s, st); }
    public static Font Sans(float s) { return F("Segoe UI", s, FontStyle.Regular); }
    public static Font SansBold(float s) { return F("Segoe UI Semibold", s, FontStyle.Regular); }

    // Honors the Windows "animation effects" setting, the desktop equivalent of prefers-reduced-motion.
    public static bool ReducedMotion { get { return !SystemInformation.UIEffectsEnabled; } }

    public static float Lerp(float a, float b, float t) { return a + (b - a) * t; }
    public static Color Alpha(Color c, int a) { return Color.FromArgb(Math.Max(0, Math.Min(255, a)), c); }
    public static Color Mix(Color a, Color b, float t)
    {
        return Color.FromArgb((int)(a.R + (b.R - a.R) * t), (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
    }

    // cubic-bezier(0.23, 1, 0.32, 1): a strong ease-out, so motion starts the instant the user acts.
    public static float EaseOut(float t)
    {
        const float x1 = 0.23f, y1 = 1f, x2 = 0.32f, y2 = 1f;
        float cx = 3 * x1, bx = 3 * (x2 - x1) - cx, ax = 1 - cx - bx;
        float cy = 3 * y1, by = 3 * (y2 - y1) - cy, ay = 1 - cy - by;
        float lo = 0, hi = 1, s = t;
        for (int i = 0; i < 22; i++)
        {
            float x = ((ax * s + bx) * s + cx) * s;
            if (x < t) lo = s; else hi = s;
            s = (lo + hi) / 2f;
        }
        return ((ay * s + by) * s + cy) * s;
    }

    public static StringFormat Center()
    {
        StringFormat f = new StringFormat();
        f.Alignment = StringAlignment.Center;
        f.LineAlignment = StringAlignment.Center;
        f.FormatFlags = StringFormatFlags.NoWrap;
        return f;
    }

    private static TextureBrush _grid;
    public static void DrawGrid(Graphics g, Rectangle r)
    {
        if (_grid == null)
        {
            Bitmap b = new Bitmap(24, 24);
            using (Graphics gg = Graphics.FromImage(b))
            {
                gg.Clear(Paper);
                b.SetPixel(0, 0, GridLine);
            }
            _grid = new TextureBrush(b);
        }
        g.FillRectangle(_grid, r);
    }

    public static void Tri(Graphics g, Color c, PointF a, PointF b, PointF d)
    {
        using (Brush br = new SolidBrush(c)) g.FillPolygon(br, new PointF[] { a, b, d });
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr hwnd, string subAppName, string subIdList);

    public static void DarkTitleBar(Form form)
    {
        form.HandleCreated += delegate { try { int v = 1; DwmSetWindowAttribute(form.Handle, 20, ref v, 4); } catch { } };
    }

    public static void DarkScrollbars(Control c)
    {
        EventHandler apply = delegate { try { SetWindowTheme(c.Handle, "DarkMode_Explorer", null); } catch { } };
        if (c.IsHandleCreated) apply(c, EventArgs.Empty);
        c.HandleCreated += apply;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

    public static void CueBanner(TextBox box, string text)
    {
        EventHandler apply = delegate { try { SendMessage(box.Handle, 0x1501, IntPtr.Zero, text); } catch { } };
        if (box.IsHandleCreated) apply(box, EventArgs.Empty);
        box.HandleCreated += apply;
    }
}

class Channel
{
    public long Start;
    public int Ms;
    public bool On;
    public float Raw = 1f;
    public float E = 1f;
}

// Base for everything that moves: one 15ms timer that only runs while a channel is animating.
class Stage : Panel
{
    protected Channel Slide = new Channel();
    protected Channel Shake = new Channel();
    protected Channel Pulse = new Channel();
    private Timer _timer = new Timer();
    public string Caption = "";
    public bool CaptionError;

    public Stage()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Ui.Paper;
        _timer.Interval = 15;
        _timer.Tick += delegate
        {
            bool any = false;
            Advance(Slide, ref any); Advance(Shake, ref any); Advance(Pulse, ref any);
            if (!any) _timer.Stop();
            Invalidate();
        };
    }

    private static void Advance(Channel c, ref bool any)
    {
        if (!c.On) return;
        double ms = (Stopwatch.GetTimestamp() - c.Start) * 1000.0 / Stopwatch.Frequency;
        c.Raw = (float)Math.Min(1.0, ms / c.Ms);
        c.E = Ui.EaseOut(c.Raw);
        if (c.Raw >= 1f) c.On = false; else any = true;
    }

    protected void Run(Channel c, int ms)
    {
        if (Ui.ReducedMotion) { c.On = false; c.Raw = c.E = 1f; Invalidate(); return; }
        c.Start = Stopwatch.GetTimestamp();
        c.Ms = ms;
        c.On = true;
        c.Raw = c.E = 0f;
        _timer.Start();
        Invalidate();
    }

    protected void Settle(Channel c) { c.On = false; c.Raw = c.E = 1f; Invalidate(); }

    protected float ShakeX
    {
        get { return Shake.On ? (float)Math.Sin(Shake.Raw * Math.PI * 6) * (1f - Shake.Raw) * 7f : 0f; }
    }

    public void SetCaption(string text, bool error) { Caption = text; CaptionError = error; Invalidate(); }
    public void Reject() { Run(Shake, 240); }

    protected void PaintBackdrop(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        Ui.DrawGrid(g, ClientRectangle);
        if (Caption.Length > 0)
            using (Brush b = new SolidBrush(CaptionError ? Ui.Err : Ui.Ink))
                g.DrawString(Caption, Ui.Serif(13f, FontStyle.Italic), b, new RectangleF(28, 20, Width - 56, 56));
    }

    protected static void DrawBox(Graphics g, RectangleF r, int value, bool active, float alpha, Font f)
    {
        int a = (int)(255 * Math.Max(0f, Math.Min(1f, alpha)));
        if (a <= 0) return;
        using (Brush b = new SolidBrush(Ui.Alpha(active ? Ui.Accent : Ui.Paper2, a))) g.FillRectangle(b, r);
        using (Pen p = new Pen(Ui.Alpha(active ? Ui.AccentDk : Ui.Line, a), 1.5f)) g.DrawRectangle(p, r.X, r.Y, r.Width, r.Height);
        using (Brush t = new SolidBrush(Ui.Alpha(active ? Ui.Paper : Ui.Ink, a)))
        using (StringFormat sf = Ui.Center())
            g.DrawString(value.ToString(), f, t, r, sf);
    }

    protected static void DrawEmptySlot(Graphics g, RectangleF r)
    {
        using (Pen p = new Pen(Ui.Rule))
        {
            p.DashStyle = DashStyle.Dot;
            g.DrawRectangle(p, r.X, r.Y, r.Width, r.Height);
        }
    }

    protected static Font ValueFont(float size)
    {
        return Ui.Mono((float)Math.Round(Math.Max(8f, Math.Min(15f, size)) * 2) / 2f, FontStyle.Bold);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _timer.Dispose();
        base.Dispose(disposing);
    }
}

// Vertical stack. Push drops in from above, pop lifts out, `top` slides to follow.
class StackStage : Stage
{
    private int _cap = 8;
    private int[] _items = new int[0];
    private int[] _prev = new int[0];
    private int _common;

    public void SetState(int cap, int[] items, bool animate)
    {
        _prev = animate ? _items : (int[])items.Clone();
        _items = (int[])items.Clone();
        _cap = Math.Max(1, cap);
        int c = 0;
        while (c < _prev.Length && c < _items.Length && _prev[c] == _items[c]) c++;
        _common = c;
        if (animate && !(c == _prev.Length && c == _items.Length)) Run(Slide, 200);
        else Settle(Slide);
    }

    public void PulseTop() { Run(Pulse, 420); }

    private float PointerY(int count, float bottom, float slotH)
    {
        return count == 0 ? bottom : bottom - (count - 0.5f) * slotH;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        PaintBackdrop(g);
        int count = _items.Length;
        float bottom = Height - 48f;
        float slotH = Math.Max(16f, Math.Min(52f, (bottom - 64f) / _cap));
        float w = Math.Max(120f, Math.Min(260f, Width - 320f));
        float left = (Width - w) / 2f - 30f, right = left + w;
        float tubeTop = bottom - slotH * _cap;
        float ease = Slide.E;
        Font vf = ValueFont(slotH * 0.36f);

        GraphicsState st = g.Save();
        g.TranslateTransform(ShakeX, 0);

        for (int i = 0; i < _cap; i++)
        {
            RectangleF slot = new RectangleF(left, bottom - (i + 1) * slotH, w, slotH);
            RectangleF box = RectangleF.Inflate(slot, -6, -3);
            using (Brush b = new SolidBrush(Ui.Muted))
            using (StringFormat far = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })
                g.DrawString(i.ToString(), Ui.Mono(9f, FontStyle.Regular), b, new RectangleF(left - 54, slot.Y, 40, slotH), far);
            if (i >= count) { DrawEmptySlot(g, box); continue; }
            if (i < _common) { DrawBox(g, box, _items[i], i == count - 1, 1f, vf); continue; }

            bool dropIn = _prev.Length == _common && count - _common == 1;
            RectangleF moved = box;
            float alpha;
            if (dropIn) { moved.Y -= (1f - ease) * slotH * 2.4f; alpha = Math.Min(1f, ease * 3f); }
            else { moved.Y += (1f - ease) * 10f; alpha = ease; }
            DrawBox(g, moved, _items[i], i == count - 1, alpha, vf);
        }

        if (Slide.On || Slide.E < 1f)
        {
            int removed = _prev.Length - _common;
            for (int j = _common; j < _prev.Length; j++)
            {
                RectangleF box = RectangleF.Inflate(new RectangleF(left, bottom - (j + 1) * slotH, w, slotH), -6, -3);
                bool lift = removed == 1 && _items.Length == _common;
                box.Y -= ease * slotH * (lift ? 1.6f : 0.5f);
                DrawBox(g, box, _prev[j], j == _prev.Length - 1, 1f - ease, vf);
            }
        }

        using (Pen wall = new Pen(Ui.Line, 2.5f))
        {
            wall.LineJoin = LineJoin.Miter;
            g.DrawLines(wall, new PointF[] { new PointF(left, tubeTop - 10), new PointF(left, bottom), new PointF(right, bottom), new PointF(right, tubeTop - 10) });
        }

        if (count == 0 && !Slide.On)
            using (Brush b = new SolidBrush(Ui.Muted))
            using (StringFormat sf = Ui.Center())
                g.DrawString("empty", Ui.Serif(13f, FontStyle.Italic), b, new RectangleF(left, bottom - slotH * 2, w, slotH * 2), sf);

        float py = Ui.Lerp(PointerY(_prev.Length, bottom, slotH), PointerY(count, bottom, slotH), ease);
        Ui.Tri(g, Ui.Accent, new PointF(right + 10, py), new PointF(right + 24, py - 8), new PointF(right + 24, py + 8));
        using (Brush b = new SolidBrush(Ui.Accent))
            g.DrawString("top", Ui.Serif(13f, FontStyle.Italic), b, right + 30, py - 11);
        using (Brush b = new SolidBrush(Ui.Muted))
            g.DrawString("= " + (count - 1), Ui.Mono(10f, FontStyle.Regular), b, right + 62, py - 8);

        if (Pulse.On && count > 0)
        {
            RectangleF top = RectangleF.Inflate(new RectangleF(left, bottom - count * slotH, w, slotH), -6, -3);
            top.Inflate(Pulse.E * 9f, Pulse.E * 9f);
            using (Pen p = new Pen(Ui.Alpha(Ui.Accent, (int)(220 * (1f - Pulse.E))), 2f)) g.DrawRectangle(p, top.X, top.Y, top.Width, top.Height);
        }
        g.Restore(st);
    }
}

// Horizontal pipe. Enqueue slides in at the rear, dequeue slides out at the front.
class QueueStage : Stage
{
    private int _cap = 8;
    private int[] _items = new int[0];
    private int[] _prev = new int[0];
    private int _kind; // 0 none, 1 enqueue, 2 dequeue

    public void SetState(int cap, int[] items, bool animate)
    {
        _prev = animate ? _items : (int[])items.Clone();
        _items = (int[])items.Clone();
        _cap = Math.Max(1, cap);
        _kind = 0;
        if (animate && _items.Length == _prev.Length + 1 && PrefixEquals(_prev, _items, 0, 0, _prev.Length)) _kind = 1;
        else if (animate && _prev.Length > 0 && _items.Length == _prev.Length - 1 && PrefixEquals(_prev, _items, 1, 0, _items.Length)) _kind = 2;
        if (_kind != 0) Run(Slide, 200); else Settle(Slide);
    }

    private static bool PrefixEquals(int[] a, int[] b, int aOff, int bOff, int n)
    {
        for (int i = 0; i < n; i++) if (a[aOff + i] != b[bOff + i]) return false;
        return true;
    }

    public void PulseFront() { Run(Pulse, 420); }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        PaintBackdrop(g);
        int count = _items.Length;
        float slotW = Math.Max(30f, Math.Min(92f, (Width - 240f) / _cap));
        float slotH = Math.Min(80f, slotW);
        float total = slotW * _cap;
        float left = (Width - total) / 2f;
        float top = Height * 0.5f - slotH / 2f;
        float ease = Slide.E;
        Font vf = ValueFont(slotW * 0.3f);

        GraphicsState st = g.Save();
        g.TranslateTransform(ShakeX, 0);

        for (int i = 0; i < _cap; i++)
        {
            RectangleF slot = new RectangleF(left + i * slotW, top, slotW, slotH);
            RectangleF box = RectangleF.Inflate(slot, -3, -6);
            using (Brush b = new SolidBrush(Ui.Muted))
            using (StringFormat sf = Ui.Center())
                g.DrawString(i.ToString(), Ui.Mono(9f, FontStyle.Regular), b, new RectangleF(slot.X, top + slotH + 12, slotW, 16), sf);
            if (i >= count) { DrawEmptySlot(g, box); continue; }
            RectangleF moved = box;
            float alpha = 1f;
            if (_kind == 2) moved.X += (1f - ease) * slotW;
            if (_kind == 1 && i == count - 1) { moved.X += (1f - ease) * slotW * 2f; alpha = Math.Min(1f, ease * 3f); }
            DrawBox(g, moved, _items[i], i == 0, alpha, vf);
        }
        if (_kind == 2 && (Slide.On || ease < 1f))
        {
            RectangleF ghost = RectangleF.Inflate(new RectangleF(left, top, slotW, slotH), -3, -6);
            ghost.X -= ease * slotW * 1.4f;
            DrawBox(g, ghost, _prev[0], true, 1f - ease, vf);
        }

        using (Pen rail = new Pen(Ui.Line, 2.5f))
        {
            g.DrawLine(rail, left - 14, top - 4, left + total + 14, top - 4);
            g.DrawLine(rail, left - 14, top + slotH + 4, left + total + 14, top + slotH + 4);
        }
        using (Brush b = new SolidBrush(Ui.Muted))
        using (StringFormat far = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })
        using (StringFormat near = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
        {
            g.DrawString("← dequeue", Ui.Serif(12f, FontStyle.Italic), b, new RectangleF(left - 130, top, 108, slotH), far);
            g.DrawString("enqueue →", Ui.Serif(12f, FontStyle.Italic), b, new RectangleF(left + total + 22, top, 108, slotH), near);
        }

        if (count == 0 && !Slide.On)
            using (Brush b = new SolidBrush(Ui.Muted))
            using (StringFormat sf = Ui.Center())
                g.DrawString("empty", Ui.Serif(13f, FontStyle.Italic), b, new RectangleF(left, top, total, slotH), sf);

        if (count > 0)
        {
            float fx = left + slotW / 2f;
            Ui.Tri(g, Ui.Accent, new PointF(fx, top - 12), new PointF(fx - 7, top - 25), new PointF(fx + 7, top - 25));
            using (Brush b = new SolidBrush(Ui.Accent))
            using (StringFormat sf = Ui.Center())
                g.DrawString("front", Ui.Serif(13f, FontStyle.Italic), b, new RectangleF(fx - 40, top - 50, 80, 22), sf);
            float fromRear = _prev.Length > 0 ? left + (_prev.Length - 0.5f) * slotW : left + (count - 0.5f) * slotW;
            float rx = Ui.Lerp(fromRear, left + (count - 0.5f) * slotW, ease);
            float ry = top + slotH + 36;
            Ui.Tri(g, Ui.Accent, new PointF(rx, ry), new PointF(rx - 7, ry + 13), new PointF(rx + 7, ry + 13));
            using (Brush b = new SolidBrush(Ui.Accent))
            using (StringFormat sf = Ui.Center())
                g.DrawString("rear", Ui.Serif(13f, FontStyle.Italic), b, new RectangleF(rx - 40, ry + 14, 80, 22), sf);
        }

        if (Pulse.On && count > 0)
        {
            RectangleF f = RectangleF.Inflate(new RectangleF(left, top, slotW, slotH), -3, -6);
            f.Inflate(Pulse.E * 9f, Pulse.E * 9f);
            using (Pen p = new Pen(Ui.Alpha(Ui.Accent, (int)(220 * (1f - Pulse.E))), 2f)) g.DrawRectangle(p, f.X, f.Y, f.Width, f.Height);
        }
        g.Restore(st);
    }
}

class CalcRow
{
    public string Left = "";
    public string Right = "";
    public string Digit = "";
}

class CalcStep
{
    public int Rows;
    public string[] Head = new string[0];
    public bool[] Hi = new bool[0];
    public string Caption = "";
    public string Output = "";
    public bool Error;
    public int Read;
}

class Lesson
{
    public string Heading = "";
    public string OutputLabel = "";
    public List<CalcRow> Rows = new List<CalcRow>();
    public List<CalcStep> Steps = new List<CalcStep>();
    public int Index;
}

// Worked-solution view: the expression being reduced, the calculations done so far, the answer.
class CalcView : Stage
{
    private Lesson _l;
    private CalcStep _cur, _prev;

    public void Show(Lesson l, int index, bool animate)
    {
        CalcStep next = l.Steps[index];
        bool same = _l == l && _cur != null;
        _prev = animate && same ? _cur : next;
        _l = l;
        _cur = next;
        SetCaption(next.Caption, next.Error);
        if (animate && _prev != next) Run(Slide, 200); else Settle(Slide);
    }

    private static bool SameHead(CalcStep a, CalcStep b)
    {
        if (a.Head.Length != b.Head.Length) return false;
        for (int i = 0; i < a.Head.Length; i++)
            if (a.Head[i] != b.Head[i] || a.Hi[i] != b.Hi[i]) return false;
        return true;
    }

    private void DrawHead(Graphics g, CalcStep s, float y, float alpha)
    {
        if (alpha <= 0f || s.Head.Length == 0) return;
        float size = 30f, total = 0;
        for (; size >= 16f; size -= 4f)
        {
            total = 0;
            foreach (string t in s.Head) total += g.MeasureString(t, Ui.Mono(size, FontStyle.Bold), 2000, StringFormat.GenericTypographic).Width + 24;
            if (total - 24 <= Width - 60) break;
        }
        float x = (Width - (total - 24)) / 2f;
        int a = (int)(255 * Math.Min(1f, alpha));
        using (StringFormat sf = new StringFormat(StringFormat.GenericTypographic))
            for (int i = 0; i < s.Head.Length; i++)
            {
                Font f = Ui.Mono(size, FontStyle.Bold);
                Color c = s.Hi[i] ? (s.Error ? Ui.Err : Ui.Accent) : Ui.Ink;
                using (Brush b = new SolidBrush(Ui.Alpha(c, a))) g.DrawString(s.Head[i], f, b, x, y, sf);
                x += g.MeasureString(s.Head[i], f, 2000, sf).Width + 24;
            }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        PaintBackdrop(g);
        if (_l == null || _cur == null) return;
        float ease = Slide.E;
        bool anim = Slide.On || ease < 1f;

        using (Brush m = new SolidBrush(Ui.Muted))
        using (StringFormat sf = Ui.Center())
            g.DrawString(_l.Heading.ToUpperInvariant(), Ui.SansBold(8.5f), m, new RectangleF(0, 92, Width, 16), sf);
        if (anim && _prev != _cur && !SameHead(_prev, _cur))
        {
            DrawHead(g, _prev, 118, 1f - ease);
            DrawHead(g, _cur, 118 + (1f - ease) * 6f, ease);
        }
        else DrawHead(g, _cur, 118, 1f);

        int total = _l.Rows.Count;
        float rw = Math.Min(Width - 80f, 660f), x0 = (Width - rw) / 2f, y0 = 192f;
        float rowH = Math.Max(26f, Math.Min(44f, total > 0 ? (Height - y0 - 110f) / total : 44f));
        Font rf = Ui.Mono(rowH >= 36f ? 18f : 14f, FontStyle.Regular);
        int count = Math.Max(_cur.Rows, _prev.Rows);
        for (int i = 0; i < count; i++)
        {
            bool shown = i < _cur.Rows, was = i < _prev.Rows;
            float alpha = shown == was ? 1f : (shown ? ease : 1f - ease);
            float y = y0 + i * rowH + (shown && !was ? (1f - ease) * 10f : 0f);
            int a = (int)(255 * alpha);
            bool latest = shown && i == _cur.Rows - 1;
            CalcRow row = _l.Rows[i];
            RectangleF line = new RectangleF(x0, y, rw, rowH);
            using (StringFormat mid = new StringFormat { LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap })
            using (StringFormat far = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })
            {
                using (Brush b = new SolidBrush(Ui.Alpha(Ui.Muted, a / 2)))
                    g.DrawString((i + 1).ToString(), Ui.Mono(10f, FontStyle.Regular), b, new RectangleF(x0, y, 30, rowH), mid);
                using (Brush b = new SolidBrush(Ui.Alpha(latest ? Ui.Ink : Ui.Muted, a)))
                    g.DrawString(row.Left, rf, b, new RectangleF(x0 + 34, y, rw * 0.5f, rowH), mid);
                using (Brush b = new SolidBrush(Ui.Alpha(latest ? Ui.Accent : Ui.Muted, a)))
                    g.DrawString(row.Right, rf, b, new RectangleF(x0 + rw * 0.52f, y, rw * 0.4f, rowH), mid);
                if (row.Digit.Length > 0 && _cur.Read > 0 && i >= total - _cur.Read)
                    using (Brush b = new SolidBrush(Ui.Accent))
                        g.DrawString(row.Digit, Ui.Mono(20f, FontStyle.Bold), b, new RectangleF(x0, y, rw, rowH), far);
            }
            using (Pen p = new Pen(Ui.Alpha(Ui.Rule, a))) g.DrawLine(p, x0, y + rowH - 1, x0 + rw, y + rowH - 1);
        }

        if (_cur.Output.Length > 0)
        {
            int a = (int)(255 * (_prev.Output.Length == 0 && anim ? ease : 1f));
            using (Brush m = new SolidBrush(Ui.Alpha(Ui.Muted, a)))
            using (Brush v = new SolidBrush(Ui.Alpha(Ui.Accent, a)))
            using (StringFormat sf = Ui.Center())
            {
                g.DrawString(_l.OutputLabel.ToUpperInvariant(), Ui.SansBold(8.5f), m, new RectangleF(0, Height - 92, Width, 16), sf);
                g.DrawString(_cur.Output, Ui.Mono(32f, FontStyle.Bold), v, new RectangleF(0, Height - 76, Width, 46), sf);
            }
        }
    }
}

class TabStrip : Stage
{
    private string[] _names;
    private int _sel, _prevSel, _hover = -1;
    public event Action<int> Selected;

    public TabStrip(string[] names) { _names = names; }

    private RectangleF[] Rects()
    {
        RectangleF[] r = new RectangleF[_names.Length];
        float x = 0;
        for (int i = 0; i < r.Length; i++)
        {
            float w = TextRenderer.MeasureText(_names[i], Ui.SansBold(10.5f)).Width + 12;
            r[i] = new RectangleF(x, 0, w, Height);
            x += w + 14;
        }
        return r;
    }

    public void Select(int i)
    {
        if (i == _sel) return;
        _prevSel = _sel; _sel = i;
        Run(Slide, 180);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        int h = -1;
        RectangleF[] r = Rects();
        for (int i = 0; i < r.Length; i++) if (r[i].Contains(e.Location)) h = i;
        if (h != _hover) { _hover = h; Cursor = h >= 0 ? Cursors.Hand : Cursors.Default; Invalidate(); }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e) { _hover = -1; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        RectangleF[] r = Rects();
        for (int i = 0; i < r.Length; i++)
            if (r[i].Contains(e.Location) && Selected != null) Selected(i);
        base.OnMouseDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(Ui.Paper);
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        RectangleF[] r = Rects();
        for (int i = 0; i < r.Length; i++)
        {
            Color c = i == _sel || i == _hover ? Ui.Ink : Ui.Muted;
            using (Brush b = new SolidBrush(c))
            using (StringFormat sf = Ui.Center())
                g.DrawString(_names[i], Ui.SansBold(10.5f), b, new RectangleF(r[i].X, 2, r[i].Width, Height - 2), sf);
        }
        float x = Ui.Lerp(r[_prevSel].X, r[_sel].X, Slide.E), w = Ui.Lerp(r[_prevSel].Width, r[_sel].Width, Slide.E);
        using (Brush b = new SolidBrush(Ui.Accent)) g.FillRectangle(b, x, Height - 3, w, 3);
    }
}

enum InkStyle { Primary, Secondary, Quiet, Accent }

class InkButton : Button
{
    public InkStyle Style = InkStyle.Secondary;
    private bool _hover, _down;

    public InkButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        FlatStyle = FlatStyle.Flat;
        Cursor = Cursors.Hand;
        Size = new Size(80, 34);
        Margin = new Padding(0, 0, 8, 0);
        Font = Ui.SansBold(9.5f);
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; _down = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { _down = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { _down = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(Parent != null ? Parent.BackColor : Ui.Paper);
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        // Pressing shrinks the face by a pixel each side: the 0.97 scale of the web, in pixels.
        Rectangle r = _down && Enabled ? new Rectangle(1, 1, Width - 3, Height - 3) : new Rectangle(0, 0, Width - 1, Height - 1);
        Color ink = Ui.Ink;
        if (!Enabled) ink = Ui.Mix(Ui.Paper, Ui.Ink, 0.28f);
        else if (Style == InkStyle.Primary)
        {
            using (Brush b = new SolidBrush(_down ? Ui.AccentDk : _hover ? Ui.Accent : Ui.Ink)) g.FillRectangle(b, r);
            ink = Ui.Paper;
        }
        else if (Style == InkStyle.Accent)
        {
            using (Brush b = new SolidBrush(_down ? Ui.AccentDk : _hover ? Ui.Mix(Ui.Accent, Color.White, 0.15f) : Ui.Accent)) g.FillRectangle(b, r);
            ink = Ui.Paper;
        }
        else if (Style == InkStyle.Secondary)
        {
            using (Brush b = new SolidBrush(_down ? Ui.Mix(Ui.Paper, Ui.Ink, 0.14f) : _hover ? Ui.Mix(Ui.Paper, Ui.Ink, 0.07f) : Ui.Paper)) g.FillRectangle(b, r);
            using (Pen p = new Pen(Ui.Line)) g.DrawRectangle(p, r);
        }
        else ink = _hover ? Ui.Accent : Ui.Muted;

        if (!Enabled && Style != InkStyle.Quiet)
            using (Pen p = new Pen(Ui.Rule)) g.DrawRectangle(p, r);

        using (Brush t = new SolidBrush(ink))
        using (StringFormat sf = Ui.Center())
            g.DrawString(Text, Font, t, new RectangleF(r.X, r.Y, r.Width + 1, r.Height + 1), sf);
        if (Style == InkStyle.Quiet && _hover && Enabled)
        {
            SizeF sz = g.MeasureString(Text, Font);
            using (Pen u = new Pen(Ui.Accent)) g.DrawLine(u, (Width - sz.Width) / 2f + 4, Height / 2f + 10, (Width + sz.Width) / 2f - 4, Height / 2f + 10);
        }
        if (Focused && ShowFocusCues)
            using (Pen f = new Pen(Ui.Accent) { DashStyle = DashStyle.Dot }) g.DrawRectangle(f, 3, 3, Width - 7, Height - 7);
    }
}

// Underlined field: no box, so the toolbar reads as one line of a notebook.
class LineInput : Panel
{
    private TextBox _box = new TextBox();
    public event EventHandler Submit;

    public LineInput(string cue, int width)
    {
        DoubleBuffered = true;
        Size = new Size(width, 34);
        Margin = new Padding(0, 0, 12, 0);
        BackColor = Ui.Side;
        _box.BorderStyle = BorderStyle.None;
        _box.BackColor = Ui.Side;
        _box.ForeColor = Ui.Ink;
        _box.Font = Ui.Mono(12f, FontStyle.Regular);
        _box.GotFocus += delegate { Invalidate(); };
        _box.LostFocus += delegate { Invalidate(); };
        _box.KeyDown += delegate (object s, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                if (Submit != null) Submit(this, EventArgs.Empty);
            }
        };
        Ui.CueBanner(_box, cue);
        Controls.Add(_box);
        Layout += delegate { _box.SetBounds(2, 6, Math.Max(10, Width - 4), _box.Height); };
    }

    public override string Text { get { return _box.Text; } set { _box.Text = value; } }
    public TextBox Inner { get { return _box; } }
    public new void Focus() { _box.Focus(); }
    public void SelectAllText() { _box.SelectAll(); }

    protected override void OnPaint(PaintEventArgs e)
    {
        bool f = _box.Focused;
        using (Brush b = new SolidBrush(f ? Ui.Accent : Ui.Line))
            e.Graphics.FillRectangle(b, 0, Height - (f ? 2 : 1), Width, f ? 2 : 1);
    }
}

class PostfixStep
{
    public int Number;
    public string Token;
    public string Action;
    public string StackContents;
    public string Kind;
}

// =====================================================================
//  Main form: three parts. Display (left), Controls (right), Journal (bottom).
// =====================================================================

class PaneHeader : Panel
{
    public string Title = "";
    public string Status = "";
    public bool Hot;
    public bool TopRule;

    public PaneHeader()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Ui.Side;
        Dock = DockStyle.Fill;
        Margin = new Padding(0);
    }

    public void SetTitle(string title) { Title = title; Invalidate(); }
    public void SetStatus(string status, bool hot) { Status = status; Hot = hot; Invalidate(); }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        using (Pen p = new Pen(Ui.Rule))
        {
            if (TopRule) g.DrawLine(p, 0, 0, Width, 0);
            g.DrawLine(p, 0, Height - 1, Width, Height - 1);
        }
        using (StringFormat mid = new StringFormat { LineAlignment = StringAlignment.Center })
        using (StringFormat far = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })
        {
            using (Brush b = new SolidBrush(Ui.Muted))
                g.DrawString(Title.ToUpperInvariant(), Ui.SansBold(8.5f), b, new RectangleF(20, 0, Width - 40, Height), mid);
            using (Brush b = new SolidBrush(Hot ? Ui.Accent : Ui.Muted))
                g.DrawString(Status, Ui.Sans(9f), b, new RectangleF(20, 0, Width - 40, Height), far);
        }
    }
}

class StackForm : Form
{
    private const int DefaultCapacity = 8;
    private Stack stack = new Stack(DefaultCapacity);
    private Queue queue = new Queue(DefaultCapacity);
    private bool stackStale, queueStale;
    private TabStrip tabs;
    private StackStage stackStage = new StackStage();
    private QueueStage queueStage = new QueueStage();
    private CalcView calc = new CalcView();
    private PaneHeader displayHeader = new PaneHeader();
    private FlowLayoutPanel[] bars = new FlowLayoutPanel[4];
    private FlowLayoutPanel transport = new FlowLayoutPanel();
    private LineInput stackInput, queueInput, postfixInput, binaryInput;
    private InkButton stackDisplayBtn, queueDisplayBtn, backBtn, nextBtn, playBtn, resetBtn;
    private RichTextBox journal = new RichTextBox();
    private Lesson postfixLesson, binaryLesson, lesson;
    private Timer playTimer = new Timer();
    private int tab = -1;

    public StackForm()
    {
        Text = "Stacks and Queues Laboratory";
        ClientSize = new Size(1240, 780);
        MinimumSize = new Size(1040, 660);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Ui.Paper;
        Font = Ui.Sans(10f);
        Ui.DarkTitleBar(this);

        postfixLesson = HintLesson("Expression", "Result",
            "Type a postfix expression such as 12 3 + 4 * (spaces or commas separate tokens) and press Evaluate. Each operator is worked out one step at a time.");
        binaryLesson = HintLesson("Decimal number", "Binary",
            "Enter a whole number and press Convert. It is divided by 2 step by step, and the remainders spell the binary number.");

        TableLayoutPanel root = new TableLayoutPanel();
        root.Dock = DockStyle.Fill;
        root.ColumnCount = 1;
        root.RowCount = 3;
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 1));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.BackColor = Ui.Paper;
        Controls.Add(root);

        Panel header = new Panel();
        header.Dock = DockStyle.Fill;
        header.Margin = new Padding(0);
        header.BackColor = Ui.Paper;
        header.Paint += delegate (object s, PaintEventArgs e)
        {
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            using (Brush b = new SolidBrush(Ui.Ink))
                e.Graphics.DrawString("Stacks & Queues", Ui.Serif(17f, FontStyle.Bold), b, 28, 12);
        };
        tabs = new TabStrip(new string[] { "Stack", "Queue", "Postfix", "Binary" });
        tabs.SetBounds(262, 0, 520, 56);
        tabs.Selected += delegate (int i) { SelectTab(i); };
        header.Controls.Add(tabs);
        root.Controls.Add(header, 0, 0);
        root.Controls.Add(Rule(), 0, 1);

        // Body: [display + journal] | rule | [controls]
        TableLayoutPanel body = new TableLayoutPanel();
        body.Dock = DockStyle.Fill;
        body.ColumnCount = 3;
        body.RowCount = 1;
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
        body.Margin = new Padding(0);
        root.Controls.Add(body, 0, 2);

        TableLayoutPanel left = new TableLayoutPanel();
        left.Dock = DockStyle.Fill;
        left.ColumnCount = 1;
        left.RowCount = 4;
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));
        left.Margin = new Padding(0);
        body.Controls.Add(left, 0, 0);
        body.Controls.Add(Rule(), 1, 0);

        displayHeader.SetTitle("Display");
        left.Controls.Add(displayHeader, 0, 0);

        Panel host = new Panel();
        host.Dock = DockStyle.Fill;
        host.Margin = new Padding(0);
        stackStage.Dock = DockStyle.Fill;
        queueStage.Dock = DockStyle.Fill;
        calc.Dock = DockStyle.Fill;
        host.Controls.Add(stackStage);
        host.Controls.Add(queueStage);
        host.Controls.Add(calc);
        left.Controls.Add(host, 0, 1);

        PaneHeader journalHeader = new PaneHeader();
        journalHeader.TopRule = true;
        journalHeader.SetTitle("Journal");
        InkButton clear = Btn("Clear", InkStyle.Quiet, delegate { journal.Clear(); });
        clear.Size = new Size(56, 26);
        clear.Margin = new Padding(0);
        clear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        clear.Location = new Point(journalHeader.Width - 68, 4);
        journalHeader.Layout += delegate { clear.Location = new Point(journalHeader.Width - clear.Width - 12, 4); };
        journalHeader.Controls.Add(clear);
        left.Controls.Add(journalHeader, 0, 2);

        Panel journalPanel = new Panel();
        journalPanel.Dock = DockStyle.Fill;
        journalPanel.Margin = new Padding(0);
        journalPanel.BackColor = Ui.Paper;
        journalPanel.Padding = new Padding(20, 10, 12, 8);
        journal.Dock = DockStyle.Fill;
        journal.ReadOnly = true;
        journal.BorderStyle = BorderStyle.None;
        journal.BackColor = Ui.Paper;
        journal.ForeColor = Ui.Ink;
        journal.Font = Ui.Mono(10f, FontStyle.Regular);
        journal.DetectUrls = false;
        journal.TabStop = false;
        Ui.DarkScrollbars(journal);
        journalPanel.Controls.Add(journal);
        left.Controls.Add(journalPanel, 0, 3);

        // Controls (right)
        TableLayoutPanel right = new TableLayoutPanel();
        right.Dock = DockStyle.Fill;
        right.ColumnCount = 1;
        right.RowCount = 2;
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.Margin = new Padding(0);
        right.BackColor = Ui.Side;
        body.Controls.Add(right, 2, 0);
        PaneHeader controlsHeader = new PaneHeader();
        controlsHeader.SetTitle("Controls");
        right.Controls.Add(controlsHeader, 0, 0);
        Panel barHost = new Panel();
        barHost.Dock = DockStyle.Fill;
        barHost.Margin = new Padding(0);
        barHost.BackColor = Ui.Side;
        barHost.SizeChanged += delegate { FitBars(); };
        right.Controls.Add(barHost, 0, 1);

        BuildStructureBar(0, "stack", out stackInput, out stackDisplayBtn, DoPush, DoPop, DoStackPeek,
            delegate { Log("IS EMPTY", stack.IsEmpty() ? "The stack is empty." : "The stack is not empty."); },
            delegate { Log("IS FULL", stack.IsFull() ? "The stack is full." : "The stack is not full."); },
            DoStackDisplay, DoResizeStack);
        BuildStructureBar(1, "queue", out queueInput, out queueDisplayBtn, DoEnqueue, DoDequeue, DoQueuePeek,
            delegate { Log("IS EMPTY", queue.IsEmpty() ? "The queue is empty." : "The queue is not empty."); },
            delegate { Log("IS FULL", queue.IsFull() ? "The queue is full." : "The queue is not full."); },
            DoQueueDisplay, DoResizeQueue);
        BuildLessonBars();
        foreach (FlowLayoutPanel b in bars) barHost.Controls.Add(b);

        playTimer.Interval = 1000;
        playTimer.Tick += delegate { if (lesson.Index < lesson.Steps.Count - 1) GoTo(lesson.Index + 1, true); else StopPlay(); };

        Log("READY", "Use the controls on the right. The display only changes when you press Display.");
        RefreshStack(false);
        RefreshQueue(false);
        SelectTab(0);
    }

    private static Panel Rule()
    {
        Panel p = new Panel();
        p.Dock = DockStyle.Fill;
        p.Margin = new Padding(0);
        p.BackColor = Ui.Rule;
        return p;
    }

    // ---- Controls panel ------------------------------------------------------

    private FlowLayoutPanel NewBar(int index)
    {
        FlowLayoutPanel f = new FlowLayoutPanel();
        f.Dock = DockStyle.Fill;
        f.FlowDirection = FlowDirection.TopDown;
        f.WrapContents = false;
        f.Padding = new Padding(20, 18, 20, 16);
        f.BackColor = Ui.Side;
        bars[index] = f;
        return f;
    }

    private InkButton Btn(string text, InkStyle style, Action click)
    {
        InkButton b = new InkButton();
        b.Text = text;
        b.Style = style;
        b.Height = 38;
        b.Margin = new Padding(0, 0, 0, 8);
        b.Click += delegate { click(); };
        return b;
    }

    private InkButton Gap(InkButton b)
    {
        b.Margin = new Padding(0, 0, 0, 20);
        return b;
    }

    private Label Section(string text)
    {
        Label l = new Label();
        l.Text = text.ToUpperInvariant();
        l.AutoSize = false;
        l.Height = 22;
        l.Margin = new Padding(0, 0, 0, 2);
        l.TextAlign = ContentAlignment.BottomLeft;
        l.Font = Ui.SansBold(8.5f);
        l.ForeColor = Ui.Muted;
        l.BackColor = Ui.Side;
        return l;
    }

    private void BuildStructureBar(int index, string noun, out LineInput input, out InkButton displayBtn,
        Action push, Action pop, Action peek, Action isEmpty, Action isFull, Action display, Action resize)
    {
        FlowLayoutPanel bar = NewBar(index);
        bool isStack = index == 0;
        bar.Controls.Add(Section("Value"));
        input = new LineInput("a whole number", 260);
        input.Margin = new Padding(0, 0, 0, 20);
        input.Submit += delegate { push(); };
        bar.Controls.Add(input);
        bar.Controls.Add(Btn(isStack ? "Push" : "Enqueue", InkStyle.Primary, push));
        bar.Controls.Add(Btn(isStack ? "Pop" : "Dequeue", InkStyle.Secondary, pop));
        bar.Controls.Add(Gap(Btn("Peek", InkStyle.Secondary, peek)));
        bar.Controls.Add(Btn("Is Empty", InkStyle.Secondary, isEmpty));
        bar.Controls.Add(Gap(Btn("Is Full", InkStyle.Secondary, isFull)));
        displayBtn = Btn("Display", InkStyle.Secondary, display);
        bar.Controls.Add(displayBtn);
        bar.Controls.Add(Btn("Resize", InkStyle.Secondary, resize));
    }

    private void BuildLessonBars()
    {
        FlowLayoutPanel p = NewBar(2);
        p.Controls.Add(Section("Postfix expression"));
        postfixInput = new LineInput("e.g. 12 3 + 4 *", 260);
        postfixInput.Text = "12 3 + 4 *";
        postfixInput.Margin = new Padding(0, 0, 0, 20);
        postfixInput.Submit += delegate { DoPostfixEvaluation(); };
        p.Controls.Add(postfixInput);
        p.Controls.Add(Btn("Evaluate", InkStyle.Primary, DoPostfixEvaluation));

        FlowLayoutPanel b = NewBar(3);
        b.Controls.Add(Section("Decimal number"));
        binaryInput = new LineInput("a whole number", 260);
        binaryInput.Text = "13";
        binaryInput.Margin = new Padding(0, 0, 0, 20);
        binaryInput.Submit += delegate { DoDecimalToBinary(); };
        b.Controls.Add(binaryInput);
        b.Controls.Add(Btn("Convert", InkStyle.Primary, DoDecimalToBinary));

        transport.FlowDirection = FlowDirection.TopDown;
        transport.WrapContents = false;
        transport.AutoSize = true;
        transport.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        transport.BackColor = Ui.Side;
        transport.Margin = new Padding(0, 12, 0, 0);
        transport.Padding = new Padding(0);
        transport.Controls.Add(Section("Step through"));
        backBtn = Btn("◀  Back", InkStyle.Secondary, delegate { StopPlay(); GoTo(lesson.Index - 1, true); });
        nextBtn = Btn("Next  ▶", InkStyle.Secondary, delegate { StopPlay(); GoTo(lesson.Index + 1, true); });
        playBtn = Btn("Play", InkStyle.Secondary, TogglePlay);
        resetBtn = Btn("Reset", InkStyle.Secondary, delegate { StopPlay(); GoTo(0, true); });
        transport.Controls.Add(backBtn);
        transport.Controls.Add(nextBtn);
        transport.Controls.Add(playBtn);
        transport.Controls.Add(resetBtn);
    }

    private void FitBars()
    {
        foreach (FlowLayoutPanel b in bars)
        {
            if (b == null) continue;
            int w = Math.Max(100, b.ClientSize.Width - b.Padding.Horizontal);
            foreach (Control c in b.Controls)
            {
                c.Width = w;
                if (c == transport) foreach (Control t in transport.Controls) t.Width = w;
            }
        }
    }

    // ---- Tabs ----------------------------------------------------------------

    private void SelectTab(int i)
    {
        StopPlay();
        tab = i;
        tabs.Select(i);
        stackStage.Visible = i == 0;
        queueStage.Visible = i == 1;
        calc.Visible = i >= 2;
        for (int k = 0; k < bars.Length; k++) bars[k].Visible = k == i;
        displayHeader.SetTitle(i == 0 ? "Stack display" : i == 1 ? "Queue display" : i == 2 ? "Postfix calculation" : "Binary calculation");
        if (i >= 2)
        {
            bars[i].Controls.Add(transport);
            ShowLesson(i == 2 ? postfixLesson : binaryLesson);
        }
        FitBars();
        UpdateStatus();
        if (i == 0) stackInput.Focus();
        else if (i == 1) queueInput.Focus();
        else if (i == 2) postfixInput.Focus();
        else binaryInput.Focus();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData >= (Keys.Control | Keys.D1) && keyData <= (Keys.Control | Keys.D4))
        {
            SelectTab((int)(keyData & Keys.KeyCode) - (int)Keys.D1);
            return true;
        }
        bool typing = postfixInput.Inner.Focused || binaryInput.Inner.Focused;
        if (tab >= 2 && !typing)
        {
            if (keyData == Keys.Right) { StopPlay(); GoTo(lesson.Index + 1, true); return true; }
            if (keyData == Keys.Left) { StopPlay(); GoTo(lesson.Index - 1, true); return true; }
            if (keyData == Keys.Space) { TogglePlay(); return true; }
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    // The display shows a snapshot. It only changes when Display is pressed, so we tell the user when it is behind.
    private void UpdateStatus()
    {
        bool stale = tab == 0 ? stackStale : tab == 1 ? queueStale : false;
        if (tab <= 1)
            displayHeader.SetStatus(stale ? "Out of date — press Display to update" : "Up to date", stale);
        else if (lesson != null)
            displayHeader.SetStatus(lesson.Steps.Count > 1 ? "Step " + lesson.Index + " of " + (lesson.Steps.Count - 1) : "", false);
        stackDisplayBtn.Style = stackStale ? InkStyle.Accent : InkStyle.Secondary;
        queueDisplayBtn.Style = queueStale ? InkStyle.Accent : InkStyle.Secondary;
        stackDisplayBtn.Invalidate();
        queueDisplayBtn.Invalidate();
    }

    // ---- Stack / queue operations -------------------------------------------

    private void RefreshStack(bool animate)
    {
        int[] a = new int[stack.GetCount()];
        for (int i = 0; i < a.Length; i++) a[i] = stack.GetItem(i);
        stackStage.SetState(stack.GetMaxSize(), a, animate);
        stackStale = false;
        UpdateStatus();
    }

    private void RefreshQueue(bool animate)
    {
        int[] a = new int[queue.GetCount()];
        for (int i = 0; i < a.Length; i++) a[i] = queue.GetItem(i);
        queueStage.SetState(queue.GetMaxSize(), a, animate);
        queueStale = false;
        UpdateStatus();
    }

    private void Fail(Stage stage, string message)
    {
        stage.Reject();
        Log("ERROR", message);
    }

    private bool ReadValue(LineInput input, Stage stage, string verb, out int value)
    {
        if (int.TryParse(input.Text.Trim(), out value)) return true;
        Fail(stage, "Invalid input. Enter a whole number to " + verb + ".");
        input.Focus();
        input.SelectAllText();
        return false;
    }

    private void DoPush()
    {
        if (stack.IsFull())
        {
            Fail(stackStage, "Stack overflow. Cannot push because the stack is full.");
            return;
        }
        int value;
        if (!ReadValue(stackInput, stackStage, "push", out value)) return;
        stack.Push(value);
        stackStale = true;
        Log("PUSH", "Added " + value + " to the stack. It is now the top.");
        UpdateStatus();
        stackInput.Text = "";
        stackInput.Focus();
    }

    private void DoPop()
    {
        if (stack.IsEmpty())
        {
            Fail(stackStage, "Stack underflow. Cannot pop because the stack is empty.");
            return;
        }
        int value = stack.Pop();
        stackStale = true;
        Log("POP", "Removed " + value + " from the stack (last in, first out).");
        UpdateStatus();
    }

    private void DoStackPeek()
    {
        if (stack.IsEmpty())
        {
            Fail(stackStage, "Cannot peek because the stack is empty.");
            return;
        }
        if (!stackStale) stackStage.PulseTop();
        Log("PEEK", "Top element: " + stack.Peek() + ".");
    }

    private void DoStackDisplay()
    {
        RefreshStack(true);
        if (stack.GetCount() == 0)
        {
            Log("DISPLAY", "Stack visualization refreshed. Stack is empty.");
            return;
        }
        StringBuilder message = new StringBuilder("Stack visualization refreshed. Contents (top to bottom): ");
        for (int i = stack.GetCount() - 1; i >= 0; i--)
        {
            message.Append(stack.GetItem(i));
            if (i > 0) message.Append(", ");
        }
        message.Append('.');
        Log("DISPLAY", message.ToString());
    }

    private int AskCapacity(string noun, int current, int count, Func<int, bool> resize)
    {
        string hint = null;
        while (true)
        {
            string input = Program.Prompt("Enter the new " + noun + " capacity (1 to " + StructureLimits.MaximumCapacity + "):",
                "Resize " + noun, current.ToString(), hint);
            if (input == null) return 0;
            int size;
            if (int.TryParse(input.Trim(), out size) && resize(size)) return size;
            hint = "Must be 1 to " + StructureLimits.MaximumCapacity + " and at least the current item count (" + count + ").";
            Log("ERROR", "The " + noun + " capacity must be from 1 to " + StructureLimits.MaximumCapacity
                + " and cannot be less than the current item count (" + count + ").");
        }
    }

    private void DoResizeStack()
    {
        int size = AskCapacity("stack", stack.GetMaxSize(), stack.GetCount(), stack.Resize);
        if (size == 0) return;
        stackStale = true;
        Log("RESIZE", "Stack capacity changed to " + size + ". Press Display to refresh the visualization.");
        UpdateStatus();
    }

    private void DoEnqueue()
    {
        if (queue.IsFull())
        {
            Fail(queueStage, "Queue overflow. Cannot enqueue because the queue is full.");
            return;
        }
        int value;
        if (!ReadValue(queueInput, queueStage, "enqueue", out value)) return;
        queue.Enqueue(value);
        queueStale = true;
        Log("ENQUEUE", "Added " + value + " to the rear of the queue.");
        UpdateStatus();
        queueInput.Text = "";
        queueInput.Focus();
    }

    private void DoDequeue()
    {
        if (queue.IsEmpty())
        {
            Fail(queueStage, "Queue underflow. Cannot dequeue because the queue is empty.");
            return;
        }
        int value = queue.Dequeue();
        queueStale = true;
        Log("DEQUEUE", "Removed " + value + " from the front of the queue (first in, first out).");
        UpdateStatus();
    }

    private void DoQueuePeek()
    {
        if (queue.IsEmpty())
        {
            Fail(queueStage, "Cannot peek because the queue is empty.");
            return;
        }
        if (!queueStale) queueStage.PulseFront();
        Log("PEEK", "Front element: " + queue.Peek() + ".");
    }

    private void DoQueueDisplay()
    {
        RefreshQueue(true);
        if (queue.GetCount() == 0)
        {
            Log("DISPLAY", "Queue visualization refreshed. Queue is empty.");
            return;
        }
        StringBuilder message = new StringBuilder("Queue visualization refreshed. Front to rear: ");
        for (int i = 0; i < queue.GetCount(); i++)
        {
            message.Append(queue.GetItem(i));
            if (i < queue.GetCount() - 1) message.Append(", ");
        }
        message.Append('.');
        Log("DISPLAY", message.ToString());
    }

    private void DoResizeQueue()
    {
        int size = AskCapacity("queue", queue.GetMaxSize(), queue.GetCount(), queue.Resize);
        if (size == 0) return;
        queueStale = true;
        Log("RESIZE", "Queue capacity changed to " + size + ". Press Display to refresh the visualization.");
        UpdateStatus();
    }

    // ---- Calculations (postfix, binary) ----------------------------------------

    private static Lesson HintLesson(string heading, string outLabel, string hint)
    {
        Lesson l = new Lesson();
        l.Heading = heading;
        l.OutputLabel = outLabel;
        CalcStep s = new CalcStep();
        s.Caption = hint;
        l.Steps.Add(s);
        return l;
    }

    private static string Sym(string t)
    {
        return t == "*" ? "×" : t == "/" ? "÷" : t == "$" ? "^" : t;
    }

    // "12 + 3 = 15." becomes "12 + 3 = 15" with a proper operator sign.
    private static string Calc(string action)
    {
        string a = action.TrimEnd('.');
        int eq = a.IndexOf(" = ");
        if (eq < 0) return a;
        string[] p = a.Substring(0, eq).Split(' ');
        return p.Length == 3 ? p[0] + " " + Sym(p[1]) + " " + p[2] + a.Substring(eq) : a;
    }

    private static int[] ParseStack(string contents)
    {
        string inner = contents.Trim('[', ']').Trim();
        if (inner.Length == 0) return new int[0];
        string[] parts = inner.Split(',');
        int[] a = new int[parts.Length];
        for (int i = 0; i < a.Length; i++) a[i] = int.Parse(parts[i].Trim());
        return a;
    }

    private static Lesson BuildPostfixLesson(List<PostfixStep> steps, int result)
    {
        Lesson l = new Lesson();
        l.Heading = "Expression";
        l.OutputLabel = "Result";
        List<string> tokens = new List<string>();
        foreach (PostfixStep s in steps)
            if (s.Kind != "RESULT" && s.Token != "END") tokens.Add(s.Token);

        CalcStep prev = new CalcStep();
        prev.Head = tokens.ConvertAll(Sym).ToArray();
        prev.Hi = new bool[prev.Head.Length];
        prev.Caption = "Read left to right. The first operator works on the two numbers just before it. Replace all three with the answer, then repeat.";
        l.Steps.Add(prev);

        int k = -1;
        foreach (PostfixStep s in steps)
        {
            if (s.Kind != "RESULT" && s.Token != "END") k++;
            if (s.Kind == "OPERAND") continue;
            CalcStep cs = new CalcStep();
            cs.Rows = prev.Rows;
            cs.Head = prev.Head;
            cs.Hi = new bool[prev.Head.Length];
            if (s.Kind == "OPERATOR")
            {
                int[] st = ParseStack(s.StackContents);
                List<string> head = new List<string>();
                foreach (int v in st) head.Add(v.ToString());
                for (int j = k + 1; j < tokens.Count; j++) head.Add(Sym(tokens[j]));
                cs.Head = head.ToArray();
                cs.Hi = new bool[head.Count];
                cs.Hi[st.Length - 1] = true;
                string calc = Calc(s.Action);
                CalcRow row = new CalcRow();
                row.Left = calc;
                row.Right = "→ " + string.Join(" ", cs.Head);
                l.Rows.Add(row);
                cs.Rows = l.Rows.Count;
                cs.Caption = "Operator " + Sym(s.Token) + " works on the two numbers before it: " + calc + ". Replace all three with " + st[st.Length - 1] + ".";
            }
            else if (s.Kind == "RESULT")
            {
                if (cs.Hi.Length > 0) cs.Hi[0] = true;
                cs.Output = result.ToString();
                cs.Caption = "Only one number is left, so that is the answer: " + result + ".";
            }
            else
            {
                cs.Error = true;
                cs.Caption = s.Action;
                if (s.Action.Contains("Unsupported") || s.Action.Contains("insufficient"))
                {
                    int[] st = ParseStack(s.StackContents);
                    List<string> head = new List<string>();
                    foreach (int v in st) head.Add(v.ToString());
                    for (int j = k; j < tokens.Count; j++) head.Add(Sym(tokens[j]));
                    cs.Head = head.ToArray();
                    cs.Hi = new bool[head.Count];
                    if (st.Length < head.Count) cs.Hi[st.Length] = true;
                }
            }
            l.Steps.Add(cs);
            prev = cs;
        }
        return l;
    }

    private static Lesson BuildBinaryLesson(int value, string binary)
    {
        Lesson l = new Lesson();
        l.Heading = "Decimal number";
        l.OutputLabel = "Binary";
        CalcStep start = new CalcStep();
        start.Head = new string[] { value.ToString() };
        start.Hi = new bool[1];
        start.Caption = "Divide by 2 again and again, keeping each remainder. Stop when the quotient reaches 0.";
        l.Steps.Add(start);

        List<int> rem = new List<int>();
        int n = value;
        do
        {
            int r = n % 2;
            rem.Add(r);
            CalcRow row = new CalcRow();
            row.Left = n + " ÷ 2 = " + (n / 2);
            row.Right = "remainder " + r;
            row.Digit = r.ToString();
            l.Rows.Add(row);
            CalcStep cs = new CalcStep();
            cs.Head = start.Head;
            cs.Hi = start.Hi;
            cs.Rows = l.Rows.Count;
            cs.Caption = n + " ÷ 2 = " + (n / 2) + " with remainder " + r + ". Keep the " + r + ".";
            l.Steps.Add(cs);
            n /= 2;
        } while (n > 0);

        string written = "";
        for (int j = 1; j <= rem.Count; j++)
        {
            written += rem[rem.Count - j];
            CalcStep cs = new CalcStep();
            cs.Head = start.Head;
            cs.Hi = start.Hi;
            cs.Rows = rem.Count;
            cs.Read = j;
            cs.Output = written;
            cs.Caption = j == rem.Count
                ? "Done. " + value + " in decimal is " + binary + " in binary."
                : "Now read the remainders from the bottom up. The last one found is the first digit.";
            l.Steps.Add(cs);
        }
        return l;
    }

    private void ShowLesson(Lesson l)
    {
        lesson = l;
        calc.Show(l, l.Index, false);
        SyncTransport();
        UpdateStatus();
    }

    private void GoTo(int i, bool animate)
    {
        lesson.Index = Math.Max(0, Math.Min(lesson.Steps.Count - 1, i));
        calc.Show(lesson, lesson.Index, animate);
        SyncTransport();
        UpdateStatus();
    }

    private void SyncTransport()
    {
        int last = lesson.Steps.Count - 1;
        bool ready = last > 0;
        backBtn.Enabled = ready && lesson.Index > 0;
        resetBtn.Enabled = ready && lesson.Index > 0;
        nextBtn.Enabled = ready && lesson.Index < last;
        playBtn.Enabled = ready;
        playBtn.Text = playTimer.Enabled ? "Pause" : "Play";
        playBtn.Invalidate();
    }

    private void TogglePlay()
    {
        if (lesson.Steps.Count < 2) return;
        if (playTimer.Enabled) { StopPlay(); return; }
        if (lesson.Index >= lesson.Steps.Count - 1) GoTo(0, false);
        playTimer.Start();
        SyncTransport();
    }

    private void StopPlay()
    {
        playTimer.Stop();
        if (lesson != null && tab >= 2) SyncTransport();
    }

    private void StartLesson(Lesson l)
    {
        ShowLesson(l);
        TogglePlay();
    }

    private void DoPostfixEvaluation()
    {
        string expression = postfixInput.Text;
        int result;
        string error;
        List<PostfixStep> steps;
        bool succeeded = Program.TryEvaluatePostfix(expression, out result, out error, out steps);
        postfixLesson = BuildPostfixLesson(steps, result);
        if (succeeded) Log("POSTFIX", "Result for " + expression.Trim() + " = " + result + ".");
        else Log("ERROR", "Postfix evaluation failed: " + error);
        StopPlay();
        StartLesson(postfixLesson);
    }

    private void DoDecimalToBinary()
    {
        int value;
        string binary;
        if (!int.TryParse(binaryInput.Text.Trim(), out value) || !Program.TryConvertDecimalToBinary(value, out binary))
        {
            binaryLesson = HintLesson("Decimal number", "Binary", "Enter a non-negative whole number to convert.");
            binaryLesson.Steps[0].Error = true;
            ShowLesson(binaryLesson);
            Log("ERROR", "Enter a non-negative whole number to convert.");
            return;
        }
        binaryLesson = BuildBinaryLesson(value, binary);
        Log("BINARY", "Decimal " + value + " = binary " + binary + ".");
        StopPlay();
        StartLesson(binaryLesson);
    }

    // ---- Journal -------------------------------------------------------------

    private void Log(string op, string message)
    {
        bool error = op == "ERROR";
        journal.SelectionStart = journal.TextLength;
        journal.SelectionLength = 0;
        journal.SelectionFont = Ui.Mono(10f, FontStyle.Regular);
        journal.SelectionColor = Ui.Muted;
        journal.AppendText(DateTime.Now.ToString("HH:mm:ss") + "  ");
        journal.SelectionFont = Ui.Mono(10f, FontStyle.Bold);
        journal.SelectionColor = error ? Ui.Err : Ui.Ink;
        journal.AppendText(op.PadRight(9));
        journal.SelectionFont = Ui.Mono(10f, FontStyle.Regular);
        journal.SelectionColor = error ? Ui.Err : Ui.Mix(Ui.Ink, Ui.Muted, 0.3f);
        journal.AppendText(message + "\n");
        journal.SelectionStart = journal.TextLength;
        journal.ScrollToCaret();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) playTimer.Dispose();
        base.Dispose(disposing);
    }
}

// ---------------- Program class ----------------
class Program
{
    public static bool TryConvertDecimalToBinary(int value, out string binary)
    {
        binary = null;
        if (value < 0) return false;
        if (value == 0)
        {
            binary = "0";
            return true;
        }
        Stack remainders = new Stack(32);
        int remaining = value;
        while (remaining > 0)
        {
            remainders.Push(remaining % 2);
            remaining /= 2;
        }
        StringBuilder result = new StringBuilder();
        while (!remainders.IsEmpty()) result.Append(remainders.Pop());
        binary = result.ToString();
        return true;
    }

    private static int IntegerPower(int baseValue, int exponent)
    {
        if (exponent < 0) throw new ArgumentOutOfRangeException("exponent");
        int result = 1;
        checked
        {
            while (exponent > 0)
            {
                if ((exponent & 1) == 1) result *= baseValue;
                exponent >>= 1;
                if (exponent > 0) baseValue *= baseValue;
            }
        }
        return result;
    }
    public static bool TryEvaluatePostfix(string expression, out int result, out string error)
    {
        List<PostfixStep> ignoredSteps;
        return TryEvaluatePostfix(expression, out result, out error, out ignoredSteps);
    }

    public static bool TryEvaluatePostfix(string expression, out int result, out string error, out List<PostfixStep> steps)
    {
        result = 0;
        error = null;
        steps = new List<PostfixStep>();
        if (expression == null) expression = "";
        expression = expression.Replace(',', ' '); // a comma separates tokens exactly like a space
        Stack values = new Stack(Math.Max(1, expression.Length));
        bool hasWhitespace = false;
        for (int i = 0; i < expression.Length; i++)
        {
            if (char.IsWhiteSpace(expression[i]))
            {
                hasWhitespace = true;
                break;
            }
        }

        string[] tokens;
        if (hasWhitespace)
            tokens = expression.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        else
        {
            tokens = new string[expression.Length];
            for (int i = 0; i < expression.Length; i++) tokens[i] = expression[i].ToString();
        }

        string currentToken = "END";
        try
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                currentToken = token;
                int operand;
                if (int.TryParse(token, out operand) && operand >= 0)
                {
                    values.Push(operand);
                    AddPostfixStep(steps, token, "Push operand " + operand + ".", values, "OPERAND");
                    continue;
                }
                if (token != "+" && token != "-" && token != "*" && token != "/" && token != "^" && token != "$")
                {
                    error = "Unsupported token '" + token + "'.";
                    AddPostfixStep(steps, token, "Error: " + error, values, "ERROR");
                    return false;
                }
                if (values.GetCount() < 2)
                {
                    error = "The expression has insufficient operands for '" + token + "'.";
                    AddPostfixStep(steps, token, "Error: " + error, values, "ERROR");
                    return false;
                }

                int right = values.Pop();
                int left = values.Pop();
                if ((token == "^" || token == "$") && right < 0)
                {
                    error = "The exponent must be a non-negative whole number.";
                    AddPostfixStep(steps, token, "Error: " + error, values, "ERROR");
                    return false;
                }
                int intermediate;
                checked
                {
                    switch (token)
                    {
                        case "+": intermediate = left + right; break;
                        case "-": intermediate = left - right; break;
                        case "*": intermediate = left * right; break;
                        case "^":
                        case "$": intermediate = IntegerPower(left, right); break;
                        default:
                            if (right == 0)
                            {
                                error = "Division by zero is not allowed.";
                                AddPostfixStep(steps, token, "Error: " + error, values, "ERROR");
                                return false;
                            }
                            intermediate = left / right;
                            break;
                    }
                }
                values.Push(intermediate);
                AddPostfixStep(steps, token, left + " " + token + " " + right + " = " + intermediate + ".", values, "OPERATOR");
            }
        }
        catch (OverflowException)
        {
            error = "The expression result is outside the supported integer range.";
            AddPostfixStep(steps, currentToken, "Error: " + error, values, "ERROR");
            return false;
        }

        if (values.GetCount() != 1)
        {
            error = "The expression must leave exactly one value on the stack.";
            AddPostfixStep(steps, "END", "Error: " + error, values, "ERROR");
            return false;
        }
        result = values.Pop();
        values.Push(result);
        AddPostfixStep(steps, "RESULT", "Final result: " + result + ".", values, "RESULT");
        return true;
    }

    private static void AddPostfixStep(List<PostfixStep> steps, string token, string action, Stack values, string kind)
    {
        StringBuilder contents = new StringBuilder();
        contents.Append("[");
        for (int i = 0; i < values.GetCount(); i++)
        {
            if (i > 0) contents.Append(", ");
            contents.Append(values.GetItem(i));
        }
        contents.Append("]");
        PostfixStep step = new PostfixStep();
        step.Number = steps.Count + 1;
        step.Token = token;
        step.Action = action;
        step.StackContents = contents.ToString();
        step.Kind = kind;
        steps.Add(step);
    }

    // Dark input dialog for Resize. Returns null when cancelled; "hint" shows an inline validation message.
    public static string Prompt(string text, string title, string defaultValue, string hint)
    {
        using (Form form = new Form())
        {
            form.Text = title;
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.ShowInTaskbar = false;
            form.BackColor = Ui.Side;
            form.ForeColor = Ui.Ink;
            form.Font = Ui.Sans(10f);
            Ui.DarkTitleBar(form);
            int width = 440, pad = 24;

            Label label = new Label();
            label.Text = text;
            label.AutoSize = false;
            label.ForeColor = Ui.Ink;
            label.BackColor = Ui.Side;
            label.Width = width - pad * 2;
            label.Height = label.GetPreferredSize(new Size(label.Width, 0)).Height;
            label.Location = new Point(pad, pad);

            LineInput box = new LineInput("", width - pad * 2);
            box.Text = defaultValue;
            box.Location = new Point(pad, label.Bottom + 14);

            Label hintLabel = new Label();
            hintLabel.AutoSize = false;
            hintLabel.Text = hint ?? "";
            hintLabel.ForeColor = Ui.Err;
            hintLabel.BackColor = Ui.Side;
            hintLabel.Font = Ui.Sans(9f);
            hintLabel.SetBounds(pad, box.Bottom + 6, width - pad * 2, hint == null ? 6 : 22);

            InkButton ok = new InkButton();
            ok.Text = "OK";
            ok.Style = InkStyle.Primary;
            ok.DialogResult = DialogResult.OK;
            ok.SetBounds(width - pad - 100, hintLabel.Bottom + 10, 100, 38);
            InkButton cancel = new InkButton();
            cancel.Text = "Cancel";
            cancel.DialogResult = DialogResult.Cancel;
            cancel.SetBounds(width - pad - 210, ok.Top, 100, 38);
            box.Submit += delegate { ok.PerformClick(); };

            form.ClientSize = new Size(width, ok.Bottom + pad);
            form.Controls.AddRange(new Control[] { label, box, hintLabel, cancel, ok });
            form.AcceptButton = ok;
            form.CancelButton = cancel;
            form.Shown += delegate { box.Focus(); box.SelectAllText(); };
            return form.ShowDialog(Form.ActiveForm) == DialogResult.OK ? box.Text : null;
        }
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Application.Run(new StackForm());
    }
}
