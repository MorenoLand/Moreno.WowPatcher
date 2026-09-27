using Moreno.WowPatcher.Patching;

namespace Moreno.WowPatcher.UI;

internal sealed class MainForm : Form
{
	private static readonly Color Surface = Color.FromArgb(25, 29, 36);
	private static readonly Color SurfaceRaised = Color.FromArgb(34, 40, 49);
	private static readonly Color Border = Color.FromArgb(58, 67, 80);
	private static readonly Color PrimaryText = Color.FromArgb(235, 238, 243);
	private static readonly Color Muted = Color.FromArgb(154, 164, 178);
	private static readonly Color Accent = Color.FromArgb(91, 141, 239);
	private readonly TextBox _pathBox = new();
	private readonly ComboBox _patchSelector = new();
	private readonly DarkButton _verifyButton = new();
	private readonly DarkButton _applyButton = new();
	private readonly DarkButton _restoreButton = new();
	private readonly Label _statusLabel = new();
	private readonly PatchEngine _patchEngine = new();

	public MainForm()
	{
		Text = "WoW Patcher";
		StartPosition = FormStartPosition.CenterScreen;
		ClientSize = new Size(470, 166);
		FormBorderStyle = FormBorderStyle.FixedSingle;
		MaximizeBox = false;
		BackColor = Surface;
		ForeColor = PrimaryText;
		Font = new Font("Segoe UI", 9.5f);
		AutoScaleMode = AutoScaleMode.Dpi;
		BuildUi();
	}

	private void BuildUi()
	{
		var root = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
			ColumnCount = 1,
			RowCount = 4,
			BackColor = Surface,
		};
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
		Controls.Add(root);

