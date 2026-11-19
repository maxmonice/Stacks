using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

class Stack
{
    private int[] _stackArray;
    private int _top;
    private int _maxSize;

    public Stack(int size)
    {
        _maxSize = size;
        _stackArray = new int[_maxSize];
        _top = -1;
    }

    public bool IsEmpty() { return _top == -1; }

    public bool IsFull() { return _top == _maxSize - 1; }

    public void Push(int item) { _stackArray[++_top] = item; }

    public int Pop() { return _stackArray[_top--]; }

    public int Peek() { return _stackArray[_top]; }

    public int GetMaxSize() { return _maxSize; }

    public int GetCount() { return _top + 1; }

    public int GetItem(int index) { return _stackArray[index]; }
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
        Graphics g = e.Graphics;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using (LinearGradientBrush bg = new LinearGradientBrush(ClientRectangle,
            Color.FromArgb(31, 58, 95), Color.FromArgb(3, 102, 214), 0f))
        {
            g.FillRectangle(bg, ClientRectangle);
        }

        StringFormat center = new StringFormat();
        center.Alignment = StringAlignment.Center;
        center.LineAlignment = StringAlignment.Center;

        using (Font titleFont = new Font("Segoe UI", 17, FontStyle.Bold))
        using (Font subFont = new Font("Segoe UI", 9))
        using (Brush white = new SolidBrush(Color.White))
        using (Brush soft = new SolidBrush(Color.FromArgb(205, 220, 240)))
        {
            g.DrawString("STACK OPERATIONS", titleFont, white,
                new RectangleF(0, 6, Width, 36), center);
            g.DrawString("Array-based stack  |  LIFO: Last In, First Out", subFont, soft,
                new RectangleF(0, 40, Width, 22), center);
        }
    }
}

class StackContainer : Panel
{
    private Stack _stack;
    public Color LineColor = Color.Black;

    public StackContainer()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
    }

    public void SetStack(Stack stack)
    {
        _stack = stack;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_stack == null) return;

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using (Pen border = new Pen(Color.FromArgb(214, 221, 230)))
        {
            g.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
        }

        int max = _stack.GetMaxSize();
        int count = _stack.GetCount();
        int left = 80, right = Width - 95;
        int bottom = Height - 20;
        float slotH = Math.Min(60f, (bottom - 20) / (float)max);
        float topY = bottom - slotH * max;

        Color green = Color.FromArgb(46, 160, 67);
        Color blue = Color.FromArgb(3, 102, 214);
        Color grayText = Color.FromArgb(140, 150, 162);

        StringFormat center = new StringFormat();
        center.Alignment = StringAlignment.Center;
        center.LineAlignment = StringAlignment.Center;
        StringFormat rightAlign = new StringFormat();
        rightAlign.Alignment = StringAlignment.Far;
        rightAlign.LineAlignment = StringAlignment.Center;

        float fontSize = Math.Max(6f, Math.Min(14f, slotH * 0.4f));

        using (Font valueFont = new Font("Segoe UI", fontSize, FontStyle.Bold))
        using (Font indexFont = new Font("Segoe UI", 9))
        using (Font topFont = new Font("Segoe UI", 10, FontStyle.Bold))
        using (Brush whiteBrush = new SolidBrush(Color.White))
        using (Brush grayBrush = new SolidBrush(grayText))
        using (Brush greenBrush = new SolidBrush(green))
        {
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
                    g.DrawString(_stack.GetItem(i).ToString(), valueFont, whiteBrush, slot, center);
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
                    new StringFormat { LineAlignment = StringAlignment.Center });
            }
        }
    }
}

class StackForm : Form
{
    private Stack stack;
    private Label statusLabel = new Label();
    private Label messageLabel = new Label();
    private StackContainer stackView = new StackContainer();

    private Color navy = Color.FromArgb(31, 58, 95);
    private Color lightBlue = Color.FromArgb(220, 230, 241);

