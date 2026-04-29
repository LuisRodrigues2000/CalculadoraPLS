using CalculadoraPLS.ViewModels;

namespace CalculadoraPLS.Views;

public partial class PlsPage : ContentPage
{
    // ── Controle de reentrância da máscara ───────────────────────────────────
    private bool _atualizandoEstimada = false;
    private bool _atualizandoReal = false;

    public PlsPage()
    {
        InitializeComponent();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PONTO 1 — MÁSCARA MONETÁRIA (Venda Estimada e Venda Real)
    // ─────────────────────────────────────────────────────────────────────────

    private void EntryEstimada_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_atualizandoEstimada) return;
        _atualizandoEstimada = true;
        try
        {
            var texto = e.NewTextValue ?? "";
            if (BindingContext is PlsViewModel vm)
                vm.VendaEstimada = texto;
        }
        finally { _atualizandoEstimada = false; }
    }

    private void EntryReal_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_atualizandoReal) return;
        _atualizandoReal = true;
        try
        {
            var texto = e.NewTextValue ?? "";
            if (BindingContext is PlsViewModel vm)
                vm.VendaReal = texto;
        }
        finally { _atualizandoReal = false; }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  HELPERS DE FORMATAÇÃO MONETÁRIA
    // ─────────────────────────────────────────────────────────────────────────

    private static string FormatarMoeda(string digitosCrus)
    {
        try
        {
            if (string.IsNullOrEmpty(digitosCrus)) return "";
            var apenasNumeros = ExtrairDigitos(digitosCrus);
            if (string.IsNullOrEmpty(apenasNumeros)) return "";
            if (!long.TryParse(apenasNumeros, out long valorCentavos)) return "";

            long reais = valorCentavos / 100;
            long centavos = valorCentavos % 100;

            return $"R$ {FormatarMilhar(reais.ToString())},{centavos:D2}";
        }
        catch { return ""; }
    }

    private static string FormatarMilhar(string inteiro)
    {
        if (string.IsNullOrEmpty(inteiro)) return "0";
        var resultado = "";
        var contador = 0;
        for (int i = inteiro.Length - 1; i >= 0; i--)
        {
            if (contador > 0 && contador % 3 == 0)
                resultado = "." + resultado;
            resultado = inteiro[i] + resultado;
            contador++;
        }
        return resultado;
    }

    private static string ExtrairDigitos(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return "";
        var sb = new System.Text.StringBuilder();
        foreach (char c in texto)
            if (char.IsDigit(c)) sb.Append(c);
        return sb.ToString();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PONTO 2 — LINHA DE FOCO (Venda Estimada e Venda Real)
    // ─────────────────────────────────────────────────────────────────────────

    private void EntryEstimada_Focused(object? sender, FocusEventArgs e)
        => FocusLineEstimada.BackgroundColor = Color.FromArgb("#0008FF");

    private void EntryEstimada_Unfocused(object? sender, FocusEventArgs e)
        => FocusLineEstimada.BackgroundColor = Color.FromArgb("#E8E8F0");

    private void EntryReal_Focused(object? sender, FocusEventArgs e)
        => FocusLineReal.BackgroundColor = Color.FromArgb("#0008FF");

    private void EntryReal_Unfocused(object? sender, FocusEventArgs e)
        => FocusLineReal.BackgroundColor = Color.FromArgb("#E8E8F0");

    // ─────────────────────────────────────────────────────────────────────────
    //  LINHA DE FOCO — CAMPOS DE CARNE (evento genérico reutilizável)
    //  O BoxView de foco fica na Row=1 do Grid pai do Entry.
    // ─────────────────────────────────────────────────────────────────────────

    private void OnEntryInputFocused(object? sender, FocusEventArgs e)
        => DefinirLinhaFocoCarne(sender, "#0008FF");

    private void OnEntryInputUnfocused(object? sender, FocusEventArgs e)
        => DefinirLinhaFocoCarne(sender, "#E8E8F0");

    /// <summary>
    /// Localiza o BoxView de foco que está na Row=1 do Grid pai do Entry
    /// e aplica a cor informada.
    /// </summary>
    private static void DefinirLinhaFocoCarne(object? sender, string hexCor)
    {
        if (sender is not Entry entry) return;

        // O Entry está dentro de um Grid (RowDefinitions="*,2")
        if (entry.Parent is Grid grid)
        {
            var linha = grid.Children
                .OfType<BoxView>()
                .FirstOrDefault(b => Grid.GetRow(b) == 1);

            if (linha is not null)
                linha.BackgroundColor = Color.FromArgb(hexCor);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PONTO 3 — ÁREA DE TOQUE DOS CARDS DE VENDA
    // ─────────────────────────────────────────────────────────────────────────

    private void OnCardEstimadaTapped(object? sender, TappedEventArgs e)
        => EntryEstimada.Focus();

    private void OnCardRealTapped(object? sender, TappedEventArgs e)
        => EntryReal.Focus();

    // ─────────────────────────────────────────────────────────────────────────
    //  NAVEGAÇÃO ENTRE CAMPOS — VENDAS
    // ─────────────────────────────────────────────────────────────────────────

    private void EntryEstimada_Completed(object? sender, EventArgs e)
        => EntryReal.Focus();

    private void EntryReal_Completed(object? sender, EventArgs e)
        => EntryHbRecipientes.Focus();   // avança para o primeiro campo de carne

    // ─────────────────────────────────────────────────────────────────────────
    //  NAVEGAÇÃO ENTRE CAMPOS — CARNES (Tab order completo)
    // ─────────────────────────────────────────────────────────────────────────

    private void OnEntryHbRecipientesCompleted(object? sender, EventArgs e)
        => EntryHbUnidades.Focus();

    private void OnEntryHbUnidadesCompleted(object? sender, EventArgs e)
        => EntryWhopperRecipientes.Focus();

    private void OnEntryWhopperRecipientesCompleted(object? sender, EventArgs e)
        => EntryWhopperUnidades.Focus();

    private void OnEntryWhopperUnidadesCompleted(object? sender, EventArgs e)
        => EntryRebelRecipientes.Focus();

    private void OnEntryRebelRecipientesCompleted(object? sender, EventArgs e)
        => EntryRebelUnidades.Focus();

    /// <summary>
    /// Último campo — fecha o teclado e dispara o cálculo automaticamente
    /// se todos os dados estiverem preenchidos.
    /// </summary>
    private void OnEntryRebelUnidadesCompleted(object? sender, EventArgs e)
    {
        EntryRebelUnidades.Unfocus();

        if (BindingContext is PlsViewModel vm && vm.CalcularCommand.CanExecute(null))
        {
            vm.CalcularCommand.Execute(null);
            _ = AnimarResultadosAsync();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  BOTÃO CALCULAR
    // ─────────────────────────────────────────────────────────────────────────

    private async void OnCalcularTapped(object? sender, TappedEventArgs e)
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HAPTIC] {ex.Message}");
        }

        await BtnCalcular.ScaleToAsync(0.95, 70, Easing.CubicOut);
        await BtnCalcular.ScaleToAsync(1.00, 70, Easing.CubicIn);

        if (BindingContext is PlsViewModel vm && vm.CalcularCommand.CanExecute(null))
            vm.CalcularCommand.Execute(null);

        await AnimarResultadosAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ANIMAÇÃO DE ENTRADA DOS RESULTADOS
    // ─────────────────────────────────────────────────────────────────────────

    private async Task AnimarResultadosAsync()
    {
        if (BindingContext is not PlsViewModel vm) return;
        if (!vm.ResultadoVisivel) return;

        PainelResultados.Opacity = 0;
        PainelResultados.TranslationY = 40;
        PainelResultados.IsVisible = true;

        CardDiferenca.Opacity = 0;
        CardDiferenca.TranslationY = 30;

        await PainelResultados.FadeToAsync(1, 300, Easing.CubicOut);
        PainelResultados.TranslationY = 0;

        await Task.WhenAll(
            CardDiferenca.FadeToAsync(1, 350, Easing.CubicOut),
            CardDiferenca.TranslateToAsync(0, 0, 350, Easing.CubicOut)
        );

        await Task.WhenAll(
            AnimarContadorAsync(vm.DiferencaReal),
            AnimarCardsCarneAsync()
        );

        await Task.Delay(100);
        await MainScroll.ScrollToAsync(ScrollAnchor, ScrollToPosition.Start, animated: true);

        await AnimarFabEntradaAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  FAB — ANIMAÇÃO DE ENTRADA
    // ─────────────────────────────────────────────────────────────────────────

    private async Task AnimarFabEntradaAsync()
    {
        BtnLimparContainer.Opacity = 0;
        BtnLimparContainer.TranslationY = 40;
        BtnLimparContainer.Scale = 0.7;
        BtnLimparContainer.IsVisible = true;

        await Task.WhenAll(
            BtnLimparContainer.FadeToAsync(1, 350, Easing.CubicOut),
            BtnLimparContainer.TranslateToAsync(0, 0, 400, Easing.SpringOut),
            BtnLimparContainer.ScaleToAsync(1, 400, Easing.SpringOut)
        );
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  FAB — ANIMAÇÃO DE SAÍDA
    // ─────────────────────────────────────────────────────────────────────────

    private async Task AnimarFabSaidaAsync()
    {
        await Task.WhenAll(
            BtnLimparContainer.FadeToAsync(0, 220, Easing.CubicIn),
            BtnLimparContainer.TranslateToAsync(0, 40, 220, Easing.CubicIn),
            BtnLimparContainer.ScaleToAsync(0.7, 220, Easing.CubicIn)
        );

        BtnLimparContainer.IsVisible = false;
        BtnLimparContainer.TranslationY = 40;
        BtnLimparContainer.Scale = 0.7;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CONTADOR ANIMADO DA DIFERENÇA %
    // ─────────────────────────────────────────────────────────────────────────

    private async Task AnimarContadorAsync(double valorFinal)
    {
        // ✅ Arredonda o percentual para inteiro antes de exibir (regra da apostila)
        double valorExibir = Math.Round(valorFinal, MidpointRounding.AwayFromZero);

        const int duracaoMs = 800;
        const int frames = 40;
        const int delayMs = duracaoMs / frames;

        for (int i = 1; i <= frames; i++)
        {
            var progresso = (double)i / frames;
            var progressoEased = 1 - Math.Pow(1 - progresso, 3);
            var valorAtual = valorExibir * progressoEased;
            var sinal = valorAtual >= 0 ? "+" : "";

            LblDiferenca.Text = $"{sinal}{valorAtual:F0}%";
            await Task.Delay(delayMs);
        }

        var sinalFinal = valorExibir >= 0 ? "+" : "";
        LblDiferenca.Text = $"{sinalFinal}{valorExibir:F0}%";
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CASCATA DOS CARDS DE CARNE
    // ─────────────────────────────────────────────────────────────────────────

    private async Task AnimarCardsCarneAsync()
    {
        await Task.Delay(50);

        var cards = ListaCarnes.Children
            .OfType<VisualElement>()
            .ToList();

        if (cards.Count == 0) return;

        foreach (var card in cards)
        {
            card.Opacity = 0;
            card.TranslationY = 30;
        }

        foreach (var card in cards)
        {
            _ = Task.WhenAll(
                card.FadeToAsync(1, 400, Easing.CubicOut),
                card.TranslateToAsync(0, 0, 400, Easing.CubicOut)
            );
            await Task.Delay(80);
        }

        await Task.Delay(400);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  BOTÃO LIMPAR
    // ─────────────────────────────────────────────────────────────────────────

    private async void OnLimparTapped(object? sender, TappedEventArgs e)
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HAPTIC] {ex.Message}");
        }

        await Task.WhenAll(
            BtnLimpar.RotateToAsync(360, 320, Easing.CubicOut),
            AnimarFabSaidaAsync()
        );
        BtnLimpar.Rotation = 0;

        if (PainelResultados.IsVisible)
        {
            await PainelResultados.FadeToAsync(0, 200, Easing.CubicOut);
            PainelResultados.IsVisible = false;
            PainelResultados.TranslationY = 40;
        }

        // Limpa o ViewModel (zera todos os campos incluindo os de carne)
        if (BindingContext is PlsViewModel vm && vm.LimparCommand.CanExecute(null))
            vm.LimparCommand.Execute(null);

        // Limpa os Entries de venda manualmente (evita reentrância da máscara)
        _atualizandoEstimada = true;
        _atualizandoReal = true;
        EntryEstimada.Text = "";
        EntryReal.Text = "";
        _atualizandoEstimada = false;
        _atualizandoReal = false;

        // Limpa os Entries de carne (binding bidirecional já cuida,
        // mas limpamos o Text para garantir que o teclado não mantenha cache)
        EntryHbRecipientes.Text = "";
        EntryHbUnidades.Text = "";
        EntryWhopperRecipientes.Text = "";
        EntryWhopperUnidades.Text = "";
        EntryRebelRecipientes.Text = "";
        EntryRebelUnidades.Text = "";

        await MainScroll.ScrollToAsync(0, 0, animated: true);
        EntryEstimada.Focus();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PAINEL EXPANSÍVEL DOS CARDS DE CARNE
    // ─────────────────────────────────────────────────────────────────────────

    private async void OnCardCarneTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not VerticalStackLayout stackInterno) return;

        VerticalStackLayout? painelDetalhes = null;
        Label? chevron = null;

        foreach (var filho in stackInterno.Children)
        {
            // Localiza o chevron dentro do Grid de cabeçalho
            if (filho is Grid grid && chevron is null)
            {
                foreach (var itemGrid in grid.Children)
                {
                    if (itemGrid is Label lbl && lbl.FontSize == 32)
                    {
                        chevron = lbl;
                        break;
                    }
                }
            }

            // Painel expansível: último VerticalStackLayout do stack
            if (filho is VerticalStackLayout vsl)
                painelDetalhes = vsl;
        }

        if (painelDetalhes is null) return;

        bool abrindo = !painelDetalhes.IsVisible;

        if (abrindo)
        {
            painelDetalhes.Opacity = 0;
            painelDetalhes.IsVisible = true;
            await painelDetalhes.FadeToAsync(1, 220, Easing.CubicOut);

            if (chevron is not null)
            {
                chevron.Text = "∨";
                chevron.TextColor = Color.FromArgb("#0008FF");
            }
        }
        else
        {
            await painelDetalhes.FadeToAsync(0, 160, Easing.CubicOut);
            painelDetalhes.IsVisible = false;

            if (chevron is not null)
            {
                chevron.Text = "›";
                chevron.TextColor = Color.FromArgb("#CCCCDD");
            }
        }
    }
}
