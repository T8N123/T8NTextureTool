using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;
using ImageMagick;

public class Theme
{
    public string Name { get; set; }
    public int[] BackColor { get; set; }
    public int[] ForeColor { get; set; }
    public int[] ButtonColor { get; set; }
    public int[] AccentColor { get; set; }

    public Color GetColor(int[] rgb)
    {
        return Color.FromArgb(rgb[0], rgb[1], rgb[2]);
    }
}

public class Preferences
{
    public string SelectedTheme { get; set; }
    public string LastInputFile { get; set; }
    public string LastOutputFolder { get; set; }

    public bool Mips { get; set; }
    public bool Alpha { get; set; }
    public bool StripAlpha { get; set; }
}

public static class ThemeManager
{
    public static Theme CurrentTheme;

    public static List<Theme> Themes = new List<Theme>();

    public static string ThemeFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "themes");

    public static void LoadThemes()
    {
        if (!Directory.Exists(ThemeFolder))
            Directory.CreateDirectory(ThemeFolder);

        Themes.Clear();

        // Load custom themes
        foreach (var file in Directory.GetFiles(ThemeFolder, "*.json"))
        {
            try
            {
                string json = File.ReadAllText(file);
                var theme = JsonSerializer.Deserialize<Theme>(json);
                Themes.Add(theme);
            }
            catch { }
        }

        // Add built-in themes
        Themes.Add(GetDarkTheme());
        Themes.Add(GetLightTheme());
        Themes.Add(GetT8NTheme());
        Themes.Add(GetMissingTheme());

        CurrentTheme = Themes[0];
    }

    public static Theme GetDarkTheme() => new Theme
    {
        Name = "Dark",
        BackColor = new[] { 20, 20, 20 },
        ForeColor = new[] { 255, 255, 255 },
        ButtonColor = new[] { 40, 40, 40 },
        AccentColor = new[] { 40, 120, 255 }
    };

    public static Theme GetLightTheme() => new Theme
    {
        Name = "Light",
        BackColor = new[] { 240, 240, 240 },
        ForeColor = new[] { 20, 20, 20 },
        ButtonColor = new[] { 220, 220, 220 },
        AccentColor = new[] { 0, 120, 215 }
    };

    public static Theme GetT8NTheme() => new Theme
    {
        Name = "T8N",
        BackColor = new[] { 10, 30, 10 },
        ForeColor = new[] { 120, 255, 120 },
        ButtonColor = new[] { 20, 60, 20 },
        AccentColor = new[] { 0, 255, 100 }
    };

        public static Theme GetMissingTheme() => new Theme
    {
        Name = "Missing",
        BackColor = new[] { 255, 0, 234 },
        ForeColor = new[] { 0, 0, 0 },
        ButtonColor = new[] { 106, 0, 99 },
        AccentColor = new[] { 255, 91, 242 }
    };
}

public static class PreferencesManager
{
    public static string PrefFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "preferences");
    public static string PrefFile = Path.Combine(PrefFolder, "prefs.json");

    public static Preferences Current = new Preferences();

    public static void Load()
    {
        if (!Directory.Exists(PrefFolder))
            Directory.CreateDirectory(PrefFolder);

        if (File.Exists(PrefFile))
        {
            try
            {
                string json = File.ReadAllText(PrefFile);
                Current = JsonSerializer.Deserialize<Preferences>(json) ?? new Preferences();
            }
            catch
            {
                Current = new Preferences();
            }
        }
    }

    public static void Save()
    {
        if (!Directory.Exists(PrefFolder))
            Directory.CreateDirectory(PrefFolder);

        string json = JsonSerializer.Serialize(Current, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(PrefFile, json);
    }
}

namespace T8NTextureTool
{
    public partial class Form1 : Form
    {
        string inputFile = "";
        string outputFolder = "";

