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
    //  PONTO 1 — MÁSCARA MONETÁRIA
    // ─────────────────────────────────────────────────────────────────────────

    private void EntryEstimada_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_atualizandoEstimada) return;
        _atualizandoEstimada = true;

        try
        {
            var texto = e.NewTextValue ?? "";
            System.Diagnostics.Debug.WriteLine($"[ESTIMADA] TextChanged: '{texto}'");

            if (BindingContext is PlsViewModel vm)
                vm.VendaEstimada = texto;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ESTIMADA] ERRO: {ex}");
        }
        finally
        {
            _atualizandoEstimada = false;
        }
    }

    private void EntryReal_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_atualizandoReal) return;
        _atualizandoReal = true;

        try
        {
            var texto = e.NewTextValue ?? "";
            System.Diagnostics.Debug.WriteLine($"[REAL] TextChanged: '{texto}'");

            if (BindingContext is PlsViewModel vm)
                vm.VendaReal = texto;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[REAL] ERRO: {ex}");
        }
        finally
        {
            _atualizandoReal = false;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  HELPERS DE FORMATAÇÃO MONETÁRIA
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Converte dígitos crus em R$ 0,00.</summary>
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

            string centavosStr = centavos.ToString("D2");
            string reaisStr = FormatarMilhar(reais.ToString());

            return $"R$ {reaisStr},{centavosStr}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FORMAT_MOEDA] Erro: {ex.Message}");
            return "";
        }
    }

    /// <summary>Insere pontos de milhar. Ex: "1234567" → "1.234.567"</summary>
    private static string FormatarMilhar(string inteiro)
    {
        if (string.IsNullOrEmpty(inteiro)) return "0";

        var resultado = "";
        var contador = 0;
        var tamanho = inteiro.Length;

        for (int i = tamanho - 1; i >= 0; i--)
        {
            if (contador > 0 && contador % 3 == 0)
                resultado = "." + resultado;

            resultado = inteiro[i] + resultado;
            contador++;
        }

        return resultado;
    }

    /// <summary>Extrai somente dígitos de uma string.</summary>
    private static string ExtrairDigitos(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return "";

        var sb = new System.Text.StringBuilder();
        foreach (char c in texto)
        {
            if (char.IsDigit(c))
                sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>Converte dígitos crus para decimal. Ex: "36917" → 369.17</summary>
    private static decimal DigitosParaDecimal(string digitosCrus)
    {
        if (string.IsNullOrEmpty(digitosCrus)) return 0m;
        if (!long.TryParse(digitosCrus, out long centavos)) return 0m;
        return centavos / 100m;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PONTO 2 — LINHA DE FOCO VERDE
    // ─────────────────────────────────────────────────────────────────────────

    private void EntryEstimada_Focused(object? sender, FocusEventArgs e)
        => FocusLineEstimada.BackgroundColor = Color.FromArgb("#0008FF"); // era #4CAF50

    private void EntryEstimada_Unfocused(object? sender, FocusEventArgs e)
        => FocusLineEstimada.BackgroundColor = Color.FromArgb("#E8E8F0"); // era #252525

    private void EntryReal_Focused(object? sender, FocusEventArgs e)
        => FocusLineReal.BackgroundColor = Color.FromArgb("#0008FF");     // era #4CAF50

    private void EntryReal_Unfocused(object? sender, FocusEventArgs e)
        => FocusLineReal.BackgroundColor = Color.FromArgb("#E8E8F0");     // era #252525

    // ─────────────────────────────────────────────────────────────────────────
    //  PONTO 3 — ÁREA DE TOQUE DOS CARDS DE VENDA
    // ─────────────────────────────────────────────────────────────────────────

    private void OnCardEstimadaTapped(object? sender, TappedEventArgs e)
        => EntryEstimada.Focus();

    private void OnCardRealTapped(object? sender, TappedEventArgs e)
        => EntryReal.Focus();

    // ─────────────────────────────────────────────────────────────────────────
    //  NAVEGAÇÃO ENTRE CAMPOS
    // ─────────────────────────────────────────────────────────────────────────

    private void EntryEstimada_Completed(object? sender, EventArgs e)
        => EntryReal.Focus();

    private void EntryReal_Completed(object? sender, EventArgs e)
    {
        EntryReal.Unfocus();

        if (BindingContext is PlsViewModel vm && vm.CalcularCommand.CanExecute(null))
            vm.CalcularCommand.Execute(null);

        _ = AnimarResultadosAsync();
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
    //  PONTO 7 — ANIMAÇÃO DE ENTRADA DOS RESULTADOS + FAB
    // ─────────────────────────────────────────────────────────────────────────

    private async Task AnimarResultadosAsync()
    {
        if (BindingContext is not PlsViewModel vm) return;
        if (!vm.ResultadoVisivel) return;

        // ── Estado inicial dos resultados ─────────────────────────────────
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

        // ── FAB aparece após os resultados entrarem ───────────────────────
        await AnimarFabEntradaAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  FAB — ANIMAÇÃO DE ENTRADA (spring suave vindo de baixo)
    // ─────────────────────────────────────────────────────────────────────────

    private async Task AnimarFabEntradaAsync()
    {
        // Garante estado inicial limpo antes de exibir
        BtnLimparContainer.Opacity = 0;
        BtnLimparContainer.TranslationY = 40;
        BtnLimparContainer.Scale = 0.7;
        BtnLimparContainer.IsVisible = true;

        // Sobe e aparece com efeito spring
        await Task.WhenAll(
            BtnLimparContainer.FadeToAsync(1, 350, Easing.CubicOut),
            BtnLimparContainer.TranslateToAsync(0, 0, 400, Easing.SpringOut),
            BtnLimparContainer.ScaleToAsync(1, 400, Easing.SpringOut)
        );
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  FAB — ANIMAÇÃO DE SAÍDA (desce e desaparece)
    // ─────────────────────────────────────────────────────────────────────────

    private async Task AnimarFabSaidaAsync()
    {
        await Task.WhenAll(
            BtnLimparContainer.FadeToAsync(0, 220, Easing.CubicIn),
            BtnLimparContainer.TranslateToAsync(0, 40, 220, Easing.CubicIn),
            BtnLimparContainer.ScaleToAsync(0.7, 220, Easing.CubicIn)
        );

        BtnLimparContainer.IsVisible = false;

        // Reseta para próxima entrada
        BtnLimparContainer.TranslationY = 40;
        BtnLimparContainer.Scale = 0.7;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PONTO 8 — CONTADOR ANIMADO
    // ─────────────────────────────────────────────────────────────────────────

    private async Task AnimarContadorAsync(double valorFinal)
    {
        const int duracaoMs = 800;
        const int frames = 40;
        const int delayMs = duracaoMs / frames;

        for (int i = 1; i <= frames; i++)
        {
            var progresso = (double)i / frames;
            var progressoEased = 1 - Math.Pow(1 - progresso, 3);
            var valorAtual = valorFinal * progressoEased;
            var sinal = valorAtual >= 0 ? "+" : "";

            LblDiferenca.Text = $"{sinal}{valorAtual:F2}%";
            await Task.Delay(delayMs);
        }

        var sinalFinal = valorFinal >= 0 ? "+" : "";
        LblDiferenca.Text = $"{sinalFinal}{valorFinal:F2}%";
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

        if (cards.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine("[ANIM] Nenhum card encontrado em ListaCarnes");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[ANIM] {cards.Count} cards encontrados");

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

        // Gira o ícone e esconde o FAB ao mesmo tempo
        await Task.WhenAll(
            BtnLimpar.RotateToAsync(360, 320, Easing.CubicOut),
            AnimarFabSaidaAsync()
        );
        BtnLimpar.Rotation = 0;

        // Esconde os resultados
        if (PainelResultados.IsVisible)
        {
            await PainelResultados.FadeToAsync(0, 200, Easing.CubicOut);
            PainelResultados.IsVisible = false;
            PainelResultados.TranslationY = 40;
        }

        // Limpa o ViewModel
        if (BindingContext is PlsViewModel vm && vm.LimparCommand.CanExecute(null))
            vm.LimparCommand.Execute(null);

        // Limpa os campos de texto
        _atualizandoEstimada = true;
        _atualizandoReal = true;
        EntryEstimada.Text = "";
        EntryReal.Text = "";
        _atualizandoEstimada = false;
        _atualizandoReal = false;

        await MainScroll.ScrollToAsync(0, 0, animated: true);
        EntryEstimada.Focus();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PAINEL EXPANSÍVEL — CHEVRON
    // ─────────────────────────────────────────────────────────────────────────

    private async void OnCardCarneTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not VerticalStackLayout stackInterno) return;

        VerticalStackLayout? painelDetalhes = null;
        Label? chevron = null;

        foreach (var filho in stackInterno.Children)
        {
            // Chevron: Label FontSize=36 dentro do Grid de cabeçalho
            if (filho is Grid grid && chevron is null)
            {
                foreach (var itemGrid in grid.Children)
                {
                    if (itemGrid is Label lbl && lbl.FontSize == 36)
                    {
                        chevron = lbl;
                        break;
                    }
                }
            }

            // Painel expansível: último VerticalStackLayout filho do stack raiz
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
                chevron.TextColor = Color.FromArgb("#4CAF50");
            }
        }
        else
        {
            await painelDetalhes.FadeToAsync(0, 160, Easing.CubicOut);
            painelDetalhes.IsVisible = false;

            if (chevron is not null)
            {
                chevron.Text = "›";
                chevron.TextColor = Color.FromArgb("#444444");
            }
        }
    }
}
