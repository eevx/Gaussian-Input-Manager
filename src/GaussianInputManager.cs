using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;

public sealed class GaussianManager : Form
{
    static PrivateFontCollection googleFonts;
    static byte[] googleFontBytes;
    static GCHandle googleFontPin;
    static FontFamily googleFontFamily;

    static Font UiFont(float size) { return new Font(LoadGoogleSans(), size, FontStyle.Regular, GraphicsUnit.Point); }
    static Font UiFont(float size, FontStyle style) { return new Font(LoadGoogleSans(), size, style, GraphicsUnit.Point); }
    static FontFamily LoadGoogleSans()
    {
        if (googleFontFamily != null) return googleFontFamily;
        Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("GoogleSansRegular");
        if (stream == null) throw new InvalidOperationException("Embedded Google Sans font was not found.");
        using (stream) { googleFontBytes = new byte[stream.Length]; stream.Read(googleFontBytes, 0, googleFontBytes.Length); }
        googleFontPin = GCHandle.Alloc(googleFontBytes, GCHandleType.Pinned); googleFonts = new PrivateFontCollection();
        googleFonts.AddMemoryFont(googleFontPin.AddrOfPinnedObject(), googleFontBytes.Length); googleFontFamily = googleFonts.Families[0]; return googleFontFamily;
    }
    readonly Color page = Color.FromArgb(20, 18, 24), card = Color.FromArgb(33, 31, 38), input = Color.FromArgb(43, 41, 48), ink = Color.FromArgb(230, 225, 233), muted = Color.FromArgb(202, 196, 208), accent = Color.FromArgb(208, 188, 255);
    TextBox file = new TextBox(), methodBasis = new TextBox(), charge = new TextBox(), multiplicity = new TextBox();
    TextBox checkpoint = new TextBox(), title = new TextBox(), additional = new TextBox(), memory = new TextBox(), processors = new TextBox(), temperature = new TextBox(), optOptions = new TextBox(), freqOptions = new TextBox();
    CheckBox opt = new CheckBox(), freq = new CheckBox(), includeTemperature = new CheckBox(), makeLinda = new CheckBox(), sameTitle = new CheckBox();
    Label eol = new Label(); string source = "", template = "";