        private void ApplyTheme(Control parent)
        {
            var t = ThemeManager.CurrentTheme;

            this.BackColor = t.GetColor(t.BackColor);
            this.ForeColor = t.GetColor(t.ForeColor);

            foreach (Control c in parent.Controls)
            {
                if (c is Button btn)
                {
                    btn.BackColor = t.GetColor(t.ButtonColor);
                    btn.ForeColor = t.GetColor(t.ForeColor);
                    btn.FlatStyle = FlatStyle.Flat;
                }
                else if (c is TextBox tb)
                {
                    tb.BackColor = Color.FromArgb(10, 10, 10);
                    tb.ForeColor = t.GetColor(t.ForeColor);
                }
                else if (c is Label lbl)
                {
                    lbl.ForeColor = t.GetColor(t.ForeColor);
                }
                else if (c is ComboBox cb)
                {
                    cb.BackColor = t.GetColor(t.ButtonColor);
                    cb.ForeColor = t.GetColor(t.ForeColor);
                }
                else if (c is CheckBox chk)
                {
                    chk.ForeColor = t.GetColor(t.ForeColor);
                }

                if (c.HasChildren)
                    ApplyTheme(c);
            }
        }

        private void OpenThemeEditor()
        {
            Form editor = new Form()
            {
                Text = "Theme Editor",
                Size = new Size(400, 500),
                StartPosition = FormStartPosition.CenterParent
            };

            var current = new Theme()
            {
                Name = "NewTheme",
                BackColor = new[] { 20, 20, 20 },
                ForeColor = new[] { 255, 255, 255 },
                ButtonColor = new[] { 40, 40, 40 },
                AccentColor = new[] { 0, 120, 255 }
            };

            FlowLayoutPanel layout = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10)
            };

            TextBox nameBox = new TextBox() { Width = 300, Text = current.Name };

            Button bgBtn = CreateColorPicker("Background", current.BackColor, c => current.BackColor = c);
            Button fgBtn = CreateColorPicker("Text", current.ForeColor, c => current.ForeColor = c);
            Button btnBtn = CreateColorPicker("Buttons", current.ButtonColor, c => current.ButtonColor = c);
            Button accentBtn = CreateColorPicker("Accent", current.AccentColor, c => current.AccentColor = c);

            Button previewBtn = new Button() { Text = "Preview", Width = 300 };
            Button saveBtn = new Button() { Text = "Save Theme", Width = 300 };

            previewBtn.Click += (s, e) =>
            {
                current.Name = nameBox.Text;
                ThemeManager.CurrentTheme = current;
                ApplyTheme(this);
                ApplyTheme(editor);
            };

            saveBtn.Click += (s, e) =>
            {
                current.Name = nameBox.Text;

                string path = Path.Combine(ThemeManager.ThemeFolder, current.Name + ".json");
                string json = JsonSerializer.Serialize(current, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(path, json);

                ThemeManager.LoadThemes();

                MessageBox.Show("Theme saved!");
            };

            layout.Controls.Add(new Label() { Text = "Theme Name" });
            layout.Controls.Add(nameBox);
            layout.Controls.Add(bgBtn);
            layout.Controls.Add(fgBtn);
            layout.Controls.Add(btnBtn);
            layout.Controls.Add(accentBtn);
            layout.Controls.Add(previewBtn);
            layout.Controls.Add(saveBtn);

            editor.Controls.Add(layout);

            ApplyTheme(editor);

            editor.ShowDialog();
        }

        private Button CreateColorPicker(string label, int[] initial, Action<int[]> onColorChanged)
        {
            Button btn = new Button()
            {
                Text = label,
                Width = 300,
                Height = 40
            };

            btn.Click += (s, e) =>
            {
                ColorDialog cd = new ColorDialog();

                cd.Color = Color.FromArgb(initial[0], initial[1], initial[2]);

                if (cd.ShowDialog() == DialogResult.OK)
                {
                    int[] rgb = new[] { (int)cd.Color.R, (int)cd.Color.G, (int)cd.Color.B };
                    onColorChanged(rgb);

                    btn.BackColor = cd.Color;
                }
            };

            return btn;
        }