    public StackForm(int size)
    {
        stack = new Stack(size);

        Text = "Stack Operations";
        Size = new Size(440, 620);
        MinimumSize = new Size(440, 620);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(244, 246, 248);
        Font = new Font("Segoe UI", 10);

        TableLayoutPanel layout = new TableLayoutPanel();
        layout.Dock = DockStyle.Fill;
        layout.ColumnCount = 1;
        layout.RowCount = 5;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
        Controls.Add(layout);

        // Title bar (gradient header)
        HeaderPanel header = new HeaderPanel();
        header.Dock = DockStyle.Fill;
        header.Margin = new Padding(0);
        layout.Controls.Add(header, 0, 0);

        // Status line
        statusLabel.Dock = DockStyle.Fill;
        statusLabel.TextAlign = ContentAlignment.MiddleCenter;
        statusLabel.BackColor = lightBlue;
        statusLabel.ForeColor = navy;
        statusLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        layout.Controls.Add(statusLabel, 0, 1);

        // Stack container (hidden until Display is clicked)
        stackView.Dock = DockStyle.Fill;
        stackView.Margin = new Padding(20, 12, 20, 12);
        stackView.LineColor = navy;
        stackView.Visible = false;
        layout.Controls.Add(stackView, 0, 2);

        // Message line
        messageLabel.Dock = DockStyle.Fill;
        messageLabel.TextAlign = ContentAlignment.MiddleCenter;
        messageLabel.ForeColor = navy;
        messageLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        layout.Controls.Add(messageLabel, 0, 3);

        // The 6 buttons
        TableLayoutPanel buttons = new TableLayoutPanel();
        buttons.Dock = DockStyle.Fill;
        buttons.Padding = new Padding(10);
        buttons.ColumnCount = 3;
        buttons.RowCount = 2;
        for (int i = 0; i < 3; i++)
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        for (int i = 0; i < 2; i++)
            buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        layout.Controls.Add(buttons, 0, 4);

        Button pushBtn = MakeButton("Push", buttons, 0, 0, Color.FromArgb(46, 160, 67));
        Button popBtn = MakeButton("Pop", buttons, 1, 0, Color.FromArgb(215, 58, 73));
        Button isFullBtn = MakeButton("Is Full", buttons, 2, 0, Color.FromArgb(227, 131, 0));
        Button isEmptyBtn = MakeButton("Is Empty", buttons, 0, 1, Color.FromArgb(227, 131, 0));
        Button peekBtn = MakeButton("Peek", buttons, 1, 1, Color.FromArgb(3, 102, 214));
        Button displayBtn = MakeButton("Display", buttons, 2, 1, Color.FromArgb(111, 66, 193));

        pushBtn.Click += (s, e) => DoPush();
        popBtn.Click += (s, e) => DoPop();
        isFullBtn.Click += (s, e) => ShowMessage(stack.IsFull()
            ? "Yes, the stack is FULL." : "No, the stack is not full.");
        isEmptyBtn.Click += (s, e) => ShowMessage(stack.IsEmpty()
            ? "Yes, the stack is EMPTY." : "No, the stack is not empty.");
        peekBtn.Click += (s, e) => DoPeek();
        displayBtn.Click += (s, e) => DoDisplay();

        UpdateView();
    }

    private Button MakeButton(string text, TableLayoutPanel panel, int col, int row, Color color)
    {
        Button b = new Button();
        b.Text = text;
        b.Dock = DockStyle.Fill;
        b.Margin = new Padding(5);
        b.BackColor = color;
        b.ForeColor = Color.White;
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color, 0.2f);
        b.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(color, 0.1f);
        b.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        b.Cursor = Cursors.Hand;
        panel.Controls.Add(b, col, row);
        return b;
    }

    private void DoPush()
    {
        // Stack is already full -> STACK OVERFLOW
        if (stack.IsFull())
        {
            ShowMessage("STACK OVERFLOW! Cannot push, the stack is full.");
            MessageBox.Show("STACK OVERFLOW!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        string input = Program.Prompt("Enter a number to push:", "Push", "");
        if (input == null) return;

        int value;
        if (int.TryParse(input.Trim(), out value))
        {
            stack.Push(value);
            ShowMessage("Pushed: " + value);
        }
        else
        {
            ShowMessage("Invalid input! Enter a whole number.");
        }
    }

    private void DoPop()
    {
        // Nothing to remove -> STACK UNDERFLOW
        if (stack.IsEmpty())
        {
            ShowMessage("STACK UNDERFLOW! Cannot pop, the stack is empty.");
            MessageBox.Show("STACK UNDERFLOW!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
            ShowMessage("Popped: " + stack.Pop());
        }
    }

    private void DoPeek()
    {
        if (stack.IsEmpty())
            ShowMessage("The stack is empty. Nothing to peek.");
        else
            ShowMessage("Top element: " + stack.Peek());
    }

    private void DoDisplay()
    {
        // Display works like a toggle: click once to show, click again to hide
        if (stackView.Visible)
        {
            stackView.Visible = false;
            ShowMessage("Stack container hidden.");
            return;
        }
        stackView.Visible = true;
        if (stack.IsEmpty())
        {
            ShowMessage("The stack is empty.");
            return;
        }
        StringBuilder sb = new StringBuilder("Stack (top to bottom): ");
        for (int i = stack.GetCount() - 1; i >= 0; i--)
        {
            sb.Append(stack.GetItem(i));
            if (i > 0) sb.Append(", ");
        }
        ShowMessage(sb.ToString());
    }

    private void ShowMessage(string msg)
    {
        messageLabel.Text = msg;
        // red for errors, blue for normal messages
        bool isError = msg.Contains("OVERFLOW") || msg.Contains("UNDERFLOW") || msg.Contains("Invalid");
        messageLabel.ForeColor = isError ? Color.FromArgb(215, 58, 73) : navy;
        UpdateView();
    }

    // Refresh the status line and the picture of the stack
    private void UpdateView()
    {
        statusLabel.Text = "Size: " + stack.GetMaxSize()
            + "  |  Items: " + stack.GetCount()
            + "  |  Full: " + (stack.IsFull() ? "Yes" : "No")
            + "  |  Empty: " + (stack.IsEmpty() ? "Yes" : "No");

        stackView.SetStack(stack);   // redraw the container
    }
}

// ---------------- Program class ----------------
class Program
{
    // Small input box (C# has no built-in one like Java's JOptionPane)
    public static string Prompt(string text, string title, string defaultValue)
    {
        Form form = new Form();
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

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();

        // Ask for the size of the stack first
        int size = 0;
        while (size <= 0)
        {
            string input = Prompt("Enter the size of the stack:", "Stack Size", "5");
            if (input == null) return; // user cancelled
            if (!int.TryParse(input.Trim(), out size)) size = 0;
        }

        Application.Run(new StackForm(size));
    }
}