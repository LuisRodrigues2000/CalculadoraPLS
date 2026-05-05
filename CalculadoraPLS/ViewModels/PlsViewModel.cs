using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CalculadoraPLS.Models;

namespace CalculadoraPLS.ViewModels
{
    /// <summary>
    /// ViewModel principal da calculadora PLS.
    /// Implementa INotifyPropertyChanged para atualizar a tela automaticamente.
    /// </summary>
    public class PlsViewModel : INotifyPropertyChanged
    {
        // ─── Cultura pt-BR para parsing ──────────────────────────────────────
        private static readonly CultureInfo _ptBR = new("pt-BR");

        // ─── Campos privados — Vendas ────────────────────────────────────────
        private string _vendaEstimada = string.Empty;
        private string _vendaReal = string.Empty;
        private string _vendaEstimadaFormatada = string.Empty;
        private string _vendaRealFormatada = string.Empty;

        // ─── Campos privados — Inputs por carne ─────────────────────────────
        private string _hbRecipientes = string.Empty;
        private string _hbUnidades = string.Empty;
        private string _whopperRecipientes = string.Empty;
        private string _whopperUnidades = string.Empty;
        private string _rebelRecipientes = string.Empty;
        private string _rebelUnidades = string.Empty;

        // ─── Campos privados — Inputs Especiais ─────────────────────────────
        private string _bkChickenUnidades = string.Empty;
        private string _bkChickenRecipientes = string.Empty;
        private string _chickenJrUnidades = string.Empty;
        private string _chickenJrRecipientes = string.Empty;
        private string _tenderCrispUnidades = string.Empty;
        private string _tenderCrispRecipientes = string.Empty;

        // ─── Campos privados — Resultado ────────────────────────────────────
        private double _diferencaReal;
        private bool _resultadoVisivel;
        private string _corDiferenca = "#3A3A6A";
        private string _corDiferencaFundo = "#F5F5FA";
        private string _labelTendencia = "● IGUAL";
        private string _diferencaFormatada = "0,00%";

        // ─── Campo privado — Loading ─────────────────────────────────────────
        private bool _isLoading;

        // ════════════════════════════════════════════════════════════════════
        //  PROPRIEDADES — VENDAS
        // ════════════════════════════════════════════════════════════════════

        public string VendaEstimada
        {
            get => _vendaEstimada;
            set { _vendaEstimada = value; OnPropertyChanged(); AtualizarPodeCalcular(); }
        }

        public string VendaReal
        {
            get => _vendaReal;
            set { _vendaReal = value; OnPropertyChanged(); AtualizarPodeCalcular(); }
        }

        public string VendaEstimadaFormatada
        {
            get => _vendaEstimadaFormatada;
            set { _vendaEstimadaFormatada = value; OnPropertyChanged(); }
        }

        public string VendaRealFormatada
        {
            get => _vendaRealFormatada;
            set { _vendaRealFormatada = value; OnPropertyChanged(); }
        }

        // ════════════════════════════════════════════════════════════════════
        //  PROPRIEDADES — INPUTS CARNES
        // ════════════════════════════════════════════════════════════════════

        public string HbRecipientes
        {
            get => _hbRecipientes;
            set { _hbRecipientes = value; OnPropertyChanged(); AtualizarPodeCalcular(); }
        }

        public string HbUnidades
        {
            get => _hbUnidades;
            set { _hbUnidades = value; OnPropertyChanged(); AtualizarPodeCalcular(); }
        }

        public string WhopperRecipientes
        {
            get => _whopperRecipientes;
            set { _whopperRecipientes = value; OnPropertyChanged(); AtualizarPodeCalcular(); }
        }

        public string WhopperUnidades
        {
            get => _whopperUnidades;
            set { _whopperUnidades = value; OnPropertyChanged(); AtualizarPodeCalcular(); }
        }

        public string RebelRecipientes
        {
            get => _rebelRecipientes;
            set { _rebelRecipientes = value; OnPropertyChanged(); AtualizarPodeCalcular(); }
        }