        public Form1()
        {
            InitializeComponent();

            ThemeManager.LoadThemes();
            PreferencesManager.Load();

            MenuStrip menu = new MenuStrip()
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };

            var fileMenu = new ToolStripMenuItem("File");
            var preferencesMenu = new ToolStripMenuItem("Preferences");
            var helpMenu = new ToolStripMenuItem("Help");
            var selectImageItem = new ToolStripMenuItem("Select Image");
            var exportMenu = new ToolStripMenuItem("Export");
            var selectFolderItem = new ToolStripMenuItem("Select Output Folder");

            fileMenu.DropDownItems.Add(selectImageItem);            
            
            exportMenu.DropDownItems.Add(selectFolderItem);                        

            menu.Items.Add(fileMenu);
            menu.Items.Add(exportMenu);
            menu.Items.Add(preferencesMenu);
            menu.Items.Add(helpMenu);

            this.Controls.Add(menu);

            Panel root = new Panel()
            {
                Dock = DockStyle.Fill,
            };

            this.Controls.Add(root);

            Panel content = new Panel()
            {
                Size = new Size(460, 600), // your "designed size"
                BackColor = Color.Transparent
            };

            root.Controls.Add(content);

            root.Resize += (s, e) =>
            {
                content.Left = (root.Width - content.Width) / 2;
                content.Top = (root.Height - content.Height) / 2;
            };