    GaussianManager()
    {
        Text = "Gaussian Input Manager"; Width = 760; Height = 720; MinimumSize = new Size(680, 560); StartPosition = FormStartPosition.CenterScreen;
        Font = UiFont(9.5f); BackColor = page; ForeColor = ink; AutoScaleMode = AutoScaleMode.Dpi; using (Stream iconStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AppIcon")) { if (iconStream != null) Icon = new Icon(iconStream); }
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16, 12, 16, 12), ColumnCount = 1, RowCount = 4, BackColor = page };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); Controls.Add(root);
        var head = new Panel { Dock = DockStyle.Fill, BackColor = page };
        var logo = new PictureBox { Location = new Point(0, 2), Size = new Size(38, 38), SizeMode = PictureBoxSizeMode.Zoom, BackColor = page };
        using (Stream logoStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AppLogo")) { if (logoStream != null) using (Bitmap sourceLogo = new Bitmap(logoStream)) logo.Image = new Bitmap(sourceLogo); }
        head.Controls.Add(logo);
        head.Controls.Add(new Label { Text = "Gaussian Input Manager", Font = UiFont(17, FontStyle.Bold), ForeColor = ink, AutoSize = true, Location = new Point(48, 0) });
        head.Controls.Add(new Label { Text = "Edit Gaussian run settings", Font = UiFont(9.5f), ForeColor = muted, AutoSize = true, Location = new Point(50, 35) }); root.Controls.Add(head, 0, 0);

        var sourceCard = Card(); var srcGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, BackColor = card, Padding = new Padding(2) };
        srcGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170)); srcGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); srcGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105)); srcGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); srcGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        LabelField(srcGrid, "INPUT FILE", 0); file.Dock = DockStyle.Fill; StyleInput(file); srcGrid.Controls.Add(file, 1, 0); var browse = Button("Browse", false); browse.Click += SelectFile; srcGrid.Controls.Add(browse, 2, 0);
        eol.Text = "Select a .com or .gjf file"; eol.ForeColor = muted; eol.AutoSize = true; eol.Margin = new Padding(3, 8, 3, 0); srcGrid.Controls.Add(eol, 1, 1); srcGrid.SetColumnSpan(eol, 2); sourceCard.Controls.Add(srcGrid); root.Controls.Add(sourceCard, 0, 1);

        var settings = Card(); settings.Dock = DockStyle.Top; settings.Height = 436; var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = page }; var fields = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = card, ColumnCount = 2, RowCount = 11, Padding = new Padding(2) };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170)); fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 11; i++) fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        settings.Controls.Add(fields);
        LabelField(fields, "METHOD / BASIS", 0); methodBasis.Dock = DockStyle.Fill; StyleInput(methodBasis); fields.Controls.Add(methodBasis, 1, 0); Hint(fields, "Example: b3lyp/def2tzvp", 0);
        LabelField(fields, "JOB TYPES", 1); var jobs = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = card, Margin = new Padding(0), Padding = new Padding(0, 8, 0, 0) }; SetupCheck(opt, "opt"); SetupCheck(freq, "freq"); opt.Checked = true; jobs.Controls.Add(opt); jobs.Controls.Add(freq); fields.Controls.Add(jobs, 1, 1); Hint(fields, "Select one or both", 1);
        LabelField(fields, "OPT OPTIONS", 2); optOptions.Dock = DockStyle.Fill; StyleInput(optOptions); fields.Controls.Add(optOptions, 1, 2); Hint(fields, "Example: calcfc,ts,noeigentest", 2);
        LabelField(fields, "FREQ OPTIONS", 3); freqOptions.Dock = DockStyle.Fill; StyleInput(freqOptions); fields.Controls.Add(freqOptions, 1, 3); Hint(fields, "Example: noraman", 3);
        opt.CheckedChanged += delegate { optOptions.Enabled = opt.Checked; }; freq.CheckedChanged += delegate { freqOptions.Enabled = freq.Checked; };
        LabelField(fields, "ADDITIONAL KEYWORDS", 4); additional.Dock = DockStyle.Fill; StyleInput(additional); fields.Controls.Add(additional, 1, 4); Hint(fields, "Inserted before geom=connectivity", 4);
        LabelField(fields, "CHARGE / MULTIPLICITY", 5); fields.Controls.Add(Pair(charge, multiplicity, "Charge", "Multiplicity"), 1, 5);
        LabelField(fields, "JOB TITLE", 6); title.Dock = DockStyle.Fill; StyleInput(title); fields.Controls.Add(title, 1, 6); Hint(fields, "Also used for output and Linda job names", 6);
        LabelField(fields, "CHECKPOINT FILE", 7); checkpoint.Dock = DockStyle.Fill; StyleInput(checkpoint); sameTitle.Text = "Match title"; sameTitle.Checked = true; sameTitle.AutoSize = true; sameTitle.ForeColor = ink; sameTitle.BackColor = card; sameTitle.FlatStyle = FlatStyle.Flat; sameTitle.FlatAppearance.CheckedBackColor = accent; var checkpointRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = card, Margin = new Padding(0) }; checkpointRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); checkpointRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); checkpointRow.Controls.Add(checkpoint, 0, 0); checkpointRow.Controls.Add(sameTitle, 1, 0); fields.Controls.Add(checkpointRow, 1, 7);
        sameTitle.CheckedChanged += delegate { checkpoint.Enabled = !sameTitle.Checked; if (sameTitle.Checked) SyncCheckpoint(); }; title.TextChanged += delegate { if (sameTitle.Checked) SyncCheckpoint(); };
        LabelField(fields, "MEMORY / PROCESSORS", 8); fields.Controls.Add(Pair(memory, processors, "Memory (e.g. 8GB)", "Processors"), 1, 8); Hint(fields, "Updates Gaussian and Linda settings", 8);
        LabelField(fields, "TEMPERATURE", 9); var tempRow = new TableLayoutPanel { ColumnCount = 2, RowCount = 1, Dock = DockStyle.Fill, Margin = new Padding(0), BackColor = card }; tempRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190)); tempRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        includeTemperature.Text = "Include temperature"; includeTemperature.Dock = DockStyle.Fill; includeTemperature.Margin = new Padding(2, 6, 2, 2); includeTemperature.ForeColor = ink; includeTemperature.BackColor = card; includeTemperature.FlatStyle = FlatStyle.Flat; includeTemperature.FlatAppearance.CheckedBackColor = accent; tempRow.Controls.Add(includeTemperature, 0, 0); StyleInput(temperature); temperature.Dock = DockStyle.Fill; tempRow.Controls.Add(temperature, 1, 0); includeTemperature.CheckedChanged += delegate { temperature.Enabled = includeTemperature.Checked; }; fields.Controls.Add(tempRow, 1, 9); Hint(fields, "Kelvin; optional", 9);
        LabelField(fields, "LINDA SCRIPT", 10); makeLinda.Text = "Generate linda.sh"; makeLinda.Checked = true; makeLinda.Dock = DockStyle.Left; makeLinda.AutoSize = true; makeLinda.ForeColor = ink; makeLinda.BackColor = card; makeLinda.FlatStyle = FlatStyle.Flat; makeLinda.FlatAppearance.CheckedBackColor = accent; fields.Controls.Add(makeLinda, 1, 10); Hint(fields, "Uses the built-in template", 10);
        scroll.Controls.Add(settings); root.Controls.Add(scroll, 0, 2);
        var footer = new Panel { Dock = DockStyle.Fill, BackColor = page }; var generate = Button("Generate files", true); generate.Width = 180; generate.Height = 38; generate.Anchor = AnchorStyles.Right | AnchorStyles.Top; generate.Location = new Point(footer.Width - generate.Width, 8); footer.Resize += delegate { generate.Left = footer.ClientSize.Width - generate.Width; }; generate.Click += Generate; footer.Controls.Add(generate); root.Controls.Add(footer, 0, 3);
        title.Text = "DefaultJobName"; SyncCheckpoint(); checkpoint.Enabled = !sameTitle.Checked; temperature.Enabled = false;
    }
    void SetupCheck(CheckBox c, string text) { c.Text = text; c.AutoSize = true; c.ForeColor = ink; c.BackColor = card; c.FlatStyle = FlatStyle.Flat; c.FlatAppearance.CheckedBackColor = accent; c.Margin = new Padding(4, 0, 20, 0); }
    void SyncCheckpoint() { checkpoint.Text = title.Text.Trim() == "" ? "" : title.Text.Trim() + ".chk"; }
    Panel Card() { return new MaterialCard { Dock = DockStyle.Fill, BackColor = card, Padding = new Padding(12), Margin = new Padding(0, 5, 0, 5), BorderStyle = BorderStyle.None }; }
    void LabelField(TableLayoutPanel g, string text, int row) { var l = new Label { Text = text, ForeColor = muted, Font = UiFont(8.3f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 8, 0), BackColor = card }; g.Controls.Add(l, 0, row); }
    void Hint(TableLayoutPanel g, string text, int row) { }
    void StyleInput(Control c) { c.Font = UiFont(10); c.BackColor = input; c.ForeColor = ink; if (c is TextBox) { ((TextBox)c).BorderStyle = BorderStyle.FixedSingle; ((TextBox)c).Margin = new Padding(2, 2, 2, 2); } else c.Margin = new Padding(2, 2, 2, 2); }
    Control Pair(TextBox left, TextBox right, string leftHint, string rightHint) { var p = new TableLayoutPanel { ColumnCount = 2, RowCount = 1, Dock = DockStyle.Fill, Margin = new Padding(0), BackColor = card }; p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); p.Controls.Add(PairCell(left, leftHint), 0, 0); p.Controls.Add(PairCell(right, rightHint), 1, 0); return p; }
    Control PairCell(TextBox box, string caption) { var panel = new Panel { Dock = DockStyle.Fill, BackColor = card, Margin = new Padding(0, 0, 8, 0) }; var label = new Label { Text = caption, Location = new Point(0, 0), Height = 13, Width = 180, Font = UiFont(8f), ForeColor = muted, BackColor = card }; StyleInput(box); box.Dock = DockStyle.None; box.Location = new Point(0, 13); box.Height = 22; box.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top; panel.Resize += delegate { box.Width = Math.Max(20, panel.ClientSize.Width); label.Width = panel.ClientSize.Width; }; panel.Controls.Add(box); panel.Controls.Add(label); return panel; }
    Button Button(string text, bool primary) { Button b = primary ? (Button)new GradientButton() : new Button(); b.Text = text; b.FlatStyle = FlatStyle.Flat; b.BackColor = primary ? accent : Color.FromArgb(49, 45, 56); b.ForeColor = primary ? Color.FromArgb(56, 30, 78) : accent; b.Font = UiFont(9.5f, FontStyle.Bold); b.Cursor = Cursors.Hand; b.Margin = new Padding(4); b.FlatAppearance.BorderSize = primary ? 0 : 1; b.FlatAppearance.BorderColor = Color.FromArgb(73, 69, 79); return b; }

    void SelectFile(object s, EventArgs e) { using (var d = new OpenFileDialog { Filter = "Gaussian input (*.com;*.gjf)|*.com;*.gjf|All files (*.*)|*.*" }) if (d.ShowDialog() == DialogResult.OK) { file.Text = d.FileName; source = File.ReadAllText(file.Text); ReadInput(source); } }
    string[] Lines(string value) { return Regex.Split(value, "\\r\\n|\\n|\\r"); }
    bool IsBlank(string s) { return String.IsNullOrWhiteSpace(s); }
    void ReadInput(string text)
    {
        string eol = text.Contains("\r\n") ? "Windows CRLF" : (text.Contains("\n") ? "Unix LF" : (text.Contains("\r") ? "Classic Mac CR" : "No line breaks detected"));
        this.eol.Text = "Detected " + eol + " · output is Unix LF";
        string[] a = Lines(text); int route = -1;
        for (int i = 0; i < a.Length; i++) { string t = a[i].Trim(); if (t.StartsWith("%mem=", StringComparison.OrdinalIgnoreCase)) memory.Text = t.Substring(5); else if (t.StartsWith("%chk=", StringComparison.OrdinalIgnoreCase)) checkpoint.Text = t.Substring(5); else if (t.StartsWith("%nprocshared=", StringComparison.OrdinalIgnoreCase)) processors.Text = t.Substring(13); else if (t.StartsWith("#")) { route = i; break; } }
        if (route < 0) { source = text; return; }
        int routeEnd = Array.FindIndex(a, route + 1, x => IsBlank(x)); if (routeEnd < 0) routeEnd = a.Length;
        string routeText = String.Join(" ", a, route, routeEnd - route).Trim(); routeText = Regex.Replace(routeText, "^#+\\s*", ""); ParseRoute(routeText);
        int titleIndex = routeEnd + 1;
        int chargeIndex = titleIndex + 1; while (chargeIndex < a.Length && !Regex.IsMatch(a[chargeIndex].Trim(), @"^-?\d+\s+-?\d+$") && chargeIndex < titleIndex + 8) chargeIndex++;
        title.Text = titleIndex < a.Length && !IsBlank(a[titleIndex]) ? a[titleIndex].Trim() : "DefaultJobName";
        if (chargeIndex < a.Length) { Match m = Regex.Match(a[chargeIndex].Trim(), @"^(-?\d+)\s+(-?\d+)$"); if (m.Success) { charge.Text = m.Groups[1].Value; multiplicity.Text = m.Groups[2].Value; } }
        source = text; checkpoint.Enabled = !sameTitle.Checked;
    }
    void ParseRoute(string r)
    {
        Match optMatch = Regex.Match(r, @"(?i)\bopt(?:\s*=\s*(\([^)]*\)|[^\s]+))?"); Match freqMatch = Regex.Match(r, @"(?i)\bfreq(?:\s*=\s*(\([^)]*\)|[^\s]+))?");
        opt.Checked = optMatch.Success; freq.Checked = freqMatch.Success; optOptions.Text = optMatch.Success ? CleanOption(optMatch.Groups[1].Value) : ""; freqOptions.Text = freqMatch.Success ? CleanOption(freqMatch.Groups[1].Value) : "";
        Match slash = Regex.Match(r, @"(?i)\b([a-z][a-z0-9+*()\-]*)\s*/\s*([a-z0-9][a-z0-9+*()\-]*)\b");
        if (slash.Success) methodBasis.Text = slash.Groups[1].Value + "/" + slash.Groups[2].Value;
        else {
            string withoutJobs = Regex.Replace(r, @"(?i)\b(opt|freq)(?:\s*=\s*(\([^)]*\)|[^\s]+))?", "");
            Match method = Regex.Match(withoutJobs, @"(?i)\b([a-z][a-z0-9+*()\-]*)\b");
            Match basis = Regex.Match(withoutJobs, @"(?i)\b(def2[a-z0-9+*()\-]+|6-3[12][a-z0-9+*()\-]*|sto-3g|cc-p[a-z0-9+*()\-]+|aug-cc-p[a-z0-9+*()\-]+|sdd|lanl[a-z0-9+*()\-]*|midi[a-z0-9+*()\-]*)\b");
            if (method.Success && basis.Success) methodBasis.Text = method.Groups[1].Value + "/" + basis.Value;
        }
        Match tm = Regex.Match(r, @"(?i)\btemperature\s*=\s*([^\s]+)"); includeTemperature.Checked = tm.Success; temperature.Text = tm.Success ? tm.Groups[1].Value : "";
        string cleaned = Regex.Replace(r, @"(?i)\b(opt|freq)(?:\s*=\s*(\([^)]*\)|[^\s]+))?", ""); cleaned = Regex.Replace(cleaned, @"(?i)\bgeom\s*=\s*connectivity\b|\btemperature\s*=\s*[^\s]+", "");
        if (slash.Success) cleaned = cleaned.Replace(slash.Value, "");
        else { string b = Regex.Match(cleaned, @"(?i)\b(def2[a-z0-9+*()\-]+|6-3[12][a-z0-9+*()\-]*|sto-3g|cc-p[a-z0-9+*()\-]+|aug-cc-p[a-z0-9+*()\-]+|sdd|lanl[a-z0-9+*()\-]*|midi[a-z0-9+*()\-]*)\b").Value; if (b != "") cleaned = cleaned.Replace(b, ""); if (methodToken(r) != "") cleaned = cleaned.Replace(methodToken(r), ""); }
        additional.Text = Regex.Replace(cleaned, @"\s+", " ").Trim();
    }
    string methodToken(string r) { string t = Regex.Replace(r, @"(?i)\b(opt|freq)(?:\s*=\s*(\([^)]*\)|[^\s]+))?", ""); Match m = Regex.Match(t, @"(?i)\b([a-z][a-z0-9+*()\-]*)\b"); return m.Success ? m.Groups[1].Value : ""; }
    string CleanOption(string s) { return s.Trim().Trim('(', ')', ' '); }
    string NewRoute()
    {
        string mb = methodBasis.Text.Trim(); if (mb == "" || !mb.Contains("/")) throw new Exception("Enter method and basis together, for example b3lyp/def2tzvp.");
        var terms = new List<string>(); if (opt.Checked) terms.Add("opt" + (optOptions.Text.Trim() == "" ? "" : "=(" + CleanOption(optOptions.Text) + ")")); if (freq.Checked) terms.Add("freq" + (freqOptions.Text.Trim() == "" ? "" : "=" + CleanOption(freqOptions.Text)));
        if (terms.Count == 0) throw new Exception("Select at least one job type: opt, freq, or both.");
        terms.Add(mb); if (additional.Text.Trim() != "") terms.Add(additional.Text.Trim()); terms.Add("geom=connectivity");
        if (includeTemperature.Checked) { double v; if (!Double.TryParse(temperature.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v)) throw new Exception("Temperature must be a number, for example 393.15."); terms.Add("temperature=" + temperature.Text.Trim()); }
        return "# " + String.Join(" ", terms.ToArray());
    }
    void Generate(object s, EventArgs e)
    {
        try {
            if (source == "") throw new Exception("Select an existing Gaussian input file first."); string[] a = Lines(source); int route = Array.FindIndex(a, x => x.TrimStart().StartsWith("#")); if (route < 0) throw new Exception("No Gaussian route line found.");
            int routeEnd = Array.FindIndex(a, route + 1, x => IsBlank(x)); if (routeEnd < 0) throw new Exception("Could not find the blank line after the route section."); int titleIndex = routeEnd + 1;
            int chargeIndex = titleIndex + 1; while (chargeIndex < a.Length && !Regex.IsMatch(a[chargeIndex].Trim(), @"^-?\d+\s+-?\d+$") && chargeIndex < titleIndex + 8) chargeIndex++;
            if (titleIndex >= a.Length || chargeIndex >= a.Length) throw new Exception("Could not find the title and charge/multiplicity header.");
            if (!Regex.IsMatch(a[chargeIndex].Trim(), @"^-?\d+\s+-?\d+$")) throw new Exception("Charge/multiplicity must contain two integers, for example 0 1.");
            string jobTitle = String.IsNullOrWhiteSpace(title.Text) ? "DefaultJobName" : title.Text.Trim(); if (String.IsNullOrWhiteSpace(charge.Text) || String.IsNullOrWhiteSpace(multiplicity.Text)) throw new Exception("Charge and multiplicity are required.");
            string chk = sameTitle.Checked ? jobTitle + ".chk" : checkpoint.Text.Trim(); if (chk == "") throw new Exception("Checkpoint filename is required."); if (!chk.EndsWith(".chk", StringComparison.OrdinalIgnoreCase)) chk += ".chk";
            if (String.IsNullOrWhiteSpace(memory.Text) || String.IsNullOrWhiteSpace(processors.Text)) throw new Exception("Memory and processor count are required.");
            int bodyStart = chargeIndex + 1; string body = String.Join("\n", a, bodyStart, a.Length - bodyStart).TrimEnd('\r', '\n');
            string header = String.Join("\n", new string[] { "%mem=" + memory.Text.Trim(), "%chk=" + chk, "%nprocshared=" + processors.Text.Trim(), NewRoute(), "", jobTitle, "", charge.Text.Trim() + " " + multiplicity.Text.Trim() });
            string output = header + "\n" + body + new String('\n', 40); output = Regex.Replace(output, "(?im)^%lindaworkers=.*(?:\\r?\\n)?", ""); string dir = Path.GetDirectoryName(Path.GetFullPath(file.Text)); File.WriteAllText(Path.Combine(dir, jobTitle + ".com"), output, new UTF8Encoding(false));
            if (makeLinda.Checked) { Stream templateStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("LindaTemplate"); if (templateStream == null) throw new Exception("The built-in Linda script template is missing."); using (var reader = new StreamReader(templateStream)) template = reader.ReadToEnd(); string sh = Regex.Replace(template, @"(?m)^(\s*#SBATCH\s+--job-name\s*=\s*).*$", m => m.Groups[1].Value + jobTitle); sh = Regex.Replace(sh, @"(?m)^(\s*JobFile\s*=\s*).*$", m => m.Groups[1].Value + jobTitle); sh = Regex.Replace(sh, @"(?m)^(\s*#SBATCH\s+--cpus-per-task\s*=\s*)\d+.*$", m => m.Groups[1].Value + processors.Text.Trim()); if (Regex.IsMatch(sh, @"(?m)^\s*#SBATCH\s+--mem\s*=")) sh = Regex.Replace(sh, @"(?m)^(\s*#SBATCH\s+--mem\s*=\s*).*$", m => m.Groups[1].Value + memory.Text.Trim()); else sh = Regex.Replace(sh, @"(?m)^(\s*#SBATCH\s+--job-name\s*=.*)$", "$1\n#SBATCH --mem=" + memory.Text.Trim()); File.WriteAllText(Path.Combine(dir, "linda.sh"), Regex.Replace(sh, "\\r\\n|\\r|\\n", "\n"), new UTF8Encoding(false)); }
            MessageBox.Show("Generated " + jobTitle + ".com" + (makeLinda.Checked ? " and linda.sh" : "") + " in:\n" + dir, "Files generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
        } catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot generate", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
    [STAThread] static void Main() { Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false); Application.Run(new GaussianManager()); }
}