        public string RebelUnidades
        {
            get => _rebelUnidades;
            set { _rebelUnidades = value; OnPropertyChanged(); AtualizarPodeCalcular(); }
        }

        // ════════════════════════════════════════════════════════════════════
        //  PROPRIEDADES — INPUTS ESPECIAIS
        // ════════════════════════════════════════════════════════════════════

        public string BkChickenUnidades
        {
            get => _bkChickenUnidades;
            set { _bkChickenUnidades = value; OnPropertyChanged(); AtualizarPodeCalcular(); }
        }

        public string BkChickenRecipientes
        {
            get => _bkChickenRecipientes;
            set { _bkChickenRecipientes = value; OnPropertyChanged(); AtualizarPodeCalcular(); }
        }

        public string ChickenJrUnidades
        {
            get => _chickenJrUnidades;
            set { _chickenJrUnidades = value; OnPropertyChanged(); AtualizarPodeCalcular(); }
        }

        public string ChickenJrRecipientes
        {
            get => _chickenJrRecipientes;
            set { _chickenJrRecipientes = value; OnPropertyChanged(); AtualizarPodeCalcular(); }
        }

        public string TenderCrispUnidades
        {
            get => _tenderCrispUnidades;
            set { _tenderCrispUnidades = value; OnPropertyChanged(); AtualizarPodeCalcular(); }
        }

        public string TenderCrispRecipientes
        {
            get => _tenderCrispRecipientes;
            set { _tenderCrispRecipientes = value; OnPropertyChanged(); AtualizarPodeCalcular(); }
        }

        // ════════════════════════════════════════════════════════════════════
        //  PROPRIEDADES — RESULTADO
        // ════════════════════════════════════════════════════════════════════

        public double DiferencaReal
        {
            get => _diferencaReal;
            private set { _diferencaReal = value; OnPropertyChanged(); AtualizarCorELabel(); }
        }

        public string DiferencaFormatada
        {
            get => _diferencaFormatada;
            private set { _diferencaFormatada = value; OnPropertyChanged(); }
        }

        public string CorDiferenca
        {
            get => _corDiferenca;
            private set { _corDiferenca = value; OnPropertyChanged(); }
        }

        public string CorDiferencaFundo
        {
            get => _corDiferencaFundo;
            private set { _corDiferencaFundo = value; OnPropertyChanged(); }
        }

        public string LabelTendencia
        {
            get => _labelTendencia;
            private set { _labelTendencia = value; OnPropertyChanged(); }
        }

        public bool ResultadoVisivel
        {
            get => _resultadoVisivel;
            set { _resultadoVisivel = value; OnPropertyChanged(); }
        }

        /// <summary>Controla a visibilidade do indicador de carregamento no botão.</summary>
        public bool IsLoading
        {
            get => _isLoading;
            private set { _isLoading = value; OnPropertyChanged(); }
        }

        /// <summary>Resultados das carnes (HB, Whopper, Rebel)</summary>
        public ObservableCollection<ResultadoCarne> Resultados { get; } = new();

        /// <summary>Resultados dos especiais (BK Chicken, Chicken Jr.)</summary>
        public ObservableCollection<ResultadoCarne> ResultadosEspeciais { get; } = new();

        // ════════════════════════════════════════════════════════════════════
        //  COMANDOS
        // ════════════════════════════════════════════════════════════════════

        public ICommand CalcularCommand { get; }
        public ICommand LimparCommand { get; }

        // ════════════════════════════════════════════════════════════════════
        //  CONSTRUTOR
        // ════════════════════════════════════════════════════════════════════

        public PlsViewModel()
        {
            // ✅ Wrapper assíncrono — permite usar async/await no Command
            CalcularCommand = new Command(async () => await CalcularAsync(), PodeCalcular);
            LimparCommand = new Command(Limpar);
        }