            // Layout
            FlowLayoutPanel layout = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false,
                Padding = new Padding(20),
            };

            content.Controls.Add(layout);

            foreach (Control c in layout.Controls)
            {
                c.Width = 400;
                c.Margin = new Padding(0, 10, 0, 10);
            }

            layout.ControlAdded += (s, e) =>
            {
                var c = e.Control;
                c.Anchor = AnchorStyles.None;
            };      

            layout.Resize += (s, e) =>
            {
                foreach (Control c in layout.Controls)
                {
                    c.Left = (layout.ClientSize.Width - c.Width) / 2;
                }
            };                  

            // Window
            try
            {
                var stream = System.Reflection.Assembly
                    .GetExecutingAssembly()
                    .GetManifestResourceStream("T8NTextureTool.app.ico");                 

                if (stream != null)
                    this.Icon = new Icon(stream);
            }
            catch { }
            this.ShowIcon = true;
            this.Text = "T8N Texture Tool";
            this.Size = new Size(500, 650);
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.ForeColor = Color.White;

            int left = 20;
            int width = 420;
            int y = 20;

            // Resolution
            Label pxLabel = new Label()
            {
                Text = "Resolution",
                
                ForeColor = Color.LightGray
            };
            y += 25;

            TextBox pxBox = new TextBox()
            {
                
                Width = width,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            y += 45;

            // ASTC
            Label astcLabel = new Label()
            {
                Text = "ASTC Block Size",
                
                ForeColor = Color.LightGray
            };
            y += 25;

            Label inputLabel = new Label()
            {
                Text = "Input Image",
                ForeColor = Color.LightGray
            };

            Label outputLabel = new Label()
            {
                Text = "Output Folder",
                ForeColor = Color.LightGray
            };

            Label nameLabel = new Label()
            {
                Text = "Output File Name Pattern",
                ForeColor = Color.LightGray
            };

            TextBox nameBox = new TextBox()
            {
                Width = width,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "output_{index}"
            };

            ComboBox astcBox = new ComboBox()
            {
                
                Width = width,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            astcBox.Items.AddRange(new string[] { "6x6", "4x4" });
            astcBox.SelectedIndex = 0;
            y += 50;

            // Checkboxes
            FlowLayoutPanel optionsPanel = new FlowLayoutPanel()
            {
                
                Width = width,
                Height = 40
            };

            CheckBox mips = CreateCheckBox("1Mips");
            CheckBox alpha = CreateCheckBox("Alpha");
            CheckBox stripAlpha = CreateCheckBox("Strip Alpha");

            TextBox inputPathBox = new TextBox()
            {
                Width = width,
                ReadOnly = true,
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.LightGray,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "No image selected"
            };

            TextBox outputPathBox = new TextBox()
            {
                Width = width,
                ReadOnly = true,
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.LightGray,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "No output folder selected"
            };

            optionsPanel.Controls.Add(mips);
            optionsPanel.Controls.Add(alpha);
            optionsPanel.Controls.Add(stripAlpha);

            // Apply saved preferences
            var prefs = PreferencesManager.Current;

            inputFile = prefs.LastInputFile ?? "";
            outputFolder = prefs.LastOutputFolder ?? "";

            mips.Checked = prefs.Mips;
            alpha.Checked = prefs.Alpha;
            stripAlpha.Checked = prefs.StripAlpha;

            if (!string.IsNullOrEmpty(inputFile))
                inputPathBox.Text = inputFile;

            if (!string.IsNullOrEmpty(outputFolder))
                outputPathBox.Text = outputFolder;

            if (!File.Exists(inputFile))
            {
                inputFile = "";
                inputPathBox.Text = "No image selected";
            }

            if (!Directory.Exists(outputFolder))
            {
                outputFolder = "";
                outputPathBox.Text = "No output folder selected";
            }

            // Apply theme
            if (!string.IsNullOrEmpty(prefs.SelectedTheme))
            {
                var theme = ThemeManager.Themes.FirstOrDefault(t => t.Name == prefs.SelectedTheme);
                if (theme != null)
                    ThemeManager.CurrentTheme = theme;
            }            

            y += 60;

            // File buttons
            // OLD --> Button fileBtn = CreateButton("Select Image");
            //         fileBtn.Location = new Point(left, y);

            // OLD --> Button folderBtn = CreateButton("Select Output Folder");
            //         folderBtn.Location = new Point(left + 210, y);


            // Run button
            Button runBtn = CreateButton("Run Command");
            runBtn.Location = new Point(left, y);
            runBtn.Width = width;
            runBtn.Height = 45;
            runBtn.BackColor = Color.FromArgb(40, 120, 255);

            // Status label
            Label statusLabel = new Label()
            {
                Text = "Ready",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = width,
                ForeColor = Color.LightGray
            };

            // ================= EVENTS =================

            preferencesMenu.Click += (s, e) =>
            {
                Form pref = new Form()
                {
                    Text = "Preferences",
                    Size = new Size(300, 400),
                    BackColor = ThemeManager.CurrentTheme.GetColor(ThemeManager.CurrentTheme.BackColor)
                };

                ListBox themeList = new ListBox()
                {
                    Dock = DockStyle.Fill
                };

                foreach (var theme in ThemeManager.Themes)
                    themeList.Items.Add(theme.Name);

                themeList.SelectedIndexChanged += (s2, e2) =>
                {
                    var selected = ThemeManager.Themes[themeList.SelectedIndex];
                    ThemeManager.CurrentTheme = selected;
                    ApplyTheme(this);
                    ApplyTheme(pref);
                };

                Button editThemeBtn = new Button()
                {
                    Text = "Create / Edit Theme",
                    Dock = DockStyle.Bottom,
                    Height = 40
                };

                Button savePrefsBtn = new Button()
                {
                    Text = "Save Preferences",
                    Dock = DockStyle.Bottom,
                    Height = 40
                };

                savePrefsBtn.Click += (s4, e4) =>
                {
                    var prefs = PreferencesManager.Current;

                    prefs.SelectedTheme = ThemeManager.CurrentTheme.Name;
                    prefs.LastInputFile = inputFile;
                    prefs.LastOutputFolder = outputFolder;

                    prefs.Mips = mips.Checked;
                    prefs.Alpha = alpha.Checked;
                    prefs.StripAlpha = stripAlpha.Checked;

                    PreferencesManager.Save();

                    MessageBox.Show("Preferences saved!");
                };                

                editThemeBtn.Click += (s3, e3) =>
                {
                    OpenThemeEditor();
                };

                pref.Controls.Add(editThemeBtn);
                pref.Controls.Add(savePrefsBtn);
                pref.Controls.Add(themeList);
                pref.ShowDialog();
            };

            selectImageItem.Click += (s, e) =>
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "PNG Files (*.png)|*.png";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    inputFile = ofd.FileName;
                    selectImageItem.Text = "Select Image ✔";

                    inputPathBox.Text = inputFile; // update textbox
                }
            };            

            selectFolderItem.Click += (s, e) =>
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    outputFolder = fbd.SelectedPath;
                    selectFolderItem.Text = "Output Folder ✔";

                    outputPathBox.Text = outputFolder; // ADD THIS
                }
            };

            // Run Command
            runBtn.Click += async (s, e) =>
            {
                if (string.IsNullOrEmpty(inputFile) || string.IsNullOrEmpty(outputFolder))
                {
                    MessageBox.Show("Missing input/output");
                    return;
                }

                statusLabel.Text = "Processing...";
                statusLabel.ForeColor = Color.Gold;

                try
                {
                    int px = int.Parse(pxBox.Text);

                    using (var image = new MagickImage(inputFile))
                    {
                        if (stripAlpha.Checked)
                        {
                            image.Alpha(AlphaOption.Off);
                        }

                        int count = 0;

                        for (int y = 0; y < image.Height; y += px)
                        {
                            for (int x = 0; x < image.Width; x += px)
                            {
                                var clone = image.Clone();

                                var geometry = new MagickGeometry(x, y, (uint)px, (uint)px) 
                                {
                                    IgnoreAspectRatio = true
                                };

                                clone.Crop(geometry);

                                // Equivalent to +repage
                                clone.Page = new MagickGeometry();

                                string astc = astcBox.Text.Contains("4") ? "4" : "6";

                                string prefix = px.ToString();
                                if (mips.Checked) prefix += "mips";
                                if (astc == "4") prefix += "astc4x4";
                                if (alpha.Checked) prefix += "alpha";

                                string pattern = nameBox.Text;

                                // Replace tokens
                                string fileNameFormatted = pattern
                                    .Replace("{index}", count.ToString("D4"))
                                    .Replace("{x}", x.ToString())
                                    .Replace("{y}", y.ToString())
                                    .Replace("{px}", px.ToString());

                                string fileName = Path.Combine(outputFolder, $"{prefix}{fileNameFormatted}.png");
                                clone.Write(fileName);

                                count++;
                            }
                        }
                    }

                    statusLabel.Text = "✔ Success!";
                    statusLabel.ForeColor = Color.LightGreen;
                }
                catch (Exception ex)
                {
                    statusLabel.Text = "Error: " + ex.Message;
                    statusLabel.ForeColor = Color.Red;
                }
            };

            // Add controls
            layout.Controls.Add(pxLabel);
            layout.Controls.Add(pxBox);
            layout.Controls.Add(astcLabel);
            layout.Controls.Add(astcBox);
            layout.Controls.Add(optionsPanel);
            // OLD --> layout.Controls.Add(fileBtn);
            // OLD --> layout.Controls.Add(folderBtn);
            layout.Controls.Add(runBtn);
            layout.Controls.Add(statusLabel);
            layout.Controls.Add(inputLabel);
            layout.Controls.Add(inputPathBox);
            layout.Controls.Add(outputLabel);
            layout.Controls.Add(outputPathBox);
            layout.Controls.Add(nameLabel);
            layout.Controls.Add(nameBox);

            ApplyTheme(this);
        }

        private Button CreateButton(string text)
        {
            return new Button()
            {
                Text = text,
                Width = 200,
                Height = 40,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
        }

        private CheckBox CreateCheckBox(string text)
        {
            return new CheckBox()
            {
                Text = text,
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(10, 10, 20, 10)
            };
        }
    }
}