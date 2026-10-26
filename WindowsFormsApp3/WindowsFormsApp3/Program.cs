using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
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

class HeaderPanel : Panel
{
    public HeaderPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using (LinearGradientBrush bg = new LinearGradientBrush(ClientRectangle,
            Color.FromArgb(31, 58, 95), Color.FromArgb(3, 102, 214), 0f))
        {
            g.FillRectangle(bg, ClientRectangle);
        }

        using (StringFormat center = new StringFormat())
        {
            center.Alignment = StringAlignment.Center;
            center.LineAlignment = StringAlignment.Center;

            using (Font titleFont = new Font("Segoe UI", 17, FontStyle.Bold))
            using (Font subFont = new Font("Segoe UI", 9))
            using (Brush white = new SolidBrush(Color.White))
            using (Brush soft = new SolidBrush(Color.FromArgb(205, 220, 240)))
            {
                g.DrawString("STACK OPERATIONS", titleFont, white,
                    new RectangleF(0, 6, Width, 36), center);
                g.DrawString("Array-based structures  |  Stack LIFO, Queue FIFO", subFont, soft,
                    new RectangleF(0, 40, Width, 22), center);
            }
        }
    }
}

class StackContainer : Panel
{
    private int _maxSize;
    private int[] _items = new int[0];
    public Color LineColor = Color.Black;

    public StackContainer()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
    }

    public void SetSnapshot(int maxSize, int[] items)
    {
        if (maxSize <= 0) throw new ArgumentOutOfRangeException("maxSize");
        if (items == null) throw new ArgumentNullException("items");
        if (items.Length > maxSize) throw new ArgumentException("The snapshot cannot exceed the container capacity.", "items");

        _maxSize = maxSize;
        _items = (int[])items.Clone();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_maxSize <= 0 || ClientSize.Width <= 0 || ClientSize.Height <= 0) return;

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using (Pen border = new Pen(Color.FromArgb(214, 221, 230)))
        {
            g.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
        }

        int max = _maxSize;
        int count = _items.Length;
        int left = 80, right = Width - 95;
        int bottom = Height - 20;
        float slotH = Math.Min(60f, (bottom - 20) / (float)max);
        float topY = bottom - slotH * max;

        Color green = Color.FromArgb(46, 160, 67);
        Color blue = Color.FromArgb(3, 102, 214);
        Color grayText = Color.FromArgb(140, 150, 162);

        float fontSize = Math.Max(6f, Math.Min(14f, slotH * 0.4f));

        using (StringFormat center = new StringFormat())
        using (StringFormat rightAlign = new StringFormat())
        using (Font valueFont = new Font("Segoe UI", fontSize, FontStyle.Bold))
        using (Font indexFont = new Font("Segoe UI", 9))
        using (Font topFont = new Font("Segoe UI", 10, FontStyle.Bold))
        using (Brush whiteBrush = new SolidBrush(Color.White))
        using (Brush grayBrush = new SolidBrush(grayText))
        using (Brush greenBrush = new SolidBrush(green))
        {
            center.Alignment = StringAlignment.Center;
            center.LineAlignment = StringAlignment.Center;
            rightAlign.Alignment = StringAlignment.Far;
            rightAlign.LineAlignment = StringAlignment.Center;

            for (int i = 0; i < max; i++)
            {
                RectangleF slot = new RectangleF(left, bottom - (i + 1) * slotH, right - left, slotH);
                RectangleF inner = RectangleF.Inflate(slot, -3, -3);

                if (i < count)
                {
                    bool isTop = (i == count - 1);
                    Color c1 = isTop ? Color.FromArgb(70, 190, 95) : Color.FromArgb(40, 135, 235);
                    Color c2 = isTop ? green : blue;
                    using (LinearGradientBrush fill = new LinearGradientBrush(inner, c1, c2, 90f))
                    {
                        g.FillRectangle(fill, inner);
                    }
                    g.DrawString(_items[i].ToString(), valueFont, whiteBrush, slot, center);
                }
                else
                {
                    using (Brush empty = new SolidBrush(Color.FromArgb(241, 244, 248)))
                    {
                        g.FillRectangle(empty, inner);
                    }
                }

                g.DrawString(i.ToString(), indexFont, grayBrush,
                    new RectangleF(left - 38, slot.Y, 28, slotH), rightAlign);
            }

            using (Pen divider = new Pen(Color.FromArgb(205, 213, 224), 1.5f))
            {
                for (int i = 1; i < max; i++)
                {
                    float y = bottom - i * slotH;
                    g.DrawLine(divider, left, y, right, y);
                }
            }

            using (Pen wall = new Pen(LineColor, 4))
            {
                wall.LineJoin = LineJoin.Round;
                wall.StartCap = LineCap.Round;
                wall.EndCap = LineCap.Round;
                g.DrawLines(wall, new PointF[] {
                    new PointF(left, topY - 6),
                    new PointF(left, bottom),
                    new PointF(right, bottom),
                    new PointF(right, topY - 6)
                });
            }

            if (count == 0)
            {
                using (Font emptyFont = new Font("Segoe UI", 12, FontStyle.Italic | FontStyle.Bold))
                {
                    g.DrawString("EMPTY", emptyFont, grayBrush,
                        new RectangleF(left, topY, right - left, bottom - topY), center);
                }
            }
            else
            {
                float cy = bottom - (count - 0.5f) * slotH;
                g.FillPolygon(greenBrush, new PointF[] {
                    new PointF(right + 10, cy),
                    new PointF(right + 22, cy - 7),
                    new PointF(right + 22, cy + 7)
                });
                g.DrawString("TOP", topFont, greenBrush,
                    new RectangleF(right + 26, cy - 12, 60, 24),
                    center);
            }
        }
    }
}