        // ════════════════════════════════════════════════════════════════════
        //  LÓGICA PRINCIPAL
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Habilita o botão Calcular somente se:
        /// - Venda Estimada > 0
        /// - Venda Real preenchida
        /// - Pelo menos uma carne OU especial com recipientes ou unidades > 0
        /// </summary>
        private bool PodeCalcular()
        {
            // Bloqueia o botão também enquanto o loading estiver ativo
            if (_isLoading) return false;

            bool vendasOk = TryParseValor(VendaEstimada, out double est) &&
                            TryParseValor(VendaReal, out _) &&
                            est > 0;

            bool algumaCarne =
                ParseIntSafe(HbRecipientes) > 0 || ParseIntSafe(HbUnidades) > 0 ||
                ParseIntSafe(WhopperRecipientes) > 0 || ParseIntSafe(WhopperUnidades) > 0 ||
                ParseIntSafe(RebelRecipientes) > 0 || ParseIntSafe(RebelUnidades) > 0 ||
                ParseIntSafe(BkChickenUnidades) > 0 || ParseIntSafe(BkChickenRecipientes) > 0 ||
                ParseIntSafe(ChickenJrUnidades) > 0 || ParseIntSafe(ChickenJrRecipientes) > 0 ||
                ParseIntSafe(TenderCrispUnidades) > 0 || ParseIntSafe(TenderCrispRecipientes) > 0;

            return vendasOk && algumaCarne;
        }

        /// <summary>Força reavaliação do CanExecute do botão Calcular.</summary>
        private void AtualizarPodeCalcular()
            => ((Command)CalcularCommand).ChangeCanExecute();

        /// <summary>
        /// Substitui o Calcular() síncrono.
        /// Roda o processamento em background para não travar a UI,
        /// enquanto exibe o indicador de loading no botão.
        /// </summary>
        private async Task CalcularAsync()
        {
            if (!TryParseValor(VendaEstimada, out double estimada) ||
                !TryParseValor(VendaReal, out double real))
                return;

            // ── Ativa o loading e desabilita o botão ────────────────────────
            IsLoading = true;
            AtualizarPodeCalcular();

            // ── Variáveis que receberão os resultados do background ──────────
            double diferenca = 0;
            ResultadoCarne? hbResult = null;
            ResultadoCarne? wpResult = null;
            ResultadoCarne? rebelResult = null;
            ResultadoCarne? bkResult = null;
            ResultadoCarne? cjResult = null;
            ResultadoCarne? tcResult = null;

            // ── Cálculo em background (não trava a UI thread) ────────────────
            await Task.Run(() =>
            {
                diferenca = PlsModel.CalcularDiferencaPercentual(estimada, real);

                // Lê inputs — Carnes
                int hbRec = ParseIntSafe(HbRecipientes);
                int hbUn = ParseIntSafe(HbUnidades);
                int wpRec = ParseIntSafe(WhopperRecipientes);
                int wpUn = ParseIntSafe(WhopperUnidades);
                int rebelRec = ParseIntSafe(RebelRecipientes);
                int rebelUn = ParseIntSafe(RebelUnidades);

                // Lê inputs — Especiais
                int bkRec = ParseIntSafe(BkChickenRecipientes);
                int bkUn = ParseIntSafe(BkChickenUnidades);
                int cjRec = ParseIntSafe(ChickenJrRecipientes);
                int cjUn = ParseIntSafe(ChickenJrUnidades);
                int tcRec = ParseIntSafe(TenderCrispRecipientes);
                int tcUn = ParseIntSafe(TenderCrispUnidades);

                // Calcula — Carnes
                hbResult = PlsModel.CalcularCarne("HB", PlsModel.CapacidadeHB, hbRec, hbUn, diferenca);
                wpResult = PlsModel.CalcularCarne("Whopper", PlsModel.CapacidadeWhopper, wpRec, wpUn, diferenca);
                rebelResult = PlsModel.CalcularCarne("Rebel", PlsModel.CapacidadeRebel, rebelRec, rebelUn, diferenca);

                // Calcula — Especiais
                bkResult = PlsModel.CalcularCarne("BK Chicken", PlsModel.CapacidadeBkChicken, bkRec, bkUn, diferenca);
                cjResult = PlsModel.CalcularCarne("Chicken Jr.", PlsModel.CapacidadeChickenJr, cjRec, cjUn, diferenca);
                tcResult = PlsModel.CalcularCarne("Tender Crisp", PlsModel.CapacidadeTenderCrisp, tcRec, tcUn, diferenca);
            });

            // ── Atualiza a UI de volta na main thread ────────────────────────
            DiferencaReal = diferenca;

            Resultados.Clear();
            Resultados.Add(hbResult!);
            Resultados.Add(wpResult!);
            Resultados.Add(rebelResult!);

            ResultadosEspeciais.Clear();
            ResultadosEspeciais.Add(bkResult!);
            ResultadosEspeciais.Add(cjResult!);
            ResultadosEspeciais.Add(tcResult!);

            ResultadoVisivel = true;

            // ── Desativa o loading e reabilita o botão ───────────────────────
            IsLoading = false;
            AtualizarPodeCalcular();
        }

