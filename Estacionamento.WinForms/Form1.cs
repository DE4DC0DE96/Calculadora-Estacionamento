using System.Globalization;
using Estacionamento.Core.Models;
using Estacionamento.Core.Services;

namespace Estacionamento.WinForms;

public partial class Form1 : Form
{
    private readonly CultureInfo culture = CultureInfo.GetCultureInfo("pt-BR");
    private readonly ParkingSettings settings;
    private readonly ParkingCalculator calculator;

    private ComboBox tipoVeiculoComboBox = null!;
    private DateTimePicker entradaPicker = null!;
    private DateTimePicker saidaPicker = null!;
    private Label permanenciaValueLabel = null!;
    private Label minutosValueLabel = null!;
    private Label horasValueLabel = null!;
    private Label valorValueLabel = null!;
    private Label statusLabel = null!;

    public Form1()
    {
        InitializeComponent();
        settings = ParkingSettingsProvider.Load();
        calculator = new ParkingCalculator(settings);
        BuildLayout();
    }

    private void BuildLayout()
    {
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(820, 560);
        ClientSize = new Size(920, 620);
        BackColor = Color.FromArgb(241, 244, 248);
        Font = new Font("Segoe UI", 10F);

        TableLayoutPanel shell = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 112F));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        Controls.Add(shell);

        shell.Controls.Add(CreateHeader(), 0, 0);
        shell.Controls.Add(CreateMainContent(), 0, 1);

        statusLabel = new Label
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 0, 24, 0),
            ForeColor = Color.FromArgb(174, 54, 67),
            TextAlign = ContentAlignment.MiddleLeft
        };
        shell.Controls.Add(statusLabel, 0, 2);
    }

    private Control CreateHeader()
    {
        Panel header = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(22, 34, 48),
            Padding = new Padding(28, 18, 28, 16)
        };

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
        header.Controls.Add(layout);

        Label title = new()
        {
            Text = "Calculadora de Estacionamento",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.BottomLeft
        };
        layout.Controls.Add(title, 0, 0);

        Label subtitle = new()
        {
            Text = "Tarifas configuráveis por arquivo JSON",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(204, 214, 226),
            TextAlign = ContentAlignment.TopLeft
        };
        layout.Controls.Add(subtitle, 0, 1);

        Label rule = new()
        {
            Text = $"Grátis até {settings.ToleranciaSaidaGratuitaMinutos} min\r\n+ {settings.ToleranciaDemaisHorasMinutos} min nas horas cobradas",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(188, 231, 206),
            TextAlign = ContentAlignment.MiddleRight
        };
        layout.Controls.Add(rule, 1, 0);
        layout.SetRowSpan(rule, 2);

        return header;
    }

    private Control CreateMainContent()
    {
        TableLayoutPanel content = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 2,
            RowCount = 1
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56F));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44F));

        content.Controls.Add(CreateInputPanel(), 0, 0);
        content.Controls.Add(CreateResultPanel(), 1, 0);

        return content;
    }

    private Control CreateInputPanel()
    {
        GroupBox box = CreateGroupBox("Dados da permanência");

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 14, 16, 16),
            ColumnCount = 2,
            RowCount = 8
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        box.Controls.Add(layout);

        tipoVeiculoComboBox = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10F)
        };
        tipoVeiculoComboBox.Items.AddRange(settings.Tarifas.Keys.Order(StringComparer.CurrentCultureIgnoreCase).Cast<object>().ToArray());
        if (tipoVeiculoComboBox.Items.Count > 0)
        {
            tipoVeiculoComboBox.SelectedIndex = 0;
        }

        entradaPicker = CreateDateTimePicker(DateTime.Today.AddHours(8));
        saidaPicker = CreateDateTimePicker(DateTime.Today.AddHours(9));

        AddField(layout, "Tipo de veículo", tipoVeiculoComboBox, 0);
        AddField(layout, "Entrada", entradaPicker, 1);
        AddField(layout, "Saída", saidaPicker, 2);

        Button calcularButton = new()
        {
            Text = "Calcular",
            Dock = DockStyle.Left,
            Width = 150,
            Height = 38,
            BackColor = Color.FromArgb(28, 97, 185),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        calcularButton.FlatAppearance.BorderSize = 0;
        calcularButton.Click += CalcularButton_Click;
        layout.Controls.Add(calcularButton, 1, 3);

        Label tariffsTitle = new()
        {
            Text = "Tarifas carregadas",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(43, 52, 64),
            TextAlign = ContentAlignment.BottomLeft
        };
        layout.Controls.Add(tariffsTitle, 0, 5);
        layout.SetColumnSpan(tariffsTitle, 2);

        Control tariffsPanel = CreateTariffsPanel();
        layout.Controls.Add(tariffsPanel, 0, 6);
        layout.SetColumnSpan(tariffsPanel, 2);

        Label note = new()
        {
            Text = "Permanências de até 15 minutos não são cobradas.",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(91, 101, 116),
            TextAlign = ContentAlignment.MiddleLeft
        };
        layout.Controls.Add(note, 0, 7);
        layout.SetColumnSpan(note, 2);

        AcceptButton = calcularButton;
        return box;
    }

    private Control CreateResultPanel()
    {
        GroupBox box = CreateGroupBox("Resultado");

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18, 14, 18, 18),
            ColumnCount = 1,
            RowCount = 7
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        box.Controls.Add(layout);

        valorValueLabel = new Label
        {
            Text = "R$ 0,00",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 28F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 123, 91),
            TextAlign = ContentAlignment.MiddleLeft
        };
        layout.Controls.Add(valorValueLabel, 0, 0);

        permanenciaValueLabel = CreateMetric(layout, "Permanência", 2);
        minutosValueLabel = CreateMetric(layout, "Minutos considerados", 3);
        horasValueLabel = CreateMetric(layout, "Horas cobradas", 4);

        Label footer = new()
        {
            Text = "O valor é calculado com arredondamento para cima após aplicar a tolerância.",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(91, 101, 116),
            TextAlign = ContentAlignment.BottomLeft
        };
        layout.Controls.Add(footer, 0, 6);

        return box;
    }

    private Control CreateTariffsPanel()
    {
        FlowLayoutPanel panel = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Color.White
        };

        foreach ((string tipo, ParkingRate tarifa) in settings.Tarifas.OrderBy(item => item.Key, StringComparer.CurrentCultureIgnoreCase))
        {
            Label row = new()
            {
                AutoSize = false,
                Width = 390,
                Height = 30,
                ForeColor = Color.FromArgb(49, 57, 67),
                Text = $"{tipo}: 1ª hora {tarifa.PrimeiraHora.ToString("C", culture)} | demais {tarifa.DemaisHoras.ToString("C", culture)}",
                TextAlign = ContentAlignment.MiddleLeft
            };
            panel.Controls.Add(row);
        }

        return panel;
    }

    private static GroupBox CreateGroupBox(string title)
    {
        return new GroupBox
        {
            Text = title,
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(43, 52, 64),
            BackColor = Color.White
        };
    }

    private static DateTimePicker CreateDateTimePicker(DateTime value)
    {
        return new DateTimePicker
        {
            Dock = DockStyle.Fill,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy HH:mm",
            Value = value,
            Font = new Font("Segoe UI", 10F)
        };
    }

    private static void AddField(TableLayoutPanel content, string labelText, Control input, int row)
    {
        Label label = new()
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(49, 57, 67),
            Font = new Font("Segoe UI", 10F, FontStyle.Regular)
        };

        content.Controls.Add(label, 0, row);
        content.Controls.Add(input, 1, row);
    }

    private static Label CreateMetric(TableLayoutPanel layout, string labelText, int row)
    {
        TableLayoutPanel metric = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        metric.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
        metric.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));

        Label label = new()
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(91, 101, 116),
            Font = new Font("Segoe UI", 10F, FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleLeft
        };

        Label value = new()
        {
            Text = "-",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(28, 36, 48),
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleRight
        };

        metric.Controls.Add(label, 0, 0);
        metric.Controls.Add(value, 1, 0);
        layout.Controls.Add(metric, 0, row);

        return value;
    }

    private void CalcularButton_Click(object? sender, EventArgs e)
    {
        try
        {
            statusLabel.Text = string.Empty;
            string tipoVeiculo = tipoVeiculoComboBox.SelectedItem?.ToString() ?? string.Empty;
            ParkingCalculationResult result = calculator.Calculate(tipoVeiculo, entradaPicker.Value, saidaPicker.Value);

            permanenciaValueLabel.Text = FormatDuration(result.Permanencia);
            minutosValueLabel.Text = $"{result.MinutosConsiderados} min";
            horasValueLabel.Text = result.HorasCobradas == 0 ? "Grátis" : $"{result.HorasCobradas} hora(s)";
            valorValueLabel.Text = result.ValorTotal.ToString("C", culture);
            valorValueLabel.ForeColor = result.ValorTotal == 0m
                ? Color.FromArgb(68, 78, 91)
                : Color.FromArgb(15, 123, 91);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or FileNotFoundException)
        {
            permanenciaValueLabel.Text = "-";
            minutosValueLabel.Text = "-";
            horasValueLabel.Text = "-";
            valorValueLabel.Text = "R$ 0,00";
            valorValueLabel.ForeColor = Color.FromArgb(68, 78, 91);
            statusLabel.Text = ex.Message;
        }
    }

    private static string FormatDuration(TimeSpan duration)
    {
        int totalHours = (int)Math.Floor(duration.TotalHours);
        int minutes = duration.Minutes;

        if (totalHours == 0)
        {
            return $"{minutes} minuto(s)";
        }

        return $"{totalHours} hora(s) e {minutes} minuto(s)";
    }
}