class QueueContainer : Panel
{
    private int _maxSize;
    private int[] _items = new int[0];
    public Color LineColor = Color.Black;

    public QueueContainer()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
    }

    public void SetSnapshot(int maxSize, int[] items)
    {
        if (maxSize <= 0) throw new ArgumentOutOfRangeException("maxSize");
        if (items == null) throw new ArgumentNullException("items");
        if (items.Length > maxSize) throw new ArgumentException("The snapshot cannot exceed the container capacity.", "items");

        _maxSize = maxSize;
        _items = (int[])items.Clone();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_maxSize <= 0 || ClientSize.Width <= 0 || ClientSize.Height <= 0) return;

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        using (Pen border = new Pen(Color.FromArgb(214, 221, 230)))
            g.DrawRectangle(border, 0, 0, Width - 1, Height - 1);

        int left = 52, right = Width - 52;
        float slotWidth = Math.Max(1, (right - left) / (float)_maxSize);
        float y = Height * 0.42f;
        float slotHeight = Math.Min(64, Math.Max(30, Height * 0.22f));
        float fontSize = Math.Max(6, Math.Min(14, slotWidth * 0.35f));
        using (Font valueFont = new Font("Segoe UI", fontSize, FontStyle.Bold))
        using (Font labelFont = new Font("Segoe UI", 9, FontStyle.Bold))
        using (Font indexFont = new Font("Segoe UI", 8))
        using (Brush white = new SolidBrush(Color.White))
        using (Brush gray = new SolidBrush(Color.FromArgb(140, 150, 162)))
        using (Brush green = new SolidBrush(Color.FromArgb(46, 160, 67)))
        using (Brush blue = new SolidBrush(Color.FromArgb(3, 102, 214)))
        using (StringFormat center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
        {
            for (int i = 0; i < _maxSize; i++)
            {
                RectangleF slot = new RectangleF(left + i * slotWidth, y, slotWidth, slotHeight);
                RectangleF inner = RectangleF.Inflate(slot, -3, -3);
                if (i < _items.Length)
                {
                    Color first = i == 0 ? Color.FromArgb(70, 190, 95) : Color.FromArgb(40, 135, 235);
                    Color second = i == 0 ? Color.FromArgb(46, 160, 67) : Color.FromArgb(3, 102, 214);
                    using (LinearGradientBrush fill = new LinearGradientBrush(inner, first, second, 0f))
                        g.FillRectangle(fill, inner);
                    g.DrawString(_items[i].ToString(), valueFont, white, slot, center);
                }
                else
                {
                    using (Brush empty = new SolidBrush(Color.FromArgb(241, 244, 248)))
                        g.FillRectangle(empty, inner);
                }
                g.DrawString(i.ToString(), indexFont, gray,
                    new RectangleF(slot.X, slot.Bottom + 4, slot.Width, 18), center);
            }

            using (Pen wall = new Pen(LineColor, 3))
                g.DrawRectangle(wall, left, y, slotWidth * _maxSize, slotHeight);

            if (_items.Length == 0)
            {
                using (Font emptyFont = new Font("Segoe UI", 12, FontStyle.Italic | FontStyle.Bold))
                    g.DrawString("EMPTY", emptyFont, gray,
                        new RectangleF(left, y + slotHeight + 28, right - left, 30), center);
            }
            else
            {
                g.DrawString("FRONT", labelFont, green,
                    new RectangleF(left - 32, y - 28, slotWidth + 64, 22), center);
                float rearX = left + (_items.Length - 1) * slotWidth;
                g.DrawString("REAR", labelFont, blue,
                    new RectangleF(rearX - 32, y + slotHeight + 22, slotWidth + 64, 22), center);
            }
        }
    }
}

