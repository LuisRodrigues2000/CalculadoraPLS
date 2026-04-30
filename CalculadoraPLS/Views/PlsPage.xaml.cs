using System.ComponentModel;
using CalculadoraPLS.ViewModels;

namespace CalculadoraPLS.Views;

public partial class PlsPage : ContentPage
{
    private bool _atualizandoEstimada = false;
    private bool _atualizandoReal = false;

    public PlsPage()
    {
        InitializeComponent();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  FORMATAÇÃO DE MOEDA
    // ─────────────────────────────────────────────────────────────────────────

    private void EntryEstimada_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_atualizandoEstimada) return;
        _atualizandoEstimada = true;
        try
        {
            if (BindingContext is PlsViewModel vm)
                vm.VendaEstimada = e.NewTextValue ?? "";
        }
        finally { _atualizandoEstimada = false; }
    }

    private void EntryReal_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_atualizandoReal) return;
        _atualizandoReal = true;
        try
        {
            if (BindingContext is PlsViewModel vm)
                vm.VendaReal = e.NewTextValue ?? "";
        }
        finally { _atualizandoReal = false; }
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
    //  FOCO — LINHAS ANIMADAS
    // ─────────────────────────────────────────────────────────────────────────

    private void EntryEstimada_Focused(object? sender, FocusEventArgs e)
        => FocusLineEstimada.BackgroundColor = Color.FromArgb("#0008FF");

    private void EntryEstimada_Unfocused(object? sender, FocusEventArgs e)
        => FocusLineEstimada.BackgroundColor = Color.FromArgb("#E8E8F0");

    private void EntryReal_Focused(object? sender, FocusEventArgs e)
        => FocusLineReal.BackgroundColor = Color.FromArgb("#0008FF");

    private void EntryReal_Unfocused(object? sender, FocusEventArgs e)
        => FocusLineReal.BackgroundColor = Color.FromArgb("#E8E8F0");

    private void OnEntryInputFocused(object? sender, FocusEventArgs e)
        => DefinirLinhaFocoCarne(sender, "#0008FF");

    private void OnEntryInputUnfocused(object? sender, FocusEventArgs e)
        => DefinirLinhaFocoCarne(sender, "#E8E8F0");

    private static void DefinirLinhaFocoCarne(object? sender, string hexCor)
    {
        if (sender is not Entry entry) return;
        if (entry.Parent is Grid grid)
        {
            var linha = grid.Children
                .OfType<BoxView>()
                .FirstOrDefault(b => Grid.GetRow(b) == 1);
            if (linha is not null)
                linha.BackgroundColor = Color.FromArgb(hexCor);
        }
    }

    private void OnCardEstimadaTapped(object? sender, TappedEventArgs e)
        => EntryEstimada.Focus();

    private void OnCardRealTapped(object? sender, TappedEventArgs e)
        => EntryReal.Focus();

    // ─────────────────────────────────────────────────────────────────────────
    //  NAVEGAÇÃO — VENDAS
    // ─────────────────────────────────────────────────────────────────────────

    private void EntryEstimada_Completed(object? sender, EventArgs e)
        => EntryReal.Focus();

    private void EntryReal_Completed(object? sender, EventArgs e)
        => EntryHbUnidades.Focus();

    // ─────────────────────────────────────────────────────────────────────────
    //  NAVEGAÇÃO — CARNES
    // ─────────────────────────────────────────────────────────────────────────

    private void OnEntryHbUnidadesCompleted(object? sender, EventArgs e)
        => EntryHbRecipientes.Focus();

    private void OnEntryHbRecipientesCompleted(object? sender, EventArgs e)
        => EntryWhopperUnidades.Focus();

    private void OnEntryWhopperUnidadesCompleted(object? sender, EventArgs e)
        => EntryWhopperRecipientes.Focus();

    private void OnEntryWhopperRecipientesCompleted(object? sender, EventArgs e)
        => EntryRebelUnidades.Focus();

    private void OnEntryRebelUnidadesCompleted(object? sender, EventArgs e)
        => EntryRebelRecipientes.Focus();

    private void OnEntryRebelRecipientesCompleted(object? sender, EventArgs e)
        => EntryBkChickenUnidades.Focus();

    // ─────────────────────────────────────────────────────────────────────────
    //  NAVEGAÇÃO — ESPECIAIS
    // ─────────────────────────────────────────────────────────────────────────

    private void OnEntryBkChickenUnidadesCompleted(object? sender, EventArgs e)
        => EntryBkChickenRecipientes.Focus();

    private void OnEntryBkChickenRecipientesCompleted(object? sender, EventArgs e)
        => EntryChickenJrUnidades.Focus();

    private void OnEntryChickenJrUnidadesCompleted(object? sender, EventArgs e)
        => EntryChickenJrRecipientes.Focus();

    private async void OnEntryChickenJrRecipientesCompleted(object? sender, EventArgs e)
    {
        EntryChickenJrRecipientes.Unfocus();

        if (BindingContext is PlsViewModel vm && vm.CalcularCommand.CanExecute(null))
        {
            vm.CalcularCommand.Execute(null);
            await EsperarCalculoTerminarAsync(vm);
            await AnimarResultadosAsync();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  BOTÃO CALCULAR
    //  ✅ É o único responsável por executar o cálculo — sem Command no XAML
    // ─────────────────────────────────────────────────────────────────────────

    private async void OnCalcularTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not PlsViewModel vm) return;
        if (!vm.CalcularCommand.CanExecute(null)) return;

        // Feedback tátil
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[HAPTIC] {ex.Message}"); }

        // Animação de pressão do botão
        await BtnCalcular.ScaleToAsync(0.95, 70, Easing.CubicOut);
        await BtnCalcular.ScaleToAsync(1.00, 70, Easing.CubicIn);

        // Executa o cálculo via ViewModel
        vm.CalcularCommand.Execute(null);

        // Aguarda o cálculo assíncrono terminar
        await EsperarCalculoTerminarAsync(vm);

        if (vm.ResultadoVisivel)
            await AnimarResultadosAsync();
    }

    private static async Task EsperarCalculoTerminarAsync(PlsViewModel vm)
    {
        await Task.Delay(50);

        const int timeoutMs = 5000;
        const int intervaloMs = 50;
        int esperado = 0;

        while (vm.IsLoading && esperado < timeoutMs)
        {
            await Task.Delay(intervaloMs);
            esperado += intervaloMs;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ANIMAÇÃO DE ENTRADA DOS RESULTADOS
    // ─────────────────────────────────────────────────────────────────────────

    private async Task AnimarResultadosAsync()
    {
        if (BindingContext is not PlsViewModel vm) return;
        if (!vm.ResultadoVisivel) return;

        // 1. Esconde tudo imediatamente
        PainelResultados.IsVisible = false;
        PainelResultados.Opacity = 0;
        PainelResultados.TranslationY = 40;
        CardDiferenca.Opacity = 0;
        CardDiferenca.TranslationY = 30;

        // 2. Torna o painel visível para o BindableLayout renderizar os filhos
        PainelResultados.IsVisible = true;

        // 3. Aguarda o BindableLayout terminar de criar os cards (polling por contagem)
        await AguardarCardsAsync(ListaCarnes, vm.Resultados.Count);
        await AguardarCardsAsync(ListaEspeciais, vm.ResultadosEspeciais.Count);

        // 4. Agora os cards existem — reseta opacidade e posição para animação
        foreach (var card in ListaCarnes.Children.OfType<VisualElement>())
        {
            card.Opacity = 0;
            card.TranslationY = 30;
        }
        foreach (var card in ListaEspeciais.Children.OfType<VisualElement>())
        {
            card.Opacity = 0;
            card.TranslationY = 30;
        }

        // 5. Um frame extra para o reset ser aplicado antes de animar
        await Task.Delay(32);

        // 6. Anima o painel principal
        await PainelResultados.FadeToAsync(1, 300, Easing.CubicOut);
        PainelResultados.TranslationY = 0;

        // 7. Anima o card de diferença
        await Task.WhenAll(
            CardDiferenca.FadeToAsync(1, 350, Easing.CubicOut),
            CardDiferenca.TranslateToAsync(0, 0, 350, Easing.CubicOut)
        );

        // 8. Anima contador e cards em paralelo
        await Task.WhenAll(
            AnimarContadorAsync(vm.DiferencaReal),
            AnimarCardsCarneAsync(),
            AnimarCardsEspeciaisAsync()
        );

        await Task.Delay(100);
        await MainScroll.ScrollToAsync(ScrollAnchor, ScrollToPosition.Start, animated: true);

        await AnimarFabEntradaAsync();
    }

    /// <summary>
    /// Polling que aguarda o BindableLayout criar a quantidade esperada de cards.
    /// Timeout de segurança de 1 segundo.
    /// </summary>
    private static async Task AguardarCardsAsync(Layout lista, int quantidadeEsperada)
    {
        if (quantidadeEsperada == 0) return;

        const int timeoutMs = 1000;
        const int intervaloMs = 16; // ~1 frame a 60fps
        int aguardado = 0;

        while (lista.Children.Count < quantidadeEsperada && aguardado < timeoutMs)
        {
            await Task.Delay(intervaloMs);
            aguardado += intervaloMs;
        }
    }

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

    private async Task AnimarContadorAsync(double valorFinal)
    {
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

    private async Task AnimarCardsCarneAsync()
    {
        // ✅ Sem reset aqui — já garantido em AnimarResultadosAsync após AguardarCardsAsync
        var cards = ListaCarnes.Children.OfType<VisualElement>().ToList();
        if (cards.Count == 0) return;

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

    private async Task AnimarCardsEspeciaisAsync()
    {
        await Task.Delay(200);

        // ✅ Sem reset aqui — já garantido em AnimarResultadosAsync após AguardarCardsAsync
        var cards = ListaEspeciais.Children.OfType<VisualElement>().ToList();
        if (cards.Count == 0) return;

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
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[HAPTIC] {ex.Message}"); }

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

        if (BindingContext is PlsViewModel vm && vm.LimparCommand.CanExecute(null))
            vm.LimparCommand.Execute(null);

        _atualizandoEstimada = true;
        _atualizandoReal = true;
        EntryEstimada.Text = "";
        EntryReal.Text = "";
        _atualizandoEstimada = false;
        _atualizandoReal = false;

        EntryHbUnidades.Text = "";
        EntryHbRecipientes.Text = "";
        EntryWhopperUnidades.Text = "";
        EntryWhopperRecipientes.Text = "";
        EntryRebelUnidades.Text = "";
        EntryRebelRecipientes.Text = "";

        EntryBkChickenUnidades.Text = "";
        EntryBkChickenRecipientes.Text = "";
        EntryChickenJrUnidades.Text = "";
        EntryChickenJrRecipientes.Text = "";

        await MainScroll.ScrollToAsync(0, 0, animated: true);
        EntryEstimada.Focus();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PAINEL EXPANSÍVEL DOS CARDS DE CARNE / ESPECIAIS
    // ─────────────────────────────────────────────────────────────────────────

    private async void OnCardCarneTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not VerticalStackLayout stackInterno) return;

        VerticalStackLayout? painelDetalhes = null;
        Label? chevron = null;

        foreach (var filho in stackInterno.Children)
        {
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

    // ─────────────────────────────────────────────────────────────────────────
    //  CICLO DE VIDA
    // ─────────────────────────────────────────────────────────────────────────

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is PlsViewModel vm)
            vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (BindingContext is PlsViewModel vm)
            vm.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlsViewModel.IsLoading))
        {
            if (BindingContext is PlsViewModel vm && vm.IsLoading)
                AnimarProgressoAsync();
        }
    }

    private async void AnimarProgressoAsync()
    {
        BarraProgresso.WidthRequest = 0;

        double larguraTotal = BtnCalcular.Width;
        const uint duracao = 1000;
        const uint passos = 60;
        const uint intervalo = duracao / passos;

        for (int i = 1; i <= passos; i++)
        {
            if (BindingContext is PlsViewModel vm && !vm.IsLoading) break;

            double progresso = (double)i / passos;
            double easedProgresso = progresso < 0.5
                ? 2 * progresso * progresso
                : 1 - Math.Pow(-2 * progresso + 2, 2) / 2;

            BarraProgresso.WidthRequest = larguraTotal * easedProgresso;
            await Task.Delay((int)intervalo);
        }

        BarraProgresso.WidthRequest = larguraTotal;
        await Task.Delay(150);
        BarraProgresso.WidthRequest = 0;
    }
}