        /// <summary>Reseta todos os campos para o estado inicial.</summary>
        private void Limpar()
        {
            // Vendas
            VendaEstimada = string.Empty;
            VendaReal = string.Empty;
            VendaEstimadaFormatada = string.Empty;
            VendaRealFormatada = string.Empty;

            // Carnes
            HbRecipientes = string.Empty;
            HbUnidades = string.Empty;
            WhopperRecipientes = string.Empty;
            WhopperUnidades = string.Empty;
            RebelRecipientes = string.Empty;
            RebelUnidades = string.Empty;

            // Especiais
            BkChickenUnidades = string.Empty;
            BkChickenRecipientes = string.Empty;
            ChickenJrUnidades = string.Empty;
            TenderCrispUnidades = string.Empty;
            TenderCrispRecipientes = string.Empty;
            ChickenJrRecipientes = string.Empty;

            // Resultado
            DiferencaReal = 0;
            ResultadoVisivel = false;
            Resultados.Clear();
            ResultadosEspeciais.Clear();

            // ✅ Garante reset do loading caso tenha ficado preso
            IsLoading = false;
            AtualizarPodeCalcular();
        }

        /// <summary>
        /// Atualiza cor, fundo e label de tendência conforme o sinal da diferença.
        /// </summary>
        private void AtualizarCorELabel()
        {
            double valorExibir = Math.Round(_diferencaReal, MidpointRounding.AwayFromZero);
            var sinal = valorExibir >= 0 ? "+" : "";
            DiferencaFormatada = $"{sinal}{valorExibir:F0}%";

            if (_diferencaReal > 0)
            {
                CorDiferenca = "#0008FF";
                CorDiferencaFundo = "#EEF0FF";
                LabelTendencia = "▲ ACIMA";
            }
            else if (_diferencaReal < 0)
            {
                CorDiferenca = "#D32F2F";
                CorDiferencaFundo = "#FFEEEE";
                LabelTendencia = "▼ ABAIXO";
            }
            else
            {
                CorDiferenca = "#AAAACC";
                CorDiferencaFundo = "#F5F5FA";
                LabelTendencia = "● IGUAL";
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  HELPERS
        // ════════════════════════════════════════════════════════════════════

        /// <summary>Parse de decimal aceitando ponto (invariant) ou vírgula (pt-BR).</summary>
        private static bool TryParseValor(string texto, out double valor)
        {
            if (string.IsNullOrWhiteSpace(texto)) { valor = 0; return false; }

            if (double.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out valor))
                return true;

            if (double.TryParse(texto, NumberStyles.Any, _ptBR, out valor))
                return true;

            valor = 0;
            return false;
        }

        /// <summary>Parse seguro de inteiro. Retorna 0 se vazio ou inválido.</summary>
        private static int ParseIntSafe(string texto)
            => int.TryParse(texto?.Trim(), out int v) ? Math.Max(0, v) : 0;

        // ════════════════════════════════════════════════════════════════════
        //  INotifyPropertyChanged
        // ════════════════════════════════════════════════════════════════════

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