class StackForm : Form
{
    private Stack stack;
    private Queue queue;
    private Label statusLabel = new Label();
    private Label visualizationHeading;
    private TextBox operationLog = new TextBox();
    private FlowLayoutPanel navigationPanel = new FlowLayoutPanel();
    private StackContainer stackView = new StackContainer();
    private QueueContainer queueView = new QueueContainer();
    private bool stackHasSnapshot;
    private bool queueHasSnapshot;
    private string currentMode = "MAIN";

    private Color navy = Color.FromArgb(31, 58, 95);
    private Color lightBlue = Color.FromArgb(220, 230, 241);

    public StackForm()
    {
        Text = "Stacks and Queues Laboratory";
        Size = new Size(1080, 720);
        MinimumSize = new Size(820, 560);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(244, 246, 248);
        Font = new Font("Segoe UI", 10);

        TableLayoutPanel layout = new TableLayoutPanel();
        layout.Dock = DockStyle.Fill;
        layout.ColumnCount = 2;
        layout.RowCount = 2;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        Controls.Add(layout);

        HeaderPanel header = new HeaderPanel();
        header.Dock = DockStyle.Fill;
        header.Margin = new Padding(0);
        layout.Controls.Add(header, 0, 0);
        layout.SetColumnSpan(header, 2);

        Panel logPanel = CreateSectionPanel("OPERATION LOG", 250);
        logPanel.Margin = new Padding(12, 12, 6, 12);
        layout.Controls.Add(logPanel, 0, 1);
        TableLayoutPanel logLayout = (TableLayoutPanel)logPanel.Controls[0];
        operationLog.Dock = DockStyle.Fill;
        operationLog.Multiline = true;
        operationLog.ReadOnly = true;
        operationLog.ScrollBars = ScrollBars.Vertical;
        operationLog.WordWrap = true;
        operationLog.BackColor = Color.White;
        operationLog.ForeColor = navy;
        operationLog.BorderStyle = BorderStyle.None;
        operationLog.Font = new Font("Consolas", 9.5f);
        operationLog.Margin = new Padding(12, 8, 12, 8);
        logLayout.Controls.Add(operationLog, 0, 1);
        navigationPanel.Dock = DockStyle.Fill;
        navigationPanel.Padding = new Padding(8, 4, 8, 8);
        navigationPanel.Margin = new Padding(4);
        navigationPanel.WrapContents = true;
        navigationPanel.AutoScroll = true;
        logLayout.Controls.Add(navigationPanel, 0, 2);

        Panel visualizationPanel = CreateSectionPanel("STACK VISUALIZATION", 0);
        visualizationPanel.Margin = new Padding(6, 12, 12, 12);
        layout.Controls.Add(visualizationPanel, 1, 1);
        TableLayoutPanel visualizationLayout = (TableLayoutPanel)visualizationPanel.Controls[0];
        visualizationLayout.RowStyles[1] = new RowStyle(SizeType.Absolute, 42);
        visualizationLayout.RowStyles[2] = new RowStyle(SizeType.Percent, 100);
        visualizationHeading = (Label)visualizationLayout.GetControlFromPosition(0, 0);
        statusLabel.Dock = DockStyle.Fill;
        statusLabel.TextAlign = ContentAlignment.MiddleCenter;
        statusLabel.BackColor = lightBlue;
        statusLabel.ForeColor = navy;
        statusLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        statusLabel.Margin = new Padding(12, 4, 12, 4);
        visualizationLayout.Controls.Add(statusLabel, 0, 1);

        stackView.Dock = DockStyle.Fill;
        stackView.Margin = new Padding(12, 8, 12, 12);
        stackView.LineColor = navy;
        stackView.Visible = false;
        visualizationLayout.Controls.Add(stackView, 0, 2);
        queueView.Dock = DockStyle.Fill;
        queueView.Margin = new Padding(12, 8, 12, 12);
        queueView.LineColor = navy;
        queueView.Visible = false;
        visualizationLayout.Controls.Add(queueView, 0, 2);

        ShowMainMenu();
        UpdateView();
    }

