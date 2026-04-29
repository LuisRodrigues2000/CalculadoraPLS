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
        // HB
        private string _hbRecipientes = string.Empty;
        private string _hbUnidades = string.Empty;
        // Whopper
        private string _whopperRecipientes = string.Empty;
        private string _whopperUnidades = string.Empty;
        // Rebel
        private string _rebelRecipientes = string.Empty;
        private string _rebelUnidades = string.Empty;

        // ─── Campos privados — Resultado ────────────────────────────────────
        private double _diferencaReal;
        private bool _resultadoVisivel;
        private string _corDiferenca = "#3A3A6A";
        private string _corDiferencaFundo = "#F5F5FA";
        private string _labelTendencia = "● IGUAL";
        private string _diferencaFormatada = "0,00%";

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
        //  PROPRIEDADES — INPUTS POR CARNE
        // ════════════════════════════════════════════════════════════════════

        /// <summary>Recipientes HB informados pelo usuário (texto do Entry)</summary>
        public string HbRecipientes
        {
            get => _hbRecipientes;
            set { _hbRecipientes = value; OnPropertyChanged(); AtualizarPodeCalcular(); }
        }

        /// <summary>Unidades avulsas HB informadas pelo usuário (texto do Entry)</summary>
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

        /// <summary>Lista de resultados por tipo de carne (alimenta o BindableLayout)</summary>
        public ObservableCollection<ResultadoCarne> Resultados { get; } = new();

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
            CalcularCommand = new Command(Calcular, PodeCalcular);
            LimparCommand = new Command(Limpar);
        }

        // ════════════════════════════════════════════════════════════════════
        //  LÓGICA PRINCIPAL
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Habilita o botão Calcular somente se:
        /// - Venda Estimada > 0
        /// - Venda Real preenchida
        /// - Pelo menos uma carne com recipientes ou unidades > 0
        /// </summary>
        private bool PodeCalcular()
        {
            bool vendasOk = TryParseValor(VendaEstimada, out double est) &&
                            TryParseValor(VendaReal, out _) &&
                            est > 0;

            // Verifica se pelo menos uma carne foi preenchida
            bool algumaCarne =
                ParseIntSafe(HbRecipientes) > 0 || ParseIntSafe(HbUnidades) > 0 ||
                ParseIntSafe(WhopperRecipientes) > 0 || ParseIntSafe(WhopperUnidades) > 0 ||
                ParseIntSafe(RebelRecipientes) > 0 || ParseIntSafe(RebelUnidades) > 0;

            return vendasOk && algumaCarne;
        }

        /// <summary>Força reavaliação do CanExecute do botão Calcular.</summary>
        private void AtualizarPodeCalcular()
            => ((Command)CalcularCommand).ChangeCanExecute();

        /// <summary>Executa o cálculo principal do PLS.</summary>
        private void Calcular()
        {
            if (!TryParseValor(VendaEstimada, out double estimada) ||
                !TryParseValor(VendaReal, out double real))
                return;

            // Diferença percentual
            DiferencaReal = PlsModel.CalcularDiferencaPercentual(estimada, real);

            // Lê os inputs de cada carne (0 se vazio)
            int hbRec = ParseIntSafe(HbRecipientes);
            int hbUn = ParseIntSafe(HbUnidades);
            int wpRec = ParseIntSafe(WhopperRecipientes);
            int wpUn = ParseIntSafe(WhopperUnidades);
            int rebelRec = ParseIntSafe(RebelRecipientes);
            int rebelUn = ParseIntSafe(RebelUnidades);

            // Reconstrói os cards de resultado
            Resultados.Clear();
            Resultados.Add(PlsModel.CalcularCarne("HB", PlsModel.CapacidadeHB, hbRec, hbUn, DiferencaReal));
            Resultados.Add(PlsModel.CalcularCarne("Whopper", PlsModel.CapacidadeWhopper, wpRec, wpUn, DiferencaReal));
            Resultados.Add(PlsModel.CalcularCarne("Rebel", PlsModel.CapacidadeRebel, rebelRec, rebelUn, DiferencaReal));

            ResultadoVisivel = true;
        }

        /// <summary>Reseta todos os campos para o estado inicial.</summary>
        private void Limpar()
        {
            VendaEstimada = string.Empty;
            VendaReal = string.Empty;
            VendaEstimadaFormatada = string.Empty;
            VendaRealFormatada = string.Empty;

            HbRecipientes = string.Empty;
            HbUnidades = string.Empty;
            WhopperRecipientes = string.Empty;
            WhopperUnidades = string.Empty;
            RebelRecipientes = string.Empty;
            RebelUnidades = string.Empty;

            DiferencaReal = 0;
            ResultadoVisivel = false;
            Resultados.Clear();
        }

        /// <summary>
        /// Atualiza cor, fundo e label de tendência conforme o sinal da diferença.
        /// </summary>
        private void AtualizarCorELabel()
        {
            // ✅ Exibe o percentual arredondado para inteiro (regra da apostila)
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

        /// <summary>
        /// Parse seguro de inteiro. Retorna 0 se vazio ou inválido.
        /// </summary>
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