internal sealed class MaterialCard : Panel
{
    public MaterialCard() { SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true); }
    protected override void OnPaintBackground(PaintEventArgs e) { e.Graphics.Clear(Parent == null ? BackColor : Parent.BackColor); using (GraphicsPath path = Shape(ClientRectangle)) using (SolidBrush brush = new SolidBrush(BackColor)) e.Graphics.FillPath(brush, path); }
    protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); Rectangle r = ClientRectangle; r.Width -= 1; r.Height -= 1; using (GraphicsPath path = Shape(r)) using (Pen pen = new Pen(Color.FromArgb(73, 69, 79), 1)) e.Graphics.DrawPath(pen, path); }
    GraphicsPath Shape(Rectangle r) { GraphicsPath p = new GraphicsPath(); int d = 28; p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right-d, r.Y, d, d, 270, 90); p.AddArc(r.Right-d, r.Bottom-d, d, d, 0, 90); p.AddArc(r.X, r.Bottom-d, d, d, 90, 90); p.CloseFigure(); return p; }
}

internal sealed class GradientButton : Button
{
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Rectangle bounds = ClientRectangle; bounds.Width -= 1; bounds.Height -= 1;
        using (GraphicsPath path = new GraphicsPath())
        {
            int d = 12; path.AddArc(bounds.X, bounds.Y, d, d, 180, 90); path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90); path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90); path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90); path.CloseFigure();
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(208, 188, 255))) e.Graphics.FillPath(brush, path);
        }
        TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, Color.FromArgb(56, 30, 78), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
    }
}