    private Panel CreateSectionPanel(string title, int bottomHeight)
    {
        Panel panel = new Panel();
        panel.Dock = DockStyle.Fill;
        panel.Margin = new Padding(12);
        panel.BackColor = Color.White;
        panel.Padding = new Padding(1);
        panel.BorderStyle = BorderStyle.FixedSingle;

        TableLayoutPanel content = new TableLayoutPanel();
        content.Dock = DockStyle.Fill;
        content.ColumnCount = 1;
        content.RowCount = 3;
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, bottomHeight));
        panel.Controls.Add(content);

        Label heading = new Label();
        heading.Text = title;
        heading.Dock = DockStyle.Fill;
        heading.TextAlign = ContentAlignment.MiddleLeft;
        heading.Padding = new Padding(14, 0, 0, 0);
        heading.ForeColor = navy;
        heading.BackColor = Color.FromArgb(246, 248, 251);
        heading.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        content.Controls.Add(heading, 0, 0);
        return panel;
    }

    private Button AddMenuButton(string text, Color color, EventHandler handler)
    {
        Button button = new Button();
        button.Text = text;
        button.Width = 166;
        button.Height = 42;
        button.Margin = new Padding(5);
        button.BackColor = color;
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color, 0.2f);
        button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(color, 0.1f);
        button.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        button.Click += handler;
        navigationPanel.Controls.Add(button);
        return button;
    }

    private void ShowMainMenu()
    {
        currentMode = "MAIN";
        stackView.Visible = false;
        queueView.Visible = false;
        visualizationHeading.Text = "LABORATORY VISUALIZATION";
        navigationPanel.Controls.Clear();
        AddMenuButton("STACK", Color.FromArgb(3, 102, 214), (s, e) => ShowStackMenu());
        AddMenuButton("QUEUE", Color.FromArgb(3, 102, 214), (s, e) => ShowQueueMenu());
        AddMenuButton("POSTFIX EVALUATION", Color.FromArgb(111, 66, 193), (s, e) => DoPostfixEvaluation());
        AddMenuButton("DECIMAL TO BINARY", Color.FromArgb(111, 66, 193), (s, e) => DoDecimalToBinary());
        AddMenuButton("EXIT", Color.FromArgb(215, 58, 73), (s, e) => Close());
        UpdateView();
    }

    private void ShowStackMenu()
    {
        if (stack == null)
        {
            int capacity = RequestCapacity("stack");
            if (capacity == 0)
            {
                ShowMainMenu();
                return;
            }
            stack = new Stack(capacity);
            LogOperation("INIT", "Stack initialized with capacity " + capacity + ".");
        }
        currentMode = "STACK";
        stackView.Visible = stackHasSnapshot;
        queueView.Visible = false;
        visualizationHeading.Text = "STACK VISUALIZATION";
        navigationPanel.Controls.Clear();
        AddMenuButton("Push", Color.FromArgb(46, 160, 67), (s, e) => DoPush());
        AddMenuButton("Pop", Color.FromArgb(215, 58, 73), (s, e) => DoPop());
        AddMenuButton("Peek", Color.FromArgb(3, 102, 214), (s, e) => DoStackPeek());
        AddMenuButton("Is Empty", Color.FromArgb(227, 131, 0), (s, e) => LogOperation("IS EMPTY", stack.IsEmpty() ? "The stack is empty." : "The stack is not empty."));
        AddMenuButton("Is Full", Color.FromArgb(227, 131, 0), (s, e) => LogOperation("IS FULL", stack.IsFull() ? "The stack is full." : "The stack is not full."));
        AddMenuButton("Display", Color.FromArgb(111, 66, 193), (s, e) => DoStackDisplay());
        AddMenuButton("Resize", navy, (s, e) => DoResizeStack());
        AddMenuButton("Back to Main Menu", navy, (s, e) => ShowMainMenu());
        UpdateView();
    }

    private void ShowQueueMenu()
    {
        if (queue == null)
        {
            int capacity = RequestCapacity("queue");
            if (capacity == 0)
            {
                ShowMainMenu();
                return;
            }
            queue = new Queue(capacity);
            LogOperation("INIT", "Queue initialized with capacity " + capacity + ".");
        }
        currentMode = "QUEUE";
        stackView.Visible = false;
        queueView.Visible = queueHasSnapshot;
        visualizationHeading.Text = "QUEUE VISUALIZATION";
        navigationPanel.Controls.Clear();
        AddMenuButton("Enqueue", Color.FromArgb(46, 160, 67), (s, e) => DoEnqueue());
        AddMenuButton("Dequeue", Color.FromArgb(215, 58, 73), (s, e) => DoDequeue());
        AddMenuButton("Peek", Color.FromArgb(3, 102, 214), (s, e) => DoQueuePeek());
        AddMenuButton("Is Empty", Color.FromArgb(227, 131, 0), (s, e) => LogOperation("IS EMPTY", queue.IsEmpty() ? "The queue is empty." : "The queue is not empty."));
        AddMenuButton("Is Full", Color.FromArgb(227, 131, 0), (s, e) => LogOperation("IS FULL", queue.IsFull() ? "The queue is full." : "The queue is not full."));
        AddMenuButton("Display", Color.FromArgb(111, 66, 193), (s, e) => DoQueueDisplay());
        AddMenuButton("Resize", navy, (s, e) => DoResizeQueue());
        AddMenuButton("Back to Main Menu", navy, (s, e) => ShowMainMenu());
        UpdateView();
    }

    private int RequestCapacity(string structureName)
    {
        while (true)
        {
            string input = Program.Prompt(
                "Enter the " + structureName + " capacity (1 to " + StructureLimits.MaximumCapacity + "):",
                "Initialize " + structureName,
                "5");
            if (input == null) return 0;

            int capacity;
            if (int.TryParse(input.Trim(), out capacity)
                && capacity >= 1
                && capacity <= StructureLimits.MaximumCapacity)
            {
                return capacity;
            }

            LogOperation("ERROR", "Enter a " + structureName + " capacity from 1 to "
                + StructureLimits.MaximumCapacity + ".");
        }
    }

    private void DoPush()
    {
        if (stack.IsFull())
        {
            LogOperation("ERROR", "Stack overflow. Cannot push because the stack is full.");
            MessageBox.Show("STACK OVERFLOW!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        string input = Program.Prompt("Enter a number to push:", "Push", "");
        if (input == null) return;
        int value;
        if (!int.TryParse(input.Trim(), out value))
        {
            LogOperation("ERROR", "Invalid input. Enter a whole number to push.");
            return;
        }
        stack.Push(value);
        LogOperation("PUSH", "Added " + value + " to the stack.");
    }

    private void DoPop()
    {
        if (stack.IsEmpty())
        {
            LogOperation("ERROR", "Stack underflow. Cannot pop because the stack is empty.");
            MessageBox.Show("STACK UNDERFLOW!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        LogOperation("POP", "Removed " + stack.Pop() + " from the stack.");
    }

    private void DoStackPeek()
    {
        if (stack.IsEmpty()) LogOperation("ERROR", "Cannot peek because the stack is empty.");
        else LogOperation("PEEK", "Top element: " + stack.Peek() + ".");
    }

    private void DoStackDisplay()
    {
        int[] snapshot = new int[stack.GetCount()];
        for (int i = 0; i < snapshot.Length; i++) snapshot[i] = stack.GetItem(i);
        stackView.SetSnapshot(stack.GetMaxSize(), snapshot);
        stackHasSnapshot = true;
        stackView.Visible = true;
        if (snapshot.Length == 0)
        {
            LogOperation("DISPLAY", "Stack visualization refreshed. Stack is empty.");
            return;
        }
        StringBuilder message = new StringBuilder("Stack visualization refreshed. Contents (top to bottom): ");
        for (int i = snapshot.Length - 1; i >= 0; i--)
        {
            message.Append(snapshot[i]);
            if (i > 0) message.Append(", ");
        }
        message.Append('.');
        LogOperation("DISPLAY", message.ToString());
    }

    private void DoResizeStack()
    {
        string input = Program.Prompt("Enter the new stack capacity (1 to " + StructureLimits.MaximumCapacity + "):",
            "Resize Stack", stack.GetMaxSize().ToString());
        if (input == null) return;
        int newSize;
        if (!int.TryParse(input.Trim(), out newSize)
            || !stack.Resize(newSize))
        {
            LogOperation("ERROR", "Stack capacity must be from 1 to " + StructureLimits.MaximumCapacity
                + " and cannot be less than the current item count (" + stack.GetCount() + ").");
            return;
        }
        LogOperation("RESIZE", "Stack capacity changed to " + newSize + ". Display to refresh the visualization.");
    }

    private void DoEnqueue()
    {
        if (queue.IsFull())
        {
            LogOperation("ERROR", "Queue overflow. Cannot enqueue because the queue is full.");
            MessageBox.Show("QUEUE OVERFLOW!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        string input = Program.Prompt("Enter a number to enqueue:", "Enqueue", "");
        if (input == null) return;
        int value;
        if (!int.TryParse(input.Trim(), out value))
        {
            LogOperation("ERROR", "Invalid input. Enter a whole number to enqueue.");
            return;
        }
        queue.Enqueue(value);
        LogOperation("ENQUEUE", "Added " + value + " to the rear of the queue.");
    }

    private void DoDequeue()
    {
        if (queue.IsEmpty())
        {
            LogOperation("ERROR", "Queue underflow. Cannot dequeue because the queue is empty.");
            MessageBox.Show("QUEUE UNDERFLOW!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        LogOperation("DEQUEUE", "Removed " + queue.Dequeue() + " from the front of the queue.");
    }

    private void DoQueuePeek()
    {
        if (queue.IsEmpty()) LogOperation("ERROR", "Cannot peek because the queue is empty.");
        else LogOperation("PEEK", "Front element: " + queue.Peek() + ".");
    }

    private void DoQueueDisplay()
    {
        int[] snapshot = new int[queue.GetCount()];
        for (int i = 0; i < snapshot.Length; i++) snapshot[i] = queue.GetItem(i);
        queueView.SetSnapshot(queue.GetMaxSize(), snapshot);
        queueHasSnapshot = true;
        queueView.Visible = true;
        if (snapshot.Length == 0)
        {
            LogOperation("DISPLAY", "Queue visualization refreshed. Queue is empty.");
            return;
        }
        StringBuilder message = new StringBuilder("Queue visualization refreshed. Front to rear: ");
        for (int i = 0; i < snapshot.Length; i++)
        {
            message.Append(snapshot[i]);
            if (i < snapshot.Length - 1) message.Append(", ");
        }
        message.Append('.');
        LogOperation("DISPLAY", message.ToString());
    }

    private void DoResizeQueue()
    {
        string input = Program.Prompt("Enter the new queue capacity (1 to " + StructureLimits.MaximumCapacity + "):",
            "Resize Queue", queue.GetMaxSize().ToString());
        if (input == null) return;
        int newSize;
        if (!int.TryParse(input.Trim(), out newSize)
            || !queue.Resize(newSize))
        {
            LogOperation("ERROR", "Queue capacity must be from 1 to " + StructureLimits.MaximumCapacity
                + " and cannot be less than the current item count (" + queue.GetCount() + ").");
            return;
        }
        LogOperation("RESIZE", "Queue capacity changed to " + newSize + ". Display to refresh the visualization.");
    }

    private void DoPostfixEvaluation()
    {
        string expression = Program.Prompt(
            "Enter postfix values and operators. Separate multi-digit operands with spaces (example: 12 3 +):",
            "Postfix Evaluation", "");
        if (expression == null) return;
        int result;
        string error;
        if (Program.TryEvaluatePostfix(expression, out result, out error))
            LogOperation("POSTFIX", "Result for " + expression.Trim() + " = " + result + ".");
        else
            LogOperation("ERROR", "Postfix evaluation failed: " + error);
    }

    private void DoDecimalToBinary()
    {
        string input = Program.Prompt("Enter a non-negative whole number:", "Decimal to Binary", "");
        if (input == null) return;
        int value;
        string binary;
        if (!int.TryParse(input.Trim(), out value) || !Program.TryConvertDecimalToBinary(value, out binary))
        {
            LogOperation("ERROR", "Enter a non-negative whole number to convert.");
            return;
        }
        LogOperation("BINARY", "Decimal " + value + " = binary " + binary + ".");
    }

    private void LogOperation(string operation, string message)
    {
        operationLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] "
            + operation + " — " + message + Environment.NewLine);
        operationLog.SelectionStart = operationLog.TextLength;
        operationLog.ScrollToCaret();
        UpdateView();
    }

    private void UpdateView()
    {
        if (currentMode == "STACK")
        {
            statusLabel.Text = "STACK  |  Capacity: " + stack.GetMaxSize()
                + "  |  Items: " + stack.GetCount()
                + "  |  Full: " + (stack.IsFull() ? "Yes" : "No")
                + "  |  Empty: " + (stack.IsEmpty() ? "Yes" : "No");
        }
        else if (currentMode == "QUEUE")
        {
            statusLabel.Text = "QUEUE  |  Capacity: " + queue.GetMaxSize()
                + "  |  Items: " + queue.GetCount()
                + "  |  Full: " + (queue.IsFull() ? "Yes" : "No")
                + "  |  Empty: " + (queue.IsEmpty() ? "Yes" : "No");
        }
        else
        {
            string stackCapacity = stack == null ? "Not initialized" : stack.GetMaxSize().ToString();
            string queueCapacity = queue == null ? "Not initialized" : queue.GetMaxSize().ToString();
            statusLabel.Text = "Stack capacity: " + stackCapacity
                + "  |  Queue capacity: " + queueCapacity + "  |  Choose an option from the menu.";
        }
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

    public static bool TryEvaluatePostfix(string expression, out int result, out string error)
    {
        result = 0;
        error = null;
        if (expression == null) expression = "";
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
        {
            tokens = expression.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            tokens = new string[expression.Length];
            for (int i = 0; i < expression.Length; i++)
                tokens[i] = expression[i].ToString();
        }

        try
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                int operand;
                if (int.TryParse(token, out operand) && operand >= 0)
                {
                    values.Push(operand);
                    continue;
                }
                if (token != "+" && token != "-" && token != "*" && token != "/")
                {
                    error = "Unsupported token '" + token + "'. Use non-negative whole-number operands and +, -, *, / only.";
                    return false;
                }
                if (values.GetCount() < 2)
                {
                    error = "The expression has insufficient operands for '" + token + "'.";
                    return false;
                }

                int right = values.Pop();
                int left = values.Pop();
                int intermediate;
                checked
                {
                    switch (token)
                    {
                        case "+": intermediate = left + right; break;
                        case "-": intermediate = left - right; break;
                        case "*": intermediate = left * right; break;
                        default:
                            if (right == 0)
                            {
                                error = "Division by zero is not allowed.";
                                return false;
                            }
                            intermediate = left / right;
                            break;
                    }
                }
                values.Push(intermediate);
            }
        }
        catch (OverflowException)
        {
            error = "The expression result is outside the supported integer range.";
            return false;
        }

        if (values.GetCount() != 1)
        {
            error = "The expression must leave exactly one value on the stack.";
            return false;
        }
        result = values.Pop();
        return true;
    }

    // Small input box (C# has no built-in one like Java's JOptionPane)
    public static string Prompt(string text, string title, string defaultValue)
    {
        using (Form form = new Form())
        {
            form.Text = title;
            form.ClientSize = new Size(300, 110);
            form.StartPosition = FormStartPosition.CenterScreen;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MinimizeBox = false;
            form.MaximizeBox = false;

            Label label = new Label();
            label.Text = text;
            label.SetBounds(10, 10, 280, 20);

            TextBox box = new TextBox();
            box.Text = defaultValue;
            box.SetBounds(10, 35, 280, 25);

            Button ok = new Button();
            ok.Text = "OK";
            ok.DialogResult = DialogResult.OK;
            ok.SetBounds(130, 70, 75, 28);

            Button cancel = new Button();
            cancel.Text = "Cancel";
            cancel.DialogResult = DialogResult.Cancel;
            cancel.SetBounds(215, 70, 75, 28);

            form.Controls.AddRange(new Control[] { label, box, ok, cancel });
            form.AcceptButton = ok;
            form.CancelButton = cancel;

            return form.ShowDialog() == DialogResult.OK ? box.Text : null;
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