		var fileLayout = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			ColumnCount = 2,
			RowCount = 1,
			BackColor = Surface,
		};
		fileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		fileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82));
		_pathBox.Dock = DockStyle.Fill;
		_pathBox.BackColor = SurfaceRaised;
		_pathBox.ForeColor = PrimaryText;
		_pathBox.BorderStyle = BorderStyle.FixedSingle;
		_pathBox.Margin = new Padding(0, 1, 8, 1);
		_pathBox.PlaceholderText = "wow.exe";
		_pathBox.TextChanged += PathChanged;
		fileLayout.Controls.Add(_pathBox, 0, 0);
		var browseButton = CreateButton("Browse", false);
		browseButton.Dock = DockStyle.Fill;
		browseButton.Margin = new Padding(0);
		browseButton.Click += BrowseClicked;
		fileLayout.Controls.Add(browseButton, 1, 0);
		root.Controls.Add(fileLayout, 0, 0);

		var patchLayout = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			ColumnCount = 2,
			RowCount = 1,
			BackColor = Surface,
		};
		patchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		patchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82));
		_patchSelector.DataSource = PatchCatalog.Definitions;
		_patchSelector.DisplayMember = nameof(PatchDefinition.Name);
		_patchSelector.DropDownStyle = ComboBoxStyle.DropDownList;
		_patchSelector.Dock = DockStyle.Fill;
		_patchSelector.BackColor = SurfaceRaised;
		_patchSelector.ForeColor = PrimaryText;
		_patchSelector.FlatStyle = FlatStyle.Flat;
		_patchSelector.Margin = new Padding(0, 1, 8, 1);
		patchLayout.Controls.Add(_patchSelector, 0, 0);
		_verifyButton.Text = "Verify";
		_verifyButton.Click += VerifyClicked;
		patchLayout.Controls.Add(_verifyButton, 1, 0);
		root.Controls.Add(patchLayout, 0, 1);

		var actions = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = false,
			BackColor = Surface,
			Padding = new Padding(0, 6, 0, 0),
		};
		_applyButton.Text = "Apply";
		_applyButton.Enabled = false;
		_applyButton.Click += ApplyClicked;
		actions.Controls.Add(_applyButton);
		_restoreButton.Text = "Restore";
		_restoreButton.Enabled = false;
		_restoreButton.Click += RestoreClicked;
		actions.Controls.Add(_restoreButton);
		root.Controls.Add(actions, 0, 2);
		ApplyButtonStyle(_verifyButton, Accent);
		ApplyButtonStyle(_applyButton, SurfaceRaised);
		ApplyButtonStyle(_restoreButton, SurfaceRaised);
		_statusLabel.Dock = DockStyle.Fill;
		_statusLabel.ForeColor = Muted;
		_statusLabel.TextAlign = ContentAlignment.MiddleLeft;
		_statusLabel.AutoEllipsis = true;
		root.Controls.Add(_statusLabel, 0, 3);
		_patchSelector.SelectedIndexChanged += PatchSelectedChanged;
		SetStatus("Select wow.exe.");
	}

	private DarkButton CreateButton(string text, bool primary)
	{
		var button = new DarkButton
		{
			Text = text,
			AutoSize = false,
			Size = new Size(80, 32),
			FlatStyle = FlatStyle.Flat,
			BackColor = primary ? Accent : SurfaceRaised,
			ForeColor = PrimaryText,
			UseVisualStyleBackColor = false,
			TextAlign = ContentAlignment.MiddleCenter,
			Padding = new Padding(0),
			Margin = new Padding(0, 0, 8, 0),
		};
		ApplyButtonStyle(button, primary ? Accent : SurfaceRaised);
		return button;
	}

	private static void ApplyButtonStyle(DarkButton button, Color color)
	{
		button.BackColor = color;
		button.ForeColor = PrimaryText;
		button.FlatAppearance.BorderSize = 1;
		button.FlatAppearance.BorderColor = Border;
		button.FlatAppearance.MouseOverBackColor = Color.FromArgb(
			Math.Min(color.R + 18, 255),
			Math.Min(color.G + 18, 255),
			Math.Min(color.B + 18, 255));
		button.FlatAppearance.MouseDownBackColor = Color.FromArgb(
			Math.Max(color.R - 12, 0),
			Math.Max(color.G - 12, 0),
			Math.Max(color.B - 12, 0));
	}

	private void BrowseClicked(object? sender, EventArgs e)
	{
		using var dialog = new OpenFileDialog
		{
			Title = "Choose wow.exe",
			Filter = "WoW executable (wow.exe)|wow.exe|Executable files (*.exe)|*.exe",
			CheckFileExists = true,
			Multiselect = false,
		};
		if (dialog.ShowDialog(this) != DialogResult.OK) return;
		_pathBox.Text = dialog.FileName;
		VerifyClicked(this, EventArgs.Empty);
	}

	private void VerifyClicked(object? sender, EventArgs e)
	{
		ShowPatchResult(_patchEngine.Inspect(_pathBox.Text.Trim(), SelectedPatch));
	}

	private void ApplyClicked(object? sender, EventArgs e)
	{
		ShowPatchResult(_patchEngine.Apply(_pathBox.Text.Trim(), SelectedPatch));
	}

	private void RestoreClicked(object? sender, EventArgs e)
	{
		ShowPatchResult(_patchEngine.Restore(_pathBox.Text.Trim(), SelectedPatch));
	}

	private PatchDefinition? SelectedPatch => _patchSelector.SelectedItem as PatchDefinition;

	private void ShowPatchResult(PatchResult result)
	{
		SetStatus(result.Message);
		var ready = result.State == PatchState.Ready;
		var applied = result.State == PatchState.Applied;
		_applyButton.Enabled = ready;
		_restoreButton.Enabled = applied;
		_verifyButton.Text = result.State switch
		{
			PatchState.Ready => "Ready",
			PatchState.Applied => "Applied",
			PatchState.Partial => "Partial",
			_ => "Verify",
		};
		var color = result.State switch
		{
			PatchState.Ready or PatchState.Applied => Color.FromArgb(47, 118, 82),
			PatchState.Partial => Color.FromArgb(140, 100, 46),
			_ => Color.FromArgb(112, 52, 60),
		};
		ApplyButtonStyle(_verifyButton, color);
		_pathBox.BackColor = result.State switch
		{
			PatchState.Ready or PatchState.Applied => Color.FromArgb(33, 65, 49),
			PatchState.Partial => Color.FromArgb(70, 58, 37),
			_ => SurfaceRaised,
		};
	}

	private void PathChanged(object? sender, EventArgs e)
	{
		_pathBox.BackColor = SurfaceRaised;
		_verifyButton.Text = "Verify";
		_applyButton.Enabled = false;
		_restoreButton.Enabled = false;
		ApplyButtonStyle(_verifyButton, Accent);
		SetStatus(_pathBox.TextLength == 0 ? "Select wow.exe." : "Ready to verify selected patch.");
	}

	private void PatchSelectedChanged(object? sender, EventArgs e)
	{
		_applyButton.Enabled = false;
		_restoreButton.Enabled = false;
		_verifyButton.Text = "Verify";
		ApplyButtonStyle(_verifyButton, Accent);
		SetStatus("Ready to verify selected patch.");
	}

	private void SetStatus(string message)
	{
		_statusLabel.Text = message;
	}

	private sealed class DarkButton : Button
	{
		private bool _isHovered;
		private bool _isPressed;

		public DarkButton()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
			TabStop = false;
		}

		protected override bool ShowFocusCues => false;

		protected override void OnMouseEnter(EventArgs e)
		{
			_isHovered = true;
			Invalidate();
			base.OnMouseEnter(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			_isHovered = false;
			_isPressed = false;
			Invalidate();
			base.OnMouseLeave(e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			_isPressed = e.Button == MouseButtons.Left;
			Invalidate();
			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			_isPressed = false;
			Invalidate();
			base.OnMouseUp(e);
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			Invalidate();
			base.OnEnabledChanged(e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			var bounds = ClientRectangle;
			var background = BackColor;
			if (!Enabled) background = Color.FromArgb(29, 34, 41);
			else if (_isPressed) background = FlatAppearance.MouseDownBackColor;
			else if (_isHovered) background = FlatAppearance.MouseOverBackColor;
			var foreground = Enabled ? ForeColor : Color.FromArgb(122, 130, 143);
			using var backgroundBrush = new SolidBrush(background);
			using var borderPen = new Pen(!Enabled ? Color.FromArgb(52, 59, 70) : FlatAppearance.BorderColor);
			e.Graphics.FillRectangle(backgroundBrush, bounds);
			e.Graphics.DrawRectangle(borderPen, 0, 0, Math.Max(0, bounds.Width - 1), Math.Max(0, bounds.Height - 1));
			TextRenderer.DrawText(
				e.Graphics,
				Text,
				Font,
				bounds,
				foreground,
				TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		}
	}
}
